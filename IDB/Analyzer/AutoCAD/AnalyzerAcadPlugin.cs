
using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.Runtime;

[assembly: ExtensionApplication(typeof(IDB.Analyzer.AutoCAD.AnalyzerAcadPlugin))]

namespace IDB.Analyzer.AutoCAD
{
    public class AnalyzerAcadPlugin : IExtensionApplication
    {
        public void Initialize()
        {
            InitializeLogging();
        }

        public void Terminate()
        {
        }

        private static void InitializeLogging()
        {
            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location + ".log4net");
            log4net.Config.XmlConfigurator.Configure(fi);
        }
    }
}
