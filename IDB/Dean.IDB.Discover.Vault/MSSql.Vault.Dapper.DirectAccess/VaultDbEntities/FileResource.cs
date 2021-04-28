using System;
using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class FileResource : IVaultDbEntity
    {
        public long ResourceId { get; set; } // bigint, not null

        public long FileMasterId { get; set; } // bigint, not null

        public long FileSize { get; set; } // bigint, not null

        public int Checksum { get; set; } // int, not null

        public int Version { get; set; } // int, not null

        public string Extension { get; set; } // nvarchar(256), not null

        public Guid ResourceGuid { get; set; } // uniqueidentifier, not null

        public Guid rowguid { get; set; } // uniqueidentifier, not null


        public long GetId()
        {
            return ResourceId;
        }

        public string GetSelectString()
        {
            return "SELECT ResourceId, FileMasterId, FileSize, Checksum, Version, Extension, ResourceGuid FROM dbo.FileResource";
        }


        public void Populate(SqlDataReader reader)
        {
            var id = (long)reader[0];
            var fileMasterId = (long)reader[1];
            var checksum = reader.GetInt32(2);

            ResourceId = id;
            FileMasterId = fileMasterId;
            Checksum = checksum;
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }
    }
}