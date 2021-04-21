using IDB.Core.INI;

namespace IDB.Translate.BCP
{
    public class Settings
    {
        private static readonly string _ini;
        static Settings()
        {
            _ini = Core.Settings.GetIniFile();
        }

        public static string CustomFilesOrderByFields
        {
            get
            {
                var ini = new IniHandler(_ini);
                return ini.ReadValue("CustomOrderSettings", "CustomFilesOrderByFields");
            }
        }

        public static string CustomItemsOrderByFields
        {
            get
            {
                var ini = new IniHandler(_ini);
                return ini.ReadValue("CustomOrderSettings", "CustomItemsOrderByFields");
            }
        }
    }
}
