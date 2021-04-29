using System;
using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class Revision : IVaultDbEntity
    {
        public long RevisionId { get; set; } // bigint, not null

        public long? RevisionDefinitionId { get; set; } // bigint, null

        public long MasterId { get; set; } // bigint, not null

        public string RevisionLabel { get; set; } // nvarchar(50), not null

        public Single RevisionOrder { get; set; } // real, not null

        public long? MaxIterationId { get; set; } // bigint, null

        public long? MaxConsumableIterationId { get; set; } // bigint, null

        // LifeCycleStateId not in Revision table anymore
        // public long? LifeCycleStateId { get; set; } // bigint, null

        public Guid rowguid { get; set; } // uniqueidentifier, not null


        public long GetId()
        {
            return RevisionId;
        }

        public string GetSelectString()
        {
            return "SELECT RevisionId, RevisionDefinitionId, MasterId, RevisionLabel, RevisionOrder, MaxIterationId, MaxConsumableIterationId FROM dbo.Revision";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }
    }
}