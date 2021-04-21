using System.Data.SqlClient;

namespace IDB.Load.BCP.IDB.Content
{
    class ItemUdp
    {
        string name;
        string value;
        int version;
        string itemNumber;
        public ItemUdp(string udpName, string udpValue, string itemNumber, int iterationVersion)
        {
            name = udpName;
            value = udpValue;
            version = iterationVersion;
            this.itemNumber = itemNumber;
        }
        internal void UdpInsert()
        {
            var connectionString = MainForm.InputConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SqlCommand("update Items set  [" + "UDP_" + name + "] = '" + value + "' where Version='" + version + "' AND ItemNumber='" + itemNumber + "'", connection))
                {
                    cmd.ExecuteScalar();
                }

            }
        }

    }

}
