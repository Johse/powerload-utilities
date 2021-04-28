using System;
using System.Data.SqlClient;


namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class Property : IVaultDbEntity
    {
        public long PropertyID { get; set; } // bigint, not null

        public long PropertyDefID { get; set; } // bigint, not null

        public long EntityID { get; set; } // bigint, not null

        public object Value { get; set; } // sql_variant, null

        // public Guid rowguid { get; set; } // uniqueidentifier, not null



        public long GetId()
        {
            return PropertyID;
        }

        public string GetSelectString()
        {
            return "SELECT PropertyID, PropertyDefID, EntityID, Value FROM dbo.Property";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }

    }
}