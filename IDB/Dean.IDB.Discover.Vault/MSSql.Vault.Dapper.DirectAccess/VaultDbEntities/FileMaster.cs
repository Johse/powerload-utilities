using System;
using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class FileMaster : IVaultDbEntity, IEquatable<FileMaster>
    {

        public long FileMasterID { get; set; } // bigint, not null

        public bool CheckedOut { get; set; } // bit, not null

        public string CheckoutLocalSpec { get; set; } // nvarchar(256), null

        public string CheckoutMachine { get; set; } // nvarchar(60), null

        public long FileClassification { get; set; } // bigint, not null

        public bool Hidden { get; set; } // bit, not null

        public int FileStatus { get; set; } // int, not null

        public bool Unmanaged { get; set; } // bit, not null

        public long FolderId { get; set; } // bigint, not null

        public string TipFileBaseName { get; set; } // nvarchar(256), null

        public Guid rowguid { get; set; } // uniqueidentifier, not null


        public long GetId()
        {
            return FileMasterID;
        }

        public string GetSelectString()
        {
            return "SELECT FileMasterID, CheckedOut, CheckoutLocalSpec, CheckoutMachine, FileClassification, Hidden, FileStatus, Unmanaged, FolderId, TipFileBaseName FROM dbo.FileMaster";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }

        #region implement IEquatable interfaces

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            FileMaster objAsFileMaster = obj as FileMaster;
            if (objAsFileMaster == null) return false;
            else return Equals(objAsFileMaster);
        }

        public bool Equals(FileMaster other)
        {
            if (other == null) return false;
            return (this.FileMasterID.Equals(other.FileMasterID));
        }

        public override int GetHashCode()
        {
            return (int)this.FileMasterID;
        }


        #endregion implement IEquatable interfaces


    }
}