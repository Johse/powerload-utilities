using System;
using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class Entity : IVaultDbEntity
    {
        public long EntityId { get; set; } // bigint, not null

        public long EntityClassID { get; set; } // bigint, not null

        public DateTime? CreateDate { get; set; } // datetime, null

        public long? CreateUserID { get; set; } // bigint, null

        public bool? Active { get; set; } // bit, null

        public bool IsConsumable { get; set; } // bit, not null

        public Guid rowguid { get; set; } // uniqueidentifier, not null


        public long GetId()
        {
            return EntityId;
        }

        public string GetSelectString()
        {
            return "SELECT EntityId, EntityClassID, CreateDate, CreateUserID, Active, IsConsumable FROM dbo.Entity";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }
    }
}