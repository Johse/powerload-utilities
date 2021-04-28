using System;
using System.Data.SqlClient;


namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class EntityToAcl : IVaultDbEntity
    {
        public long EntityId { get; set; } // bigint, not null

        public long EntityClassId { get; set; } // bigint, not null

        public long AclId { get; set; } // bigint, not null

        public byte AclType { get; set; } // tinyint, not null

        public Guid rowguid { get; set; } // uniqueidentifier, not null

        public long GetId()
        {
            return EntityId;
        }

        public string GetSelectString()
        {
            return "SELECT EntityId, EntityClassId, AclId, AclType FROM dbo.EntityToAcl WHERE  AclType = 1";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }

    }
}
