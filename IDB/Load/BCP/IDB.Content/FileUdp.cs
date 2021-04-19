using System.Data.SqlClient;
namespace IDB.Load.BCP
{
    public class FileUdp
    {
        string name;
        string value;
        string fullFileName;
        int fileVersion;
        internal FileUdp(string udpName, string udpValue, string localFullFileName, int fileVersion)
        {
            name = udpName;
            fullFileName = localFullFileName;
            value = udpValue;
            this.fileVersion = fileVersion;

        }
        
    }
}
