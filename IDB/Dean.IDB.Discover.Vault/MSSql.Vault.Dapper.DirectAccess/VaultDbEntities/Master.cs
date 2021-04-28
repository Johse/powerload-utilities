using System;
using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class Master : IVaultDbEntity
    {
        public long MasterID { get; set; } // bigint, not null

        public long? TipIterationID { get; set; } // bigint, null

        public long? TipCommittedIterationID { get; set; } // bigint, null

        public long? MaxRevisionId { get; set; } // bigint, null

        public long? MaxConsumableRevisionId { get; set; } // bigint, null

        public bool ControlledByChangeOrder { get; set; } // bit, not null

        public Guid rowguid { get; set; } // uniqueidentifier, not null


        public long GetId()
        {
            return MasterID;
        }

        public string GetSelectString()
        {
            return "SELECT MasterID, TipIterationID, TipCommittedIterationID, MaxRevisionId, MaxConsumableRevisionId, ControlledByChangeOrder FROM dbo.Master";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }
    }
}