using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using bcpDevKit;
using bcpDevKit.Entities;
using bcpDevKit.Entities.General;
using bcpDevKit.Entities.Items;
using bcpDevKit.Entities.Vault;
using Dapper;
using IDB.Core.Data.Entity;
using IDB.Core.Data.Link;
using IDB.Core.Data.Relation;
using IDB.Translate.BCP.Helper;
using log4net;
using File = IDB.Core.Data.Entity.File;

namespace IDB.Translate.BCP
{
    public partial class MainForm : Form
    {
        private static readonly ILog Log = LogManager.GetLogger("IDBTranslateBCP");

        private Dictionary<long, Folder> _folders;
        private Dictionary<long, File> _files;
        private Dictionary<long, Item> _items;
        private Dictionary<long, CustomObject> _customObjects;

        private List<FileFileRelation> _fileFileRelations;
        private List<ItemFileRelation> _itemFileRelations;
        private List<ItemItemRelation> _itemItemRelations;

        private List<CustomObjectCustomObjectLink> _customObjectCustomObjectLinks;

        private int _progressTotalCount;
        private int _progressPct;
        private string _progressText;

        private bool _exportInProgress;

        //TODO: Links

        public MainForm()
        {
            InitializeLogging();
            InitializeComponent();

            txtConnectionString.Text = Core.Settings.IdbConnectionString;
            txtExportDirectory.Text = Core.Settings.ExportPath;
            var vaultVersion = Core.Settings.VaultVersion;
            comboBoxVaultVersion.Text = comboBoxVaultVersion.Items.Contains(vaultVersion) ? vaultVersion : "2020";
        }

