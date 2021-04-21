using System.Collections.Generic;

namespace IDB.Data.DbEntity
{
    public interface IDbEntity
    {
        Dictionary<string, object> UserDefinedProperties { get; set; }
    }
}