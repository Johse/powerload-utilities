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

    public enum RenamedStateEnum
    {
        NotAnalyzed, No, Yes
    }

    public enum CREOFamilyFileTypeEnum
    {
        NonFamily,
        Instance,
        Nested,
        Generic,
        FamilyTableEmpty
    }

    [Flags]
    public enum CREOFileStatus : ulong
    {
        NoStatus = 0,

        // file is renamed
        Renamed = 1L << 0,

        // renamed and has parents needing resolution, from file up the lineage of parents
        WParNdngRes = 1L << 1,
        WFamParNdngRes = 1L << 2,

        // family file having renamed instances or generator
        FamHasRenInst = 1L << 3,
        FamHasRenDirInst = 1L << 4,
        FamHasRenGeneric = 1L << 5,
        FamHasRenDirGeneric = 1L << 6,

        // children states
        HasRenNonFamChld = 1L << 7,
        HasRenDirNonFamChld = 1L << 8,
        HasRenFamChld = 1L << 9,
        HasRenDirFamChld = 1L << 10,
        HasMsclsfdChld = 1L << 11,
        HasMsclsfdDirChld = 1L << 12,
        HasChldDirInstance = 1L << 13,
        HasChldInstance = 1L << 14,
        HasChldDirNested = 1L << 15,
        HasChldNested = 1L << 16,
        HasChldDirGeneric = 1L << 17,
        HasChldGeneric = 1L << 18,
        PointsToMultGenerics = 1L << 19,
        IsMisclassified = 1L << 20,

        // top level/lowest level
        IsTopLevel = 1L << 21,
        IsLowLevel = 1L << 22,

        // has a drawing or dwf associated with it
        HasDrawing = 1L << 23,
        HasDwf = 1L << 24,

        IsNonFamily = 1L << 25,
        IsInstance = 1L << 26,
        IsNested = 1L << 27,
        IsGeneric = 1L << 28,
        HasEmptyFamilyTable = 1L << 29,

        // is dependent on a file that points to multiple generics
        HasChldDirPntgToMultGen = 1L << 30,
        HasChldDPntgToMultGen = 1L << 31,

        // is found in the ToId of the Superceded associations
        SupercededAsFamily = 1L << 32,

        // has a renamed child or grandchild and has been checked in before the rename
        NeedsResolution = 1L << 33

    }


    public class VaultFileMasterOld : IEquatable<VaultFileMasterOld>
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(VaultFileMasterOld));


        static public string VaultFileStoreRoot { get; set; }
        static public string WorkspaceRoot { get; set; }

        // properties from db entities
        public FileMaster mFileMaster { get; private set; }
        public long mFileMasterID;
        public Master mMaster { get; private set; }
        public CategoryOnEntity mCategoryOnEntity { get; private set; }
        public Folder mFolder { get; private set; }
        public List<FileResource> mFileResourceList { get; private set; }
        public List<FileIterationExtended> mFileIterExtendList { get; private set; }
        public List<Iteration> mIterationList { get; private set; }
        public List<Revision> mRevisionList { get; private set; }
        public List<Entity> mEntityList { get; private set; }

        // properties specific to file iterations
        public List<Property> mFileIterationPropertyList;
        public List<PropertyDef> mFileIterationPropertyDefList;


        public FileIterationExtended mTipCommittedFileIterExtended { get; private set; }
        public string FileName { get; private set; }
        public string Extension { get; private set; }

        public string VaultFullFilePathName { get; private set; }


        // family table information
        public CREOFamilyFileTypeEnum mFamilyType { get; private set; }

        public bool mPointsToMoreThanOneGeneric = false;
        public long mOriginalClassification;
        public long mUpdateClassification = 0;
        public bool bMisclassified = false;
        public bool HasMisclassifiedChildren = false;

        public List<VaultFileMasterOld> MisclassifiedVFMChildren { get; private set; }

        // file status information
        public CREOFileStatus mCREOFileStatus { get; set; }


        // property to manage how family generics, instances, and nested
        public int mFamilyDepth { get; private set; }
        public int mFamilyMaxDepth { get; private set; }
        public List<VaultFileMasterOld> RolledUpVFMInstances { get; private set; }
        public List<VaultFileMasterOld> DirectVFMInstances { get; private set; }
        public List<VaultFileMasterOld> FamilyVFMGenericsPath { get; private set; }
        public VaultFileMasterOld DirectFamilyVFMGeneric { get; private set; }
        public VaultFileMasterOld TopVFMGeneric { get; private set; }


        // maintain information on if it has been renamed
        public RenamedStateEnum RenamedState { get; private set; }
        public long RenameUser { get; private set; }
        public DateTime RenamedDate { get; private set; }
        public string OldName { get; private set; }

        // information for parent of a renamed component
        public bool HasRenamedChildren { get; private set; }
        public int mChildMaxDepth { get; private set; }

        public List<VaultFileMasterOld> RolledUpVFMChildren { get; private set; }

        public List<VaultFileMasterOld> DirectVFMChildren { get; private set; }

        public int mParentMaxDepth { get; private set; }

        public List<VaultFileMasterOld> RolledUpVFMParents { get; private set; }
        public List<VaultFileMasterOld> DirectVFMParents { get; private set; }
        public List<VaultFileMasterOld> RenamedOutOfTurnVFMChildren { get; private set; }

        public bool mChildrenAnalayzed { get; private set; }



        // constructor for the VaultFileMasterOld
        public VaultFileMasterOld(FileMaster fileMaster, VaultCacheOld vaultCache, bool bForFileDiscovery = false)
        {
            RenamedState = RenamedStateEnum.NotAnalyzed;

            // assign all of the content
            mFileMaster = fileMaster;
            mOriginalClassification = fileMaster.FileClassification;
            mFileMasterID = fileMaster.FileMasterID;

            mFolder = vaultCache.GetFolderById(mFileMaster.FolderId);
            mMaster = vaultCache._mastersForFiles[mFileMaster.FileMasterID];
            if (!bForFileDiscovery)
            {
                mCategoryOnEntity = vaultCache._categoriesOnEntitiesForFiles[mFileMaster.FileMasterID];

                // get the Iteration list
                mIterationList = vaultCache._fileIterationsByMasterId[mFileMaster.FileMasterID].ToList();

                // get the Revision list
                mRevisionList = vaultCache._fileRevisionsByMasterId[mFileMaster.FileMasterID].ToList();

                // setup the file resource list
                mFileResourceList = vaultCache._fileResourcesByMasterId[mFileMaster.FileMasterID].ToList();

                // get the Entity list
                mEntityList = vaultCache._fileEntitiesByMasterId[mFileMaster.FileMasterID].ToList();

                // properties specific to file iterations
                //mFileIterationPropertyList = vaultCache._propertiesByFileMasterId[mFileMaster.FileMasterID].ToList();
                //mFileIterationPropertyDefList = vaultCache._propertDefsByFileMasterId[mFileMaster.FileMasterID].Distinct().ToList();
            }

            // get the FileIteration list
            mFileIterExtendList = vaultCache._fileIterExtndsByMasterId[mFileMaster.FileMasterID].ToList();

            // get the tip committed version
            mTipCommittedFileIterExtended = mFileIterExtendList.FirstOrDefault(fi => fi.FileIterationId == mMaster.TipCommittedIterationID);

            // TODO: once in a while the TipCommittedIterationID does not match any of the iterations of the file
            if (mTipCommittedFileIterExtended == null)
            {
                mTipCommittedFileIterExtended = mFileIterExtendList.LastOrDefault();
            }



            // must use the mFileMaster.TipFileBaseName
            Extension = Path.GetExtension(mTipCommittedFileIterExtended.FileName);
            FileName = mFileMaster.TipFileBaseName + Extension;
            VaultFullFilePathName = mFolder.VaultPath + "/" + FileName;
            mFamilyType = CREOFamilyFileTypeEnum.NonFamily;
            mCREOFileStatus = CREOFileStatus.NoStatus;

            mChildrenAnalayzed = false;
        }

        #region implement IEquatable interfaces


        public override string ToString()
        {
            return this.FileName + "\t" + this.VaultFullFilePathName;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            VaultFileMasterOld objAsVaultFileMaster = obj as VaultFileMasterOld;
            if (objAsVaultFileMaster == null) return false;
            else return Equals(objAsVaultFileMaster);
        }
        public override int GetHashCode()
        {
            return (int)this.mMaster.MasterID;
        }
        public bool Equals(VaultFileMasterOld other)
        {
            if (other == null) return false;
            return (this.mMaster.MasterID.Equals(other.mMaster.MasterID));
        }

        #endregion implement IEquatable interfaces



        /// <summary>
        /// calculate the file store location
        /// </summary>
        /// <param name="vaultFiles">The file in question</param>
        /// <returns>The path to the file</returns>
        public string WorkspaceFolderAndFileName()
        {
            return (this.VaultFullFilePathName.Replace("$/", WorkspaceRoot).Replace("/", "\\"));
        }

        /// <summary>
        /// calculate the file store location
        /// </summary>
        /// <param name="vaultFiles">The file in question</param>
        /// <returns>The path to the file</returns>
        public string FileStoreFolderAndFileName()
        {
            // get the resource associated with this.mTipCommittedFileIterExtended
            FileResource fileResource = mFileResourceList.FirstOrDefault(fr => fr.ResourceId == this.mTipCommittedFileIterExtended.ResourceId);

            return (FileStoreFolderAndFileName(fileResource));
        }




        /// <summary>
        /// calculate the file store location
        /// </summary>
        /// <param name="vaultFiles">The file in question</param>
        /// <returns>The path to the file</returns>
        public string FileStoreFolderAndFileName(FileIterationExtended fileIteration)
        {
            // get the resource associated with this FileIteration
            FileResource fileResource = mFileResourceList.FirstOrDefault(fr => fr.ResourceId == fileIteration.ResourceId);

            return (FileStoreFolderAndFileName(fileResource));
        }

        /// <summary>
        /// calculate the file store location
        /// </summary>
        /// <param name="vaultFiles">The file in question</param>
        /// <returns>The path to the file</returns>
        public string FileStoreFolderAndFileName(FileResource fileResource)
        {
            // TODO: efficiently check to make sure that the path does not end with the path separator already
            string sFileStoreFile = VaultFileStoreRoot + "\\" + FileStoreFolderName() + "\\" + FileStoreFileName(fileResource);

            return (sFileStoreFile);
        }



        /// <summary>
        /// get the file store folder name for the file
        /// </summary>
        /// <param name="vaultFiles">the file in question</param>
        /// <returns>the folder in the file stor</returns>
        protected string FileStoreFolderName()
        {
            // build a string with 18 characters padded by 000
            long nPrimaryFolder = mFileMaster.FileMasterID / 1000;
            string paddedString = nPrimaryFolder.ToString("D18");

            string sFolderName = "";
            int[] indexArray = new[] { 0, 3, 6, 9, 12, 15 };
            foreach (int nIndex in indexArray)
            {
                sFolderName += paddedString.Substring(nIndex, 3) + "\\";
            }

            sFolderName += mFileMaster.FileMasterID.ToString();

            return (sFolderName);
        }


        /// <summary>
        /// get the filestore filename
        /// </summary>
        /// <param name="vaultFiles">The file in question</param>
        /// <returns>The mangled file name</returns>
        protected string FileStoreFileName(FileResource fileResource)
        {
            string sFileName = mFileMaster.FileMasterID.ToString() + "_" + fileResource.ResourceId.ToString() + "_" + fileResource.Version.ToString() + "." + fileResource.Extension;

            return (sFileName);
        }


        // method to get all filestore filenames
        static public List<string> GetAllFileStoreFolderAndFileNames(List<VaultFileMasterOld> vaultFileMasterList)
        {
            List<string> fileStoreFolderAndFileNames = new List<string>();

            // iterate through the list of VaultFileMasterOld
            foreach (VaultFileMasterOld vfm in vaultFileMasterList)
            {
                fileStoreFolderAndFileNames.AddRange(vfm.GetAllFileStoreFolderAndFileNames());
            }

            return (fileStoreFolderAndFileNames);
        }

        // method to get all filestore filenames
        public List<string> GetAllFileStoreFolderAndFileNames()
        {
            List<string> fileStoreFolderAndFileNames = new List<string>();

            // iterate through the list of VaultFileMasterOld
            foreach (FileResource fr in mFileResourceList)
            {
                fileStoreFolderAndFileNames.Add(FileStoreFolderAndFileName(fr));
            }

            return (fileStoreFolderAndFileNames);
        }

        // method to get all unique users who created files
        static public List<long> GetCreatedByUserIds(List<VaultFileMasterOld> vaultFileMasterList)
        {
            List<long> createdByUserIds = new List<long>();

            // iterate through the list of VaultFileMasterOld
            foreach (VaultFileMasterOld vfm in vaultFileMasterList)
            {
                createdByUserIds.AddRange(vfm.GetCreatedByUserIds());
            }

            return (createdByUserIds.Distinct().ToList());
        }


        // method to get all unique users who created files
        public List<long> GetCreatedByUserIds()
        {
            return (mEntityList.Select(ent => ent.CreateUserID.Value).Distinct().ToList());

            //List<long> createdByUserIds = new List<long>();

            //// iterate through each entity and get the created by user
            //foreach (Entity ent in mEntityList)
            //{
            //    createdByUserIds.Add(ent.CreateUserId);
            //}

            //return (createdByUserIds.Distinct().ToList());
        }


        // CategoryDefinition
        // method to get all unique category definition IDs
        static public List<long> GetMasterCategoryDefIds(List<VaultFileMasterOld> vaultFileMasterList)
        {
            List<long> categoryDefIds = new List<long>();

            // iterate through the list of VaultFileMasterOld
            foreach (VaultFileMasterOld vfm in vaultFileMasterList)
            {
                categoryDefIds.Add(vfm.mCategoryOnEntity.CategoryDefId);
            }

            return (categoryDefIds.Distinct().ToList());
        }


        // PropertyDefinition
        // method to get all unique property definition IDs
        static public List<long> GetPropertyDefIds(List<VaultFileMasterOld> vaultFileMasterList)
        {
            List<long> propertyDefIds = new List<long>();

            // iterate through the list of VaultFileMasterOld
            foreach (VaultFileMasterOld vfm in vaultFileMasterList)
            {
            }

            return (propertyDefIds.Distinct().ToList());
        }


        // LifecycleDefinition
        // method to get all unique life cycle definition IDs
        static public List<long> GetLifecycleDefIds(List<VaultFileMasterOld> vaultFileMasterList, VaultCacheOld vaultCache)
        {
            List<long> lifecycleStateIds = GetLifecycleStateIds(vaultFileMasterList);

            // get the LifeCycleState objects from the VaultCacheOld
            var lifeCycleDefIds = vaultCache._lifeCycleStates.Where(kvp => lifecycleStateIds.Contains(kvp.Key)).Select(kvp => kvp.Value.LifeCycleDefId);

            return (lifeCycleDefIds.Distinct().ToList());
        }


        // LifeCycleState
        // method to get all unique life cycle state IDs
        static public List<long> GetLifecycleStateIds(List<VaultFileMasterOld> vaultFileMasterList)
        {
            List<long> lifecycleStateIds = new List<long>();

            // iterate through the list of VaultFileMasterOld
            foreach (VaultFileMasterOld vfm in vaultFileMasterList)
            {
                lifecycleStateIds.AddRange(vfm.GetLifecycleStateIds());
            }

            return (lifecycleStateIds.Distinct().ToList());
        }

        // LifeCycleState
        // method to get all unique life cycle state IDs
        public List<long> GetLifecycleStateIds()
        {
            // TODO: find workaround with new content
            // return (mRevisionList.Where(rev => rev.LifeCycleStateId != null).Select(rev => rev.LifeCycleStateId.Value).Distinct().ToList());
            return (null);
        }



        // RevisionDefinition
        // method to get all unique revision definition IDs
        static public List<long> GetRevisionDefIds(List<VaultFileMasterOld> vaultFileMasterList)
        {
            List<long> revisionDefIds = new List<long>();

            // iterate through the list of VaultFileMasterOld
            foreach (VaultFileMasterOld vfm in vaultFileMasterList)
            {
                revisionDefIds.AddRange(vfm.GetRevisionDefIds());
            }

            return (revisionDefIds.Distinct().ToList());
        }

        // RevisionDefinition
        // method to get all unique revision definition IDs
        public List<long> GetRevisionDefIds()
        {
            return (mRevisionList.Where(rev => rev.RevisionDefinitionId != null).Select(rev => rev.RevisionDefinitionId.Value).Distinct().ToList());

            //List<long> revisionDefIds = new List<long>();

            //// iterate through the list of Revisions
            //foreach (Revision rev in mRevisionList)
            //{
            //    revisionDefIds.Add(rev.RevisionDefinitionId);
            //}

            //return (revisionDefIds.Distinct().ToList());
        }


        // things that may not be within VaultBCP capabilities

        // EntityClassId
        // method to get all unique EntityClassId
        static public List<long> GetEntityClassIds(List<VaultFileMasterOld> vaultFileMasterList)
        {
            List<long> entityClassIds = new List<long>();

            // iterate through the list of VaultFileMasterOld
            foreach (VaultFileMasterOld vfm in vaultFileMasterList)
            {
                entityClassIds.AddRange(vfm.GetRevisionDefIds());
            }

            return (entityClassIds.Distinct().ToList());
        }

        // EntityClassId
        // method to get all unique EntityClassId
        public List<long> GetEntityClassIds()
        {
            return (mEntityList.Select(ent => ent.EntityClassID).Distinct().ToList());

            //List<long> entityClassIds = new List<long>();

            //// iterate through the list of VaultFileMasterOld
            //foreach (Entity ent in mEntityList)
            //{
            //    entityClassIds.Add(ent.EntityClassId);
            //}

            //return (entityClassIds.Distinct().ToList());
        }

        // vfm.mFileIterExtendList.First().LifeCycleStateName
        // LifeCycleStateName
        // method to get all unique LifeCycleStateName
        static public List<string> GetLifeCycleStateNames(List<VaultFileMasterOld> vaultFileMasterList)
        {
            List<string> lifeCycleStateNames = new List<string>();

            // iterate through the list of VaultFileMasterOld
            foreach (VaultFileMasterOld vfm in vaultFileMasterList)
            {
                lifeCycleStateNames.AddRange(vfm.GetLifeCycleStateNames());
            }

            return (lifeCycleStateNames.Distinct().ToList());
        }

        // vfm.mFileIterExtendList.First().LifeCycleStateName
        // LifeCycleStateName
        // method to get all unique LifeCycleStateName
        public List<string> GetLifeCycleStateNames()
        {
            return (mFileIterExtendList.Select(fi => fi.LifeCycleStateName).Distinct().ToList());
        }


        // vfm.mFileMaster.FileClassification (long)
        // vfm.mFileMaster.FileStatus (int)

        #region child rename analysis

        // check to see if this has been renamed
        public bool BeenRenamed()
        {
            // check to see if it has been tested
            if (RenamedState == RenamedStateEnum.NotAnalyzed)
            {
                // parse through each iteration in reverse order and see if the names changed
                Dictionary<int, Iteration> iterationDictionary = mIterationList.ToDictionary(it => it.IterationNumber, it => it);
                List<int> interationNumberList = mIterationList.Select(it => it.IterationNumber).ToList();
                interationNumberList.Sort();

                // check to see if the file has been renamed within Vault - FileMaster.TipFileBaseName is different than mTipCommittedFileIterExtended
                DateTime lastDateTime = mTipCommittedFileIterExtended.CheckinDate.Value;
                long lastCheckinUser = this.mEntityList.FirstOrDefault(ent => ent.EntityId == mTipCommittedFileIterExtended.FileIterationId).CreateUserID.Value;
                Iteration iteration = null;
                FileIterationExtended fileIteration = null;
                if (string.Compare(this.FileName, this.mTipCommittedFileIterExtended.FileName, true) != 0)
                {
                    iteration = iterationDictionary[interationNumberList.Last()];
                    fileIteration = mFileIterExtendList.FirstOrDefault(fi => fi.FileIterationId == iteration.IterationID);

                    // set the last DateTime
                    if (fileIteration.CheckinDate != null)
                    {
                        lastDateTime = fileIteration.CheckinDate.Value;
                    }

                    lastCheckinUser = this.mEntityList.FirstOrDefault(ent => ent.EntityId == fileIteration.FileIterationId).CreateUserID.Value;
                    OldName = this.mTipCommittedFileIterExtended.FileName;

                    // set the rename state so that we can generate the info
                    RenamedState = RenamedStateEnum.Yes;
                }
                else
                {
                    // parse through the iterations and find the last rename
                    for (int nIndex = interationNumberList.Count() - 1; nIndex >= 0; nIndex--)
                    {
                        int itNum = interationNumberList[nIndex];
                        iteration = iterationDictionary[itNum];
                        fileIteration = mFileIterExtendList.FirstOrDefault(fi => fi.FileIterationId == iteration.IterationID);

                        // check to see if the name changed
                        if (string.Compare(fileIteration.FileName, mTipCommittedFileIterExtended.FileName, true) != 0)
                        {
                            RenamedState = RenamedStateEnum.Yes;
                            OldName = fileIteration.FileName;
                            break;
                        }

                        // set the last DateTime
                        if (fileIteration.CheckinDate != null)
                        {
                            lastDateTime = fileIteration.CheckinDate.Value;
                        }

                        lastCheckinUser = this.mEntityList.FirstOrDefault(ent => ent.EntityId == fileIteration.FileIterationId).CreateUserID.Value;
                    }
                }

                // check the status
                if (RenamedState == RenamedStateEnum.Yes)
                {
                    RenameUser = lastCheckinUser;
                    RenamedDate = lastDateTime;

                    this.mCREOFileStatus |= CREOFileStatus.Renamed;


                    // Displays 4/10/2008 6:30 AM                       
                    //string logString = string.Format("File Renamed: {0} (On {1}) (Original Name: {2})",
                    //                                    this.FileName, RenamedDate.ToString("g", CultureInfo.CreateSpecificCulture("en-us")), OldName);
                    //logString += Environment.NewLine;

                    //logString += string.Format("Found at: {0}", this.VaultFullFilePathName);
                    //logString += Environment.NewLine;

                    //// way to not display to the screen
                    //// TODO: figure out a way to implement fields in Interfaces
                    //((LogExtender)Logger).m_ILog.Debug(logString);
                }
                else
                {
                    RenamedState = RenamedStateEnum.No;
                }
            }


            return (RenamedState == RenamedStateEnum.Yes);
        }

        // method to build relationships
        // method to analyze for Solidworks Family table parts, instances, and generics
        //-- finding generics
        //-- generics can be children of assemblies
        //-- generics ARE children of their instances
        //-- nested are both instances and generics, they will be both in a FromId and a ToId
        //-- are pointed to by parents that have "fa.Source like 'FamTableRelation:%'"
        //-- can also be pointed to by parents that "don't have "fa.Source like 'FamTableRelation:%'"
        //-- don't point to any children as "fa.Source like 'FamTableRelation:%'"
        // we assume that all of the file relationships have already been obtained, superceded have been found, etc
        static public void BuildVaultFileMasterRelationships(VaultCacheOld vaultCache)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug(string.Format("VaultFileMasterOld.BuildVaultFileMasterRelationships(): Start"));


            // get the FromMasterId associated with the "To" VaultFileMasterOld
            // by creating an anonymous object dynamically to facilitate the ToLookup on resultant list
            var fromMasterIdAndToVaultFileMasterList = (from fileAssocExt in vaultCache._fileAssociationsExtended
                                                        join toFMKVP in vaultCache._vaultFileMastersByFileMasterIdDictionary
                                                        on fileAssocExt.ToMasterID equals toFMKVP.Key
                                                        select new { fromMasterId = fileAssocExt.FromMasterID, toVaultFileMaster = toFMKVP.Value });


            // create the lookup
            ILookup<long, VaultFileMasterOld> toVaultFileMastersByFromFileMasterId = fromMasterIdAndToVaultFileMasterList.Distinct().ToLookup(kvp => kvp.fromMasterId, kvp => kvp.toVaultFileMaster);
            Logger.Debug(string.Format("toVaultFileMastersByFromFileMasterId ILookup: {0:n0}", toVaultFileMastersByFromFileMasterId.Count));


            Logger.Debug(string.Format("VaultFileMasterOld.BuildVaultFileMasterRelationships(): Prep Complete {0}", swp.LapTimeString()));


            // parse through the list and analyze children/etc.
            // setup a dictionary to hold rolled up children for a top level parent
            Dictionary<long, List<VaultFileMasterOld>> rolledUpVFMChildrenForVFMParentMasterId = new Dictionary<long, List<VaultFileMasterOld>>();
            foreach (VaultFileMasterOld vaultFileMaster in vaultCache._vaultFileMasters)
            {
                // don't re-analze file
                if (!vaultFileMaster.mChildrenAnalayzed)
                {
                    vaultFileMaster.RecurseAndBuildVaultFileMasterRelationships(vaultCache, rolledUpVFMChildrenForVFMParentMasterId, toVaultFileMastersByFromFileMasterId);
                }
            }


            Logger.Debug(string.Format("VaultFileMasterOld.BuildVaultFileMasterRelationships(): Total Time {0}", swp.ElapsedTimeString()));
        }

        // method to build relationships
        // method to analyze for Solidworks Family table parts, instances, and generics
        //-- finding generics
        //-- generics can be children of assemblies
        //-- generics ARE children of their instances
        //-- nested are both instances and generics, they will be both in a FromId and a ToId
        //-- are pointed to by parents that have "fa.Source like 'FamTableRelation:%'"
        //-- can also be pointed to by parents that "don't have "fa.Source like 'FamTableRelation:%'"
        //-- don't point to any children as "fa.Source like 'FamTableRelation:%'"
        // we assume that all of the file relationships have already been obtained, superceded have been found, etc
        public void RecurseAndBuildVaultFileMasterRelationships(VaultCacheOld vaultCache,
                                                                Dictionary<long, List<VaultFileMasterOld>> rolledUpVFMChildrenForVFMParentMasterId,
                                                                ILookup<long, VaultFileMasterOld> toVaultFileMastersByFromFileMasterId)
        {
            StopwatchPlus swp = new StopwatchPlus();


            // add a place holder so that this does not get analyzed within itself on a circular dependency
            if (!this.mChildrenAnalayzed)
            {
                this.mChildrenAnalayzed = true;

                rolledUpVFMChildrenForVFMParentMasterId.Add(this.mFileMasterID, new List<VaultFileMasterOld>());

                // break when we visit the following file for testing
                //if ((string.Compare(this.FileName, "72542.sldasm", true) == 0) ||
                //        (string.Compare(this.FileName, "Screw.iam", true) == 0))
                //{
                //}

                // check to see if it has been analyzed
                BeenRenamed();

                // iterate over all of the children
                DirectVFMChildren = new List<VaultFileMasterOld>();
                mChildMaxDepth = 0;
                if (toVaultFileMastersByFromFileMasterId.Contains(this.mFileMasterID))
                {
                    DirectVFMChildren.AddRange(toVaultFileMastersByFromFileMasterId[this.mFileMasterID]);
                }

                RolledUpVFMChildren = new List<VaultFileMasterOld>();
                RolledUpVFMChildren.AddRange(DirectVFMChildren);
                foreach (VaultFileMasterOld childVaultFileMaster in DirectVFMChildren)
                {
                    childVaultFileMaster.RecurseAndBuildVaultFileMasterRelationships(vaultCache, rolledUpVFMChildrenForVFMParentMasterId, toVaultFileMastersByFromFileMasterId);
                    RolledUpVFMChildren.AddRange(rolledUpVFMChildrenForVFMParentMasterId[childVaultFileMaster.mFileMasterID]);

                    mChildMaxDepth = Math.Max(mChildMaxDepth, childVaultFileMaster.mChildMaxDepth + 1);
                }


                // remove any duplicates
                RolledUpVFMChildren = RolledUpVFMChildren.Distinct().ToList();
                DirectVFMChildren = DirectVFMChildren.Distinct().ToList();

                // set the value in the dictionary
                rolledUpVFMChildrenForVFMParentMasterId[this.mFileMasterID] = RolledUpVFMChildren;

                // get this resource checkin date
                // some files tip committed iteration is missing?
                DateTime resourceCheckinDate;
                if (this.mEntityList.Where(ent => ent.EntityId == this.mTipCommittedFileIterExtended.ResourceId).Any())
                {
                    resourceCheckinDate = this.mEntityList.Where(ent => ent.EntityId == this.mTipCommittedFileIterExtended.ResourceId).Last().CreateDate.Value;
                }
                else
                {
                    resourceCheckinDate = this.mEntityList.Last().CreateDate.Value;
                }

                // check to see if any of the children are renamed and the dates are newer
                RenamedOutOfTurnVFMChildren = new List<VaultFileMasterOld>();
                foreach (VaultFileMasterOld childVaultFileMaster in RolledUpVFMChildren)
                {
                    // use mod date on the file as that would have updated it to point to children at the time
                    if (childVaultFileMaster.BeenRenamed() && (childVaultFileMaster.RenamedDate > resourceCheckinDate))
                    {
                        // add the child to the renamed list
                        RenamedOutOfTurnVFMChildren.Add(childVaultFileMaster);

                        this.HasRenamedChildren = true;

                        //string logString = string.Format("Parent Vault Rename: {0} {1} (Moddate in {2})",
                        //                                    this.FileName, this.mFileMasterID, this.mTipCommittedFileIterExtended.ModDate.ToString("g", CultureInfo.CreateSpecificCulture("en-us")));
                        //logString += Environment.NewLine;

                        //logString += string.Format("Child Renamed: {0} {1} (On {2}) (Original Name: {3})",
                        //                            childVaultFileMaster.FileName, childVaultFileMaster.mFileMasterID, childVaultFileMaster.RenamedDate.ToString("g", CultureInfo.CreateSpecificCulture("en-us")), childVaultFileMaster.OldName);
                        //logString += Environment.NewLine;

                        //logString += string.Format("Parent found at: {0}", this.VaultFullFilePathName);
                        //logString += Environment.NewLine;
                        //logString += string.Format("Child found at:  {0}", childVaultFileMaster.VaultFullFilePathName);
                        //logString += Environment.NewLine;

                        //((LogExtender)Logger).m_ILog.Debug(logString);
                    }
                }
            }


        }


        // method to get rolled up parents list
        static public void RecurseAndIdentifyRolledUpParentsOld(VaultCacheOld vaultCache)
        {
            StopwatchPlus swp = new StopwatchPlus();

            // get the iterations dictionary
            Dictionary<long, VaultFileMasterOld> vaultFileMastersByFileIterationId = GetVaultFileMastersByFileIterationId(vaultCache._vaultFileMasters);

            // setup a dictionary to hold rolled up parents
            Dictionary<long, List<VaultFileMasterOld>> rolledUpVFMParentsForVFMChildMasterId = new Dictionary<long, List<VaultFileMasterOld>>();

            // parse through the list and analyze children/etc.
            foreach (VaultFileMasterOld vaultFileMaster in vaultCache._vaultFileMasters)
            {
                // don't re-analze file
                if (!rolledUpVFMParentsForVFMChildMasterId.ContainsKey(vaultFileMaster.mFileMasterID))
                {
                    vaultFileMaster.RecurseAndIdentifyRolledUpParentsOld(vaultFileMastersByFileIterationId,
                                                                        rolledUpVFMParentsForVFMChildMasterId,
                                                                        vaultCache);

                    Logger.Debug(string.Format("vaultFileMaster.RecurseAndIdentifyRolledUpParents: {0} {1}", swp.LapTimeString(), vaultFileMaster.VaultFullFilePathName));
                }
            }

            Logger.Debug(string.Format("vaultFileMaster.RecurseAndIdentifyRolledUpParents(): Total Time {0}", swp.LapTimeString()));
        }

        // method to recurse up through the parent list to get all children
        protected List<VaultFileMasterOld> RecurseAndIdentifyRolledUpParentsOld(Dictionary<long, VaultFileMasterOld> vaultFileMastersByFileIterationId,
                                                                            Dictionary<long, List<VaultFileMasterOld>> rolledUpVFMParentsForVFMChildMasterId,
                                                                            VaultCacheOld vaultCache)
        {
            // add a place holder so that this does not get analyzed within itself on a circular dependency
            if (!rolledUpVFMParentsForVFMChildMasterId.ContainsKey(this.mFileMasterID))
            {
                rolledUpVFMParentsForVFMChildMasterId.Add(this.mFileMasterID, new List<VaultFileMasterOld>());

                // iterate over all of the parents
                IEnumerable<FileIterationExtended> parentFileIterationsList = vaultCache.FindParents(this.mTipCommittedFileIterExtended);
                RolledUpVFMParents = new List<VaultFileMasterOld>();
                DirectVFMParents = new List<VaultFileMasterOld>();
                foreach (FileIterationExtended parentFileIteration in parentFileIterationsList)
                {
                    VaultFileMasterOld parentVaultFileMaster = vaultFileMastersByFileIterationId[parentFileIteration.FileIterationId];
                    RolledUpVFMParents.Add(parentVaultFileMaster);
                    DirectVFMParents.Add(parentVaultFileMaster);

                    RolledUpVFMParents.AddRange(parentVaultFileMaster.RecurseAndIdentifyRolledUpParentsOld(vaultFileMastersByFileIterationId,
                                                                                                    rolledUpVFMParentsForVFMChildMasterId,
                                                                                                    vaultCache));
                }


                // remove any duplicates
                RolledUpVFMParents = RolledUpVFMParents.Distinct().ToList();
                DirectVFMParents = DirectVFMParents.Distinct().ToList();

                // set the value in the dictionary
                rolledUpVFMParentsForVFMChildMasterId[this.mFileMasterID] = RolledUpVFMParents;
            }

            // return the rolled up parents
            return (rolledUpVFMParentsForVFMChildMasterId[this.mFileMasterID]);
        }


        // get the VaultFileMasters that are CREOFamilyFileTypeEnum.NonFamily but are misclassified
        static protected void ValidateNonFamilyClassification(VaultCacheOld vaultCache)
        {
            // get the files
            List<VaultFileMasterOld> misclassifiedNonFamilyVFMList = vaultCache._vaultFileMasters
                                                                    .Where(vfm => ((vfm.mFamilyType == CREOFamilyFileTypeEnum.NonFamily) &&
                                                                                ((vfm.mOriginalClassification == 3) || (vfm.mOriginalClassification == 4)))).ToList();

            // set the misclassification flag
            misclassifiedNonFamilyVFMList.ForEach(vfm => vfm.bMisclassified = true);
            misclassifiedNonFamilyVFMList.ForEach(vfm => vfm.mCREOFileStatus |= CREOFileStatus.IsMisclassified);
        }




        // method to analyze for Solidworks Family table parts, instances, and generics
        //-- finding generics
        //-- generics can be children of assemblies
        //-- generics ARE children of their instances
        //-- nested are both instances and generics, they will be both in a FromId and a ToId
        //-- are pointed to by parents that have "fa.Source like 'FamTableRelation:%'"
        //-- can also be pointed to by parents that "don't have "fa.Source like 'FamTableRelation:%'"
        //-- don't point to any children as "fa.Source like 'FamTableRelation:%'"
        static public void AnalyzeCREOFamilyTableInstancesAndGenerics(VaultCacheOld vaultCache, bool bGetAllGenerics)
        {
            StopwatchPlus swp = new StopwatchPlus();

            // get all of the file associations where Source contains 'FamTableRelation:%'
            // this list has already been filtered on by superceded relations and only taking the latest Tip committed for non family relations
            List<FileAssociationExtended> familyAssocations = vaultCache._fileAssociationsExtended.Where(dct => ((dct.Source != null) &&
                        (dct.Source.IndexOf("FamTableRelation:", StringComparison.CurrentCultureIgnoreCase) == 0))).Select(dct => dct).ToList();


            List<long> fromMasterIds = familyAssocations.Select(fa => fa.FromMasterID).Distinct().ToList();
            List<long> toMasterIds = familyAssocations.Select(fa => fa.ToMasterID).Distinct().ToList();

            // select those that are FromId and ToId
            List<VaultFileMasterOld> fromVaultFileMasterList = fromMasterIds.Select(mi => vaultCache._vaultFileMastersByFileMasterIdDictionary[mi]).Distinct().ToList();

            List<VaultFileMasterOld> toVaultFileMasterList = toMasterIds.Select(mi => vaultCache._vaultFileMastersByFileMasterIdDictionary[mi]).Distinct().ToList();

            // get the generics, top level, and nested family parts
            List<VaultFileMasterOld> genericVaultFileMasterList = toVaultFileMasterList.Except(fromVaultFileMasterList).ToList();
            List<VaultFileMasterOld> instanceVaultFileMasterList = fromVaultFileMasterList.Except(toVaultFileMasterList).ToList();
            List<VaultFileMasterOld> nestedVaultFileMasterList = fromVaultFileMasterList.Except(instanceVaultFileMasterList).ToList();

            // set the family property on each object
            SetFamilyTypeAndClassifications(genericVaultFileMasterList, CREOFamilyFileTypeEnum.Generic);
            SetFamilyTypeAndClassifications(instanceVaultFileMasterList, CREOFamilyFileTypeEnum.Instance);
            SetFamilyTypeAndClassifications(nestedVaultFileMasterList, CREOFamilyFileTypeEnum.Nested);

            // get the VaultFileMasters that are CREOFamilyFileTypeEnum.NonFamily but are misclassified
            ValidateNonFamilyClassification(vaultCache);

            // get the ToMasterId associated with the "From" VaultFileMasterOld
            // by creating an anonymous object dynamically to facilitate the ToLookup on resultant list
            var toMasterIdAndFamilyFromFileMasterList = (from fileAssocExt in familyAssocations
                                                         join fromFMKVP in vaultCache._vaultFileMastersByFileMasterIdDictionary
                                                         on fileAssocExt.FromMasterID equals fromFMKVP.Key
                                                         select new { toMasterId = fileAssocExt.ToMasterID, fromVaultFileMaster = fromFMKVP.Value });


            // create the lookup
            ILookup<long, VaultFileMasterOld> fromFamilyVaultFileMastersByToFileMasterId = toMasterIdAndFamilyFromFileMasterList.Distinct().ToLookup(kvp => kvp.toMasterId, kvp => kvp.fromVaultFileMaster);
            Logger.Debug(string.Format("fromFamilyVaultFileMastersByToFileMasterId ILookup: {0:n0}", fromFamilyVaultFileMastersByToFileMasterId.Count));


            Logger.Debug(string.Format("VaultFileMasterOld.AnalyzeCREOFamilyTableInstancesAndGenerics: Setup Time {0}", swp.LapTimeString()));

            // setup a lookup with the ToMasterId and FromMasterId as the key - this is from the "full" family file association list
            // used for checking instances pointing to multiple generics
            ILookup<long, FileAssociationExtended> faeByFromMasterIdLookup = vaultCache._familyFileAssociationsExtended.ToLookup(fae => fae.FromMasterID, fae => fae);
            ILookup<long, FileAssociationExtended> faeByToMasterIdLookup = vaultCache._familyFileAssociationsExtended.ToLookup(fae => fae.ToMasterID, fae => fae);


            // create a list to manage the generics already processed
            List<long> alreadyProcessedVFMIdList = new List<long>();

            // !!! testing
            // sniff the Screw.prt
            //var screwPrts = genericVaultFileMasterList.Where(vfm => vfm.VaultFullFilePathName.IndexOf("/Screw.prt", StringComparison.CurrentCultureIgnoreCase) >= 0).Select(vfm => vfm);


            //screwPrts.First().AnalyzeCREOFamilyTableInstancesAndGenerics(vaultCache, alreadyProcessedVFMList, fromFamilyVaultFileMastersByToFileMasterId);
            //Logger.Debug(string.Format("VaultFileMasterOld.AnalyzeCREOFamilyTableInstancesAndGenerics: {0} {1}", swp.LapTimeString(), screwPrts.First().VaultFullFilePathName));

            // !!! testing END


            // parse through the list of generics and reverse analyze the assemble family generics and instances
            int nProcessed = 0;
            foreach (VaultFileMasterOld vaultFileMaster in genericVaultFileMasterList)
            {
                vaultFileMaster.AnalyzeCREOFamilyTableInstancesAndGenerics(vaultCache, alreadyProcessedVFMIdList, fromFamilyVaultFileMastersByToFileMasterId, faeByFromMasterIdLookup, faeByToMasterIdLookup);

                // takes too long to output this - only used during debug and code optimization
                // Logger.Debug(string.Format("VaultFileMasterOld.AnalyzeCREOFamilyTableInstancesAndGenerics: {0} {1}", swp.LapTimeString(), vaultFileMaster.VaultFullFilePathName));

                nProcessed++;
            }

            // since we are looking at the instances that point to more than one generic, make sure it is unique
            vaultCache._fileMasterIDsPointingToMultipleGenerics = vaultCache._fileMasterIDsPointingToMultipleGenerics.Distinct().ToList();


            Logger.Debug(string.Format("VaultFileMasterOld.AnalyzeCREOFamilyTableInstancesAndGenerics: Total Time {0}", swp.ElapsedTimeString()));

        }

        // method to set the family type on the list and the Original, Update, and Valid Classification
        static protected void SetFamilyTypeAndClassifications(List<VaultFileMasterOld> familyVaultFileMasterList, CREOFamilyFileTypeEnum familyType)
        {
            // set the family type
            familyVaultFileMasterList.ForEach(vfm => vfm.mFamilyType = familyType);

            // set the update classification
            long updateClassification = familyType == CREOFamilyFileTypeEnum.Generic ? 4 : 3;
            familyVaultFileMasterList.ForEach(vfm => vfm.mUpdateClassification = updateClassification);

            // identify those family files that are not correct
            List<VaultFileMasterOld> incorrectlyClassified = familyVaultFileMasterList.Where(vfm => vfm.mOriginalClassification != vfm.mUpdateClassification).ToList();
            incorrectlyClassified.ForEach(vfm => vfm.bMisclassified = true);
            incorrectlyClassified.ForEach(vfm => vfm.mCREOFileStatus |= CREOFileStatus.IsMisclassified);

        }

        // method to set the creo file status
        static public void SetCREOFileStatus(List<VaultFileMasterOld> vaultFileMasterList, CREOFileStatus cREOFileStatus)
        {
            // set the family type
            // make sure to use the bitwise OR when setting
            vaultFileMasterList.ForEach(vfm => vfm.mCREOFileStatus |= cREOFileStatus);
        }





        // method to analyze for Solidworks Family table parts, instances, and generics
        protected void AnalyzeCREOFamilyTableInstancesAndGenerics(  VaultCacheOld vaultCache,
                                                                    List<long> alreadyProcessedVFMIdList,
                                                                    ILookup<long, VaultFileMasterOld> fromFamilyVaultFileMastersByToFileMasterId,
                                                                    ILookup<long, FileAssociationExtended> faeByFromMasterIdLookup,
                                                                    ILookup<long, FileAssociationExtended> faeByToMasterIdLookup,
                                                                    VaultFileMasterOld topVFMGeneric = null)

        {
            // don't re-analze file
            if (!alreadyProcessedVFMIdList.Contains(this.mFileMasterID))
            {
                alreadyProcessedVFMIdList.Add(this.mFileMasterID);

                // set the top generic
                this.TopVFMGeneric = topVFMGeneric;

                // if this is the top, set the topVFMGeneric to "this"
                // so that all lower level instances can adopt this top generic
                if (topVFMGeneric == null)
                {
                    topVFMGeneric = this;
                }

                // get the depth to the deepest instance for this generic or nested
                this.mFamilyDepth = 1;
                this.mFamilyMaxDepth = 1;
                if (this.mFamilyType != CREOFamilyFileTypeEnum.Generic)
                {
                    this.mFamilyDepth = DirectFamilyVFMGeneric.mFamilyDepth + 1;
                    this.mFamilyMaxDepth = DirectFamilyVFMGeneric.mFamilyDepth + 1;
                }


                // get the VaultFileMasters for the instances of this generic
                this.DirectVFMInstances = new List<VaultFileMasterOld>();
                if (fromFamilyVaultFileMastersByToFileMasterId.Contains(this.mFileMasterID))
                {
                    this.DirectVFMInstances.AddRange(fromFamilyVaultFileMastersByToFileMasterId[this.mFileMasterID].Distinct());
                }


                // set the rolled up instances
                this.RolledUpVFMInstances = new List<VaultFileMasterOld>();
                this.RolledUpVFMInstances.AddRange(this.DirectVFMInstances);

                // process each instance
                foreach (VaultFileMasterOld instanceVFM in this.DirectVFMInstances)
                {
                    instanceVFM.DirectFamilyVFMGeneric = this;

                    // set the instance generics path
                    instanceVFM.FamilyVFMGenericsPath = new List<VaultFileMasterOld>();
                    instanceVFM.FamilyVFMGenericsPath.Add(this);
                    if (this.FamilyVFMGenericsPath != null)
                    {
                        instanceVFM.FamilyVFMGenericsPath.AddRange(this.FamilyVFMGenericsPath);
                    }


                    // let the instance analyze itself
                    instanceVFM.AnalyzeCREOFamilyTableInstancesAndGenerics( vaultCache,
                                                                            alreadyProcessedVFMIdList,
                                                                            fromFamilyVaultFileMastersByToFileMasterId,
                                                                            faeByFromMasterIdLookup,
                                                                            faeByToMasterIdLookup,
                                                                            topVFMGeneric);

                    // stop if this is Screw.prt
                    if (string.Compare(this.FileName, "screw.prt", true) == 0)
                    {
                    }

                    // set the max family depth
                    this.mFamilyMaxDepth = Math.Max(this.mFamilyMaxDepth, instanceVFM.mFamilyMaxDepth);

                    // set the rolled up instances
                    this.RolledUpVFMInstances.AddRange(instanceVFM.RolledUpVFMInstances);
                    this.RolledUpVFMInstances = this.RolledUpVFMInstances.Distinct().ToList();

                    // for a sanity check, see if this points to multiple generics
                    if (faeByFromMasterIdLookup.Contains(this.mFileMasterID))
                    {
                        var thisGenericsList = faeByFromMasterIdLookup[this.mFileMasterID];
                        var thisOtherGenericsList = thisGenericsList.Where(fae => fae.ToMasterID != this.DirectFamilyVFMGeneric.mFileMasterID);

                        // add this file master ID to the list if it points to more than one Generic
                        if (thisOtherGenericsList.Any())
                        {
                            vaultCache._fileMasterIDsPointingToMultipleGenerics.Add(this.mFileMasterID);
                        }
                    }
                }

            }

            // stop if this is Screw.prt
            if (string.Compare(this.FileName, "screw.prt", true) == 0)
            {
            }


        }


        // method to identify rolled up parents
        static public void RecurseAndIdentifyRolledUpParents(VaultCacheOld vaultCache)
        {
            StopwatchPlus swp = new StopwatchPlus();


            // get the ToMasterId associated with the "From" VaultFileMasterOld
            // by creating an anonymous object dynamically to facilitate the ToLookup on resultant list
            var toMasterIdAndFromFileMasterList = (from fileAssocExt in vaultCache._fileAssociationsExtended
                                                   join fromFMKVP in vaultCache._vaultFileMastersByFileMasterIdDictionary
                                                   on fileAssocExt.FromMasterID equals fromFMKVP.Key
                                                   select new { toMasterId = fileAssocExt.ToMasterID, fromVaultFileMaster = fromFMKVP.Value });


            // create the lookup
            ILookup<long, VaultFileMasterOld> fromVaultFileMastersByToFileMasterId = toMasterIdAndFromFileMasterList.Distinct().ToLookup(kvp => kvp.toMasterId, kvp => kvp.fromVaultFileMaster);
            Logger.Debug(string.Format("fromVaultFileMastersByToFileMasterId ILookup: {0:n0}", fromVaultFileMastersByToFileMasterId.Count));


            Logger.Debug(string.Format("VaultFileMasterOld.RecurseAndIdentifyRolledUpParents: Setup Time {0}", swp.LapTimeString()));



            // create a list to manage the generics already processed
            List<long> alreadyProcessedVFMIdList = new List<long>();

            // parse through the list of files and reverse roll up the parents
            int nProcessed = 0;
            foreach (VaultFileMasterOld vaultFileMaster in vaultCache._vaultFileMasters)
            {
                vaultFileMaster.RecurseAndIdentifyRolledUpParents(vaultCache, alreadyProcessedVFMIdList, fromVaultFileMastersByToFileMasterId);

                // takes too long to output this - only used during debug and code optimization
                // Logger.Debug(string.Format("VaultFileMasterOld.RecurseAndIdentifyRolledUpParents: {0} {1}", swp.LapTimeString(), vaultFileMaster.VaultFullFilePathName));

                nProcessed++;
            }


            Logger.Debug(string.Format("VaultFileMasterOld.RecurseAndIdentifyRolledUpParents: Total Time {0}", swp.ElapsedTimeString()));

        }



        // method to identify rolled up parents
        protected void RecurseAndIdentifyRolledUpParents(VaultCacheOld vaultCache,
                                                            List<long> alreadyProcessedVFMIdList,
                                                            ILookup<long, VaultFileMasterOld> fromVaultFileMastersByToFileMasterId)

        {
            // don't re-analze file
            if (!alreadyProcessedVFMIdList.Contains(this.mFileMasterID))
            {
                alreadyProcessedVFMIdList.Add(this.mFileMasterID);

                // get the depth to the deepest parent for this file
                this.mParentMaxDepth = 0;

                // get the VaultFileMasters for the parents of this file
                this.DirectVFMParents = new List<VaultFileMasterOld>();
                if (fromVaultFileMastersByToFileMasterId.Contains(this.mFileMasterID))
                {
                    this.mParentMaxDepth = 1;
                    this.DirectVFMParents.AddRange(fromVaultFileMastersByToFileMasterId[this.mFileMasterID].Distinct());
                }


                // set the rolled up parents
                this.RolledUpVFMParents = new List<VaultFileMasterOld>();
                this.RolledUpVFMParents.AddRange(this.DirectVFMParents);

                // process each parent
                foreach (VaultFileMasterOld parentVFM in this.DirectVFMParents)
                {
                    // let the parent analyze itself
                    parentVFM.RecurseAndIdentifyRolledUpParents(vaultCache, alreadyProcessedVFMIdList, fromVaultFileMastersByToFileMasterId);

                    // set the max parent depth
                    this.mParentMaxDepth = Math.Max(this.mParentMaxDepth, parentVFM.mParentMaxDepth + 1);

                    // set the rolled up parents
                    this.RolledUpVFMParents.AddRange(parentVFM.RolledUpVFMParents);
                }

            }

            this.RolledUpVFMParents = this.RolledUpVFMParents.Distinct().ToList();
        }



        // assess the out of turn parents and find the top level files that would consume them
        static public void AnalyzeOutOfTurnParents(VaultCacheOld vaultCache,
                                                        out List<VaultFileMasterOld> outOfTurnParentsList,
                                                        out List<VaultFileMasterOld> outOfTurnTopLevelVFMParents,
                                                        out List<VaultFileMasterOld> outOfTurnParentsListFoundInTopLevels)

        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug(string.Format("VaultFileMasterOld.AnalyzeOutOfTurnParents: Start"));


            // get the list of parents that have children renamed out of turn
            outOfTurnParentsList = vaultCache._vaultFileMasters.Where(vfm => vfm.HasRenamedChildren).ToList();

            // set these files to needing resolution
            VaultFileMasterOld.SetCREOFileStatus(outOfTurnParentsList, CREOFileStatus.NeedsResolution);

            // parse list and create those that are not consumed by a top level
            outOfTurnParentsListFoundInTopLevels = new List<VaultFileMasterOld>();
            for (int nLoop = 0; nLoop < outOfTurnParentsList.Count; nLoop++)
            {
                // get the VFM
                VaultFileMasterOld outOfTurnParent = outOfTurnParentsList[nLoop];

                // if we have already identify it as being in another top level, don't process it
                if (!outOfTurnParentsListFoundInTopLevels.Contains(outOfTurnParent))
                {
                    // get this files children that are also in the out of turn list
                    var outOfTurnChildren = outOfTurnParentsList.Intersect(outOfTurnParent.RolledUpVFMChildren);

                    // add them to the tracking list
                    outOfTurnParentsListFoundInTopLevels.AddRange(outOfTurnChildren);
                }

                nLoop++;
            }

            // get the distinct list
            outOfTurnParentsListFoundInTopLevels = outOfTurnParentsListFoundInTopLevels.Distinct().ToList();

            // get the top level list
            // outOfTurnTopLevelVFMParents = outOfTurnParentsList.Where(vfm1 => !result.Any(vfm2 => vfm1.mFileMasterID == vfm2.mFileMasterID)).ToList();
            outOfTurnTopLevelVFMParents = outOfTurnParentsList.Except(outOfTurnParentsListFoundInTopLevels).ToList();

            Logger.Debug(string.Format("VaultFileMasterOld.AnalyzeOutOfTurnParents: End {0}", swp.ElapsedTimeString()));
        }


        // visit files and set their status
        static public void SetAllFileStatus(VaultCacheOld vaultCache)

        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug(string.Format("VaultFileMasterOld.SetAllFileStatus: Start"));

            // create an analsys support object
            AnalysisSupportObject aso = new AnalysisSupportObject(vaultCache);

            // set the status
            aso.SetFamilyInformationStatus();

            // set the Solidworks family types flag
            aso.SetCREOFamilyTypes();

            // set the remaining flags about the children and parents
            aso.SetFileStatus();

            Logger.Debug(string.Format("VaultFileMasterOld.SetAllFileStatus: End {0}", swp.ElapsedTimeString()));
        }


        // assess the parents that have children with a file misclassification
        static public void AnalyzeParentsWithMisclassifiedChildren(VaultCacheOld vaultCache,
                                                                    out List<VaultFileMasterOld> parentsWithMisclassifiedChildren,
                                                                    out List<VaultFileMasterOld> topLevelParentsWithMisclassifiedChildren,
                                                                    out List<VaultFileMasterOld> parentsWithMisclassifiedChildrenFoundInTopLevels)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug(string.Format("VaultFileMasterOld.AnalyzeParentsWithMisclassifiedChildren: Start"));


            // identify the parents with misclassified children

            // get the ToMasterId associated with the "From" VaultFileMasterOld
            // by creating an anonymous object dynamically to facilitate the ToLookup on resultant list
            var toMasterIdAndFromFileMasterList = (from fileAssocExt in vaultCache._fileAssociationsExtended
                                                   join fromFMKVP in vaultCache._vaultFileMastersByFileMasterIdDictionary
                                                   on fileAssocExt.FromMasterID equals fromFMKVP.Key
                                                   select new { toMasterId = fileAssocExt.ToMasterID, fromVaultFileMaster = fromFMKVP.Value });


            // create the lookup
            ILookup<long, VaultFileMasterOld> fromVaultFileMastersByToFileMasterId = toMasterIdAndFromFileMasterList.Distinct().ToLookup(kvp => kvp.toMasterId, kvp => kvp.fromVaultFileMaster);
            Logger.Debug(string.Format("fromVaultFileMastersByToFileMasterId.Count({0:n0}): {1}", fromVaultFileMastersByToFileMasterId.Count(), swp.LapTimeString()));


            // get all of the files that are misclassified
            List<long> misclassifiedFileMasterIdList = vaultCache._vaultFileMasters.Where(vfm => vfm.bMisclassified).Select(vfm => vfm.mFileMasterID).ToList();

            // from the lookup, identify the parents of these
            parentsWithMisclassifiedChildren = misclassifiedFileMasterIdList.SelectMany(mi => fromVaultFileMastersByToFileMasterId[mi]).Distinct().ToList();

            // make a lookup of the files found in the rolled up list that are also Misclassified
            foreach (VaultFileMasterOld parentVFM in parentsWithMisclassifiedChildren)
            {
                parentVFM.HasMisclassifiedChildren = true;
                parentVFM.MisclassifiedVFMChildren = parentVFM.RolledUpVFMChildren.Where(vfm => vfm.bMisclassified).ToList();
            }



            Logger.Debug(string.Format("parentsWithMisclassifiedChildren.Count({0:n0}): {1}", parentsWithMisclassifiedChildren.Count(), swp.LapTimeString()));

            Logger.Debug(string.Format("VaultFileMasterOld.AnalyzeParentsWithMisclassifiedChildren: Setup Time {0}", swp.LapTimeString()));


            // parse list and create those that are not consumed by a top level
            List<long> parentMasterIdsWithMisclassifiedChildrenFoundInTopLevels = new List<long>();
            var parentIdsWithMisclassifiedChildren = parentsWithMisclassifiedChildren.Select(vfm => vfm.mFileMasterID);
            int nLoop = 0;
            foreach (VaultFileMasterOld misclassifiedParent in parentsWithMisclassifiedChildren)
            {
                // if we have already identify it as being in another top level, don't process it
                if (!parentMasterIdsWithMisclassifiedChildrenFoundInTopLevels.Contains(misclassifiedParent.mFileMasterID))
                {
                    // get this files children that are also in the out of turn list
                    var misClassifiedChildIds = misclassifiedParent.RolledUpVFMChildren.Select(vfm => vfm.mFileMasterID);
                    var misclassifiedChildrenIds = parentIdsWithMisclassifiedChildren.Intersect(misClassifiedChildIds);

                    // add them to the tracking list
                    parentMasterIdsWithMisclassifiedChildrenFoundInTopLevels.AddRange(misclassifiedChildrenIds);

                    //// get the distinct every so often
                    if ((nLoop % 500 == 0) && (parentMasterIdsWithMisclassifiedChildrenFoundInTopLevels.Count() > 5000))
                    {
                        parentMasterIdsWithMisclassifiedChildrenFoundInTopLevels = parentMasterIdsWithMisclassifiedChildrenFoundInTopLevels.Distinct().ToList();
                    }
                }

                nLoop++;
            }

            // get the distinct list
            parentMasterIdsWithMisclassifiedChildrenFoundInTopLevels = parentMasterIdsWithMisclassifiedChildrenFoundInTopLevels.Distinct().ToList();

            // get the VaultFileMasters
            parentsWithMisclassifiedChildrenFoundInTopLevels = parentMasterIdsWithMisclassifiedChildrenFoundInTopLevels.Select(fmi => vaultCache._vaultFileMastersByFileMasterIdDictionary[fmi]).ToList();

            // get the top level list
            topLevelParentsWithMisclassifiedChildren = parentsWithMisclassifiedChildren.Except(parentsWithMisclassifiedChildrenFoundInTopLevels).ToList();


            Logger.Debug(string.Format("VaultFileMasterOld.AnalyzeParentsWithMisclassifiedChildren: End {0}", swp.ElapsedTimeString()));
        }






        // get a dictionary of all versions relative to their file iteration
        static public Dictionary<long, VaultFileMasterOld> GetVaultFileMastersByFileIterationId(IEnumerable<VaultFileMasterOld> vaultFileMasterList)
        {
            // figure out how to do this in one Linq statement
            Dictionary<long, VaultFileMasterOld> vaultFileMastersByFileIterationId = new Dictionary<long, VaultFileMasterOld>();
            foreach (VaultFileMasterOld vfm in vaultFileMasterList)
            {
                foreach (long fiid in vfm.mFileIterExtendList.Select(fi => fi.FileIterationId))
                {
                    vaultFileMastersByFileIterationId.Add(fiid, vfm);
                }
            }

            return (vaultFileMastersByFileIterationId);
        }

        #endregion child rename analysis

    }


    // opbject used to help analyze VaultFileMasterOld status
    public class AnalysisSupportObject
    {
        public VaultCacheOld mVaultCache;

        // get the top level parents, references, etc.
        protected List<long> _fromMasterIds;
        protected List<long> _toMasterIds;

        // select those that are FromId and ToId
        List<VaultFileMasterOld> _fromVaultFileMasterList;
        List<VaultFileMasterOld> _toVaultFileMasterList;

        // get the top level, references, etc
        protected List<VaultFileMasterOld> _lowestLevelVaultFileMasterList;
        public List<VaultFileMasterOld> LowestLevelVaultFileMasterList
        {
            get
            {
                if (_lowestLevelVaultFileMasterList == null)
                {
                    SetupFamilyInformation();
                }

                return (_lowestLevelVaultFileMasterList);
            }
            set { _lowestLevelVaultFileMasterList = value; }
        }


        protected List<VaultFileMasterOld> _topLevelVaultFileMasterList;
        public List<VaultFileMasterOld> TopLevelVaultFileMasterList
        {
            get
            {
                if (_topLevelVaultFileMasterList == null)
                {
                    SetupFamilyInformation();
                }

                return (_topLevelVaultFileMasterList);
            }
            set { _topLevelVaultFileMasterList = value; }
        }

        protected List<VaultFileMasterOld> _midLevelVaultFileMasterList;
        public List<VaultFileMasterOld> MidLevelVaultFileMasterList
        {
            get
            {
                if (_midLevelVaultFileMasterList == null)
                {
                    SetupFamilyInformation();
                }

                return (_midLevelVaultFileMasterList);
            }
            set { _midLevelVaultFileMasterList = value; }
        }


        // set the family types
        protected List<VaultFileMasterOld> _nonFamilyList;
        protected List<VaultFileMasterOld> _instanceList;
        protected List<VaultFileMasterOld> _nestedList;
        protected List<VaultFileMasterOld> _genericList;
        protected List<VaultFileMasterOld> _familyTableEmptyList;

        // constructor
        public AnalysisSupportObject(VaultCacheOld vaultCache)
        {
            mVaultCache = vaultCache;
        }

        // methods to calculate things for processing overall statistics and status
        protected void SetupFamilyInformation()
        {
            // get the top level parents, references, etc.
            _fromMasterIds = mVaultCache._fileAssociationsExtended.Select(fa => fa.FromMasterID).Distinct().ToList();
            _toMasterIds = mVaultCache._fileAssociationsExtended.Select(fa => fa.ToMasterID).Distinct().ToList();

            // select those that are FromId and ToId
            _fromVaultFileMasterList = _fromMasterIds.Select(mi => mVaultCache._vaultFileMastersByFileMasterIdDictionary[mi]).Distinct().ToList();

            _toVaultFileMasterList = _toMasterIds.Select(mi => mVaultCache._vaultFileMastersByFileMasterIdDictionary[mi]).Distinct().ToList();

            // get the top level, references, etc
            _lowestLevelVaultFileMasterList = _toVaultFileMasterList.Except(_fromVaultFileMasterList).ToList();
            _topLevelVaultFileMasterList = _fromVaultFileMasterList.Except(_toVaultFileMasterList).ToList();
            _midLevelVaultFileMasterList = _fromVaultFileMasterList.Except(_topLevelVaultFileMasterList).ToList();
        }


        // methods to calculate things for processing overall statistics and status
        public void SetFamilyInformationStatus()
        {
            VaultFileMasterOld.SetCREOFileStatus(LowestLevelVaultFileMasterList, CREOFileStatus.IsLowLevel);
            VaultFileMasterOld.SetCREOFileStatus(TopLevelVaultFileMasterList, CREOFileStatus.IsTopLevel);
        }

        // method to set Solidworks Family Type
        public void SetCREOFamilyTypes()
        {
            // set the family types
            _nonFamilyList = mVaultCache._vaultFileMasters.Where(vfm => vfm.mFamilyType == CREOFamilyFileTypeEnum.NonFamily).ToList();
            _instanceList = mVaultCache._vaultFileMasters.Where(vfm => vfm.mFamilyType == CREOFamilyFileTypeEnum.Instance).ToList();
            _nestedList = mVaultCache._vaultFileMasters.Where(vfm => vfm.mFamilyType == CREOFamilyFileTypeEnum.Nested).ToList();
            _genericList = mVaultCache._vaultFileMasters.Where(vfm => vfm.mFamilyType == CREOFamilyFileTypeEnum.Generic).ToList();
            _familyTableEmptyList = mVaultCache._vaultFileMasters.Where(vfm => vfm.mFamilyType == CREOFamilyFileTypeEnum.FamilyTableEmpty).ToList();

            VaultFileMasterOld.SetCREOFileStatus(_nonFamilyList, CREOFileStatus.IsNonFamily);
            VaultFileMasterOld.SetCREOFileStatus(_instanceList, CREOFileStatus.IsInstance);
            VaultFileMasterOld.SetCREOFileStatus(_nestedList, CREOFileStatus.IsNested);
            VaultFileMasterOld.SetCREOFileStatus(_genericList, CREOFileStatus.IsGeneric);
            VaultFileMasterOld.SetCREOFileStatus(_familyTableEmptyList, CREOFileStatus.HasEmptyFamilyTable);
        }

        // parse each VaultFileMasterOld and set the CREOFileStatus
        public void SetFileStatus()
        {
            // get the files that are pointing PointsToMultGenerics
            List<VaultFileMasterOld> pointsToMultGenerList = mVaultCache._fileMasterIDsPointingToMultipleGenerics.Select(fmi => mVaultCache._vaultFileMastersByFileMasterIdDictionary[fmi]).ToList();
            VaultFileMasterOld.SetCREOFileStatus(pointsToMultGenerList, CREOFileStatus.PointsToMultGenerics);

            // get the files that are superceded in a family reference
            var supercededVFMIdList = mVaultCache._supercededFamilyFileAssocExtended.Select(fae => fae.ToMasterID).Distinct();
            List<VaultFileMasterOld> supercededList = supercededVFMIdList.Select(fmi => mVaultCache._vaultFileMastersByFileMasterIdDictionary[fmi]).ToList();
            VaultFileMasterOld.SetCREOFileStatus(supercededList, CREOFileStatus.SupercededAsFamily);

            // parse through each file and set the status
            // get all of the files that need resolution
            foreach (VaultFileMasterOld vfm in mVaultCache._vaultFileMasters)
            {
                SetFileStatus(vfm);
            }

        }

        // get family files from a list
        protected List<VaultFileMasterOld> GetFamilyFilesFromList(List<VaultFileMasterOld> vaultFileMasterList)
        {
            List<VaultFileMasterOld> resultList = new List<VaultFileMasterOld>();

            if (vaultFileMasterList != null)
            {
                resultList = vaultFileMasterList.Where(vfm => vfm.mFamilyType != CREOFamilyFileTypeEnum.NonFamily).ToList();
            }

            return (resultList);
        }

        // get non family files from a list
        protected List<VaultFileMasterOld> GetNonFamilyFilesFromList(List<VaultFileMasterOld> vaultFileMasterList)
        {
            List<VaultFileMasterOld> resultList = new List<VaultFileMasterOld>();

            if (vaultFileMasterList != null)
            {
                resultList = vaultFileMasterList.Where(vfm => vfm.mFamilyType == CREOFamilyFileTypeEnum.NonFamily).ToList();
            }

            return (resultList);
        }



        // parse each VaultFileMasterOld and set the CREOFileStatus
        protected void SetFileStatus(VaultFileMasterOld vfmGiven)
        {
            // set properties on FamHasRenInst, FamHasRenDirInst, FamHasRenGeneric, FamHasRenDirGeneric
            var familyGenerics =  (vfmGiven.FamilyVFMGenericsPath != null) ? vfmGiven.FamilyVFMGenericsPath.Except(new List<VaultFileMasterOld> { vfmGiven.DirectFamilyVFMGeneric }) : new List<VaultFileMasterOld>();
            var familyInstances = (vfmGiven.RolledUpVFMInstances != null) ? vfmGiven.RolledUpVFMInstances.Except(vfmGiven.DirectVFMInstances) : new List<VaultFileMasterOld>();

            if (familyGenerics.Where(vfm => vfm.BeenRenamed()).Any())
            {
                vfmGiven.mCREOFileStatus |= CREOFileStatus.FamHasRenGeneric;
            }

            if ((vfmGiven.DirectFamilyVFMGeneric != null) && vfmGiven.DirectFamilyVFMGeneric.BeenRenamed())
            {
                vfmGiven.mCREOFileStatus |= CREOFileStatus.FamHasRenDirGeneric;
            }

            if (familyInstances.Where(vfm => vfm.BeenRenamed()).Any())
            {
                vfmGiven.mCREOFileStatus |= CREOFileStatus.FamHasRenInst;
            }

            if ((vfmGiven.DirectVFMInstances != null) && vfmGiven.DirectVFMInstances.Where(vfm => vfm.BeenRenamed()).Any())
            {
                vfmGiven.mCREOFileStatus |= CREOFileStatus.FamHasRenDirInst;
            }



            // get the content of the direct parents
            List<VaultFileMasterOld> directFamilyParents = GetFamilyFilesFromList(vfmGiven.DirectVFMParents);
            List<VaultFileMasterOld> directNonFamilyParents = GetNonFamilyFilesFromList(vfmGiven.DirectVFMParents);

            // get the direct children
            List<VaultFileMasterOld> directFamilyChildren = GetFamilyFilesFromList(vfmGiven.DirectVFMChildren);
            List<VaultFileMasterOld> directNonFamilyChildren = GetNonFamilyFilesFromList(vfmGiven.DirectVFMChildren);

            // get the rolled up parents
            List<VaultFileMasterOld> rolledUpFamilyParents = GetFamilyFilesFromList(vfmGiven.RolledUpVFMParents).Except(vfmGiven.DirectVFMParents).ToList();
            List<VaultFileMasterOld> rolledUpNonFamilyParents = GetNonFamilyFilesFromList(vfmGiven.RolledUpVFMParents).Except(vfmGiven.DirectVFMParents).ToList();

            // get the rolled up children
            List<VaultFileMasterOld> rolledUpFamilyChildren = GetFamilyFilesFromList(vfmGiven.RolledUpVFMChildren).Except(vfmGiven.DirectVFMChildren).ToList();
            List<VaultFileMasterOld> rolledUpNonFamilyChildren = GetNonFamilyFilesFromList(vfmGiven.RolledUpVFMChildren).Except(vfmGiven.DirectVFMChildren).ToList();


            // set the flags HasRenNonFamChld, HasRenDirNonFamChld, HasMsclsfdChld, HasMsclsfdDirChld, HasChldDirInstance, HasChldInstance, HasChldDirNested, HasChldNested, HasChldDirGeneric, HasChldGeneric 
            vfmGiven.mCREOFileStatus |= rolledUpNonFamilyChildren.Where(vfm => vfm.BeenRenamed()).Any() ? CREOFileStatus.HasRenNonFamChld : CREOFileStatus.NoStatus;
            vfmGiven.mCREOFileStatus |= directNonFamilyChildren.Where(vfm => vfm.BeenRenamed()).Any() ? CREOFileStatus.HasRenDirNonFamChld : CREOFileStatus.NoStatus;

            vfmGiven.mCREOFileStatus |= rolledUpFamilyChildren.Where(vfm => vfm.BeenRenamed()).Any() ? CREOFileStatus.HasRenFamChld : CREOFileStatus.NoStatus;
            vfmGiven.mCREOFileStatus |= directFamilyChildren.Where(vfm => vfm.BeenRenamed()).Any() ? CREOFileStatus.HasRenDirFamChld : CREOFileStatus.NoStatus;


            vfmGiven.mCREOFileStatus |= rolledUpFamilyChildren.Where(vfm => vfm.bMisclassified).Any() ? CREOFileStatus.HasMsclsfdChld : CREOFileStatus.NoStatus;
            vfmGiven.mCREOFileStatus |= directFamilyChildren.Where(vfm => vfm.bMisclassified).Any() ? CREOFileStatus.HasMsclsfdDirChld : CREOFileStatus.NoStatus;

            vfmGiven.mCREOFileStatus |= directFamilyChildren.Where(vfm => vfm.mFamilyType == CREOFamilyFileTypeEnum.Instance).Any() ? CREOFileStatus.HasChldDirInstance : CREOFileStatus.NoStatus;
            vfmGiven.mCREOFileStatus |= directFamilyChildren.Where(vfm => vfm.mFamilyType == CREOFamilyFileTypeEnum.Nested).Any() ? CREOFileStatus.HasChldDirNested : CREOFileStatus.NoStatus;
            vfmGiven.mCREOFileStatus |= directFamilyChildren.Where(vfm => vfm.mFamilyType == CREOFamilyFileTypeEnum.Generic).Any() ? CREOFileStatus.HasChldDirGeneric : CREOFileStatus.NoStatus;

            vfmGiven.mCREOFileStatus |= rolledUpFamilyChildren.Where(vfm => vfm.mFamilyType == CREOFamilyFileTypeEnum.Instance).Any() ? CREOFileStatus.HasChldInstance : CREOFileStatus.NoStatus;
            vfmGiven.mCREOFileStatus |= rolledUpFamilyChildren.Where(vfm => vfm.mFamilyType == CREOFamilyFileTypeEnum.Nested).Any() ? CREOFileStatus.HasChldNested : CREOFileStatus.NoStatus;
            vfmGiven.mCREOFileStatus |= rolledUpFamilyChildren.Where(vfm => vfm.mFamilyType == CREOFamilyFileTypeEnum.Generic).Any() ? CREOFileStatus.HasChldGeneric : CREOFileStatus.NoStatus;


            // is dependent on a file that points to multiple generics
            vfmGiven.mCREOFileStatus |= directFamilyChildren.Where(vfm => (vfm.mCREOFileStatus & CREOFileStatus.PointsToMultGenerics) != 0).Any() ? CREOFileStatus.HasChldDirPntgToMultGen : CREOFileStatus.NoStatus;
            vfmGiven.mCREOFileStatus |= rolledUpFamilyChildren.Where(vfm => (vfm.mCREOFileStatus & CREOFileStatus.PointsToMultGenerics) != 0).Any() ? CREOFileStatus.HasChldDPntgToMultGen : CREOFileStatus.NoStatus;



            // check for HasDwf
            vfmGiven.mCREOFileStatus |= directNonFamilyChildren.Where(vfm => string.Compare(vfm.Extension, ".dwf", true) == 0).Any() ? CREOFileStatus.HasDwf : CREOFileStatus.NoStatus;

            // check for HasDrawing
            if (directNonFamilyParents.Where(vfm => string.Compare(vfm.Extension, ".drw", true) == 0).Any())
            {
                // check to see if any of these files matches either the rename or the Tip name
                foreach (VaultFileMasterOld drawingVFM in directNonFamilyParents.Where(vfm => string.Compare(vfm.Extension, ".drw", true) == 0))
                {
                    string drawingBaseFileName = Path.GetFileNameWithoutExtension(drawingVFM.FileName);
                    if ((string.Compare(Path.GetFileNameWithoutExtension(vfmGiven.FileName), drawingBaseFileName, true) == 0) ||
                            (string.Compare(Path.GetFileNameWithoutExtension(vfmGiven.mTipCommittedFileIterExtended.FileName), drawingBaseFileName, true) == 0))
                    {
                        vfmGiven.mCREOFileStatus |= CREOFileStatus.HasDrawing;
                    }

                }
            }


            // set the WParNdngRes and WFamParNdngRes
            vfmGiven.mCREOFileStatus |= rolledUpNonFamilyParents.Where(vfm => vfm.HasRenamedChildren).Any() ? CREOFileStatus.WParNdngRes : CREOFileStatus.NoStatus;
            vfmGiven.mCREOFileStatus |= directNonFamilyParents.Where(vfm => vfm.HasRenamedChildren).Any() ? CREOFileStatus.WParNdngRes : CREOFileStatus.NoStatus;

            vfmGiven.mCREOFileStatus |= rolledUpFamilyParents.Where(vfm => vfm.HasRenamedChildren).Any() ? CREOFileStatus.WFamParNdngRes : CREOFileStatus.NoStatus;
            vfmGiven.mCREOFileStatus |= directFamilyParents.Where(vfm => vfm.HasRenamedChildren).Any() ? CREOFileStatus.WFamParNdngRes : CREOFileStatus.NoStatus;
        }
    }


}
