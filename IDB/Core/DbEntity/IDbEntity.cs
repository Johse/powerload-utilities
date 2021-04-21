using System.Collections.Generic;

namespace IDB.Core.DbEntity
{
    public interface IDbEntity
    {
        Dictionary<string, object> UserDefinedProperties { get; set; }
    }
}