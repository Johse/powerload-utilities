using System;
using System.Data.SqlClient;


namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class Acl : IVaultDbEntity
    {
        public long AclId { get; set; } // bigint, not null

        public long UserGroupId { get; set; } // bigint, not null

        public byte? AllowRead { get; set; } // tinyint, null

        public byte? AllowWrite { get; set; } // tinyint, null

        public byte? AllowDelete { get; set; } // tinyint, null

        public bool GC { get; set; } // bit, not null

        public Guid rowguid { get; set; } // uniqueidentifier, not null

        public long GetId()
        {
            return AclId;
        }

        public string GetSelectString()
        {
            return "SELECT AclId, UserGroupId, AllowRead, AllowWrite, AllowDelete, GC FROM dbo.Acl";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }
    }
}
