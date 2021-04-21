using System.Collections.Generic;

namespace IDB.DbEntity
{
    public interface IDbEntity
    {
        Dictionary<string, object> UserDefinedProperties { get; set; }
    }
}