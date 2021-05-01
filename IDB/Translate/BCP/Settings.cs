using IDB.Core.INI;

namespace IDB.Translate.BCP
{
    public class Settings
    {
        private static readonly string IniFile;

        static Settings()
        {
            IniFile = Core.Settings.GetIniFile();
        }

        public static string CustomFilesOrderByFields
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("CustomOrderSettings", "CustomFilesOrderByFields");
            }
        }

        public static string CustomItemsOrderByFields
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("CustomOrderSettings", "CustomItemsOrderByFields");
            }
        }
    }
}
