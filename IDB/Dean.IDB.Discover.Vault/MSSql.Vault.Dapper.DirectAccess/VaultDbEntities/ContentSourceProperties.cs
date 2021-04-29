using System;
using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    // the are the exclusive domain of the ResourceId within the FileResource objects
    // they may be the same properties as discovered in the "Property" table, but that table
    // is mostly for FileIterations
    public class ContentSourceProperties : IVaultDbEntity
    {
        public long ContentSourceID { get; set; } // bigint, not null

        public string ContentSourcePropertyMoniker { get; set; } // nvarchar(255), not null

        public long EntityId { get; set; } // bigint, not null

        public long EntityClassId { get; set; } // bigint, not null

        public string DataType { get; set; } // nvarchar(60), not null

        public object Value { get; set; } // sql_variant, null

        public Guid rowguid { get; set; } // uniqueidentifier, not null


        public long GetId()
        {
            return EntityId;
        }

        public string GetSelectString()
        {
            return "SELECT ContentSourceID, ContentSourcePropertyMoniker, EntityId, EntityClassId, DataType, Value  FROM dbo.ContentSourceProperties";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }

    }
}
