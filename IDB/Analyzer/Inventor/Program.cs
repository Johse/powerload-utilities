using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IDB.Analyzer.Common;
using IDB.Analyzer.Common.Db;
using IDB.Analyzer.Common.Helper;
using IDB.Analyzer.Inventor.Helper;
using log4net;
using System.IO;
using System.Xml.Serialization;


using FileInfo = System.IO.FileInfo;

namespace IDB.Analyzer.Inventor
{
    class Program
    {
        enum RunMode { NORMAL, EXPORT, IMPORT, OFFLINE }
        private static readonly ILog Log = LogManager.GetLogger("IDBAnalyzeInventor");

        private static bool UserCancelRequest = false;

        static void Main(string[] args)
        {
            // Initialize console window and log4net logging
            InitializeConsoleAndLogging();

            // Initialize common DataHandler
            DataHandler.Instance.Initialize(Log);

            // Initialize Inventor helper class
            ApprenticeServerWrapper.Instance.SetProjectFile(Common.Settings.InventorProjectFile);
            ApprenticeServerWrapper.Instance.SetCreateNewFileRelationships(Common.Settings.CreateNewFileRelationships);
            ApprenticeServerWrapper.Instance.SetParseLastUpdatedAppVersion(Common.Settings.ParseLastUpdatedAppVersion);


            // identify if we should use full path or filenames for building the relationships
            bool bUseFullFilePathNameForComparison = Common.Settings.UseFullFilePathNameForComparison;

            var runMode = GetRunModeFromParameters(args);
            Console.WriteLine("Mode: {0}", runMode);
            Log.InfoFormat("Mode: {0}", runMode);
            if (DataHandler.Instance.GetData(fromDb: (runMode != RunMode.OFFLINE && runMode != RunMode.IMPORT)))
            {
                // initialize the IDBAnalyzed flag for the content
                DataHandler.Instance.InitializeIDBAnalyzed();

                // process the content
                if (runMode != RunMode.EXPORT && runMode != RunMode.IMPORT)
                {
                    // create the ProcessingStatistics object to manage results
                    ProcessingStatistics procStats = new ProcessingStatistics();

                    // choose the type of processing, comparing full file path, or just the filename
                    if (bUseFullFilePathNameForComparison)
                    {
                        AnalyzeReferencesUsingFullFilename(ref procStats);
                    }
                    else
                    {
                        AnalyzeReferencesUsingFilename(ref procStats);
                    }
                }

                // write the data to the IDB
                DataHandler.Instance.WriteData(toDb: (runMode != RunMode.OFFLINE && runMode != RunMode.EXPORT));
            }

            Console.Write("Press <Enter> to exit... ");
            while (Console.ReadKey().Key != ConsoleKey.Enter) { }

        }

        #region Private Functions
        private static RunMode GetRunModeFromParameters(string[] args)
        {
            if (!args.Any()) return RunMode.NORMAL;
            if (args[0] == "EXPORT") return RunMode.EXPORT;
            if (args[0] == "IMPORT") return RunMode.IMPORT;
            if (args[0] == "OFFLINE") return RunMode.OFFLINE;
            return RunMode.NORMAL;
        }
        private static void InitializeConsoleAndLogging()
        {
            Console.BufferWidth = 300;
            Console.WindowWidth = 140;
            var msg = $"COOLORANGE IDB.Analyze.Inventor Tool v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} for Intermediate DB";
            Console.WriteLine(msg);
            Console.WriteLine("".PadRight(msg.Length, '*'));

            Console.CancelKeyPress += delegate
            {
                UserCancelRequest = true;
            };

            var thisAssembly = Assembly.GetExecutingAssembly();
            var fi = new FileInfo(thisAssembly.Location + ".log4net");
            log4net.Config.XmlConfigurator.Configure(fi);
            Log.Info(msg);
        }

