using IDB.Core.INI;

namespace IDB.Load.Files
{
    public class Settings
    {
        private static readonly string IniFile;

        static Settings()
        {
            IniFile = Core.Settings.GetIniFile(".Behaviors");
        }

        public static string FileCategory
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("Files", "Category");
            }
        }

        public static string FileRevisionDefinition
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("Files", "RevisionDefinition");
            }
        }

        public static string FileLifeCycleState
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("Files", "LifeCycleState");
            }
        }

        public static string FileLifeCycleDefinition
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("Files", "LifeCycleDefinition");
            }
        }

        public static string FileRevisionLabel
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("Files", "RevisionLabel");
            }
        }

        public static string FileClassification
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("Files", "Classification");
            }
        }

        public static string FileCreateUser
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("Files", "CreateUser");
            }
        }

        public static string FolderCategory
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("Folders", "Category");
            }
        }

        public static string FolderCreateUser
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("Folders", "CreateUser");
            }
        }
        public static string ExcludeFiles
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("Exclude", "ExcludeFiles");
            }
        }
        public static string ExcludeFolders
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("Exclude", "ExcludeFolders");
            }
        }
    }
}
