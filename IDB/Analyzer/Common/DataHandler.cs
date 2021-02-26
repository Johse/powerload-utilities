using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Serialization;

using log4net;
using Dapper;
using IDB.Analyzer.Common.Db;
using IDB.Analyzer.Common.Helper;
using Path = System.IO.Path;
using StreamWriter = System.IO.StreamWriter;
using StreamReader = System.IO.StreamReader;

namespace IDB.Analyzer.Common
{
    public class DataHandler
    {
        private static DataHandler _instance;
        public static DataHandler Instance => _instance ?? (_instance = new DataHandler());

        private readonly string FileDataName = "FileData";
        private readonly string FileRelationDataName = "FileRelationData";
        private readonly string NewFileRelationDataName = "NewFileRelationData";

        private ILog Log { get; set; }
        private int ConsoleBufferWidth { get; set; }

        public  Dictionary<string, string> Settings { get; private set; }
        public Dictionary<int, File> FilesById { get; private set; }
        public List<FileFileRelation> AllFileRelations { get; private set; }
        public Dictionary<int, Dictionary<int, FileFileRelation>> FileRelationsById { get; private set; }

        public Dictionary<string, File> FilesByOrigName { get; private set; }
        public List<FileFileRelation> NewFileRelations { get; private set; }

        public void Initialize(ILog logger, string connectionString, string workingDirectory, string filestorePath, string differentLoadLocalFilestorePath)
        {
            Log = logger;
            ConsoleBufferWidth = 300;

            Settings = new Dictionary<string, string>
            {
                {"ConnectionString", connectionString},
                {"WorkingDirectory", workingDirectory},
                {"FilestorePath", filestorePath},
                {"DifferentLoadLocalFilestorePath", differentLoadLocalFilestorePath}
            };
        }

        public bool GetData(bool fromDb)
        {
            return fromDb
                ? ReadDataFromDatabase()
                : ReadDataFromFiles();
        }

        public bool WriteData(bool toDb)
        {
            return toDb
                ? WriteDataToDatabase()
                : WriteDataToFiles();
        }

        private bool ReadDataFromFiles()
        {
            try
            {
                Console.Write("Reading 'Files' data from file ...");
                ReadXml(FileDataName, out IEnumerable<File> fileData);
                FilesById = fileData
                    .Select(x => new KeyValuePair<int, File>(x.FileID, x))
                    .ToDictionary(t => t.Key, t => t.Value);
                Console.WriteLine($"\rReading 'Files' data from file. Done: ({FilesById.Count}) files".PadRight(ConsoleBufferWidth, ' '));

                Console.Write("Reading existing 'FileFileRelations' data from file ...");
                ReadXml(FileRelationDataName, out IEnumerable<FileFileRelation> fileRelationData);
                AllFileRelations = fileRelationData.ToList();
                Console.WriteLine($"\rReading existing 'FileFileRelations' data from file. Done: ({AllFileRelations.Count}) files".PadRight(ConsoleBufferWidth, ' '));

                Console.Write("Reading new 'FileFileRelations' data from file ...");
                ReadXml(NewFileRelationDataName, out IEnumerable<FileFileRelation> newFileRelationData);
                NewFileRelations = newFileRelationData.ToList();
                Console.WriteLine($"\rReading new 'FileFileRelations' data from file. Done: ({NewFileRelations.Count}) files".PadRight(ConsoleBufferWidth, ' '));

                BuildAndInitializeWorkingLists();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error in ReadDataFromFiles()", ex);
                Console.WriteLine("Error in ReadDataFromFiles(): {0}", ex.Message);
                return false;
            }
        }