        private static void AnalyzeReferencesUsingFullFilename(ref ProcessingStatistics procStats)
        {
            Console.WriteLine("Analyzing Inventor references ...");
            Log.Info("Analyzing Inventor references ...");

            if (!string.IsNullOrEmpty(Common.Settings.DifferentLoadLocalFilestorePath) && string.IsNullOrEmpty(Common.Settings.FilestorePath))
            {
                Log.Error("Setting 'FilestorePath' is required if 'DifferentLoadLocalFilestorePath' is set");
                Console.WriteLine("Setting 'FilestorePath' is required if 'DifferentLoadLocalFilestorePath' is set!");
                return;
            }

            int counter = 1;
            var totalCount = DataHandler.Instance.FilesById.Count;

            foreach (var fileEntry in DataHandler.Instance.FilesById)
            {
                var lc = counter++;
                var filename = fileEntry.Value.LocalFullFileName.GetReplacedFilename();
                var msg = $"\r{lc}/{totalCount}: Analyzing references for {filename}";
                Console.Write(msg.PadRight(Console.BufferWidth, ' '));

                if (!ApprenticeServerWrapper.Instance.IsInventorFile(filename))
                    continue;

                if (!System.IO.File.Exists(filename))
                {
                    Log.InfoFormat("File ({0}) '{1}' doesn't exist!", lc, filename);
                    continue;
                }

                if (UserCancelRequest)
                {
                    Log.Info("Cancelled by user!");
                    break;
                }

                try
                {
                    Log.InfoFormat("Analyzing references for file '{0}': {1}/{2}", filename, lc, totalCount);

                    var fileRelationsByName = new Dictionary<string, FileFileRelation>(StringComparer.OrdinalIgnoreCase);

                    if (DataHandler.Instance.FileRelationsById.TryGetValue(fileEntry.Key, out var fileRelations))
                    {
                        foreach (var fileRelation in fileRelations.Values)
                        {
                            if (DataHandler.Instance.FilesById.TryGetValue(fileRelation.ChildFileID, out var file))
                                fileRelationsByName.Add(file.LocalFullFileName.GetReplacedFilename(), fileRelation);
                        }
                    }

                    if (!ApprenticeServerWrapper.Instance.OpenDocument(filename))
                        continue;

                    var missingReferences = new List<string>();
                    var unknownReferences = new List<string>();
                    ApprenticeServerWrapper.Instance.CollectReferenceInformationByFullFilePath(fileEntry.Key, fileRelationsByName, missingReferences, unknownReferences);
                    if (missingReferences.Any())
                    {
                        Log.ErrorFormat("File '{0}' has missing references!", filename);
                        foreach (var missingReference in missingReferences)
                            Log.ErrorFormat("Missing reference: {0}", missingReference);
                    }
                    if (unknownReferences.Any())
                    {
                        Log.ErrorFormat("File '{0}' has unknown references!", filename);
                        foreach (var unknownReference in unknownReferences)
                            Log.ErrorFormat("Unknown reference: {0}", unknownReference);
                    }

                    var missingOleReferences = new List<string>();
                    var unknownOleReferences = new List<string>();
                    ApprenticeServerWrapper.Instance.CollectOleReferenceInformationByFullFilePath(fileEntry.Key, fileRelationsByName, missingOleReferences, unknownOleReferences);
                    if (missingOleReferences.Any())
                    {
                        Log.ErrorFormat("File '{0}' has missing OLE references!", filename);
                        foreach (var missingReference in missingOleReferences)
                            Log.ErrorFormat("Missing OLE reference: {0}", missingReference);
                    }
                    if (unknownOleReferences.Any())
                    {
                        Log.ErrorFormat("File '{0}' has unknown OLE references!", filename);
                        foreach (var unknownOleReference in unknownOleReferences)
                            Log.ErrorFormat("Unknown OLE reference: {0}", unknownOleReference);
                    }
                }
                catch (Exception ex)
                {
                    msg = $"\nError processing references for Inventor file '{filename}'";
                    Console.WriteLine(msg);
                    Log.Error($"Error processing references for Inventor file '{filename}'", ex);
                }
                finally
                {
                    ApprenticeServerWrapper.Instance.CloseDocument();
                }
            }
            Console.WriteLine("\nAnalyzing Inventor references. Done!");
            Log.Info("Analyzing Inventor references. Done!");
        }

