using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class VaultFileIterationOld
    {
        /// <summary>
        /// The iteration id of the file in the target DB
        /// </summary>
        public long IterationId { get; set; }

        /// <summary>
        /// The MasterId of the file in the target DB
        /// </summary>
        public long MasterId { get; set; }

        /// <summary>
        /// The iteration number of the file in the target Vault DB
        /// sequences the iterations from 1 - n so that we can sort them properly
        /// </summary>
        public int IterationNumber { get; set; }

        /// <summary>
        /// points to the Revision table, gives us a window into the LifeCycleStateId
        /// and more importantly, a window into the LifeCycleState table, and if the state is IsReleasedState
        /// </summary>
        public long RevisionId { get; set; }

        /// <summary>
        /// Comment
        /// </summary>
        public String Comment { get; set; }

        /// <summary>
        /// checksum of the vaulted file
        /// </summary>
        public int Checksum { get; set; }

        /// <summary>
        /// CreateDate property from the TargetDB on the Entity table
        /// NOTE: important, this is the date that MUST be used when creating an IterationRef
        /// this can be off by miliseconds or more relative to the checkin date 
        /// if it is not exact, VaultBCP will reject it
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// ModDate property from the TargetDB
        /// </summary>
        public DateTime ModDate { get; set; }

        /// <summary>
        /// ModDate property from the TargetDB
        /// </summary>
        public DateTime CheckinDate { get; set; }

        /// <summary>
        /// Comment
        /// </summary>
        public String LifeCycleStateName { get; set; }

        /// <summary>
        /// category definition Id
        /// </summary>
        public long CategoryDefId { get; set; }

        /// <summary>
        /// category definition name
        /// </summary>
        public String CategoryDefName { get; set; }

        /// <summary>
        /// revision definition Id
        /// </summary>
        public long RevisionDefinitionId { get; set; }

        /// <summary>
        /// label of the revisions such as 1, 2, 3, A, B, C, etc
        /// </summary>
        public String RevisionLabel { get; set; }

        /// <summary>
        /// revision defition name
        /// </summary>
        public String RevisionDefinitionName { get; set; }

        /// <summary>
        /// revision definition Id
        /// </summary>
        public long LifeCycleStateId { get; set; }


        /// <summary>
        /// revision defition name
        /// </summary>
        public String LifeCycleStateDisplay { get; set; }


        /// <summary>
        /// revision definition Id
        /// </summary>
        public long LifeCycleDefId { get; set; }


        /// <summary>
        /// revision defition name
        /// </summary>
        public String LifeCycleStatName { get; set; }


        /// <summary>
        /// revision definition Id
        /// </summary>
        public bool IsReleasedState { get; set; }


        /// <summary>
        /// revision defition name
        /// </summary>
        public String LifeCycleDefName { get; set; }

    }
}