        private bool ReadDataFromDatabase()
        {
            try
            {
                using (var connection = new SqlConnection(Settings["ConnectionString"]))
                {
                    Console.WriteLine("Open intermediate database");
                    connection.Open();

                    Console.Write("Reading 'Files' table ...");
                    Log.Info("Reading 'Files' table ...");
                    FilesById = connection.Query(@"SELECT * FROM Files ORDER BY FileName, RevisionLabel")
                        .Select(x => new KeyValuePair<int, File>(x.FileID, ConvertDb.To<File>(x)))
                        .ToDictionary(t => t.Key, t => t.Value);
                    Console.WriteLine($"\rReading 'Files' table. Done: ({FilesById.Count}) files".PadRight(ConsoleBufferWidth, ' '));
                    Log.InfoFormat("Reading 'Files' table. Done: ({0}) files", FilesById.Count);

                    Console.Write("Reading 'FileFileRelations' table ...");
                    Log.Info("Reading 'FileFileRelations' table ...");
                    AllFileRelations = connection.Query<FileFileRelation>(@"SELECT * FROM FileFileRelations WHERE IsDependency = '1'").ToList();
                    Console.WriteLine($"\rReading 'FileFileRelations' table. Done: ({AllFileRelations.Count}) file/file relations".PadRight(ConsoleBufferWidth, ' '));
                    Log.InfoFormat("Reading 'FileFileRelations' table. Done: ({0}) file/file relations", AllFileRelations.Count);

                    BuildAndInitializeWorkingLists();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error in ReadingFromDatabase()", ex);
                Console.WriteLine("Error in ReadingFromDatabase(): {0}", ex.Message);
                return false;
            }
        }
        private bool WriteDataToFiles()
        {
            try
            {
                Console.Write("Writing 'Files' data to file ...");
                WriteXml(FileDataName, FilesById.Values.ToList());
                Console.WriteLine($"\rWriting 'Files' data to file. Done: ({FilesById.Count}) files".PadRight(ConsoleBufferWidth, ' '));

                Console.Write("Writing existing 'FileFileRelations' data to file ...");
                WriteXml(FileRelationDataName, AllFileRelations);
                Console.WriteLine($"\rWriting existing 'FileFileRelations' data to file. Done: ({AllFileRelations.Count}) files".PadRight(ConsoleBufferWidth, ' '));

                Console.Write("Writing new 'FileFileRelations' data to file ...");
                WriteXml(NewFileRelationDataName, NewFileRelations);
                Console.WriteLine($"\rWriting new 'FileFileRelations' data to file. Done: ({NewFileRelations.Count}) files".PadRight(ConsoleBufferWidth, ' '));

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error in WriteDataToFiles()", ex);
                Console.WriteLine("Error in WriteDataToFiles(): {0}", ex.Message);
                return false;
            }
        }

        private bool WriteDataToDatabase()
        {
            try
            {
                using (var connection = new SqlConnection(Settings["ConnectionString"]))
                {
                    Console.WriteLine("Open intermediate database");
                    connection.Open();

                    Console.Write("Updating existing rows in 'FileFileRelations' table ...");
                    Log.Info("Updating existing rows in 'FileFileRelations' table ...");
                    var updateQuery = "UPDATE FileFileRelations " +
                        "SET IsAttachment = @IsAttachment, IsDependency = @IsDependency, NeedsResolution = @NeedsResolution, Source = @Source, RefId = @RefId " +
                        "WHERE ParentFileID = @ParentFileID AND ChildFileID = @ChildFileID";
                    var result = connection.Execute(updateQuery, AllFileRelations);
                    Console.WriteLine($"\rUpdating existing rows in 'FileFileRelations' table. Done! Result: {result})".PadRight(ConsoleBufferWidth, ' '));
                    Log.InfoFormat("Updating existing rows in 'FileFileRelations' table. Done! Result: {0})", result);

                    Console.Write("Inserting new rows in 'FileFileRelations' table ...");
                    Log.Info("Inserting new rows in 'FileFileRelations' table ...");
                    var insertQuery = "INSERT INTO FileFileRelations " +
                        "(ParentFileID, ChildFileID, IsAttachment, IsDependency, NeedsResolution, Source, RefId) " +
                        "VALUES (@ParentFileID, @ChildFileID, @IsAttachment, @IsDependency, @NeedsResolution, @Source, @RefId)";
                    result = connection.Execute(insertQuery, NewFileRelations);
                    Console.WriteLine($"\rInserting new rows in 'FileFileRelations' table. Done! Result: {result})".PadRight(ConsoleBufferWidth, ' '));
                    Log.InfoFormat("Inserting new rows in 'FileFileRelations' table. Done! Result: {0})", result);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error in WritingToDatabase()", ex);
                Console.WriteLine("Error in WritingToDatabase(): {0}", ex.Message);
                return false;
            }
        }

        private void BuildAndInitializeWorkingLists()
        {
            Log.Info("Building 'Files' dictionary by original filename ...");
            FilesByOrigName = new Dictionary<string, File>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in FilesById.Values)
            {
                var origFilename = file.LocalFullFileName.GetReplacedFilename();
                if (!FilesByOrigName.ContainsKey(origFilename))
                    FilesByOrigName.Add(origFilename, file);
                else
                    Log.WarnFormat("BuildAndInitializeWorkingLists: Ignoring duplicate original filename: '{0}'", origFilename);
            }

            Log.Info("Building 'Files' dictionary by original filename. Done!");

            if (NewFileRelations == null)
                NewFileRelations = new List<FileFileRelation>();

            Log.Info("Building 'FileFileRelations' dictionary ...");
            FileRelationsById = new Dictionary<int, Dictionary<int, FileFileRelation>>();
            foreach (var relation in AllFileRelations)
            {
                if (!FileRelationsById.TryGetValue(relation.ParentFileID, out Dictionary<int, FileFileRelation> fileReferences))
                {
                    fileReferences = new Dictionary<int, FileFileRelation>();
                    FileRelationsById.Add(relation.ParentFileID, fileReferences);
                }
                fileReferences.Add(relation.ChildFileID, relation);
            }
            Log.Info("Building 'FileFileRelations' dictionary. Done!");
        }

        private void ReadXml<T>(string dataElementName, out IEnumerable<T> dataList)
        {
            var dataFilename = Path.Combine(Settings["WorkingDirectory"], dataElementName + ".xml");
            Log.InfoFormat("Reading '{0}' data from file '{1}'...", typeof(T).Name, dataFilename);
            dataList = null;
            try
            {
                using (var reader = new StreamReader(dataFilename))
                {
                    var deserializer = new XmlSerializer(typeof(List<T>), new XmlRootAttribute(dataElementName));
                    dataList = (List<T>)deserializer.Deserialize(reader);
                }
                Log.Info("Successfully read data from file!!");
            }
            catch (Exception ex)
            {
                throw new ApplicationException ($"Failed to read data from file '{dataFilename}': {ex.Message}");
            }
        }

        private void WriteXml<T>(string dataElementName, IEnumerable<T> listData)
        {
            var dataFilename = Path.Combine(Settings["WorkingDirectory"], dataElementName + ".xml");
            Log.InfoFormat("Writing '{0}' data to file '{1}' ...", typeof(T).Name, dataFilename);
            try
            {
                using (var writer = new StreamWriter(dataFilename))
                {
                    var serializer = new XmlSerializer(typeof(List<T>), new XmlRootAttribute(dataElementName));
                    serializer.Serialize(writer, listData);
                }
                Log.Info("Successfully saved data to file!");
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to write data to file '{dataFilename}': {ex.Message}");
            }
        }
    }
}