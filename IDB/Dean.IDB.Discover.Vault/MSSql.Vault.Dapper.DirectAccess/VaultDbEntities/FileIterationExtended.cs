using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;


namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class FileIterationExtended : IVaultDbEntity
    {
        public long FileIterationId { get; set; } // bigint, not null

        public long FileMasterId { get; set; } // bigint, not null

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
            // returns FileMasterId by using INNER JOIN on dbo.Iteration
            //return "SELECT FI.FileIterationId, IT.MasterId AS FileMasterId, FI.FileName, FI.ModDate, FI.CheckinDate, FI.BOMBlob, FI.DesignVisualizationAttachmentStatus, FI.IsParsed, FI.LifeCycleStateName, FI.CheckoutDate, FI.ResourceId FROM dbo.FileIteration FI " +
            //            "INNER JOIN dbo.Iteration IT ON FI.FileIterationId = IT.IterationID";

            // remove BOMBlob
            return "SELECT FI.FileIterationId, IT.MasterId AS FileMasterId, FI.FileName, FI.ModDate, FI.CheckinDate, FI.DesignVisualizationAttachmentStatus, FI.IsParsed, FI.LifeCycleStateName, FI.CheckoutDate, FI.ResourceId FROM dbo.FileIteration FI " +
                        "INNER JOIN dbo.Iteration IT ON FI.FileIterationId = IT.IterationID";

        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }


        // method to get latest file iterations based on Master.TipCommittedIterationID
        static public string GetTipCommitedSelectString()
        {
            // remove TipFileName as it is no longer needed by ETL in Vault 2013, and is no longer available in 2014
            //return "SELECT FI.FileIterationId, MA.MasterId AS FileMasterId, FI.FileName, FI.ModDate, FI.CheckinDate, FI.BOMBlob, FI.DesignVisualizationAttachmentStatus, FI.IsParsed, FI.LifeCycleStateName, FI.CheckoutDate, FI.ResourceId FROM dbo.FileIteration FI " +
            //            "INNER JOIN dbo.[Master] MA ON FI.FileIterationId = MA.TipCommittedIterationID";

            // remove BOMBlob
            return "SELECT FI.FileIterationId, MA.MasterId AS FileMasterId, FI.FileName, FI.ModDate, FI.CheckinDate, FI.DesignVisualizationAttachmentStatus, FI.IsParsed, FI.LifeCycleStateName, FI.CheckoutDate, FI.ResourceId FROM dbo.FileIteration FI " +
                        "INNER JOIN dbo.[Master] MA ON FI.FileIterationId = MA.TipCommittedIterationID";

        }

    }
}

