using System.Configuration;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using IDB.Analyzer.Common;
using log4net;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

[assembly: CommandClass(typeof(IDB.Analyzer.AutoCAD.AnalyzerAcadCommands))]

namespace IDB.Analyzer.AutoCAD
{
    class AnalyzerAcadCommands
    {
        enum RunMode { NORMAL, EXPORT, IMPORT, OFFLINE }
        private static readonly ILog Log = LogManager.GetLogger("IDBAnalyzeAutoCAD");

        #region Command Methods
        [CommandMethod("IDBAnalyzeDocs")]
        public void IDBAnalyzeDocs()
        {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;

            var msg = $"coolOrange IDB.Analyze.AutoCAD Tool v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version} for Intermediate DB";
            Log.Info(msg);
            ed.WriteMessage("\n" + msg);

            var config = ConfigurationManager.OpenExeConfiguration(GetType().Assembly.Location);
            var xmlConfig = new ConfigXmlDocument();
            xmlConfig.Load(config.FilePath);

            var filestorePath = xmlConfig.SelectSingleNode("//setting[@name='FilestorePath']").InnerText;
            var connectionString = xmlConfig.SelectSingleNode("//setting[@name='ConnectionString']").InnerText;
            var workingDirectory = xmlConfig.SelectSingleNode("//setting[@name='WorkingDirectory']").InnerText;
            var differentLoadLocalFilestorePath = xmlConfig.SelectSingleNode("//setting[@name='DifferentLoadLocalFilestorePath']").InnerText;

            // Initialize common DataHandler
            DataHandler.Instance.Initialize(Log, connectionString, workingDirectory, filestorePath, differentLoadLocalFilestorePath);

            var result = ed.GetString("\nIDB.Analyze mode (NORMAL, EXPORT, IMPORT, OFFLINE) [NORMAL]: ");
            if (result.Status != PromptStatus.OK)
            {
                Log.Info("Command cancelled by user!");
                return;
            }

            var runMode = GetRunModeFromParameters(result.StringResult);
            ed.WriteMessage($"\nSelected mode: {runMode}");
            Log.InfoFormat("Selected mode: {0}", runMode);

            ed.WriteMessage((runMode != RunMode.OFFLINE && runMode != RunMode.IMPORT) ? "\nGetting data from IDB ..." : "\nGetting data from xml ...");
            if (!DataHandler.Instance.GetData(fromDb: (runMode != RunMode.OFFLINE && runMode != RunMode.IMPORT)))
            {
                ed.WriteMessage("\nFailed to get data. Please check log-file for more details!");
                return;
            }

            if (runMode != RunMode.EXPORT && runMode != RunMode.IMPORT)
            {
                var xrefAnalyzer = new XrefAnalyzer();
                xrefAnalyzer.AnalyzeReferences();
            }
            DataHandler.Instance.WriteData(toDb: (runMode != RunMode.OFFLINE && runMode != RunMode.EXPORT));
        }
        #endregion

        #region Private Functions
        private static RunMode GetRunModeFromParameters(string arg)
        {
            if (arg == "EXPORT") return RunMode.EXPORT;
            if (arg == "IMPORT") return RunMode.IMPORT;
            if (arg == "OFFLINE") return RunMode.OFFLINE;
            return RunMode.NORMAL;
        }

        #endregion
    }
}
