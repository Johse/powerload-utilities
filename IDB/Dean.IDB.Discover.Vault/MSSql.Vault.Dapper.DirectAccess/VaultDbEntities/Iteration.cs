using System;
using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class Iteration : IVaultDbEntity, IEquatable<Iteration>
    {
        public long IterationID { get; set; } // bigint, not null

        public long MasterID { get; set; } // bigint, not null

        public int IterationNumber { get; set; } // int, not null

        public long? RevisionId { get; set; } // bigint, null

        public string Comment { get; set; } // nvarchar(4000), not null

        public long? LifeCycleStateID { get; set; } // bigint, null

        // public bool Controlled { get; set; } // bit, not null

        // public Guid rowguid { get; set; } // uniqueidentifier, not null


        public long GetId()
        {
            return IterationID;
        }

        public string GetSelectString()
        {
            return "SELECT IterationID, MasterID, IterationNumber, RevisionId, Comment, LifeCycleStateID FROM dbo.Iteration";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }


        #region implement IEquatable interfaces

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            Iteration objAsIteration = obj as Iteration;
            if (objAsIteration == null) return false;
            else return Equals(objAsIteration);
        }

        public bool Equals(Iteration other)
        {
            if (other == null) return false;
            return (this.IterationID.Equals(other.IterationID));
        }

        public override int GetHashCode()
        {
            return (int)this.IterationID;
        }


        #endregion implement IEquatable interfaces


    }
}