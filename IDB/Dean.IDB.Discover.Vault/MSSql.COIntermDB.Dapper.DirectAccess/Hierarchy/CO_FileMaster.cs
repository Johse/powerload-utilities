using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
    // class to manage groupings of CO_FileRevision objects
    public class CO_FileMaster
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(CO_FileMaster));

        // flags managing the status of the file
        [Flags]
        public enum MasterFileStatus : ulong
        {
            NoStatus = 0,

            // section contains information on renaming
            // file is renamed
            Renamed = 1L << 0,

            // has a renamed child or grandchild and has been checked in before the rename
            NeedsResolution = 1L << 1,

            // renamed and has parents needing resolution, from file up the lineage of parents
            WParNdngRes = 1L << 2,

            // children states
            HasRenamedChild = 1L << 3,
            HasRenamedDirectChilld = 1L << 4,
            HasMissingLineage = 1L << 5,
            AllMissingLineage = 1L << 6,
            RevsOutOfSeq = 1L << 7,
            MissingRevs = 1L << 8,

            // Version and state sequence
            VersNotSeq = 1L << 9,
            UnasngdStateOrder = 1L << 10,


            // top level/lowest level
            IsTopLevel = 1L << 11,
            IsLowLevel = 1L << 12,

            // has a drawing or dwf associated with it
            HasDrawing = 1L << 13,
            HasDwf = 1L << 14,

            // Revision information
            RevLabelNotSame = 1L << 15,

            // Iteration information
            STObjectIdNotSame = 1L << 16,

            // identify if there is a circular dependency
            CircularDependency = 1L << 17,

            // if this is in the UnknownReferences
            IsUnknownRef = 1L << 18,

            // identifies when file iterations are checked in out of sequence
            // this happens when PSP export shows dates not in a chronological order
            // for full LOAD, must fix these dates to be in chronological
            DatesOutOfSeq = 1L << 19,

            // manage the duplicate information
            HasDuplicateWhenFolderModified = 1L << 20,
            CannotSelectForProcessing = 1L << 21,

        }


        // manage the File table record for this file
        // there is NO file master record - this was only for the VaultBCP to SQLite conversion
        // public BSE.File mFileRecord { get; private set; }

        public string mName { get; private set; }
        public string mExtension { get; private set; }
        public bool mIsDWFFile { get; private set; }

        // CO_Folder as parent
        public CO_Folder mParentCO_Folder { get; private set; }

        // full vault path to file
        public string mFullVaultFilePath { get; private set; }

        // list of BCPFileRevisions
        public List<CO_FileRevision> mCO_FileRevisionList { get; private set; }

        // list of BCPFileIterations
        public List<CO_FileIteration> mCO_FileIterationList { get; private set; }
        public CO_FileIteration LatestCO_FileIteration { get { return (mCO_FileRevisionList.Last().mCO_FileIterationList.Last()); } }

        // manage the status of the file
        public MasterFileStatus mMasterFileStatus { get; private set; }

        // manage the VaultBCP content for building the data loader package
        public FileObject mBcpFile { get; set; }

        // manage the status of the file
        public CO_ItemMaster mCO_ItemMaster { get; private set; }


        // constructor
        public CO_FileMaster(string sName, CO_Folder parentCO_Folder)
        {
            mName = sName;
            mExtension = Path.GetExtension(mName);

            mMasterFileStatus = MasterFileStatus.NoStatus;

            mIsDWFFile = false;
            if ((mExtension.ToLower() == ".dwf") || (mExtension.ToLower() == ".dwfx"))
            {
                mIsDWFFile = true;
            }

            // set the parent folder
            mParentCO_Folder = parentCO_Folder;
            mFullVaultFilePath = mParentCO_Folder.mFullVaultFolderPath + "/" + mName;

            // setup the lists to manage objects
            mCO_FileRevisionList = new List<CO_FileRevision>();
            mCO_FileIterationList = new List<CO_FileIteration>();
        }

        // special case for Deublin and potentially OTHER customers where folders are restructured
        // outside of the normal folders based on the file relationships to the Folders table
        public void OverrideFullVaultFilePath(string newFilePath)
        {
            mFullVaultFilePath = newFilePath;
        }






        // build the CO_FileMaster and CO_FileRevision objects
        public static bool BuildFileMastersAndRevisions(Dictionary<int, CO_Folder> co_FolderDict,
                                                        Dictionary<int, CO_FileIteration> co_FileIterationDict,
                                                        ref List<CO_FileRevision> co_FileRevisionList,
                                                        ref List<CO_FileMaster> co_FileMasterList)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("BuildFileMastersAndRevisions(): Start");

            bool bSuccess = false;

            // group the iterations by their filename and folder id
            var fileMastersGrouped = co_FileIterationDict.Values.GroupBy(cfi => new { cfi.mFileName, cfi.mFolderId });
            int nGroups = fileMastersGrouped.Count();
            Logger.Debug(string.Format("Number File Iterations/Grouped As Masters: {0:n0}", co_FileIterationDict.Count(), nGroups));

            // iterate through and build the File masters
            co_FileRevisionList = new List<CO_FileRevision>();
            co_FileMasterList = new List<CO_FileMaster>();
            foreach (var masterGrp in fileMastersGrouped)
            {
                // masterGrp.Key = anonimous[.mFileName, .mFolderId]
                // masterGrp = IEnumberable<CO_FileIteration>

                // get the CO_Folder
                // note - it may be that the CO_FileIteration.mFolderId == null because
                // in the intermediate database, there was no functionality to represent the $ root folder of the Vault
                CO_Folder co_Folder = co_FolderDict[masterGrp.Key.mFolderId];

                // build the CO_FileMaster
                CO_FileMaster co_FileMaster = new CO_FileMaster(masterGrp.Key.mFileName, co_Folder);
                co_FileMasterList.Add(co_FileMaster);

                // group the CO_FileIteration by their revision labels (A, B, C) etc
                ILookup<string, CO_FileIteration> fileMasterLookup = masterGrp.ToLookup(cfi => cfi.mFileIterationRecord.RevisionLabel, cfi => cfi, StringComparer.CurrentCultureIgnoreCase);

                // iterate the revisions and build the CO_FileRevision for this master
                foreach (IGrouping<string, CO_FileIteration> groupItem in fileMasterLookup)
                {
                    // groupItem.Key = RevisionLabel<string>
                    // groupItem is IEnumerable<CO_FileIteration>

                    // build the CO_FileRevision object
                    CO_FileRevision co_FileRevision = new CO_FileRevision(groupItem.Key, groupItem);
                    co_FileRevisionList.Add(co_FileRevision);

                    // add the FileRevision and FileIterations to the master
                    co_FileMaster.mCO_FileRevisionList.Add(co_FileRevision);
                    co_FileMaster.mCO_FileIterationList.AddRange(co_FileRevision.mCO_FileIterationList);

                    // set the master for these objects
                    co_FileRevision.SetCO_FileMasterOwner(co_FileMaster);
                    co_FileRevision.mCO_FileIterationList.ForEach(cfi => cfi.SetCO_FileMasterOwner(co_FileMaster));
                }

            }

            // log total time taken
            Logger.Debug(string.Format("BuildFileMastersAndRevisions(): End {0}", swp.ElapsedTimeString()));

            bSuccess = true;

            return (bSuccess);
        }

        // set the item master for this CO_FileMaster
        public void Set_CO_ItemMaster(CO_ItemMaster coItemMaster)
        {
            if (this.mCO_ItemMaster != null)
            {
                // log total time taken
                Logger.Debug(string.Format("CO_FileMaster has multiple CO_ItemMasters: \"{0}\"", this.mName));
            }
            else
            {
                this.mCO_ItemMaster = coItemMaster;
            }

        }

    }
}
