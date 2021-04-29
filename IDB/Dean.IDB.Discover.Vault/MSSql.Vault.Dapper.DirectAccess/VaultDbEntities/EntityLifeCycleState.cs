using System;
using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class EntityLifeCycleState : IVaultDbEntity
    {
        public long LifeCycleStateId { get; set; } // bigint, not null

        public long EntityID { get; set; } // bigint, not null

        public Guid rowguid { get; set; } // uniqueidentifier, not null


        public long GetId()
        {
            return EntityID;
        }

        public string GetSelectString()
        {
            return "SELECT LifeCycleStateId, EntityID FROM dbo.EntityLifeCycleState";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }
    }
}