        private static void AnalyzeReferencesUsingFilename(ref ProcessingStatistics processStatistics)
        {
            Console.WriteLine("Analyzing Inventor references ...");
            Log.Info("Analyzing Inventor references ...");

            if (!string.IsNullOrEmpty(Common.Settings.DifferentLoadLocalFilestorePath) && string.IsNullOrEmpty(Common.Settings.FilestorePath))
            {
                Log.Error("Setting 'FilestorePath' is required if 'DifferentLoadLocalFilestorePath' is set");
                Console.WriteLine("Setting 'FilestorePath' is required if 'DifferentLoadLocalFilestorePath' is set!");
                return;
            }

            // set the number to process
            processStatistics.NumberToProcess = DataHandler.Instance.FilesById.Count;

            foreach (var fileEntry in DataHandler.Instance.FilesById)
            {
                // increment the counter
                processStatistics.TotalProcessed++;

                var filename = fileEntry.Value.LocalFullFileName.GetReplacedFilename();
                var msg = $"\r{processStatistics.TotalProcessed}/{processStatistics.NumberToProcess}: Analyzing references for {filename}";
                Console.Write(msg.PadRight(Console.BufferWidth, ' '));

                if (!System.IO.File.Exists(filename))
                {
                    processStatistics.MissingLocalFiles++;
                    Log.InfoFormat("File ({0}) '{1}' doesn't exist!", processStatistics.TotalProcessed, filename);

                    // set the record for the database
                    fileEntry.Value.IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(fileEntry.Value.IDBAnalyzeNotes, "Local file missing");

                    continue;
                }

                if (!ApprenticeServerWrapper.Instance.IsInventorFile(filename))
                {
                    processStatistics.NonInventorFiles++;

                    // set the record for the database
                    fileEntry.Value.IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(fileEntry.Value.IDBAnalyzeNotes, "Non Inventor file");

                    continue;
                }


                if (UserCancelRequest)
                {
                    processStatistics.UserCanceledProcess = true;
                    Log.Info("Cancelled by user!");
                    break;
                }

                try
                {
                    // increment the counter
                    processStatistics.TotalInventor++;

                    Log.InfoFormat("Analyzing references for file '{0}': {1}/{2}", filename, processStatistics.TotalProcessed, processStatistics.NumberToProcess);

                    var fileRelationsByName = new Dictionary<string, FileFileRelation>(StringComparer.OrdinalIgnoreCase);

                    if (DataHandler.Instance.FileRelationsById.TryGetValue(fileEntry.Key, out var fileRelations))
                    {
                        foreach (var fileRelation in fileRelations.Values)
                        {
                            // NOTE: we are using just the filename for comparison
                            if (DataHandler.Instance.FilesById.TryGetValue(fileRelation.ChildFileID, out var file))
                                fileRelationsByName.Add(file.FileName, fileRelation);
                        }
                    }

                    if (!ApprenticeServerWrapper.Instance.OpenDocument(filename))
                    {
                        processStatistics.InventorFailedToOpen++;

                        // set the record for the database
                        fileEntry.Value.IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(fileEntry.Value.IDBAnalyzeNotes, "Inventor failed to open");

                        continue;
                    }

                    // get the Application Version for the document
                    ApprenticeServerWrapper.Instance.QueryAndRecordLastUpdatedAppVersion(fileEntry.Value);

                    var addedReferences = new List<string>();
                    var unknownReferences = new List<string>();
                    ApprenticeServerWrapper.Instance.CollectReferenceInformationByFilename(fileEntry.Value, fileRelationsByName, addedReferences, unknownReferences, ref processStatistics);

                    if (addedReferences.Any())
                    {
                        processStatistics.NumberWithAddedReferences++;

                        Log.ErrorFormat("File '{0}' has references that need to be added!", filename);
                        foreach (var addedReference in addedReferences)
                            Log.ErrorFormat("Added file reference: {0}", addedReference);
                    }

                    if (unknownReferences.Any())
                    {
                        processStatistics.NumberWithMissingReferences++;

                        Log.ErrorFormat("File '{0}' has unknown references!", filename);
                        foreach (var unknownReference in unknownReferences)
                            Log.ErrorFormat("Unknown reference: {0}", unknownReference);
                    }

                    var oleIndexIssuesReferences = new List<string>();
                    var addedOleReferences = new List<string>();
                    var unknownOleReferences = new List<string>();
                    ApprenticeServerWrapper.Instance.CollectOleReferenceInformationByFilename(fileEntry.Value, fileRelationsByName, oleIndexIssuesReferences, addedOleReferences, unknownOleReferences, ref processStatistics);
                    if (oleIndexIssuesReferences.Any())
                    {
                        processStatistics.NumberWithOLEIndexIssueReferences++;

                        Log.ErrorFormat("File '{0}' has OLE references Index issues!", filename);
                        foreach (var oleIndexReference in oleIndexIssuesReferences)
                            Log.ErrorFormat("OLE references Index issues: {0}", oleIndexReference);
                    }

                    if (addedOleReferences.Any())
                    {
                        processStatistics.NumberWithAddedOLEReferences++;

                        Log.ErrorFormat("File '{0}' has OLE references that need to be added!", filename);
                        foreach (var addedReference in addedOleReferences)
                            Log.ErrorFormat("OLE reference added: {0}", addedReference);
                    }


                    if (unknownOleReferences.Any())
                    {
                        processStatistics.NumberWithMissingOLEReferences++;

                        Log.ErrorFormat("File '{0}' has unknown OLE references!", filename);
                        foreach (var unknownOleReference in unknownOleReferences)
                            Log.ErrorFormat("Unknown OLE reference: {0}", unknownOleReference);
                    }

                    // parse through and identify issues that may exist with records that did not get processed

                    // get the relationship records that have not been processed
                    // this may indicate that thes records should not be in the IDB FileFileRelations table
                    List<FileFileRelation> dependencyRecords = fileRelationsByName.Values.Where(ffr => (!ffr.IsAttachment && ffr.IsDependency && !ffr.IDBAnalyzed)).ToList();
                    dependencyRecords.ForEach(ffr => ffr.IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(fileEntry.Value.IDBAnalyzeNotes, "Dependency Not Found During Analyze"));
                    if (dependencyRecords.Any())
                    {
                        fileEntry.Value.HasRelationshipIssues = true;
                        fileEntry.Value.IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(fileEntry.Value.IDBAnalyzeNotes, string.Format("{0} Dependencies Not Found During Analyze", dependencyRecords.Count()));
                    }

                    List<FileFileRelation> oleRecords = fileRelationsByName.Values.Where(ffr => (!ffr.IsAttachment && !ffr.IsDependency && !ffr.IDBAnalyzed)).ToList();
                    oleRecords.ForEach(ffr => ffr.IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(fileEntry.Value.IDBAnalyzeNotes, "OLE Dependency Not Found During Analyze"));
                    if (oleRecords.Any())
                    {
                        fileEntry.Value.HasRelationshipIssues = true;
                        fileEntry.Value.IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(fileEntry.Value.IDBAnalyzeNotes, string.Format("{0} OLE Dependencies Not Found During Analyze", oleRecords.Count()));
                    }

                    // make a catch all for any issues that may not have been fully
                    // documented, but were caught by the process
                    // user may need to compound query in MSSQMS the Files and FileFileRelations table to identify what the issues were
                    // as they may be recorded in the FileFileRelation records
                    if (fileEntry.Value.HasRelationshipIssues)
                    {
                        fileEntry.Value.IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(fileEntry.Value.IDBAnalyzeNotes, "Has relationship issues");
                    }

                }
                catch (Exception ex)
                {
                    processStatistics.ExceptionsProcessed++;

                    // set the record for the database
                    fileEntry.Value.IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(fileEntry.Value.IDBAnalyzeNotes, "Exception in process");
                    fileEntry.Value.IDBAnalyzeNotes = ApprenticeServerWrapper.AddOrNewComment(fileEntry.Value.IDBAnalyzeNotes, ex.Message);

                    msg = $"\nError processing references for Inventor file '{filename}'";
                    Console.WriteLine(msg);

                    Log.Error($"Error processing references for Inventor file '{filename}'", ex);
                }
                finally
                {
                    ApprenticeServerWrapper.Instance.CloseDocument();
                }
            }

            // have the ProcessStatistics report on issues
            Console.WriteLine(Environment.NewLine);
            processStatistics.LogAndReportStatistics();
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("\nAnalyzing Inventor references. Done!");
            Log.Info("Analyzing Inventor references. Done!");
        }


        #endregion

    }


}
