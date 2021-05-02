using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Dapper;
using IDB.Core;
using IDB.Core.Data.Relation;
using log4net;
using File = IDB.Core.Data.Entity.File;
using Folder = IDB.Core.Data.Entity.Folder;

namespace IDB.Load.BCP
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger("IDBLoadBCP");

        private static string _connectionString;
        private static long _totalFolders;
        private static long _totalIterations;
        private static long _totalAssocs;
        private static long _processedFolders;
        private static long _processedIterations;
        private static long _processedAssocs;

        private static Dictionary<string, Folder> _existingFolders;
        private static Dictionary<UniqueFile, File> _existingFiles;
        private static Dictionary<UniqueRelation, FileFileRelation> _existingFileFileRelations;

        private static readonly HashSet<string> FolderUdps = new HashSet<string>();
        private static readonly HashSet<string> FileUdps = new HashSet<string>();
        private static readonly Dictionary<string, File> IterationIds = new Dictionary<string, File>();

        static async Task Main(string[] args)
        {
            InitializeLogging();

            string xmlFullFileName;

            if (args.Length > 0)
            {
                if (args.Length != 2)
                {
                    Log.Error("The first argument must be a SQL server connection string the second the BCP package path!");
                    Console.WriteLine("Press any key to close this window");
                    Console.ReadLine();
                    return;
                }
                _connectionString = args[0];
                xmlFullFileName = Path.Combine(args[1], "Vault.xml");
            }
            else
            {
                _connectionString = Settings.IdbConnectionString;
                xmlFullFileName = Path.Combine(Settings.ImportPath, "Vault.xml");
            }

            if (!IsValidConnectionString(_connectionString))
            {
                Log.Error($"Cannot connect to SQL Server '{_connectionString}'!");
                Console.WriteLine("Press any key to close this window");
                Console.ReadLine();
                return;
            }

            if (!System.IO.File.Exists(xmlFullFileName))
            {
                Log.Error($"The file '{xmlFullFileName}' does not exist!");
                Console.WriteLine("Press any key to close this window");
                Console.ReadLine();
                return;
            }

            var folderTask = ReadIdbFolders();
            var fileTask = ReadIdbFiles();
            var relationTask = ReadIdbFileFileRelations();
            var parseTask = ParseVaultXml(xmlFullFileName);

            await Task.WhenAll(folderTask, fileTask, relationTask, parseTask);

            Core.VaultBcp.Vault vault = parseTask.Result;

            if (vault == null)
                throw new ApplicationException("BCP file cannot be parsed");


            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                Log.Info("Adding UDP columns...");
                AddUdpColumns(connection, vault.Behaviors, FileUdps, "File");
                AddUdpColumns(connection, vault.Behaviors, FolderUdps, "Folder");

                var rootFolder = GetExistingFolder("$");
                if (rootFolder == null)
                {
                    rootFolder = new Folder();
                    rootFolder.Insert(connection);
                    _processedFolders++;
                    Log.Info("Insert root: " + rootFolder.Path);
                }
                else
                    Log.Warn("Existing root: " + rootFolder.Path);

                foreach (var bcpFile in vault.Root.Files)
                    ProcessFile(connection, rootFolder, bcpFile);

                foreach (var bcpFolder in vault.Root.Folders)
                    ProcessFolder(connection, rootFolder, bcpFolder);

                ProcessFileAssocs(connection, vault.Root.Files);
                ProcessFolderAssosc(connection, vault.Root.Folders);
            }

            Log.Info("Import finished!");
            Console.WriteLine("Press any key to close this window");
            Console.ReadLine();
        }

        private static void InitializeLogging()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var fi = new FileInfo(thisAssembly.Location + ".log4net");
            log4net.Config.XmlConfigurator.Configure(fi);

            Log.Info($"COOLORANGE {Assembly.GetExecutingAssembly().GetName().Name} v{Assembly.GetExecutingAssembly().GetName().Version}");
        }

        private static bool IsValidConnectionString(string connectionString)
        {
            if (!_connectionString.EndsWith(";"))
                _connectionString += ";";

            using (SqlConnection connection = new SqlConnection(connectionString + "Connection Timeout=3;"))
            {
                try
                {
                    connection.Open();
                    return connection.QuerySingle<int>("SELECT 1") == 1;
                }
                catch (SqlException)
                {
                    return false;
                }
            }
        }

        private static async Task ReadIdbFolders()
        {
            Log.Info("Reading Folders from IDB...");
            await Task.Run(() => {
                using (var connection = new SqlConnection(_connectionString))
                {
                    _existingFolders = Folder.GetAllFolders(connection, "SELECT FolderID, Path FROM Folders")
                        .Select(x => new KeyValuePair<string, Folder>(x.Path, new Folder(x)))
                        .ToDictionary(t => t.Key, t => t.Value, StringComparer.OrdinalIgnoreCase);
                }
            });
        }

        private static async Task ReadIdbFiles()
        {
            Log.Info("Reading Files from IDB...");
            await Task.Run(() => {
                using (var connection = new SqlConnection(_connectionString))
                {
                    _existingFiles = File.GetAllFiles(connection, "SELECT FileID, FolderID, FileName, CreateDate FROM Files")
                        .Select(x => new KeyValuePair<UniqueFile, File>(new UniqueFile(x), new File(x)))
                        .ToDictionary(t => t.Key, t => t.Value, new UniqueFileComparer());
                }
            });
        }

        private static async Task ReadIdbFileFileRelations()
        {
            Log.Info("Reading File-File Relations from IDB...");

            await Task.Run(() => {
                using (var connection = new SqlConnection(_connectionString))
                {
                    _existingFileFileRelations = FileFileRelation.GetAllFileFileRelations(connection, "SELECT ParentFileID, ChildFileID FROM FileFileRelations")
                        .Select(x =>
                            new KeyValuePair<UniqueRelation, FileFileRelation>(
                                new UniqueRelation(x.ParentFileID, x.ChildFileID), new FileFileRelation(x)))
                        .ToDictionary(t => t.Key, t => t.Value, new UniqueRelationComparer());
                }
            });
        }

        private static async Task<Core.VaultBcp.Vault> ParseVaultXml(string xmlFullFileName)
        {
            Log.Info($"Parsing {xmlFullFileName}...");
            return await Task.Run(() => {
                using (var sr = new StreamReader(xmlFullFileName))
                {
                    using (XmlTextReader reader = new XmlTextReader(sr))
                    {
                        reader.Namespaces = false;
                        var serializer = new XmlSerializer(typeof(Core.VaultBcp.Vault), new XmlRootAttribute("Vault"));
                        var vault = (Core.VaultBcp.Vault)serializer.Deserialize(reader);

                        ProcessFileUdps(vault.Root.Files);
                        ProcessFolderUdps(vault.Root.Folders);
                        Log.Info($"Processing {_totalIterations} file iterations in {_totalFolders + 1} folders...");

                        return vault;
                    }
                }
            });
        }

        private static void ProcessFolderUdps(List<Core.VaultBcp.Folder> bcpFolders)
        {
            foreach (var bcpFolder in bcpFolders)
            {
                _totalFolders++;
                foreach (var udp in bcpFolder.UDPs)
                    FolderUdps.Add(udp.Name);

                ProcessFileUdps(bcpFolder.Files);
                ProcessFolderUdps(bcpFolder.Folders);
            }
        }

        private static void ProcessFileUdps(List<Core.VaultBcp.File> bcpFiles)
        {
            foreach (var bcpFile in bcpFiles)
            {
                foreach (var bcpRevision in bcpFile.Revisions)
                {
                    foreach (var bcpIteration in bcpRevision.Iterations)
                    {
                        _totalIterations++;
                        _totalAssocs += bcpIteration.Associations.Count;
                        foreach (var udp in bcpIteration.UDPs)
                            FileUdps.Add(udp.Name);
                    }
                }
            }
        }

        private static void AddUdpColumns(SqlConnection connection, Core.VaultBcp.Behaviors behaviors, HashSet<string> udps, string entityType)
        {
            var columns = connection.Query<string>(
                    $"SELECT column_name FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{entityType}s' AND LEFT(column_name, 4) = 'UDP_'");

            foreach (var udp in udps)
            {
                var enumerable = columns as string[] ?? columns;
                if (!enumerable.Contains("UDP_" + udp))
                {
                    var propDef = behaviors?.PropertyDefinitions?.SingleOrDefault(p =>
                        p.Assignments.Any(a => a.Class.Equals(entityType) && p.Name.Equals(udp)));

                    string type;
                    if (propDef != null && propDef.Type.Equals("Numeric"))
                        type = "sql_variant";
                    else if (propDef != null && propDef.Type.Equals("DateTime"))
                        type = "datetime";
                    else if (propDef != null && propDef.Type.Equals("Bool"))
                        type = "bit";
                    else
                        type = "nvarchar(MAX)";

                    Log.Info($"Adding column UDP_{udp} to table {entityType}");
                    connection.Execute($"ALTER Table {entityType}s ADD [UDP_{udp}] {type}");
                }
            }
        }

        private static void ProcessFile(SqlConnection connection, Folder parentFolder, Core.VaultBcp.File bcpFile)
        {
            var i = 0;
            foreach (var bcpRevision in bcpFile.Revisions)
            {
                foreach (var bcpIteration in bcpRevision.Iterations)
                {
                    i++;
                    _processedIterations++;
                    var file = GetExistingFile(bcpFile.Name, parentFolder.FolderID, bcpIteration.Created.Date);
                    if (file == null)
                    {
                        file = new File(parentFolder, bcpFile, bcpRevision, bcpIteration, i);
                        file.Insert(connection);
                        _existingFiles.Add(new UniqueFile(file), file);
                        Log.Info($"Insert file: ({_processedIterations} of {_totalIterations}): {file.FileName}");
                    }
                    else
                        Log.Warn($"Existing file: ({_processedIterations} of {_totalIterations}): {file.FileName}");

                    IterationIds.Add(bcpIteration.Id, file);
                }
            }
        }

        private static void ProcessFolder(SqlConnection connection, Folder parentFolder, Core.VaultBcp.Folder bcpFolder)
        {
            _processedFolders++;
            var folder = GetExistingFolder(parentFolder.Path + "/" + bcpFolder.Name);
            if (folder == null)
            {
                folder = new Folder(parentFolder, bcpFolder);
                folder.Insert(connection);
                _existingFolders.Add(folder.Path, folder);
                Log.Info($"Insert folder ({_processedFolders} of {_totalFolders}): {folder.Path}");
            }
            else
                Log.Warn($"Existing folder ({_processedFolders} of {_totalFolders}): {folder.Path}");

            foreach (var file in bcpFolder.Files)
                ProcessFile(connection, folder, file);

            foreach (var subFolder in bcpFolder.Folders)
                ProcessFolder(connection, folder, subFolder);
        }

        private static void ProcessFolderAssosc(SqlConnection connection, List<Core.VaultBcp.Folder> bcpFolders)
        {
            foreach (var bcpFolder in bcpFolders)
            {
                ProcessFileAssocs(connection, bcpFolder.Files);
                ProcessFolderAssosc(connection, bcpFolder.Folders);
            }
        }

        private static void ProcessFileAssocs(SqlConnection connection, List<Core.VaultBcp.File> bcpFiles)
        {
            foreach (var bcpFile in bcpFiles)
            {
                foreach (var bcpRevision in bcpFile.Revisions)
                {
                    foreach (var bcpIteration in bcpRevision.Iterations)
                    {
                        foreach (var bcpAssociation in bcpIteration.Associations)
                        {
                            var parentFile = IterationIds[bcpIteration.Id];
                            var childFile = IterationIds[bcpAssociation.ChildId];
                            var isAttachment = bcpAssociation.Type == "Attachment";
                            var isDependency = bcpAssociation.Type == "Dependency";

                            _processedAssocs++;
                            var fileFileRelation = GetExistingFileFileRelation(parentFile.FileID, childFile.FileID);
                            if (fileFileRelation == null)
                            {
                                fileFileRelation = new FileFileRelation(parentFile.FileID, childFile.FileID, isAttachment, isDependency, bcpAssociation.Source, bcpAssociation.RefId);
                                fileFileRelation.Insert(connection);
                                _existingFileFileRelations.Add(new UniqueRelation(fileFileRelation.ParentFileID, fileFileRelation.ChildFileID), fileFileRelation);
                                Log.Info($"Insert association ({_processedAssocs} of {_totalAssocs}): ParentID {fileFileRelation.ParentFileID} - ChildID {fileFileRelation.ChildFileID}");
                            }
                            else
                                Log.Warn($"Existing association ({_processedAssocs} of {_totalAssocs}): ParentID {fileFileRelation.ParentFileID} - ChildID {fileFileRelation.ChildFileID}");
                        }
                    }

                    //TODO: handle RelationsRefs
                    //foreach (var bcpIterationRef in bcpRevision.IterationRefs)
                    //{
                    //}
                }
            }
        }

        private static Folder GetExistingFolder(string path)
        {
            if (_existingFolders.ContainsKey(path))
                return _existingFolders[path];

            return null;
        }

        private static File GetExistingFile(string fileName, long folderId, DateTime createDate)
        {
            var uniqueFile = new UniqueFile(fileName, folderId, createDate);
            if (_existingFiles.ContainsKey(uniqueFile))
                return _existingFiles[uniqueFile];

            return null;
        }

        private static FileFileRelation GetExistingFileFileRelation(long parentId, long childId)
        {
            var uniqueRelation = new UniqueRelation(parentId, childId);
            if (_existingFileFileRelations.ContainsKey(uniqueRelation))
                return _existingFileFileRelations[uniqueRelation];

            return null;
        }
    }
}