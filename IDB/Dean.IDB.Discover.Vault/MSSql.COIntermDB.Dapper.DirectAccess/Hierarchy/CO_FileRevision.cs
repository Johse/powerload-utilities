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
    // class to manage groupings of CO_FileIteration objects
    // owns the Revision label - same revision label of FileIterations will be grouped together
    // potential for this object to analyze sequence of revisions for consistency
    public class CO_FileRevision
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(CO_FileRevision));

        // manage the file Revisiontable record for this revision
        // there is NO revision record - this was only for the VaultBCP to SQLite conversion
        // public FileRevision mFileRevisionRecord { get; private set; }

        // CO_FileMaster as parent
        public CO_FileMaster mCO_FileMasterOwner { get; private set; }

        // CO_FileIteration as children of this revision
        public List<CO_FileIteration> mCO_FileIterationList { get; private set; }

        // manage the revision label
        public string mRevLabel { get; private set; }

        // manage the VaultBCP content for building the data loader package
        public FileRevision mBcpFileRevision { get; set; }


        // constructor
        public CO_FileRevision(string sRevLabel, IEnumerable<CO_FileIteration> co_FileIterations)
        {
            mRevLabel = sRevLabel;
            mCO_FileIterationList = co_FileIterations.ToList();

            // set the ownership
            mCO_FileIterationList.ForEach(cfi => cfi.SetCO_FileRevisionOwner(this));
        }

        // set the CO_FileMaster as owner of this object
        public void SetCO_FileMasterOwner(CO_FileMaster co_FileMaster)
        {
            mCO_FileMasterOwner = co_FileMaster;
        }
    }
}
