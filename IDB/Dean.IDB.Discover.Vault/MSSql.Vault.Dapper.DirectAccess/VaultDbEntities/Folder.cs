using System;
using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class Folder : IVaultDbEntity
    {
        public long FolderID { get; set; } // bigint, not null

        public string FolderName { get; set; } // nvarchar(255), not null

        public bool Library { get; set; } // bit, not null

        public string FullUNCPath { get; set; } // nvarchar(300), null

        public string VaultPath { get; set; } // nvarchar(1000), not null

        public long? ParentFolderId { get; set; } // bigint, null

        public Guid rowguid { get; set; } // uniqueidentifier, not null


        public long GetId()
        {
            return FolderID;
        }

        public string GetSelectString()
        {
            return "SELECT FolderID, FolderName, Library, FullUNCPath, VaultPath, ParentFolderId FROM dbo.Folder";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }
    }
}