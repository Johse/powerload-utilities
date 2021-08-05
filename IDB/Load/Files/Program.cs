using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using IDB.Core.Data.Unique;
using log4net;
using File = IDB.Core.Data.Entity.File;
using Folder = IDB.Core.Data.Entity.Folder;

namespace IDB.Load.Files
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger("IDBLoadFiles");

        private static string _connectionString;
        private static long _totalFolders;
        private static long _totalIterations;
        private static long _processedFolders;
        private static long _processedIterations;
        private static string _rootDirectory;
        private const int MAX_PATH = 260;

        private static Dictionary<string, Folder> _existingFolders;
        private static Dictionary<UniqueFile, File> _existingFiles;

        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                InitializeLogging();

                string directory;

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
                    directory = args[1];
                }
                else
                {
                    _connectionString = Core.Settings.IdbConnectionString;
                    directory = Core.Settings.ImportPath;
                }

                Log.Info($"Connection String: {_connectionString}");
                Log.Info($"Import Directory: {directory}");


                if (!IsValidConnectionString(_connectionString))
                {
                    Log.Error($"Cannot connect to SQL Server '{_connectionString}'!");
                    Console.WriteLine("Press any key to close this window");
                    Console.ReadLine();
                    return;
                }

                if (!Directory.Exists(directory))
                {
                    Log.Error($"The directory '{directory}' does not exist!");
                    Console.WriteLine("Press any key to close this window");
                    Console.ReadLine();
                    return;
                }

                _rootDirectory = directory;

                var folderTask = ReadIdbFolders();
                var fileTask = ReadIdbFiles();
                var parseTask = AnalyseDirectoryStructure(directory);

                await Task.WhenAll(folderTask, fileTask, parseTask);

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    _processedFolders++;
                    var rootFolder = GetExistingFolder("$");
                    if (rootFolder == null)
                    {
                        rootFolder = new Folder();
                        rootFolder.Insert(connection);
                        Log.Info("Insert root: " + rootFolder.Path);
                    }
                    else
                        Log.Warn("Existing root: " + rootFolder.Path);

                    foreach (var file in GetFiles(directory))
                        ProcessFile(connection, rootFolder, file);

                    foreach (var subDirectory in GetDirectories(directory))
                        ProcessFolder(connection, rootFolder, subDirectory);
                }

                Log.Info("Import finished!");
                Console.WriteLine("Press any key to close this window");
                Console.ReadLine();
            }).GetAwaiter().GetResult();
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

        private static async Task AnalyseDirectoryStructure(string directory)
        {
            Log.Info($"Parsing {directory}...");
            await Task.Run(() =>
            {
                CountDirectoriesAndFiles(directory);
                Log.Info($"Processing {_totalIterations} file iterations in {_totalFolders + 1} folders...");
            });
        }

        private static void CountDirectoriesAndFiles(string directory)
        {
            _totalFolders++;
            _totalIterations += GetFiles(directory).Count();

            foreach (var f in GetDirectories(directory))
                CountDirectoriesAndFiles(f);
        }

        private static void ProcessFile(SqlConnection connection, Folder parentFolder, string fullFileName)
        {
            _processedIterations++;
            if (fullFileName.Length > MAX_PATH)
			{
                Log.Warn($"Skipping File. Path is longer than {MAX_PATH} characters. {fullFileName}");
                return;
            }

            var fileName = Path.GetFileName(fullFileName);
            var createdDate = System.IO.File.GetCreationTimeUtc(fullFileName);

            var file = GetExistingFile(fileName, parentFolder.FolderID, createdDate);
            if (file == null)
            {
                file = new File
                {
                    FolderID = parentFolder.FolderID,
                    FileName = fileName,
                    CreateDate = createdDate,
                    LocalFullFileName = fullFileName,
					LocalFileChecksum = Core.File.Checksum.CalcChecksum(fullFileName),
					Category = Settings.FileCategory,
                    Classification = Settings.FileClassification,
                    Comment = "",
                    ContentSource = GetContentSource(fullFileName),
                    CreateUser = Settings.FileCreateUser,
                    IsHidden = false,
                    LifecycleDefinition = Settings.FileLifeCycleDefinition,
                    LifecycleState = Settings.FileLifeCycleState,
                    RevisionDefinition = Settings.FileRevisionDefinition,
                    RevisionLabel = Settings.FileRevisionLabel,
                    Version = 1
                };
                file.Insert(connection);
                _existingFiles.Add(new UniqueFile(file), file);
                Log.Info($"Insert file: ({_processedIterations} of {_totalIterations}): {file.FileName}");
            }
            else
                Log.Warn($"Existing file: ({_processedIterations} of {_totalIterations}): {file.FileName}");
        }

        private static void ProcessFolder(SqlConnection connection, Folder parentFolder, string directory)
        {
            var path = GetVaultFolderFromLocalFolder(directory);
            var folder = GetExistingFolder(path);
            if (folder == null)
            {
                folder = new Folder
                {
                    ParentFolderID = parentFolder.FolderID,
                    CreateDate = Directory.GetCreationTime(directory),
                    Category = Settings.FolderCategory,
                    CreateUser = Settings.FolderCreateUser,
                    FolderName = new DirectoryInfo(directory).Name,
                    IsLibrary = false,
                    LifecycleDefinition = null,
                    LifecycleState = null,
                    Path = path
                };

                folder.Insert(connection);
                _existingFolders.Add(folder.Path, folder);
                Log.Info($"Insert folder ({_processedFolders} of {_totalFolders}): {folder.Path}");
            }
            else
                Log.Warn($"Existing folder ({_processedFolders} of {_totalFolders}): {folder.Path}");

            foreach (var file in GetFiles(directory))
                ProcessFile(connection, folder, file);

            foreach (var subFolder in GetDirectories(directory))
                ProcessFolder(connection, folder, subFolder);
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

        private static string GetVaultFolderFromLocalFolder(string directory)
        {
            if (directory.StartsWith(_rootDirectory))
            {
                var vaultDirectory = directory.Substring(_rootDirectory.Length).Replace(@"\", "/");
                return string.Join("/", new [] {"$", vaultDirectory.TrimStart('/') }.Where(s => !string.IsNullOrEmpty(s)));
            }

            return null;
        }

        private static IEnumerable<string> GetFiles(string directory)
        {
            return Directory.EnumerateFiles(directory).Except(GetFiles(directory, Settings.ExcludeFiles));
        }

        private static IEnumerable<string> GetDirectories(string directory)
        {
            return Directory.EnumerateDirectories(directory).Except(GetDirectories(directory,Settings.ExcludeFolders));
        }

        private static IEnumerable<string> GetFiles(string sourceFolder, string filters)
        {
            return filters.Split('|').SelectMany(filter => System.IO.Directory.EnumerateFiles(sourceFolder, filter));
        }

        private static IEnumerable<string> GetDirectories(string sourceFolder, string filters)
        {
            return filters.Split('|').SelectMany(filter => System.IO.Directory.EnumerateDirectories(sourceFolder, filter));
        }

        private static string GetContentSource(string fullFileName)
        {
            /*
            Provide SystemName	SupportedSources
            ==================  ================
            AutoCAD	            .dwg,.dwt
            AutoCADElectrical	.wdp
            InventorDWG	        .dwg
            Inventor	        .ipt,.iam,.idw,.ipn,.ide
            RevitRVT	        .rvt,.rte
            RevitRFA	        .rfa
            Office	            .doc,.dot,.xls,.xlt,.ppt,.pot
            OfficeOpenXML	    .docx,.docm,.dotx,.dotm,.xltx,.xltm,.xlsx,.xlsm,.xlsb,.potx,.pptx,.pptm,.potm
            IFilter	            .*
            AutoCADC3D	        .c3ddata
            Sheet Set Manager	.dss,.dsu,.dsh
            Outlook	            .msg
            FactoryAsset	    .faf
            */

            var extension = Path.GetExtension(fullFileName).ToLower();

            if (extension.Equals(".dwg"))
            {
                try
                {
                    if (Analyzer.Inventor.Helper.ApprenticeServerWrapper.Instance.IsInventorFile(fullFileName))
                        return "Inventor";
                    return "AutoCAD";
                }
                catch (Exception ex)
                {
                    Log.Error("Inventor Apprentice is not installed. The field 'ContentSource' cannot be set for file.");
                    Log.Error(ex.Message);
                    return null;
                }
            }

            if (new[] { ".dwt" }.Contains(extension))
                return "AutoCAD";

            if (new[] { ".wdp" }.Contains(extension))
                return "AutoCADElectrical";

            if (new[] { ".ipt", ".iam", ".idw", ".ipn", ".ide" }.Contains(extension))
                return "Inventor";

            if (new[] { ".rvt", ".rte" }.Contains(extension))
                return "RevitRVT";

            if (new[] { ".rfa" }.Contains(extension))
                return "RevitRFA";

            if (new[] { ".doc", ".dot", ".xls", ".xlt", ".ppt", ".pot" }.Contains(extension))
                return "Office";

            if (new[] { ".docx", ".docm", ".dotx", ".dotm", ".xltx", ".xltm", ".xlsx", ".xlsm", ".xlsb", ".potx", ".pptx", ".pptm", ".potm" }.Contains(extension))
                return "OfficeOpenXML";

            if (new[] { ".pdf" }.Contains(extension))
                return "IFilter";

            if (new[] { ".c3ddata" }.Contains(extension))
                return "AutoCADC3D";

            if (new[] { ".dss", ".dsu", ".dsh" }.Contains(extension))
                return "Sheet Set Manager";

            if (new[] { ".msg" }.Contains(extension))
                return "Outlook";

            if (new[] { ".faf" }.Contains(extension))
                return "FactoryAsset";

            return null;
        }
    }
}
