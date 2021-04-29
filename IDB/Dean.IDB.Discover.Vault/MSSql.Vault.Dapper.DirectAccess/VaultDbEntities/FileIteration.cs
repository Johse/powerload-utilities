using System;
using System.Data.SqlClient;


namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class FileIteration : IVaultDbEntity
    {
        public long FileIterationId { get; set; } // bigint, not null

        public string FileName { get; set; } // nvarchar(256), not null

        public DateTime ModDate { get; set; } // datetime, not null

        public DateTime? CheckinDate { get; set; } // datetime, null

        public string BOMBlob { get; set; } // ntext, null

        public int DesignVisualizationAttachmentStatus { get; set; } // int, not null

        public bool IsParsed { get; set; } // bit, not null

        // Controlled is no longer in the table
        //public bool Controlled { get; set; } // bit, not null

        public string LifeCycleStateName { get; set; } // nvarchar(256), null

        public DateTime? CheckoutDate { get; set; } // datetime, null

        public long ResourceId { get; set; } // bigint, not null

        public Guid rowguid { get; set; } // uniqueidentifier, not null



        // remove TipFileName as it is no longer needed by ETL in Vault 2013, and is no longer available in 2014
        // public string TipFileName { get; set; }

        public long GetId()
        {
            return FileIterationId;
        }

        public string GetSelectString()
        {
            // remove TipFileName as it is no longer needed by ETL in Vault 2013, and is no longer available in 2014
            // return "SELECT FileIterationId, FileName, ResourceId, ModDate, CheckinDate, TipFileName FROM dbo.FileIteration";
            return "SELECT FileIterationId, FileName, ModDate, CheckinDate, BOMBlob, DesignVisualizationAttachmentStatus, IsParsed, Controlled, LifeCycleStateName, CheckoutDate, ResourceId FROM dbo.FileIteration";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }

    }
}

