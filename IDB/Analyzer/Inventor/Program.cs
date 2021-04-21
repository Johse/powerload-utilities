using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IDB.Analyzer.Common;
using IDB.Analyzer.Common.Db;
using IDB.Analyzer.Common.Helper;
using IDB.Analyzer.Inventor.Helper;
using log4net;

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

            var runMode = GetRunModeFromParameters(args);
            Console.WriteLine("Mode: {0}", runMode);
            Log.InfoFormat("Mode: {0}", runMode);
            if (DataHandler.Instance.GetData(fromDb: (runMode != RunMode.OFFLINE && runMode != RunMode.IMPORT)))
            {
                if (runMode != RunMode.EXPORT && runMode != RunMode.IMPORT)
                    AnalyzeReferences();
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
            var msg = $"coolOrange IDB.Analyze.Inventor Tool v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} for Intermediate DB";
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

        private static void AnalyzeReferences()
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
                    ApprenticeServerWrapper.Instance.CollectReferenceInformation(fileEntry.Key, fileRelationsByName, missingReferences, unknownReferences);
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
                    ApprenticeServerWrapper.Instance.CollectOleReferenceInformation(fileEntry.Key, fileRelationsByName, missingOleReferences, unknownOleReferences);
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

        #endregion
    }
}
