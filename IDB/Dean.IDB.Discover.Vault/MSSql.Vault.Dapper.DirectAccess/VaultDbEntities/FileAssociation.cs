using System;
using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class FileAssociation : IVaultDbEntity
    {
        public long FileAssociationId { get; set; } // bigint, not null

        public long FromId { get; set; } // bigint, not null

        public long ToId { get; set; } // bigint, not null

        public long? ChildRevisionId { get; set; } // bigint, null

        public DateTime CreateDate { get; set; } // datetime, not null

        public long CreateUserId { get; set; } // bigint, not null

        public bool IsDependency { get; set; } // bit, not null

        public string Source { get; set; } // nvarchar(512), null

        public string RefId { get; set; } // nvarchar(256), null

        public string ExpectedVaultPath { get; set; } // nvarchar(1257), null

        public string ParentVaultPath { get; set; } // nvarchar(1257), null

        // public string PurgeHealStatus { get; set; } // char(1), null

        public long GetId()
        {
            return FileAssociationId;
        }

        public string GetSelectString()
        {
            // return "SELECT FileAssociationId, FromId, ToId, ChildRevisionId, CreateDate, CreateUserId, IsDependency, Source, RefId, ExpectedVaultPath, ParentVaultPath, PurgeHealStatus FROM dbo.FileAssociation";
            return "SELECT FileAssociationId, FromId, ToId, ChildRevisionId, CreateDate, CreateUserId, IsDependency, Source, RefId, ExpectedVaultPath, ParentVaultPath FROM dbo.FileAssociation";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }
    }
}