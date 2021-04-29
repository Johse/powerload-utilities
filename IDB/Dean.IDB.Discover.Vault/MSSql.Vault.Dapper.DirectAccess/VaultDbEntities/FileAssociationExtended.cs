using System;
using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class FileAssociationExtended : IVaultDbEntity
    {
        public long FileAssociationId { get; set; } // bigint, not null

        public long FromId { get; set; } // bigint, not null

        public long FromMasterID { get; set; } // bigint, not null

        public long ToId { get; set; } // bigint, not null

        public long ToMasterID { get; set; } // bigint, not null

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
            //return "SELECT FA.FileAssociationId, FA.FromId, FromIT.MasterID AS FromMasterID, FA.ToId, ToIT.MasterID AS ToMasterID, FA.ChildRevisionId, FA.CreateDate, FA.CreateUserId, FA.IsDependency, FA.Source, FA.RefId, FA.ExpectedVaultPath, FA.ParentVaultPath, FA.PurgeHealStatus  FROM FileAssociation FA " +
            //            "LEFT OUTER JOIN Iteration FromIT ON FA.FromId = FromIT.IterationID " +
            //            "LEFT OUTER JOIN Iteration ToIT ON FA.ToId = ToIT.IterationID";

            return "SELECT FA.FileAssociationId, FA.FromId, FromIT.MasterID AS FromMasterID, FA.ToId, ToIT.MasterID AS ToMasterID, FA.ChildRevisionId, FA.CreateDate, FA.CreateUserId, FA.IsDependency, FA.Source, FA.RefId, FA.ExpectedVaultPath, FA.ParentVaultPath FROM FileAssociation FA " +
                        "LEFT OUTER JOIN Iteration FromIT ON FA.FromId = FromIT.IterationID " +
                        "LEFT OUTER JOIN Iteration ToIT ON FA.ToId = ToIT.IterationID";
        }


        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }

        // temporary method to getting the selections string
        // TODO: set all IVaultDbEntity.GetSelectString() to static so we don't have to instantiate a temporary object
        static public string GetAllSelectionString()
        {
            FileAssociationExtended tempFAE = new FileAssociationExtended();
            return tempFAE.GetSelectString();
        }

        // get only those relationships with the FromId matching the Master.TipCommittedIterationID
        static public string GetLatestFromSelectionString()
        {
            //return "SELECT FA.FileAssociationId, FA.FromId, FromIT.MasterID AS FromMasterID, FA.ToId, ToIT.MasterID AS ToMasterID, FA.ChildRevisionId, FA.CreateDate, FA.CreateUserId, FA.IsDependency, FA.Source, FA.RefId, FA.ExpectedVaultPath, FA.ParentVaultPath, FA.PurgeHealStatus  FROM FileAssociation FA " +
            //            "LEFT OUTER JOIN Iteration FromIT ON FA.FromId = FromIT.IterationID " +
            //            "INNER JOIN Master FromM ON FromIt.MasterID = FromM.MasterID AND FromIt.IterationID = FromM.TipCommittedIterationID " +
            //            "LEFT OUTER JOIN Iteration ToIT ON FA.ToId = ToIT.IterationID";

            return "SELECT FA.FileAssociationId, FA.FromId, FromIT.MasterID AS FromMasterID, FA.ToId, ToIT.MasterID AS ToMasterID, FA.ChildRevisionId, FA.CreateDate, FA.CreateUserId, FA.IsDependency, FA.Source, FA.RefId, FA.ExpectedVaultPath, FA.ParentVaultPath  FROM FileAssociation FA " +
                        "LEFT OUTER JOIN Iteration FromIT ON FA.FromId = FromIT.IterationID " +
                        "INNER JOIN Master FromM ON FromIt.MasterID = FromM.MasterID AND FromIt.IterationID = FromM.TipCommittedIterationID " +
                        "LEFT OUTER JOIN Iteration ToIT ON FA.ToId = ToIT.IterationID";

        }


    }
}