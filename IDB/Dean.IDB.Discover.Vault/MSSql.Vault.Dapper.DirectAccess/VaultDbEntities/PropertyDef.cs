using System;
using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class PropertyDef : IVaultDbEntity
    {
        public long PropertyDefID { get; set; } // bigint, not null

        public string FriendlyName { get; set; } // nvarchar(60), not null

        public string SystemName { get; set; } // nvarchar(60), not null

        public string DataType { get; set; } // nvarchar(60), not null

        public bool Active { get; set; } // bit, not null

        public bool BasicSearch { get; set; } // bit, not null

        public bool IsSystem { get; set; } // bit, not null

        public DateTime LastCfgUpdate { get; set; } // datetime, not null

        public object InitialValue { get; set; } // sql_variant, null

        public Guid rowguid { get; set; } // uniqueidentifier, not null



        public long GetId()
        {
            return PropertyDefID;
        }

        public string GetSelectString()
        {
            return "SELECT PropertyDefID, FriendlyName, SystemName, DataType, Active, BasicSearch, IsSystem, LastCfgUpdate, InitialValue FROM dbo.PropertyDef";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }
    }
}