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


        // identifies if we are processing single versions of files
        // if this is false, we are going to compare filenames instead of the full Inventor path
        // of the child file
        public static bool UseFullFilePathNameForComparison
        {
            get
            {
                return (GetINIBooleanValue("UseFullFilePathNameForComparison"));
            }
        }

        // identifies if we are to create new relationships
        // if they are found to be missing in the IDB
        // if this is false, a message will be given, but relationships won't be created
        // for the new children found
        public static bool CreateNewFileRelationships
        {
            get
            {
                return (GetINIBooleanValue("CreateNewFileRelationships"));
            }
        }

        // identifies if we are to identify the application version that created the file
        // IE, the "Last Updated With" property from the "Design Tracking Properties" property set in Inventor
        // if yes, we will also make sure the "ApplicationVersion" columns exists in IDB
        // before populating that property
        public static bool ParseLastUpdatedAppVersion
        {
            get
            {
                return (GetINIBooleanValue("ParseLastUpdatedAppVersion"));
            }
        }

    }
}
