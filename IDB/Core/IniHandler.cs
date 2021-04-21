using System.Runtime.InteropServices;
using System.Text;

namespace IDB.Core
{
    public class IniHandler
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        private readonly string _path;
        public IniHandler(string path)
        {
            _path = path;
        }

        public void WriteValue(string section, string key, string value)
        {
            try
            {
                WritePrivateProfileString(section, key, value, _path);
            }
            catch
            {
                // ignored
            }
        }

        public string ReadValue(string section, string key)
        {
            try
            {
                var sb = new StringBuilder(255);
                GetPrivateProfileString(section, key, "", sb, 255, _path);
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}