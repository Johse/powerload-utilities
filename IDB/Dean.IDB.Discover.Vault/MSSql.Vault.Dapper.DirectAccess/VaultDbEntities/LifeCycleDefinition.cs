using System;
using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class LifeCycleDefinition : IVaultDbEntity
    {
        public long LifeCycleDefId { get; set; } // bigint, not null

        public string Name { get; set; } // nvarchar(256), not null

        public string DisplayName { get; set; } // nvarchar(256), not null

        public string Description { get; set; } // nvarchar(256), null

        public DateTime LastUpdate { get; set; } // datetime, not null

        public bool IsSystem { get; set; } // bit, not null

        public string SystemName { get; set; } // nvarchar(256), not null

        public Guid rowguid { get; set; } // uniqueidentifier, not null


        public long GetId()
        {
            return LifeCycleDefId;
        }

        public string GetSelectString()
        {
            return "SELECT LifeCycleDefId, Name, DisplayName, Description, LastUpdate, IsSystem, SystemName FROM dbo.LifeCycleDefinition";
        }

        public IVaultDbEntity GetNullEntity()
        {
            var lifeCycleDefinition = new LifeCycleDefinition();
            return lifeCycleDefinition;
        }
    }
}