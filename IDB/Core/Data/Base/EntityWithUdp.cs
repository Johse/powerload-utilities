using System.Collections.Generic;
using IDB.Core.Data.Interface;

namespace IDB.Core.Data.Base
{
    public class BaseEntityUdp : EntityBase, IUdp
    {
        public Dictionary<string, object> UserDefinedProperties { get; set; }

        public BaseEntityUdp()
        {
            UserDefinedProperties = new Dictionary<string, object>();
        }

        public BaseEntityUdp(IDictionary<string, object> dapperRow) : this()
        {
            foreach (KeyValuePair<string, object> keyValuePair in dapperRow)
            {
                if (keyValuePair.Key.StartsWith("UDP_"))
                    UserDefinedProperties.Add(keyValuePair.Key.Substring(4), keyValuePair.Value);
                else
                    GetType().GetProperty(keyValuePair.Key)?.SetValue(this, keyValuePair.Value, null);
            }
        }
    }
}
