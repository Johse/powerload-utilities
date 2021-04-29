using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;


namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class IterationProviderAndContentSource : IVaultDbEntity
    {
        public long IterationId { get; set; } // bigint, not null

        public long ResourceId { get; set; } // bigint, not null

        public long MasterID { get; set; } // bigint, not null

        public int IterationNumber { get; set; } // int, not null

        public string ProviderDisplayName { get; set; } // nvarchar

        public long ContentSourceId { get; set; } // bigint, not null


        public long GetId()
        {
            return IterationId;
        }

        // static method to get the select string
        static public string SelectStatement()
        {
            IterationProviderAndContentSource ipcs = new IterationProviderAndContentSource();
            return (ipcs.GetSelectString());
        }

        public string GetSelectString()
        {

            return "SELECT Iteration.IterationId AS IterationId, FileIteration.ResourceId, Master.MasterID, Iteration.IterationNumber, ContentSource.DisplayName AS ProviderDisplayName, ContentSourceIndexStatus.ContentSourceId " +
                        "FROM FileIteration " +
                            "RIGHT OUTER JOIN Iteration ON FileIteration.FileIterationId = Iteration.IterationID " +
                            "RIGHT OUTER JOIN Master ON Iteration.MasterId = Master.MasterID " +
                            "RIGHT OUTER JOIN Iteration IT2 ON Master.TipCommittedIterationID = IT2.IterationID " +
                            "RIGHT OUTER JOIN FileIteration FI2 ON FI2.FileIterationId = IT2.IterationID " +
                            "RIGHT OUTER JOIN ContentSourceIndexStatus ON FI2.ResourceId = ContentSourceIndexStatus.EntityId " +
                            "RIGHT OUTER JOIN ContentSource ON ContentSourceIndexStatus.ContentSourceId = ContentSource.ContentSourceID";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }
    }
}

