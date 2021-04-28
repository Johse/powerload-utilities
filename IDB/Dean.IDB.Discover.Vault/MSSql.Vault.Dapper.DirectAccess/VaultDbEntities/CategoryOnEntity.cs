using System;
using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class CategoryOnEntity : IVaultDbEntity
    {
        public long EntityId { get; set; } // bigint, not null

        public long CategoryDefId { get; set; } // bigint, not null

        public Guid rowguid { get; set; } // uniqueidentifier, not null



        public long GetId()
        {
            return EntityId;
        }

        public string GetSelectString()
        {
            return "SELECT EntityId, CategoryDefId FROM dbo.CategoryOnEntity";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }
    }
}