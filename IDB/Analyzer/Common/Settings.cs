using IDB.Core.INI;

namespace IDB.Analyzer.Common
{
    public class Settings
    {
        private static readonly string IniFile;

        static Settings()
        {
            IniFile = Core.Settings.GetIniFile();
        }

        public static string InventorProjectFile
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("IDB.Analyzer", "InventorProjectFile");
            }
        }

        public static string FilestorePath
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("IDB.Analyzer", "FilestorePath");
            }
        }

        public static string WorkingDirectory
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("IDB.Analyzer", "WorkingDirectory");
            }
        }

        public static string DifferentLoadLocalFilestorePath
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("IDB.Analyzer", "DifferentLoadLocalFilestorePath");
            }
        }

        protected static bool GetINIBooleanValue(string iniSettingString)
        {
            var ini = new IniHandler(IniFile);

            string sValue = ini.ReadValue("IDB.Analyzer", iniSettingString);

            bool bValue = string.Compare("true", sValue.Trim(), true) == 0 ? true : false;

            return (bValue);
        }


        public static bool UseFullFilePathNameForComparison
        {
            get
            {
                return (GetINIBooleanValue("UseFullFilePathNameForComparison"));
            }
        }

        public static bool CreateNewFileRelationships
        {
            get
            {
                return (GetINIBooleanValue("CreateNewFileRelationships"));
            }
        }

    }
}
