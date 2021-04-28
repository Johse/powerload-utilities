using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace MSSql.COIntermDB.Dapper.DirectAccess.DbEntity
{
    public interface IDbEntity
    {
        string GetSelectString();
        string GetTableName();
        IDbEntity GetNullEntity();

    }

    public interface IDbEntityWithID : IDbEntity
    {
        int GetId();
    }

    public interface IDbEntityWithIDAndUDPs : IDbEntityWithID
    {
        Dictionary<string, object> UserDefinedProperties { get; set; }
    }
}