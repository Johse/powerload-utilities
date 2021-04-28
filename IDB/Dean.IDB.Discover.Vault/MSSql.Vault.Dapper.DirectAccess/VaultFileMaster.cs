using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Windows;
using System.Windows.Forms;

using MSSql.Vault.Dapper.DirectAccess.VaultDbEntities;

using log4net;
using BCPBuilderConfig;


namespace MSSql.Vault.Dapper.DirectAccess
{
    public class VaultFileMaster
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(VaultFileMaster));

        #region MasterFileStatus enum

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
        #endregion MasterFileStatus enum

        // manage the FileMaster table record for this VaultFileMaster
        public FileMaster m_FileMasterRecord { get; private set; }
        public Master m_MasterRecord { get; private set; }
        public CategoryDef m_CategoryDef { get; private set; }

        // manage the folder relationships
        // manage the parents and children
        public VaultFolder m_ParentVaultFolder { get; private set; }

        // manage the VaultFileRevisions for this master
        public List<VaultFileRevision> m_VaultFileRevisionList { get; private set; }

        // manage the status of the file
        public MasterFileStatus mMasterFileStatus { get; private set; }


        // manage the direct FileMaster children, grandchildren, parents, and grandparents
        public List<VaultFileMaster> m_ChildVaultFileMasters { get; private set; }
        public List<VaultFileMaster> m_GrandChildVaultFileMasters { get; private set; }
        public List<VaultFileMaster> m_ParentVaultFileMasters { get; private set; }
        public List<VaultFileMaster> m_GrandParentVaultFileMasters { get; private set; }

        // manage whether the FileMaster had it's hierarchy built
        public bool mRollUpProcessed { get; private set; }
        public long mHierarchyDepth { get; private set; }

        // manage whether the VaultFileMaster is part of the original search list
        // is outside of those original search folders
        public bool bInOriginalFoundList { get; set; }
        public bool bIsParentForAnalysis { get; set; }


        // constructor
        public VaultFileMaster(FileMaster fileMasterRecord, Master masterRecord, CategoryDef categoryDef, VaultFolder parentVaultFolder)
        {
            // assign the properties
            m_FileMasterRecord = fileMasterRecord;
            m_MasterRecord = masterRecord;

            m_CategoryDef = categoryDef;

            m_ParentVaultFolder = parentVaultFolder;

            m_VaultFileRevisionList = new List<VaultFileRevision>();

            mMasterFileStatus = MasterFileStatus.NoStatus;

            // set the lists
            m_ChildVaultFileMasters = new List<VaultFileMaster>();
            m_GrandChildVaultFileMasters = new List<VaultFileMaster>();
            m_ParentVaultFileMasters = new List<VaultFileMaster>();
            m_GrandParentVaultFileMasters = new List<VaultFileMaster>();
            mRollUpProcessed = false;
            mHierarchyDepth = 0;

            bInOriginalFoundList = false;
            bIsParentForAnalysis = false;
        }


        // method to build the VaultFileMasters
        public static bool BuildVaultFileMasters(   Dictionary<long, FileMaster> fileMaster_Dict,
                                                    Dictionary<long, CategoryDef> categoryDefs_Dict,
                                                    Dictionary<long, CategoryOnEntity> categoriesOnEntitiesForFileMasters_Dict,
                                                    Dictionary<long, Master> mastersForFiles,
                                                    Dictionary<long, VaultFolder> vaultFolderDictByFolderID,
                                                    ref Dictionary<long, VaultFileMaster> vaultFileMasterDict)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("BuildVaultFileMasters: Start");

            bool bSuccess = false;

            // iterate each of these and build the masters
            List<VaultFileMaster> vfmList = new List<VaultFileMaster>();
            foreach (FileMaster fm in fileMaster_Dict.Values)
            {
                // get the CategoryDef for the file master
                // may not be defined for the file
                CategoryDef categoryDef = null;
                if (categoriesOnEntitiesForFileMasters_Dict.ContainsKey(fm.FileMasterID))
                {
                    categoryDef = categoryDefs_Dict[categoriesOnEntitiesForFileMasters_Dict[fm.FileMasterID].CategoryDefId];
                }

                Master masterRecord = mastersForFiles[fm.FileMasterID];
                VaultFolder parentVaultFolder = vaultFolderDictByFolderID[fm.FolderId];

                // create the new VaultFileMaster
                VaultFileMaster vfm = new VaultFileMaster(fm, masterRecord, categoryDef, parentVaultFolder);
                vfmList.Add(vfm);
            }

            // create the dictionary
            vaultFileMasterDict = vfmList.ToDictionary(vfm => vfm.m_FileMasterRecord.FileMasterID, vfm => vfm);

            // log total time taken
            Logger.Debug(string.Format("BuildVaultFileMasters(): End {0:n0} {1}", vaultFileMasterDict.Count(), swp.ElapsedTimeString()));

            bSuccess = true;

            return (bSuccess);
        }

        // method to add a VaultFileRevision to the VaultFileMaster
        public void AddVaultFileRevision(VaultFileRevision vaultFileRevision)
        {
            m_VaultFileRevisionList.Add(vaultFileRevision);
        }

        // method to add get all VaultFileIterations
        public List<VaultFileIteration> VaultFileIterationList()
        {
            return (m_VaultFileRevisionList.SelectMany(vfr => vfr.m_VaultFileIterationList).ToList());
        }

        // method to get the file masters of any of the iterations, and relate them
        public static void AssignVaultFileMasterRelationships(IEnumerable<VaultFileMaster> vaultFileMasters)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("AssignVaultFileMasterRelationships: Start");


            // iterate over each and get the filemasters from each VaultFileIteration
            foreach (VaultFileMaster parentVaultFileMaster in vaultFileMasters)
            {
                // get the ALL of the VaultFileIterations, then select their ChildVaultFileIterations, then the childrens VaultFileRevisions, then the Revisions VaultFileMasters
                parentVaultFileMaster.m_ChildVaultFileMasters = parentVaultFileMaster.VaultFileIterationList()
                                                                    .SelectMany(vfi => vfi.m_ChildVaultFileIterationList)
                                                                    .Select(vfi => vfi.m_VaultFileRevision.m_VaultFileMaster).Distinct().ToList();

                // add this parent to the each childs parent list
                parentVaultFileMaster.m_ChildVaultFileMasters.ForEach(vfm => vfm.m_ParentVaultFileMasters.Add(parentVaultFileMaster));
            }

            // get the rolled up information from the children
            // get the child and grandchild status
            foreach (VaultFileMaster vfm in vaultFileMasters)
            {
                vfm.RollUpMasterRelationships();
            }

            // report on the number of circular dependencies
            Logger.Debug(string.Format("Total Circular Dependencies: {0:n0}", vaultFileMasters.Where(vfm => (vfm.mMasterFileStatus & MasterFileStatus.CircularDependency) != MasterFileStatus.NoStatus).Count()));

            // identify if this is top level or bottom level files
            foreach (VaultFileMaster vfm in vaultFileMasters)
            {
                if (vfm.m_ChildVaultFileMasters.Count() == 0)
                {
                    vfm.mMasterFileStatus |= MasterFileStatus.IsLowLevel;
                }

                if (vfm.m_ParentVaultFileMasters.Count() == 0)
                {
                    vfm.mMasterFileStatus |= MasterFileStatus.IsTopLevel;
                }
            }

            // log total time taken
            Logger.Debug(string.Format("AssignVaultFileMasterRelationships(): End {0}", swp.ElapsedTimeString()));
        }

        // method to iterate stack of relationships and roll up master hierarchy
        protected void RollUpMasterRelationships()
        {
            // check to see if it has been processed yet
            if (!mRollUpProcessed)
            {
                // set indicating it has been built
                mRollUpProcessed = true;

                if (this.m_ChildVaultFileMasters.Count() > 0)
                {

                    // process the children
                    foreach (VaultFileMaster vfm in this.m_ChildVaultFileMasters)
                    {
                        vfm.RollUpMasterRelationships();

                        //mChildrenMasterFileStatus |= vfm.mMasterFileStatus;
                        //mGrandChildrenMasterFileStatus |= vfm.mChildrenMasterFileStatus;
                        //mGrandChildrenMasterFileStatus |= vfm.mGrandChildrenMasterFileStatus;

                        this.mHierarchyDepth = Math.Max(this.mHierarchyDepth, vfm.mHierarchyDepth + 1);
                    }

                    // get the grandchildren list
                    this.m_GrandChildVaultFileMasters.AddRange(m_ChildVaultFileMasters.SelectMany(vfm => vfm.m_ChildVaultFileMasters));

                    // check to see if there is a circular dependency
                    List<VaultFileMaster> allGrandChildrenList = m_ChildVaultFileMasters.SelectMany(vfm => vfm.m_GrandChildVaultFileMasters).ToList();
                    if (allGrandChildrenList.Contains(this))
                    {
                        allGrandChildrenList.Remove(this);
                        mMasterFileStatus |= MasterFileStatus.CircularDependency;

                        // log the issue
                        Logger.Debug(string.Format("Circular dependency: {0}", this.GetVaultFileMasterFullPathName()));
                    }

                    this.m_GrandChildVaultFileMasters.AddRange(allGrandChildrenList);
                    this.m_GrandChildVaultFileMasters = this.m_GrandChildVaultFileMasters.Distinct().ToList();
                }
            }
        }


        // get the tip file iteration
        public VaultFileIteration GetTipVaultFileIteration()
        {
            VaultFileIteration tipVaultFileIteration = this.VaultFileIterationList().Last();

            return (tipVaultFileIteration);
        }

        // get the tip file name - including if it is currently renamed
        public string GetVaultFileMasterTipName()
        {
            VaultFileIteration tipVaultFileIteration = GetTipVaultFileIteration();

            string sVaultFileMasterTipName = this.m_FileMasterRecord.TipFileBaseName + Path.GetExtension(tipVaultFileIteration.m_FileIterationExtendedRecord.FileName);

            return (sVaultFileMasterTipName);
        }

        // get the tip file name extension
        public string GetExension()
        {
            VaultFileIteration tipVaultFileIteration = GetTipVaultFileIteration();

            string sExtension = Path.GetExtension(tipVaultFileIteration.m_FileIterationExtendedRecord.FileName);

            return (sExtension);
        }




        // report on the full vault path of the file
        public string GetVaultFileMasterFullPathName()
        {
            string sVaultFileMasterTipName = GetVaultFileMasterTipName();
            return (this.m_ParentVaultFolder.m_FolderRecord.VaultPath + "/" + sVaultFileMasterTipName);
        }


        // set the VaultFileIteration
        // _bCanBePurgedByLifecycleStateName, _bCanBePurgedByControlled, _bCanBePurgedByRevisionPosition, _bCanBePurgedByParentState
        public static  void SetCanBePurgedOnOwnMeritStatuses(IEnumerable<VaultFileMaster> vaultFileMasters)
        {
            // set _bCanBePurgedByLifecycleStateName, _bCanBePurgedByControlled, _bCanBePurgedByRevisionPosition
            foreach (VaultFileMaster vfm in vaultFileMasters)
            {
                vfm.SetCanBePurgedOnOwnMeritStatuses();
            }

            // once the first three have been set
            // iterate the VaultFileIterations and have them identify if the
            // parents can be purged
            // use only those in the primary base folders
            // this will walk up the stack and request parents to be analyzed
            List<VaultFileIteration> vaultFileIterationList = vaultFileMasters.SelectMany(vfm => vfm.VaultFileIterationList()).ToList();
            foreach (VaultFileIteration vfi in vaultFileIterationList)
            {
                vfi.VisitAndSetCanBePurgedByParentState();
            }
        }

        // set the VaultFileIteration
        // _bCanBePurgedByLifecycleStateName, _bCanBePurgedByControlled, _bCanBePurgedByRevisionPosition, _bCanBePurgedByParentState
        public void SetCanBePurgedOnOwnMeritStatuses()
        {
            // set the _bCanBePurgedByLifecycleStateName and _bCanBePurgedByControlled
            foreach (VaultFileIteration vfi in this.VaultFileIterationList())
            {
                vfi.SetCanBePurgedOnOwnMeritStatuses();
            }

            // set the _bCanBePurgedByRevisionPosition
            foreach (VaultFileRevision vfr in this.m_VaultFileRevisionList)
            {
                vfr.SetCanBePurgedByRevisionPosition();
            }

        }

    }
}
