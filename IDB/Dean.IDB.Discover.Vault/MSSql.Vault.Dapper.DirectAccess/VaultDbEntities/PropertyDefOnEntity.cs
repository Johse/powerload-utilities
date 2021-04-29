using System;
using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    //
    public class PropertyDefOnEntity : IVaultDbEntity
    {
        public long EntityId { get; set; } // bigint, not null, is actually the FileMasterId

        public long PropertyDefId { get; set; } // bigint, not null

        public bool Uncommitted { get; set; } // bit, not null

        public Guid rowguid { get; set; } // uniqueidentifier, not null


        public long GetId()
        {
            return PropertyDefId;
        }

        public string GetSelectString()
        {
            return "SELECT EntityId, PropertyDefId, Uncommitted FROM dbo.PropertyDefOnEntity";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }


    }
}
