using System.IO;
using System.Reflection;
using IDB.Core.INI;

namespace IDB.Core
{
    public class Settings
    {
        public static string GetIniFile(string additionalName = "")
        {
            var assembly = Assembly.GetCallingAssembly();
            var directory = Path.GetDirectoryName(assembly.Location);
            var fileName = Path.GetFileNameWithoutExtension(assembly.Location);
            var path = Path.Combine(directory ?? string.Empty, fileName) + additionalName + ".ini";
            return path;
        }

        private static readonly string IniFile;
        static Settings()
        {
            IniFile = GetIniFile();
        }

        public static string IdbConnectionString
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("IDB", "ConnectionString");
            }
            set
            {
                var ini = new IniHandler(IniFile);
                ini.WriteValue("IDB", "ConnectionString", value);
            }
        }

        public static string ImportPath
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("IDB", "ImportPath");
            }
            set
            {
                var ini = new IniHandler(IniFile);
                ini.WriteValue("IDB", "ImportPath", value);
            }
        }

        public static string ExportPath
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("IDB", "ExportPath");
            }
            set
            {
                var ini = new IniHandler(IniFile);
                ini.WriteValue("IDB", "ExportPath", value);
            }
        }

        public static string VaultConnectionString
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("Vault", "ConnectionString");
            }
            set
            {
                var ini = new IniHandler(IniFile);
                ini.WriteValue("Vault", "ConnectionString", value);
            }
        }

        public static string VaultVersion
        {
            get
            {
                var ini = new IniHandler(IniFile);
                return ini.ReadValue("Vault", "Version");
            }
            set
            {
                var ini = new IniHandler(IniFile);
                ini.WriteValue("Vault", "Version", value);
            }
        }
    }
}