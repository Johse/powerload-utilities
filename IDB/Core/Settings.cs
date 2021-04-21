using System.IO;
using System.Reflection;

namespace IDB.Core
{
    public class Settings
    {
        public static string GetIniFile(string additionalName = "")
        {
            var assembly = Assembly.GetCallingAssembly();
            var directory = Path.GetDirectoryName(assembly.Location);
            var fileName = Path.GetFileNameWithoutExtension(assembly.Location);
            var path = Path.Combine(directory, fileName) + additionalName + ".ini";
            return path;
        }

        private static readonly string _ini;
        static Settings()
        {
            _ini = GetIniFile();
        }

        public static string IdbConnectionString
        {
            get
            {
                var ini = new IniHandler(_ini);
                return ini.ReadValue("IDB", "ConnectionString");
            }
            set
            {
                var ini = new IniHandler(_ini);
                ini.WriteValue("IDB", "ConnectionString", value);
            }
        }

        public static string ImportPath
        {
            get
            {
                var ini = new IniHandler(_ini);
                return ini.ReadValue("IDB", "ImportPath");
            }
            set
            {
                var ini = new IniHandler(_ini);
                ini.WriteValue("IDB", "ImportPath", value);
            }
        }

        public static string ExportPath
        {
            get
            {
                var ini = new IniHandler(_ini);
                return ini.ReadValue("IDB", "ExportPath");
            }
            set
            {
                var ini = new IniHandler(_ini);
                ini.WriteValue("IDB", "ExportPath", value);
            }
        }

        public static string VaultConnectionString
        {
            get
            {
                var ini = new IniHandler(GetIniFile());
                return ini.ReadValue("Vault", "ConnectionString");
            }
            set
            {
                var ini = new IniHandler(GetIniFile());
                ini.WriteValue("Vault", "ConnectionString", value);
            }
        }

        public static string VaultVersion
        {
            get
            {
                var ini = new IniHandler(_ini);
                return ini.ReadValue("Vault", "Version");
            }
            set
            {
                var ini = new IniHandler(_ini);
                ini.WriteValue("Vault", "Version", value);
            }
        }
    }
}