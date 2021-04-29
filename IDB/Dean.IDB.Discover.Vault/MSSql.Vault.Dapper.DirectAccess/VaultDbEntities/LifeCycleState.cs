using System;
using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class LifeCycleState : IVaultDbEntity
    {
        public long LifeCycleStateId { get; set; } // bigint, not null

        public long LifeCycleDefId { get; set; } // bigint, not null

        public string Name { get; set; } // nvarchar(256), not null

        public string DisplayName { get; set; } // nvarchar(256), not null

        public string Description { get; set; } // nvarchar(256), null

        public bool IsDefault { get; set; } // bit, not null

        public bool UseStateBasedSecurity { get; set; } // bit, not null

        public long? ACLId { get; set; } // bigint, null

        public bool IsSystem { get; set; } // bit, not null

        public bool IsReleasedState { get; set; } // bit, not null

        public short RestrictPurgeOption { get; set; } // smallint, not null

        public Guid rowguid { get; set; } // uniqueidentifier, not null


        public long GetId()
        {
            return LifeCycleStateId;
        }

        public string GetSelectString()
        {
            return "SELECT LifeCycleStateId, LifeCycleDefId, Name, DisplayName, Description, IsDefault, UseStateBasedSecurity, ACLId, IsSystem, IsReleasedState, RestrictPurgeOption FROM dbo.LifeCycleState";
        }

        public IVaultDbEntity GetNullEntity()
        {
            var lifeCycleState = new LifeCycleState();
            return lifeCycleState;
        }
    }
}