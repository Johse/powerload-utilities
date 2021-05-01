using System.Collections.Generic;

namespace IDB.Core.Data.Base
{
    public class RelationBase
    {
        public object Tag { get; set; }
        // ReSharper disable once InconsistentNaming
        public string Validation_Comment { get; set; }
        // ReSharper disable once InconsistentNaming
        public string Validation_Status { get; set; }

        public RelationBase()
        {
        }

        public RelationBase(IDictionary<string, object> dapperRow)
        {
            foreach (KeyValuePair<string, object> keyValuePair in dapperRow)
            {
                if (!keyValuePair.Key.StartsWith("UDP_"))
                    GetType().GetProperty(keyValuePair.Key)?.SetValue(this, keyValuePair.Value, null);
            }
        }
    }
}