        #region UI Events
        private void OnBtnExportClick(object sender, EventArgs e)
        {
            if (_exportInProgress)
                return;

            var message = "Please make sure your package has been validated before the export runs. Do you want to continue?";
            if (MessageBox.Show(message, "Validation completed?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            _exportInProgress = true;
            btnExport.Enabled = false;
            Export();
        }

        private void OnBtnSelectClick(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog
            {
                SelectedPath = txtExportDirectory.Text, 
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
                txtExportDirectory.Text = dialog.SelectedPath;
        }

        private void OnBtnCloseClick(object sender, EventArgs e)
        {
            Close();
        }
        private void OnTxtConnectionStringTextChanged(object sender, EventArgs e)
        {
            Core.Settings.IdbConnectionString = txtConnectionString.Text;
        }

        private void OnComboBoxVaultVersionTextChanged(object sender, EventArgs e)
        {
            Core.Settings.VaultVersion = comboBoxVaultVersion.Text;
        }

        private void OnTxtExportDirectoryTextChanged(object sender, EventArgs e)
        {
            Core.Settings.ExportPath = txtExportDirectory.Text;
        }
        #endregion

        #region Functions
        private async void Export()
        {
            try
            {
                Log.Info("Start exporting ...");
                Cursor = Cursors.WaitCursor;

                var progressTotal = new Progress<int>(v => progressBarTotal.Value = v);
                var progressTotalText = new Progress<string>(t => labelProgressTotal.Text = t);
                var progressTask = new Progress<int>(v => progressBarTask.Value = v);
                var progressTaskText = new Progress<string>(t => labelProgressTask.Text = t);


                var exportDirectory = txtExportDirectory.Text;
                var bcpVersionText = comboBoxVaultVersion.Text;

                var success = await Task.Run(() => ReadFromDatabase(progressTotal, progressTotalText, progressTask, progressTaskText));
                if (!success)
                    throw new ApplicationException($"Error reading Load database. Please check log file!");

                var bcpService = await Task.Run(() => CreateBcpService(exportDirectory, bcpVersionText, progressTotal, progressTotalText, progressTask, progressTaskText));

                success = await Task.Run(() => ProcessBcpFiles(bcpService, progressTotal, progressTotalText, progressTask, progressTaskText));
                success = await Task.Run(() => ProcessBcpFolders(bcpService, progressTotal, progressTotalText, progressTask, progressTaskText));
                success = await Task.Run(() => ProcessBcpItems(bcpService, progressTotal, progressTotalText, progressTask, progressTaskText));
                success = await Task.Run(() => ProcessCustomObjects(bcpService, progressTotal, progressTotalText, progressTask, progressTaskText));
                success = await Task.Run(() => ProcessFileFileRelations(bcpService, progressTotal, progressTotalText, progressTask, progressTaskText));
                success = await Task.Run(() => ProcessItemItemRelations(bcpService, progressTotal, progressTotalText, progressTask, progressTaskText));
                success = await Task.Run(() => ProcessItemFileRelations(bcpService, progressTotal, progressTotalText, progressTask, progressTaskText));
                success = await Task.Run(() => ProcessCustomObjectRelations(bcpService, progressTotal, progressTotalText, progressTask, progressTaskText));

                success = await Task.Run(() => WriteBcpFiles(bcpService, progressTotal, progressTotalText, progressTask, progressTaskText));
                success = await Task.Run(() => RemoveSpecialCharacters(exportDirectory, progressTotal, progressTotalText, progressTask, progressTaskText));
                await Task.Run(() => FinishExport());

                Process.Start(exportDirectory);

                if (!_exportInProgress)
                    btnExport.Enabled = true;

                Log.InfoFormat("Export finished! Export directory: {0}", exportDirectory);
                MessageBox.Show("Successfully create BCP package.", "coolOrange BCP Intermediate DB utility", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log.Error("Unknown error in export", ex);
                MessageBox.Show($"Failed to create BCP package: {ex.Message}", "coolOrange BCP Intermediate DB utility", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        #endregion

        #region Tasks
        private bool ReadFromDatabase(IProgress<int> progressTotal, IProgress<string> progressTotalText, IProgress<int> progressTask, IProgress<string> progressTaskText)
        {
            try
            {
                Log.Info("Reading Load database ...");
                using (var connection = new SqlConnection(txtConnectionString.Text))
                {
                    Log.Debug("Opening connection ...");
                    connection.Open();

                    Log.Debug("Reading Folders table ...");
                    SetProgress("Progress Total: Reading intermediate DB", progressTotalText, 0, progressTotal);
                    SetProgress("Reading 'Folders' table ...", progressTaskText, 0, progressTask);
                    _folders = connection.Query(@"SELECT * FROM Folders")
                        .Select(x => new KeyValuePair<long, Folder>(x.FolderID, new Folder(x)))
                        .ToDictionary(t => t.Key, t => t.Value);
                    Log.Debug($"Reading Folders table. Done! Number of folders: {_folders.Count}");

                    Log.Debug("Reading Files table ...");
                    SetProgress("Reading 'Files' table ...", progressTaskText, 15, progressTask);
                    var filesQuery = !string.IsNullOrEmpty(Settings.CustomFilesOrderByFields)
                        ? $@"SELECT * FROM Files Where IsExcluded = 0 OR IsExcluded is NULL ORDER BY {Settings.CustomFilesOrderByFields}"
                        : @"SELECT * FROM Files Where IsExcluded = 0 OR IsExcluded is NULL ORDER BY FileName, RevisionLabel, Version";
                    _files = connection.Query(filesQuery)
                        .Select(x => new KeyValuePair<long, File>(x.FileID, new File(x)))
                        .ToDictionary(t => t.Key, t => t.Value);
                    Log.Debug($"Reading Files table. Done! Number of files: {_files.Count}");

                    Log.Debug("Reading Items table ...");
                    SetProgress("Reading 'Items' table ...", progressTaskText, 30, progressTask);
                    var itemsQuery = !string.IsNullOrEmpty(Settings.CustomItemsOrderByFields)
                        ? $@"SELECT * FROM Items ORDER BY {Settings.CustomItemsOrderByFields}"
                        : @"SELECT * FROM Items ORDER BY ItemNumber, RevisionLabel, Version";
                    _items = connection.Query(itemsQuery)
                        .Select(x => new KeyValuePair<long, Item>(x.ItemID, new Item(x)))
                        .ToDictionary(t => t.Key, t => t.Value);
                    Log.Debug($"Reading Items table. Done! Number of items: {_items.Count}");

                    Log.Debug("Reading CustomObjects table ...");
                    SetProgress("Reading 'CustomObjects' table ...", progressTaskText, 45, progressTask);
                    _customObjects = connection.Query(@"SELECT * FROM CustomObjects")
                        .Select(x => new KeyValuePair<long, CustomObject>(x.CustomObjectID, new CustomObject(x)))
                        .ToDictionary(t => t.Key, t => t.Value);
                    Log.Debug($"Reading CustomObjects table. Done! Number of custom objects: {_items.Count}");

                    Log.Debug("Reading entity relation tables ...");
                    SetProgress("Reading 'FileFileRelations' table ...", progressTaskText, 60, progressTask);
                    _fileFileRelations = connection.Query<FileFileRelation>(@"SELECT * FROM FileFileRelations").ToList();
                    SetProgress("Reading 'ItemFileRelations' table ...", progressTaskText, 70, progressTask);
                    _itemFileRelations = connection.Query<ItemFileRelation>(@"SELECT * FROM ItemFileRelations").ToList();
                    SetProgress("Reading 'ItemItemRelations' table ...", progressTaskText, 80, progressTask);
                    _itemItemRelations = connection.Query(@"SELECT * FROM ItemItemRelations").Select(x => new ItemItemRelation(x)).ToList();

                    SetProgress("Reading 'CustomObjectCustomObjectLinks' table ...", progressTaskText, 90, progressTask);
                    _customObjectCustomObjectLinks = connection.Query<CustomObjectCustomObjectLink>(@"SELECT * FROM CustomObjectCustomObjectLinks").ToList();
                    //TODO: do this for all the different link types
                    Log.Debug("Reading entity relation tables. Done!");

                    SetProgress("Reading from intermediate DB. Done!", progressTaskText, 100, progressTask);
                    SetProgress("Progress Total:", progressTotalText, 10, progressTotal);
                }
                Log.Info("Reading Load database. Done!");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error reading Load database!", ex);
                return false;
            }
        }

        private IBcpService CreateBcpService(string exportDirectory, string bcpVersionText,
            IProgress<int> progressTotal, IProgress<string> progressTotalText, IProgress<int> progressTask, IProgress<string> progressTaskText)
        {
            try
            {
                SetProgress("Progress Total: Initializing BCP Service", progressTotalText, 10, progressTotal);
                var bcpSvcBuilder = new BcpServiceBuilder {Version = GetBcpVersion(bcpVersionText)};
                if (!Directory.Exists(exportDirectory))
                    Directory.CreateDirectory(exportDirectory);
                bcpSvcBuilder.SetPackageLocation(exportDirectory);
                var bcpService = bcpSvcBuilder.Build();

                SetProgress("Initializing BCP Service. Done!", progressTaskText, 100, progressTask);
                SetProgress("Progress Total:", progressTotalText, 20, progressTotal);
                return bcpService;
            }
            catch (Exception ex)
            {
                //todo: logging
                throw new ApplicationException($"Error creating BCP service: {ex.Message}");
            }
        }

        private bool ProcessBcpFiles(IBcpService bcpService,
            IProgress<int> progressTotal, IProgress<string> progressTotalText, IProgress<int> progressTask, IProgress<string> progressTaskText)
        {
            try
            {
                Log.Info("Processing files ...");
                SetProgress("Progress Total: Processing BCP files", progressTotalText, 20, progressTotal);

                var fileMasters = _files.GroupBy(x => new { x.Value.FileName, x.Value.FolderID }).ToDictionary(x => x.Key, x => x.Select(i => i.Value).ToList());
                ProgressInit(fileMasters.Count(), "Processing BCP files", progressTaskText, progressTask);

                var count = 0;
                var totalCount = fileMasters.Count();
                foreach (var fileMaster in fileMasters)
                {
                    ProgressStep(++count, progressTaskText, progressTask);
                    var fileNameMaster = fileMaster.Value.First().FileName;
                    Log.InfoFormat("Processing file {0}/{1}: {2}, {3} iterations", count, totalCount, fileNameMaster, fileMaster.Value.Count);

                    try
                    {
                        FileObject fileObject = null;
                        var iterationCount = 0;
                        foreach (var fileIteration in fileMaster.Value)
                        {
                            iterationCount++;

                            long? folderId = fileIteration.FolderID;
                            var vaultFullFolderPath = GetFullFolderPath(folderId);
                            var vaultFullFileName = vaultFullFolderPath + "/" + fileIteration.FileName;
                            Debug.WriteLine(vaultFullFileName);

                            if (fileObject == null)
                            {
                                Log.DebugFormat("Adding first file iteration ({0}) of file: {1}, Revision: {2}", iterationCount, vaultFullFileName, fileIteration.RevisionLabel);
                                fileObject = bcpService.FileService.AddFile(vaultFullFileName, fileIteration.LocalFullFileName);
                                fileObject.LatestRevision.Label = fileIteration.RevisionLabel;
                                fileObject.LatestRevision.Definition = (string.IsNullOrEmpty(fileIteration.RevisionDefinition)) ? null : fileIteration.RevisionDefinition;
                            }
                            else
                            {
                                if (fileObject.LatestRevision.Label != fileIteration.RevisionLabel)
                                {
                                    Log.DebugFormat("Adding file iteration {0}: New revision: {1}", iterationCount, fileIteration.RevisionLabel);
                                    fileObject.AddRevision(fileIteration.LocalFullFileName, fileIteration.RevisionLabel);
                                    fileObject.LatestRevision.Label = fileIteration.RevisionLabel;
                                    fileObject.LatestRevision.Definition = (string.IsNullOrEmpty(fileIteration.RevisionDefinition)) ? null : fileIteration.RevisionDefinition;
                                }
                                else
                                {
                                    Log.DebugFormat("Adding file iteration {0}", iterationCount);
                                    if (fileObject.LatestIteration.LocalPath == fileIteration.LocalFullFileName)
                                    {
                                        // workaround for issue #46: if the new iteration has the same local name FileRevision.AddIteration() does not create a new iteration
                                        var tempLocalFullFileName = fileIteration.LocalFullFileName + "_temp";
                                        var addedIteration = fileObject.LatestRevision.AddIteration(tempLocalFullFileName);
                                        addedIteration.LocalPath = fileIteration.LocalFullFileName;
                                    }
                                    else
                                        fileObject.LatestRevision.AddIteration(fileIteration.LocalFullFileName);
                                }
                            }

							var createUser = fileIteration.CreateUser;
                            if (string.IsNullOrEmpty(fileIteration.CreateUser))
                                createUser = "cO";
                            Log.DebugFormat("Adding 'Created' element: User: {0}, Date: {1}", createUser, fileIteration.CreateDate);
                            var created = new CreatedObject(bcpService.EntitiesTable.Vault, fileIteration.CreateDate)
                            {
                                User = createUser
                            };
                            fileObject.LatestIteration.Created = created;

                            if (!string.IsNullOrEmpty(fileIteration.LifecycleDefinition) && !string.IsNullOrEmpty(fileIteration.LifecycleState))
                            {
                                Log.DebugFormat("Adding 'State' element: User: {0}, Date: {1}", fileIteration.LifecycleDefinition, fileIteration.LifecycleState);
                                var state = new StateObject(fileIteration.LifecycleDefinition, fileIteration.LifecycleState, fileObject.LatestIteration);
                                fileObject.LatestIteration.State = state;
                            }
                            else
                            {
                                Log.DebugFormat("Iteration {0} has no LifecycleDefinition or state", iterationCount);
                            }
                            fileObject.LatestIteration.Comment = fileIteration.Comment;

                            fileObject.Category = fileIteration.Category;
                            fileObject.Classification = fileIteration.Classification;

                            foreach (var udp in fileIteration.UserDefinedProperties)
                            {
                                if (udp.Value == null)
                                    continue;

                                Log.DebugFormat("Adding UDP '{0}' = '{1}'", udp.Key, udp.Value);
                                if (udp.Value is DateTime time)
                                    fileObject.LatestIteration.AddProperty(udp.Key, $"{time:yyyy-MM-ddTHH:mm:ss.fff}");
                                else
                                    fileObject.LatestIteration.AddProperty(udp.Key, udp.Value.ToString());
                            }

                            fileIteration.BcpFileIteration = fileObject.LatestIteration;
                            Log.DebugFormat("Adding file iteration. Done!" + Environment.NewLine);
                        }

                        if (fileMaster.Value.Last().IsHidden)
	                        fileObject.Hidden = "true";

                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error processing file '{fileNameMaster}'", ex);
                    }
                }
                SetProgress("Progress Total:", progressTotalText, 30, progressTotal);
                Log.Info("Processing files. Done!");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Unknown error processing files", ex);
                return false;
            }
        }

        private bool ProcessBcpFolders(IBcpService bcpService,
            IProgress<int> progressTotal, IProgress<string> progressTotalText, IProgress<int> progressTask, IProgress<string> progressTaskText)
        {
            try
            {
                SetProgress("Progress Total: Processing BCP folders", progressTotalText, 30, progressTotal);
                ProgressInit(_folders.Count(), "Processing BCP folders", progressTaskText, progressTask);

                var count = 0;
                foreach (var folder in _folders.Values)
                {
                    ProgressStep(++count, progressTaskText, progressTask);

                    long? folderId = folder.FolderID;
                    var vaultFullFolderPath = GetFullFolderPath(folderId);

                    var folderObject = bcpService.FileService.SearchFolderByPath(vaultFullFolderPath);
                    if (folderObject == null)
                        folderObject = bcpService.FileService.AddFolder(vaultFullFolderPath);

                    folderObject.SetLibrary(folder.IsLibrary);
                    folderObject.Category = folder.Category;
                    foreach (var udp in folder.UserDefinedProperties)
                    {
                        if (udp.Value == null)
                            continue;

                        if (udp.Value is DateTime time)
                            folderObject.AddProperty(udp.Key, $"{time:yyyy-MM-ddTHH:mm:ss.fff}");
                        else
                            folderObject.AddProperty(udp.Key, udp.Value.ToString());
                    }
                    folder.BcpFolderObject = folderObject;
                }
                SetProgress("Progress Total:", progressTotalText, 40, progressTotal);
                return true;
            }
            catch (Exception ex)
            {
                //todo: logging
                throw new ApplicationException($"Error processing BCP folders: {ex.Message}"); ;
            }
        }

        private bool ProcessBcpItems(IBcpService bcpService,
            IProgress<int> progressTotal, IProgress<string> progressTotalText, IProgress<int> progressTask, IProgress<string> progressTaskText)
        {
            try
            {
                Log.Info("Processing items ...");
                SetProgress("Progress Total: Processing BCP items", progressTotalText, 40, progressTotal);

                var itemMasters = _items.GroupBy(x => new { x.Value.ItemNumber }).ToDictionary(x => x.Key, x => x.Select(i => i.Value).ToList());
                ProgressInit(itemMasters.Count(), "Processing BCP items", progressTaskText, progressTask);

                var count = 0;
                var totalCount = itemMasters.Count();
                foreach (var itemMaster in itemMasters)
                {
                    ProgressStep(++count, progressTaskText, progressTask);
                    var itemNumberMaster = itemMaster.Value.First().ItemNumber;
                    Log.InfoFormat("Processing item {0}/{1}: {2}, {3} iterations", count, totalCount, itemNumberMaster, itemMaster.Value.Count);

                    try
                    {
                        ItemMaster bcpItemMaster = null;
                        var iterationCount = 0;
                        foreach (var itemIteration in itemMaster.Value)
                        {
                            iterationCount++;

                            if (bcpItemMaster == null)
                            {
                                Log.DebugFormat("Adding first item, iteration ({0}) of item: {1}, Revision: {2}", iterationCount, itemIteration.ItemNumber, itemIteration.RevisionLabel);
                                bcpItemMaster = bcpService.ItemService.AddItem(
                                    itemIteration.ItemNumber,
                                    itemIteration.Category,
                                    itemIteration.Title,
                                    itemIteration.Description);
                                bcpItemMaster.LatestItemRevision.Label = itemIteration.RevisionLabel;
                                bcpItemMaster.LatestItemRevision.DefName = itemIteration.RevisionDefinition;
                            }
                            else
                            {
                                if (bcpItemMaster.LatestItemRevision.Label != itemIteration.RevisionLabel)
                                {
                                    Log.DebugFormat("Adding item iteration {0}: New revision: {1}", iterationCount, itemIteration.RevisionLabel);
                                    bcpItemMaster.AddRevision(itemIteration.Title, itemIteration.Description);
                                    bcpItemMaster.LatestItemRevision.Label = itemIteration.RevisionLabel;
                                    bcpItemMaster.LatestItemRevision.DefName = itemIteration.RevisionDefinition;
                                }
                                else
                                {
                                    Log.DebugFormat("Adding item iteration {0}", iterationCount);
                                    var newItemIteration = bcpItemMaster.LatestItemRevision.AddIteration(itemIteration.Title, itemIteration.Description);
                                }
                            }

                            Log.DebugFormat("Adding 'Units' attribute: Unit: {0}", itemIteration.Unit);
                            bcpItemMaster.LatestIteration.Units = itemIteration.Unit;

                            var bomStructureType = GetBomStructureType(itemIteration.BomStructure);
                            Log.DebugFormat("Adding 'BOMStructure' attribute: BomStructure '{0}' -> {1}", itemIteration.Unit, bomStructureType);
                            bcpItemMaster.LatestIteration.BOMStructure = bomStructureType;

                            Log.DebugFormat("Adding 'CreateDate' attribute: Date: {0}", itemIteration.CreateDate);
                            bcpItemMaster.LatestIteration.SetCreateDateFormatted(itemIteration.CreateDate);

                            var createUser = itemIteration.CreateUser;
                            if (string.IsNullOrEmpty(itemIteration.CreateUser))
                                createUser = "cO";
                            Log.DebugFormat("Adding 'UserGroupName' element: User: {0}", createUser);
                            bcpItemMaster.LatestIteration.UserGroupName = createUser;

                            if (!string.IsNullOrEmpty(itemIteration.LifecycleDefinition))
                            {
                                Log.DebugFormat("Adding 'State' element: User: {0}, Date: {1}", itemIteration.LifecycleDefinition, itemIteration.LifecycleState);
                                var state = new StateObject(itemIteration.LifecycleDefinition, itemIteration.LifecycleState, bcpItemMaster.LatestIteration);
                                bcpItemMaster.LatestIteration.State = state;
                            }
                            else
                            {
                                Log.DebugFormat("Iteration {0} has no LifecycleDefinition", iterationCount);
                            }
                            bcpItemMaster.LatestIteration.Comment = itemIteration.Comment;

                            bcpItemMaster.Category = itemIteration.Category;

                            // set the latest iteration number to that of the itemIteration.Version
                            // NOTE: bcpToolkit is not auto-incrementing value
                            // Per https://github.com/coolOrangeLabs/bcpIntermediateDatabase/issues/47 
                            // support case #4056
                            bcpItemMaster.LatestIteration.IterationNumber = itemIteration.Version;

                            foreach (var udp in itemIteration.UserDefinedProperties)
                            {
                                if (udp.Value == null)
                                    continue;

                                Log.DebugFormat("Adding UDP '{0}' = '{1}'", udp.Key, udp.Value);
                                if (udp.Value is DateTime time)
                                    bcpItemMaster.LatestIteration.AddProperty(udp.Key, $"{time:yyyy-MM-ddTHH:mm:ss.fff}");
                                else
                                    bcpItemMaster.LatestIteration.AddProperty(udp.Key, udp.Value.ToString());
                            }

                            itemIteration.BcpItemIteration = bcpItemMaster.LatestIteration;
                            Log.DebugFormat("Adding item iteration. Done!" + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error processing item '{itemNumberMaster}'", ex);
                    }
                }
                SetProgress("Progress Total:", progressTotalText, 50, progressTotal);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Unknown error processing files", ex);
                return false;
            }
        }

        private bool ProcessCustomObjects(IBcpService bcpService,
            IProgress<int> progressTotal, IProgress<string> progressTotalText, IProgress<int> progressTask, IProgress<string> progressTaskText)
        {
            try
            {
                SetProgress("Progress Total: Processing BCP custom objects", progressTotalText, 50, progressTotal);
                ProgressInit(_customObjects.Count(), "Processing BCP custom objects", progressTaskText, progressTask);

                var count = 0;
                foreach (var customObject in _customObjects.Values)
                {
                    ProgressStep(++count, progressTaskText, progressTask);

                    var bcpCustomObject = bcpService.CustomObjectService.AddCustomObject(customObject.CustomObjectDefinition, customObject.CustomObjectName);
                    customObject.BcpCustomObject = bcpCustomObject;

                    bcpCustomObject.AddLink(_files.Values.First().BcpFileIteration);
                }
                SetProgress("Progress Total:", progressTotalText, 60, progressTotal);
                return true;
            }
            catch (Exception ex)
            {
                //todo: logging
                throw new ApplicationException($"Error processing BCP custom objects: {ex.Message}"); ;
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private bool ProcessFileFileRelations(IBcpService bcpService,
            IProgress<int> progressTotal, IProgress<string> progressTotalText, IProgress<int> progressTask, IProgress<string> progressTaskText)
        {
            try
            {
                SetProgress("Progress Total: Processing BCP file/file relations", progressTotalText, 60, progressTotal);
                ProgressInit(_files.Count(), "Processing BCP file/file relations", progressTaskText, progressTask);

                var count = 0;
                var totalCount = _files.Count();
                foreach (var fileIteration in _files.Values)
                {
                    ProgressStep(++count, progressTaskText, progressTask);
                    var fileFileRelations = _fileFileRelations.Where(f => f.ParentFileID == fileIteration.FileID);
                    Log.InfoFormat("Processing BCP file/file relations for file {0}/{1}: {2}, {3} file/file relations", count, totalCount, fileIteration.FileName, fileFileRelations.Count());

                    try
                    {
                        foreach (var fileFileRelation in fileFileRelations)
                        {
                            if (_files.TryGetValue(fileFileRelation.ChildFileID, out var childFile))
                            {
                                if (fileFileRelation.IsAttachment)
                                {
                                    fileIteration.BcpFileIteration.AddAssociation(childFile.BcpFileIteration, AssocType.Attachment);
                                }
                                else if (fileFileRelation.IsDependency)
                                {
                                    var assoc = fileIteration.BcpFileIteration.AddAssociation(childFile.BcpFileIteration, AssocType.Dependency);
                                    assoc.RefId = fileFileRelation.RefId;
                                    assoc.Source = fileFileRelation.Source;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error processing file/file relations for file '{fileIteration.FileName}'", ex);
                    }
                }
                SetProgress("Progress Total:", progressTotalText, 65, progressTotal);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Unknown error processing file/file relations", ex);
                return false;
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private bool ProcessItemItemRelations(IBcpService bcpService,
            IProgress<int> progressTotal, IProgress<string> progressTotalText, IProgress<int> progressTask, IProgress<string> progressTaskText)
        {
            try
            {
                SetProgress("Progress Total: Processing BCP item/item relations", progressTotalText, 65, progressTotal);
                ProgressInit(_itemItemRelations.Count(), "Processing BCP item/item relations", progressTaskText, progressTask);

                var count = 0;
                var totalCount = _itemItemRelations.Count;
                foreach (var itemItemRelation in _itemItemRelations)
                {
                    ProgressStep(++count, progressTaskText, progressTask);
                    Log.InfoFormat("Processing BCP item/item relations {0}/{1}: Parent ID: {2}, Child ID: {3}", count, totalCount, itemItemRelation.ParentItemID, itemItemRelation.ChildItemID);

                    try
                    {
                        if (!_items.TryGetValue(itemItemRelation.ParentItemID, out var parent))
                        {
                            Log.ErrorFormat($"Parent item with ID {itemItemRelation.ParentItemID} not found");
                            continue;
                        }
                        if (!_items.TryGetValue(itemItemRelation.ChildItemID, out var child))
                        {
                            Log.ErrorFormat($"Child item with ID {itemItemRelation.ChildItemID} not found");
                            continue;
                        }

                        var bomLink = parent.BcpItemIteration.AddBomLink(child.BcpItemIteration.Revision.Item,
                            itemItemRelation.Position, (double)itemItemRelation.Quantity, itemItemRelation.Unit,
                            (itemItemRelation.LinkType == "CALC") ? BomLinkType.CALC : BomLinkType.STAT);
                        if (UnitsHelper.Instance.HasCustomUnitDefinitions())
                            bomLink.UofMID = UnitsHelper.Instance.GetUifMid(itemItemRelation.Unit);

                        foreach (var udp in itemItemRelation.UserDefinedProperties)
                        {
                            if (udp.Value == null)
                                continue;

                            Log.DebugFormat("Adding UDP '{0}' = '{1}'", udp.Key, udp.Value);
                            if (udp.Value is DateTime time)
                                bomLink.AddProperty(udp.Key, $"{time:yyyy-MM-ddTHH:mm:ss.fff}");
                            else
                                bomLink.AddProperty(udp.Key, udp.Value.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error processing item/item relations for Parent ID: {itemItemRelation.ParentItemID}, Child ID: {itemItemRelation.ChildItemID}", ex);
                    }
                }
                SetProgress("Progress Total:", progressTotalText, 70, progressTotal);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Unknown error processing item/item relations", ex);
                return false;
            }
        }

        private bool ProcessItemFileRelations(IBcpService bcpService,
            IProgress<int> progressTotal, IProgress<string> progressTotalText, IProgress<int> progressTask, IProgress<string> progressTaskText)
        {
            try
            {
                SetProgress("Progress Total: Processing BCP item/file relations", progressTotalText, 70, progressTotal);
                ProgressInit(_itemFileRelations.Count(), "Processing BCP item/file relations", progressTaskText, progressTask);

                var count = 0;
                var totalCount = _itemFileRelations.Count;
                foreach (var itemFileRelation in _itemFileRelations)
                {
                    ProgressStep(++count, progressTaskText, progressTask);
                    Log.InfoFormat("Processing BCP item/file relations {0}/{1}: Item ID: {2}, File ID: {3}", count, totalCount, itemFileRelation.ItemID, itemFileRelation.FileID);

                    try
                    {
                        if (!_items.TryGetValue(itemFileRelation.ItemID, out var item))
                        {
                            Log.ErrorFormat($"Item with ID {itemFileRelation.ItemID} not found");
                            continue;
                        }
                        if (!_files.TryGetValue(itemFileRelation.FileID, out var file))
                        {
                            Log.ErrorFormat($"File with ID {itemFileRelation.FileID} not found");
                            continue;
                        }

                        if (itemFileRelation.IsAttachment)
                        {
                            Log.DebugFormat("Adding file as <Link> element for item iteration");
                            item.BcpItemIteration.AddAttachement(file.BcpFileIteration);
                            Log.DebugFormat("Successfully added file as <Link> element");
                        }
                        else
                        {
                            Log.DebugFormat("Creating <itemToComp> element and set 'LinkType'");
                            bcpService.ItemService.AddFileLinkToItem(item.BcpItemIteration, file.BcpFileIteration);
                            Log.DebugFormat("Successfully created <itemToComp> element and set 'LinkType'");
                        }
                    }

                    catch (Exception ex)
                    {
                        Log.Error($"Processing BCP item/file relations: Item ID: {itemFileRelation.ItemID}, Child ID: {itemFileRelation.FileID}", ex);
                    }
                }

                SetProgress("Progress Total:", progressTotalText, 75, progressTotal);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Unknown error processing item/file relations", ex);
                return false;
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private bool ProcessCustomObjectRelations(IBcpService bcpService,
            IProgress<int> progressTotal, IProgress<string> progressTotalText, IProgress<int> progressTask, IProgress<string> progressTaskText)
        {
            try
            {
                SetProgress("Progress Total: Processing BCP custom object/custom object relations", progressTotalText, 75, progressTotal);
                ProgressInit(_customObjectCustomObjectLinks.Count(), "Processing BCP custom object/custom object relations", progressTaskText, progressTask);

                var count = 0;
                foreach (var link in _customObjectCustomObjectLinks)
                {
                    ProgressStep(++count, progressTaskText, progressTask);

                    var parent = _customObjects[link.ParentCustomObjectID];
                    var child = _customObjects[link.ChildCustomObjectID];
                    parent.BcpCustomObject.AddLink(child.BcpCustomObject);
                }

                SetProgress("Progress Total:", progressTotalText, 80, progressTotal);
                return true;
            }
            catch (Exception ex)
            {
                //todo: logging
                throw new ApplicationException($"Error processing BCP custom object/custom object relations: {ex.Message}"); ;
            }
        }

        private bool WriteBcpFiles(IBcpService bcpService,
            IProgress<int> progressTotal, IProgress<string> progressTotalText, IProgress<int> progressTask, IProgress<string> progressTaskText)
        {
            try
            {
                SetProgress("Progress Total: Writing BCP files", progressTotalText, 80, progressTotal);

                // see if we want to disable configurations export
                if (DisableConfigurationExportCheckBox.Checked)
                {
                    // turn off the export
                    bcpService.Settings.DisableConfigurationExport();
                }

                bcpService.Flush();
                if (UnitsHelper.Instance.HasCustomUnitDefinitions())
                    UnitsHelper.Instance.OverrideCustomUnitDefinitionsFile(txtExportDirectory.Text);
                SetProgress("Writing BCP files. Done!", progressTaskText, 100, progressTask);

                SetProgress("Progress Total:", progressTotalText, 90, progressTotal);
                return true;
            }
            catch (Exception ex)
            {
                //todo: logging
                throw new ApplicationException($"Error writing BCP files: {ex.Message}"); ;
            }
        }

        private bool RemoveSpecialCharacters(string exportDirectory,
            IProgress<int> progressTotal, IProgress<string> progressTotalText, IProgress<int> progressTask, IProgress<string> progressTaskText)
        {
            try
            {
                SetProgress("Progress Total: Removing special characters from BCP files", progressTotalText, 90, progressTotal);
                RemoveSpecialCharactersFromXmlFiles(exportDirectory);
                SetProgress("Removing special characters. Done!", progressTaskText, 100, progressTask);

                SetProgress("Progress Total:", progressTotalText, 100, progressTotal);
                return true;
            }
            catch (Exception ex)
            {
                //todo: logging
                throw new ApplicationException($"Error removing special characters: {ex.Message}"); ;
            }
        }

        private void FinishExport()
        {
            _exportInProgress = false;
        }
        #endregion

        #region Progress
        private void SetProgress(string text, IProgress<string> progressText, int val, IProgress<int> progress)
        {
            progress.Report(val);
            progressText.Report(text);

            if (val == 100)
            {
                Thread.Sleep(3000);
                progress.Report(0);
                progressText.Report(string.Empty);
            }
        }

        private void ProgressInit(int totalCount, string text, IProgress<string> progressText, IProgress<int> progress)
        {
            _progressTotalCount = totalCount;
            _progressPct = 0;
            _progressText = text;

            SetProgress($"{_progressText}: 0/{_progressTotalCount} ({_progressPct}%)", progressText, _progressPct, progress);
        }

        private void ProgressStep(int count, IProgress<string> progressText, IProgress<int> progress)
        {
            var pct = (int)(count / (_progressTotalCount * 0.01));
            if (pct > _progressPct)
            {
                _progressPct = pct;
                SetProgress($"{_progressText}: {count}/{_progressTotalCount} ({_progressPct}%)", progressText, _progressPct, progress);
            }
        }
        #endregion

        #region Helpers
        private void RemoveSpecialCharactersFromXmlFiles(string exportFolder)
        {
            RemoveSpecialCharactersFromXmlFile(exportFolder, "Vault.xml");
            RemoveSpecialCharactersFromXmlFile(exportFolder, "ItemsWrapper.xml");
            RemoveSpecialCharactersFromXmlFile(exportFolder, "BOMwrapper.xml");
            RemoveSpecialCharactersFromXmlFile(exportFolder, "CustomObjectWrapper.xml");
        }

        private void RemoveSpecialCharactersFromXmlFile(string exportFolder, string xmlFile)
        {
            try
            {
                //Log.InfoFormat("Removing invalid special characters from BCP file '{0}' ...", xmlFile);

                long countLines = 0;
                long countReplaced = 0;
                var foundSequences = new Dictionary<string, int>();

                var xmlFileFullPath = Path.Combine(exportFolder, xmlFile);
                var xmlFileTempFile = Path.Combine(exportFolder, xmlFile + ".temp");
                using (var input = System.IO.File.OpenText(xmlFileFullPath))
                using (var output = new StreamWriter(xmlFileTempFile))
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        countLines++;
                        var lineReplaced = line.RemoveEscapeSequenceFromXmlString(foundSequences, out var countReplacedLine);
                        countReplaced += countReplacedLine;
                        output.WriteLine(lineReplaced);
                    }
                }
                //Log.DebugFormat("Deleting original file ...");
                System.IO.File.Delete(xmlFileFullPath);
                //Log.DebugFormat("Deleting original file. Done!");

                //Log.DebugFormat("Renaming temporary file ...");
                System.IO.File.Move(xmlFileTempFile, xmlFileFullPath);
                //Log.DebugFormat("Renaming temporary file. Done!");

                //Log.InfoFormat("Done! Lines: {0}, Replaced: {1}", countLines, countReplaced);
                //foreach (var escSequence in foundSequences)
                //    Log.InfoFormat("Escape sequences: {0}: {1}", escSequence.Key, escSequence.Value);
            }
            catch (Exception)
            {
                //Log.ErrorFormat("Failed to remove invalid special characters from BCP file '{0}': {1}", xmlFile, ex.Message);
            }
        }

        private string GetFullFolderPath(long? folderId)
        {
            // check if field 'Path' already contains valid Vault path
            if (_folders.TryGetValue(folderId.Value, out var folder))
            {
                if (!string.IsNullOrEmpty(folder.Path) && (folder.Path.StartsWith("$/") || folder.Path == "$"))
                    return folder.Path;
            }

            // path doesn't contain valid Vault path. Build Vault Path by going up the structure using 'ParentFolderID'
            string fullFolderPath = null;
            while (folderId != null && _folders.TryGetValue(folderId.Value, out folder))
            {
                fullFolderPath = folder.FolderName + "/" + fullFolderPath;
                folderId = folder.ParentFolderID;
            }

            return "$/" + fullFolderPath;
        }

        private BomStructureType GetBomStructureType(string bomStructure)
        {
            BomStructureType bomStructureType;
            switch (bomStructure)
            {
                case "Normal": bomStructureType = BomStructureType.Normal; break;
                case "Inseperable": bomStructureType = BomStructureType.Inseperable; break;
                case "Phantom": bomStructureType = BomStructureType.Phantom; break;
                case "Purchased": bomStructureType = BomStructureType.Purchased; break;
                case "Reference": bomStructureType = BomStructureType.Reference; break;
                default: bomStructureType = BomStructureType.Default; break;
            }
            return bomStructureType;
        }

        private BcpVersion GetBcpVersion(string bcpVersionText)
        {
            switch (bcpVersionText)
            {
                case "2017": return BcpVersion._2017;
                case "2018": return BcpVersion._2018;
                case "2019": return BcpVersion._2019;
                default: return BcpVersion._2020;
            }
        }

        private void InitializeLogging()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var fi = new FileInfo(thisAssembly.Location + ".log4net");
            log4net.Config.XmlConfigurator.Configure(fi);

            Log.Info($"powerLoad {Assembly.GetExecutingAssembly().GetName().Name} v{Assembly.GetExecutingAssembly().GetName().Version}");
        }
        #endregion
    }
}