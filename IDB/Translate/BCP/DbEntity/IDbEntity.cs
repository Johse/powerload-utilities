using System.Collections.Generic;

namespace IDB.Translate.BCP.DbEntity
{
    public interface IDbEntity
    {
        Dictionary<string, object> UserDefinedProperties { get; set; }
    }
}