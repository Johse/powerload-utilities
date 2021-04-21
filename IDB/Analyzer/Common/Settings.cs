using IDB.Core;

namespace IDB.Analyzer.Common
{
    public class Settings
    {
        private static readonly string _ini;
        static Settings()
        {
            _ini = Core.Settings.GetIniFile();
        }

        public static string InventorProjectFile
        {
            get
            {
                var ini = new IniHandler(_ini);
                return ini.ReadValue("IDB.Analyzer", "InventorProjectFile");
            }
        }

        public static string FilestorePath
        {
            get
            {
                var ini = new IniHandler(_ini);
                return ini.ReadValue("IDB.Analyzer", "FilestorePath");
            }
        }

        public static string WorkingDirectory
        {
            get
            {
                var ini = new IniHandler(_ini);
                return ini.ReadValue("IDB.Analyzer", "WorkingDirectory");
            }
        }

        public static string DifferentLoadLocalFilestorePath
        {
            get
            {
                var ini = new IniHandler(_ini);
                return ini.ReadValue("IDB.Analyzer", "DifferentLoadLocalFilestorePath");
            }
        }
    }
}
