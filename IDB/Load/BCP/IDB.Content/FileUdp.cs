namespace IDB.Load.BCP.IDB.Content
{
    public class FileUdp
    {
        string name;
        string value;
        string fullFileName;
        long fileVersion;
        internal FileUdp(string udpName, string udpValue, string localFullFileName, int fileVersion)
        {
            name = udpName;
            fullFileName = localFullFileName;
            value = udpValue;
            this.fileVersion = fileVersion;
        }
    }
}
