using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;
using BCPBuilderConfig;

using MSSql.COIntermDB.Dapper.DirectAccess.DbEntity;
using MSSql.COIntermDB.Dapper.DirectAccess.DbLink;
using MSSql.COIntermDB.Dapper.DirectAccess.DbRelation;

using bcpDevKit;
using bcpDevKit.Entities;
using bcpDevKit.Entities.Items;
using bcpDevKit.Entities.Vault;
using bcpDevKit.Entities.Configuration;
using bcpDevKit.Entities.General;



namespace MSSql.COIntermDB.Dapper.DirectAccess.Hierarchy
{
    // class to manage groupings of CO_ItemIteration objects
    // owns the Revision label - same revision label of ItemIterations will be grouped together
    // potential for this object to analyze sequence of revisions for consistency
    public class CO_ItemRevision
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(CO_ItemRevision));

        // manage the Item Revision table record for this revision
        // there is NO revision record - this was only for the VaultBCP to SQLite conversion
        // public ItemRevision mItemRevisionRecord { get; private set; }

        // CO_ItemMaster as parent
        public CO_ItemMaster mCO_ItemMasterOwner { get; private set; }

        // CO_ItemIteration as children of this revision
        public List<CO_ItemIteration> mCO_ItemIterationList { get; private set; }

        // manage the revision label
        public string mRevLabel { get; private set; }

        // manage the VaultBCP content for building the data loader package
        public ItemRevision mBcpItemRevision { get; set; }



        // constructor
        public CO_ItemRevision(string sRevLabel, IEnumerable<CO_ItemIteration> co_ItemIterations)
        {
            mRevLabel = sRevLabel;
            mCO_ItemIterationList = co_ItemIterations.ToList();

            // set the ownership
            mCO_ItemIterationList.ForEach(cfi => cfi.SetCO_ItemRevisionOwner(this));
        }

        // set the CO_ItemMaster as owner of this object
        public void SetCO_ItemMasterOwner(CO_ItemMaster co_ItemMaster)
        {
            mCO_ItemMasterOwner = co_ItemMaster;
        }

    }
}
