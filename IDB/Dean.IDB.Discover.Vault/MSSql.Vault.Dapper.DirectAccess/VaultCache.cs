using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Diagnostics;

using log4net;
using BCPBuilderConfig;

using MSSql.Vault.Dapper.DirectAccess.VaultDbEntities;


namespace MSSql.Vault.Dapper.DirectAccess
{
    public class VaultCache
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(VaultCache));

        string m_VaultConnectionString;
        string m_KvmConnectionString;

        // maintain the MSSqlVaultCacheLoader
        protected MSSqlVaultCacheLoader m_CacheLoader;
        protected MSSqlVaultCacheLoader m_KvmCacheLoader;   // this is for the KnowledgeVaultMaster
        protected string m_KVMDatabaseName;                 // actual name of the database

        // properties
        public string VaultSearchFolders { get; set; }
        public string VaultExtensionFilters { get; set; }
        public List<string> FilesOfInterest { get; set; }

        // Caches
        public Dictionary<long, UserGroup> m_UserGroups_Dict;
        public Dictionary<long, CategoryDef> m_CategoryDefs_Dict;
        public Dictionary<long, RevisionDefinition> m_RevisionDefinitions_Dict;
        public Dictionary<long, LifeCycleState> m_LifeCycleStates_Dict;
        public Dictionary<long, LifeCycleDefinition> m_LifeCycleDefinitions_Dict;

        // db folder specific information
        public Dictionary<long, Folder> m_Folder_Dict;
        public Dictionary<long, Entity> m_EntitiesForFolders_Dict;
        public Dictionary<long, CategoryOnEntity> m_CategoriesOnEntitiesForFolders_Dict;

        // db file specific information
        public Dictionary<long, FileMaster> m_FileMaster_Dict;
        public Dictionary<long, Iteration> m_IterationsForFiles_Dict;
        public long m_InitialFileIterationsDiscovered;
        public Dictionary<long, FileIterationExtended> m_FileIterationsExtended_Dict;
        public Dictionary<long, FileResource> m_FileResources_Dict;
        public Dictionary<long, Master> m_MastersForFiles_Dict;
        public Dictionary<long, CategoryOnEntity> m_CategoriesOnEntitiesForFileMasters_Dict;
        public Dictionary<long, Revision> m_RevisionsForFiles_Dict;
        public Dictionary<long, Entity> m_EntitiesForFiles_Dict;

        // db file discovery and processing
        public List<VaultFolder> m_SubsetVaultFolderList;
        public List<FileMaster> m_InitialFileMaster_List;
        public List<FileMaster> m_ChildrenFileMasterList;
        public List<FileMaster> m_ParentFileMasterList;
        public List<Iteration> m_InitialIterationList;
        public List<Iteration> m_ChildrenIterationList;
        public List<Iteration> m_ParentsIterationList;


        // db item specific information
        public Dictionary<long, ItemMaster> m_ItemMaster_Dict;
        public Dictionary<long, Iteration> m_IterationsForItems_Dict;
        public long m_InitialItemIterationsDiscovered;
        public Dictionary<long, Master> m_MastersForItems_Dict;
        public Dictionary<long, CategoryOnEntity> m_CategoriesOnEntitiesForItemMasters_Dict;
        public Dictionary<long, Revision> m_RevisionsForItems_Dict;
        public Dictionary<long, Entity> m_EntitiesForItems_Dict;

        // VaultFolder information
        public VaultFolder m_RootDocumentsFolder;
        public Dictionary<long, VaultFolder> m_VaultFolderDictByFolderID;
        public Dictionary<string, VaultFolder> m_VaultFolderDictByVaultPath;

        // VaultFileMaster, VaultFileRevision, and VaultFileIteration information
        public Dictionary<long, VaultFileMaster> m_VaultFileMasterDict;
        public Dictionary<long, VaultFileRevision> m_VaultFileRevisionDict;
        public Dictionary<long, VaultFileIteration> m_VaultFileIterationDict;

        // VaultItemMaster, VaultItemRevision, and VaultItemIteration information
        public Dictionary<long, VaultItemMaster> m_VaultItemMasterDict;
        public Dictionary<long, VaultItemRevision> m_VaultItemRevisionDict;
        public Dictionary<long, VaultItemIteration> m_VaultItemIterationDict;

        // file association information
        public List<FileAssociationExtended> m_FileAssociationsExtendedList;
        public List<long> m_FileAssociationIdList;

        // file property information
        public ILookup<long, Property> m_PropertyLookupByEntityID;


        // constructor
        public VaultCache(string sConnectionString, string sKVMDatabaseName = "KnowledgeVaultMaster")
        {
            m_VaultConnectionString = sConnectionString;

            // create the KnowledgeVaultMaster connection string
            // NOTE: user - if you want to have a different name for KVM, use the SetKVMDatabaseConnection(string sKVMDatabaseName) method
            SetKVMDatabaseConnection("KnowledgeVaultMaster");
        }


        // method to set the KVM "KnowledgeVaultMaster" to a different name
        // this is used when customer KVM and primary Vault databases are brought into SQL but not ADMS
        public void SetKVMDatabaseConnection(string sKVMDatabaseName)
        {
            SqlConnectionStringBuilder kvmBuilder = new SqlConnectionStringBuilder(m_VaultConnectionString);

            // set the initial catalog to the KnowledgeVaultMaster
            kvmBuilder.InitialCatalog = sKVMDatabaseName;
            m_KvmConnectionString = kvmBuilder.ConnectionString;
        }


        // method to load all folders, files, items and finalize cache
        public static void LoadAllFoldersFilesItemsAndFinalizeCache(VaultCache vaultCache)
        {
            LoadInitialCache(vaultCache);

            // setup the folder hierarchy
            SetupFolderHierarchies(vaultCache);

            LoadCacheAllFiles(vaultCache);

            LoadCacheAllItems(vaultCache);

            // build the VaultFolder, VaultFileMaster, VaultFileRevision, and VaultFileIteration objects
            FinalizeFileCache(vaultCache);

            // build the file associations
            FinalizeFileToFileRelationships(vaultCache);

            // build the VaultItemMaster, VaultItemRevision, and VaultItemIteration objects
            FinalizeItemCache(vaultCache);
        }

        // method to setup VaultFolders and build hierarchy
        // we need this for both FULL Vault processing and SubFile processing
        protected static void SetupFolderHierarchies(VaultCache vaultCache)
        {
            // setup the VaultFolders
            VaultFolder.SetupFolderHierarchies(vaultCache.m_Folder_Dict,
                                                vaultCache.m_CategoryDefs_Dict,
                                                vaultCache.m_CategoriesOnEntitiesForFolders_Dict,
                                                ref vaultCache.m_RootDocumentsFolder,
                                                ref vaultCache.m_VaultFolderDictByFolderID,
                                                ref vaultCache.m_VaultFolderDictByVaultPath);

        }

        #region LoadInitialCache

        // method to setup the initial cache's
        protected static void LoadInitialCache(VaultCache vaultCache)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("VaultCache LoadInitialCache(): start");

            // check to see if the MSSqlVaultCacheLoader has been enabled
            if (vaultCache.m_CacheLoader == null)
            {
                vaultCache.m_CacheLoader = new MSSqlVaultCacheLoader(vaultCache.m_VaultConnectionString);
            }

            if (vaultCache.m_KvmCacheLoader == null)
            {
                vaultCache.m_KvmCacheLoader = new MSSqlVaultCacheLoader(vaultCache.m_KvmConnectionString);
            }

            Logger.Debug("VaultCache LoadInitialCache: Start");

            // test getting all of the FileAssociation objects
            //List<FileAssociation> fileAssocationList = vaultCache.m_CacheLoader.LoadVaultEntities<FileAssociation>();
            //Logger.Debug(string.Format("fileAssocationList: {0:n0} {1}", fileAssocationList.Count(), swp.LapTimeString()));

            // test getting all of the FileAssociation objects
            //List<FileAssociationExtended> fileAssocationExtendedList = vaultCache.m_CacheLoader.LoadVaultEntities<FileAssociationExtended>();
            //Logger.Debug(string.Format("fileAssocationExtendedList: {0:n0} {1}", fileAssocationExtendedList.Count(), swp.LapTimeString()));



            // don't load if already loaded
            if (vaultCache.m_UserGroups_Dict == null)
            {
                vaultCache.m_UserGroups_Dict = vaultCache.m_KvmCacheLoader.LoadVaultEntityDictionary<UserGroup>();
            }
            Logger.Debug(string.Format("m_UserGroups_Dict: {0:n0}", vaultCache.m_UserGroups_Dict.Count));

            if (vaultCache.m_CategoryDefs_Dict == null)
            {
                vaultCache.m_CategoryDefs_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<CategoryDef>();
            }
            Logger.Debug(string.Format("m_CategoryDefs_Dict: {0:n0}", vaultCache.m_CategoryDefs_Dict.Count));

            if (vaultCache.m_RevisionDefinitions_Dict == null)
            {
                vaultCache.m_RevisionDefinitions_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<RevisionDefinition>();
            }
            Logger.Debug(string.Format("m_RevisionDefinitions_Dict: {0:n0}", vaultCache.m_RevisionDefinitions_Dict.Count));

            if (vaultCache.m_LifeCycleStates_Dict == null)
            {
                vaultCache.m_LifeCycleStates_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<LifeCycleState>();
            }
            Logger.Debug(string.Format("m_LifeCycleStates_Dict: {0:n0}", vaultCache.m_LifeCycleStates_Dict.Count));

            if (vaultCache.m_LifeCycleDefinitions_Dict == null)
            {
                vaultCache.m_LifeCycleDefinitions_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<LifeCycleDefinition>();
            }
            Logger.Debug(string.Format("m_LifeCycleDefinitions_Dict: {0:n0}", vaultCache.m_LifeCycleDefinitions_Dict.Count));

            if (vaultCache.m_Folder_Dict == null)
            {
                vaultCache.m_Folder_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<Folder>();

                // build the folders dictionary
                // vaultCache._foldersByPathDictionary = vaultCache.m_Folder_Dict.Values.ToDictionary(f => f.VaultPath, f => f, StringComparer.CurrentCultureIgnoreCase);

                // get the entities associated with the folders
                long[] folderIDArray = vaultCache.m_Folder_Dict.Values.Select(fldr => fldr.FolderID).ToArray();

                string sSelectString = VaultDbEntityHelper.GetSelectString<Entity>() + " WHERE EntityId IN @parm";
                vaultCache.m_EntitiesForFolders_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<Entity>(sSelectString, new { parm = folderIDArray });

                sSelectString = VaultDbEntityHelper.GetSelectString<CategoryOnEntity>() + " WHERE EntityId IN @parm";
                vaultCache.m_CategoriesOnEntitiesForFolders_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<CategoryOnEntity>(sSelectString, new { parm = folderIDArray });

            }
            Logger.Debug(string.Format("m_Folder_Dict: {0:n0}", vaultCache.m_Folder_Dict.Count));

            Logger.Debug(string.Format("VaultCache LoadInitialCache(): End {0}", swp.ElapsedTimeString()));
        }


        #endregion LoadInitialCache

        #region LoadCacheAllFiles

        // load all files found in whole Vault
        protected static void LoadCacheAllFiles(VaultCache vaultCache)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("VaultCache LoadCacheAllFiles(): Start");

            // check to see if the MSSqlVaultCacheLoader has been enabled
            if (vaultCache.m_CacheLoader == null)
            {
                vaultCache.m_CacheLoader = new MSSqlVaultCacheLoader(vaultCache.m_VaultConnectionString);
            }

            vaultCache.m_FileMaster_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<FileMaster>();
            Logger.Debug(string.Format("m_FileMaster_Dict: {0:n0}", vaultCache.m_FileMaster_Dict.Count));

            // once the file masters have been acquired, get the remainder of the information
            LoadRemainderOfFileCacheInformation(vaultCache);

            Logger.Debug(string.Format("VaultCache LoadCacheAllFiles(): End {0}", swp.ElapsedTimeString()));
        }

        // once the file masters have been acquired, get the remainder of the information
        protected static void LoadRemainderOfFileCacheInformation(VaultCache vaultCache)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("VaultCache LoadRemainderOfFileCacheInformation(): Start");


            // get the file iterations
            long[] fileMasterIDArray = vaultCache.m_FileMaster_Dict.Keys.ToArray();
            string sSelectString = VaultDbEntityHelper.GetSelectString<Iteration>() + " WHERE MasterID IN @parm";
            vaultCache.m_IterationsForFiles_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<Iteration>(sSelectString, new { parm = fileMasterIDArray });
            Logger.Debug(string.Format("m_IterationsForFiles_Dict from FileMasterId: {0:n0} {1}", vaultCache.m_IterationsForFiles_Dict.Count, swp.LapTimeString()));
            vaultCache.m_InitialFileIterationsDiscovered = vaultCache.m_IterationsForFiles_Dict.Count;


            // Controlled is no longer in table
            // remove the BOMBlob
            // string sSelectString = "SELECT FileIterationId, FileName, ModDate, CheckinDate, BOMBlob, DesignVisualizationAttachmentStatus, IsParsed, Controlled, LifeCycleStateName, CheckoutDate, ResourceId FROM dbo.FileIteration WHERE FileIterationId IN @parm";
            sSelectString = VaultDbEntityHelper.GetSelectString<FileIterationExtended>() + " WHERE FileIterationId IN @parm";
            vaultCache.m_FileIterationsExtended_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<FileIterationExtended>(sSelectString, new { parm = vaultCache.m_IterationsForFiles_Dict.Keys.ToArray() });
            Logger.Debug(string.Format("m_FileIterationsExtended_Dict: {0:n0} {1}", vaultCache.m_FileIterationsExtended_Dict.Count, swp.LapTimeString()));


            // get the file resources
            sSelectString = VaultDbEntityHelper.GetSelectString<FileResource>() + " WHERE FileMasterId IN @parm";
            vaultCache.m_FileResources_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<FileResource>(sSelectString, new { parm = fileMasterIDArray });
            Logger.Debug(string.Format("m_FileResources_Dict: {0:n0} {1}", vaultCache.m_FileResources_Dict.Count, swp.LapTimeString()));


            // get the masters table
            sSelectString = VaultDbEntityHelper.GetSelectString<Master>() + " WHERE MasterID IN @parm";
            vaultCache.m_MastersForFiles_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<Master>(sSelectString, new { parm = fileMasterIDArray });
            Logger.Debug(string.Format("masters: {0:n0} {1}", vaultCache.m_MastersForFiles_Dict.Count, swp.LapTimeString()));

            // get the categories
            // TODO: get categories on other entities? files, folders, etc
            sSelectString = VaultDbEntityHelper.GetSelectString<CategoryOnEntity>() + " WHERE EntityId IN @parm";
            vaultCache.m_CategoriesOnEntitiesForFileMasters_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<CategoryOnEntity>(sSelectString, new { parm = fileMasterIDArray });
            Logger.Debug(string.Format("m_CategoriesOnEntitiesForFileMasters_Dict: {0:n0} {1}", vaultCache.m_CategoriesOnEntitiesForFileMasters_Dict.Count, swp.LapTimeString()));


            // get the Revision table
            // LifeCycleStateId not in Revision table anymore
            sSelectString = VaultDbEntityHelper.GetSelectString<Revision>() + " WHERE MasterId IN @parm";
            vaultCache.m_RevisionsForFiles_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<Revision>(sSelectString, new { parm = fileMasterIDArray });
            Logger.Debug(string.Format("m_RevisionsForFiles_Dict: {0:n0} {1}", vaultCache.m_RevisionsForFiles_Dict.Count, swp.LapTimeString()));


            // get entities
            // WARNING - MUST USE Entity.CreateDate when generating FileRevisionIterationRef to already loaded files
            // instead of FileIteration.CheckinDate - they may be off by milliseconds
            sSelectString = VaultDbEntityHelper.GetSelectString<Entity>() + " WHERE EntityId IN @parm";
            vaultCache.m_EntitiesForFiles_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<Entity>(sSelectString, new { parm = vaultCache.m_IterationsForFiles_Dict.Keys.ToArray() });
            Logger.Debug(string.Format("m_EntitiesForFiles_Dict: {0:n0} {1}", vaultCache.m_EntitiesForFiles_Dict.Count, swp.LapTimeString()));

            Logger.Debug(string.Format("VaultCache LoadRemainderOfFileCacheInformation: End {0}", swp.ElapsedTimeString()));
        }

        #endregion LoadCacheAllFiles

        #region LoadCacheAllItems

        // load all Items found in whole Vault
        protected static void LoadCacheAllItems(VaultCache vaultCache)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("VaultCache LoadCacheAllItems(): Start");

            // check to see if the MSSqlVaultCacheLoader has been enabled
            if (vaultCache.m_CacheLoader == null)
            {
                vaultCache.m_CacheLoader = new MSSqlVaultCacheLoader(vaultCache.m_VaultConnectionString);
            }

            vaultCache.m_ItemMaster_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<ItemMaster>();
            Logger.Debug(string.Format("m_ItemMaster_Dict: {0:n0}", vaultCache.m_ItemMaster_Dict.Count));

            // once the file masters have been acquired, get the remainder of the information
            LoadRemainderOfItemCacheInformation(vaultCache);

            Logger.Debug(string.Format("VaultCache LoadCacheAllItems(): End {0}", swp.ElapsedTimeString()));
        }

        // once the item masters have been acquired, get the remainder of the information
        protected static void LoadRemainderOfItemCacheInformation(VaultCache vaultCache)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("VaultCache LoadRemainderOfItemCacheInformation(): Start");

            // get the file iterations
            long[] itemMasterIDArray = vaultCache.m_ItemMaster_Dict.Keys.ToArray();
            string sSelectString = VaultDbEntityHelper.GetSelectString<Iteration>() + " WHERE MasterID IN @parm";
            vaultCache.m_IterationsForItems_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<Iteration>(sSelectString, new { parm = itemMasterIDArray });
            Logger.Debug(string.Format("m_IterationsForItems_Dict: {0:n0} {1}", vaultCache.m_IterationsForItems_Dict.Count, swp.LapTimeString()));
            vaultCache.m_InitialItemIterationsDiscovered = vaultCache.m_IterationsForItems_Dict.Count;



            // get the masters table
            sSelectString = VaultDbEntityHelper.GetSelectString<Master>() + " WHERE MasterID IN @parm";
            vaultCache.m_MastersForItems_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<Master>(sSelectString, new { parm = itemMasterIDArray });
            Logger.Debug(string.Format("m_MastersForItems_Dict: {0:n0} {1}", vaultCache.m_MastersForItems_Dict.Count, swp.LapTimeString()));

            // get the categories
            // TODO: get categories on other entities? files, items, folders, etc
            sSelectString = VaultDbEntityHelper.GetSelectString<CategoryOnEntity>() + " WHERE EntityId IN @parm";
            vaultCache.m_CategoriesOnEntitiesForItemMasters_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<CategoryOnEntity>(sSelectString, new { parm = itemMasterIDArray });
            Logger.Debug(string.Format("m_CategoriesOnEntitiesForItemMasters_Dict: {0:n0} {1}", vaultCache.m_CategoriesOnEntitiesForItemMasters_Dict.Count, swp.LapTimeString()));


            // get the Revision table
            // LifeCycleStateId not in Revision table anymore
            // sSelectString = "SELECT RevisionId, RevisionDefinitionId, MasterId, RevisionLabel, RevisionOrder, MaxIterationId, MaxConsumableIterationId, LifeCycleStateId FROM dbo.Revision WHERE MasterId IN @parm";
            sSelectString = VaultDbEntityHelper.GetSelectString<Revision>() + " WHERE MasterId IN @parm";
            vaultCache.m_RevisionsForItems_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<Revision>(sSelectString, new { parm = itemMasterIDArray });
            Logger.Debug(string.Format("m_RevisionsForItems_Dict: {0:n0} {1}", vaultCache.m_RevisionsForItems_Dict.Count, swp.LapTimeString()));


            // get entities
            // WARNING - MUST USE Entity.CreateDate when generating FileRevisionIterationRef to already loaded files
            // instead of FileIteration.CheckinDate - they may be off by milliseconds
            sSelectString = VaultDbEntityHelper.GetSelectString<Entity>() + " WHERE EntityId IN @parm";
            vaultCache.m_EntitiesForItems_Dict = vaultCache.m_CacheLoader.LoadVaultEntityDictionary<Entity>(sSelectString, new { parm = vaultCache.m_IterationsForItems_Dict.Keys.ToArray() });
            Logger.Debug(string.Format("m_EntitiesForItems_Dict: {0:n0} {1}", vaultCache.m_EntitiesForItems_Dict.Count, swp.LapTimeString()));


            Logger.Debug(string.Format("VaultCache LoadRemainderOfItemCacheInformation(): End {0}", swp.ElapsedTimeString()));
        }



        #endregion LoadCacheAllItems

        #region FinalizeFileCache

        // build the VaultFolder, VaultFileMaster, VaultFileRevision, and VaultFileIteration objects
        protected static void FinalizeFileCache(VaultCache vaultCache)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("FinalizeFileCache(): Start");


            // build the VaultFileMaster, VaultFileRevision, and VaultFileIteration objects
            VaultFileMaster.BuildVaultFileMasters(vaultCache.m_FileMaster_Dict,
                                                    vaultCache.m_CategoryDefs_Dict,
                                                    vaultCache.m_CategoriesOnEntitiesForFileMasters_Dict,
                                                    vaultCache.m_MastersForFiles_Dict,
                                                    vaultCache.m_VaultFolderDictByFolderID,
                                                    ref vaultCache.m_VaultFileMasterDict);

            VaultFileRevision.BuildVaulFileRevisions(vaultCache.m_RevisionsForFiles_Dict,
                                                        vaultCache.m_VaultFileMasterDict,
                                                        ref vaultCache.m_VaultFileRevisionDict);

            // get the Properties for each of the FilIteration objects
            // reset the lap time
            swp.LapTimeString();
            long[] fileIterationIdArray = vaultCache.m_FileIterationsExtended_Dict.Keys.ToArray();
            string sSelectString = VaultDbEntityHelper.GetSelectString<Property>() + " WHERE EntityID IN @parm";
            List<Property> propertyList = vaultCache.m_CacheLoader.LoadVaultEntities<Property>(sSelectString, new { parm = fileIterationIdArray });
            vaultCache.m_PropertyLookupByEntityID = propertyList.ToLookup(prp => prp.EntityID, prp => prp);
            Logger.Debug(string.Format("vaultCache.m_PropertyLookupByEntityID: {0:n0}/{1:n0} {2}", vaultCache.m_PropertyLookupByEntityID.Count, propertyList.Count(), swp.LapTimeString()));


            VaultFileIteration.BuildVaulFileIterations(vaultCache.m_FileIterationsExtended_Dict,
                                                        vaultCache.m_IterationsForFiles_Dict,
                                                        vaultCache.m_FileResources_Dict,
                                                        vaultCache.m_EntitiesForFiles_Dict,
                                                        vaultCache.m_VaultFileRevisionDict,
                                                        vaultCache.m_PropertyLookupByEntityID,
                                                        ref vaultCache.m_VaultFileIterationDict);




            // log total time taken
            Logger.Debug(string.Format("FinalizeFileCache(): End {0}", swp.ElapsedTimeString()));
        }

        #endregion FinalizeFileCache

        #region FinalizeFileToFileRelationships

        // finalize seting up the Cache using business logic to make the associations
        protected static void FinalizeFileToFileRelationships(VaultCache vaultCache)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("FinalizeFileToFileRelationships(): Start");


            // get ALL file associations
            // check to see if the associations have any content
            if (vaultCache.m_FileAssociationsExtendedList == null)
            {
                vaultCache.m_FileAssociationsExtendedList = GetFileAssociationExtendedList(vaultCache, vaultCache.m_FileMaster_Dict.Values, vaultCache.m_FileAssociationIdList);
                Logger.Debug(string.Format("vaultCache.m_FileAssociationsExtendedList: {0:n0} {1}", vaultCache.m_FileAssociationsExtendedList.Count, swp.LapTimeString()));
            }

            // assign the VaultFileIteration relationships
            VaultFileIteration.AssignFileToFileIterationRelationships(vaultCache.m_VaultFileIterationDict, vaultCache.m_FileAssociationsExtendedList);

            // assign the VaultFileMaster relationships
            VaultFileMaster.AssignVaultFileMasterRelationships(vaultCache.m_VaultFileMasterDict.Values);

            // log total time taken
            Logger.Debug(string.Format("FinalizeFileToFileRelationships(): End {0}", swp.ElapsedTimeString()));
        }

        // method to get the associations based on FromMasterId and ToMasterId
        protected static List<FileAssociationExtended> GetFileAssociationExtendedList(VaultCache vaultCache, IEnumerable<FileMaster> fileMasters, List<long> fileAssociationIdList)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug(string.Format("GetFileAssociationExtendedList(): Start"));

            // get the associations for the FromId file master
            // check to see if the list contains FileAssociationId information
            List<FileAssociationExtended> totalFileAssocExtendList;
            if ((fileAssociationIdList != null) && (fileAssociationIdList.Count() > 1))
            {
                long[] fileAssociationIdArray = fileAssociationIdList.Distinct().ToArray();
                string sSelectString = VaultDbEntityHelper.GetSelectString<FileAssociationExtended>() + " WHERE FA.FileAssociationId IN @parm";
                totalFileAssocExtendList = vaultCache.m_CacheLoader.LoadVaultEntities<FileAssociationExtended>(sSelectString, new { parm = fileAssociationIdArray });
            }
            else
            {
                // get all of the associations
                totalFileAssocExtendList = vaultCache.m_CacheLoader.LoadVaultEntities<FileAssociationExtended>();
            }
            Logger.Debug(string.Format("FileAssociationExtended Found: {0:n0} {1}", totalFileAssocExtendList.Count(), swp.LapTimeString()));

            return (totalFileAssocExtendList);
        }

        // method to get the associations based on FromMasterId and ToMasterId
        protected static List<FileAssociationExtended> GetFileAssociationExtendedListXXX(VaultCache vaultCache, IEnumerable<FileMaster> fileMasters)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug(string.Format("GetFileAssociationExtendedList(): Start, Number of MasterIDs: {0:n0}", fileMasters.Count()));

            // get the associations for the FromId file master
            long[] masterIDArray = fileMasters.Select(fm => fm.FileMasterID).ToArray();
            string sSelectString = VaultDbEntityHelper.GetSelectString<FileAssociationExtended>() + " WHERE FromIT.MasterID IN @parm";
            List<FileAssociationExtended> fromFileAssocExtendList = vaultCache.m_CacheLoader.LoadVaultEntities<FileAssociationExtended>(sSelectString, new { parm = masterIDArray });
            Logger.Debug(string.Format("FileAssociationExtended Found As FromMasterID List: {0:n0} {1}", fromFileAssocExtendList.Count(), swp.LapTimeString()));

            // get the associations for the FromId file master
            sSelectString = VaultDbEntityHelper.GetSelectString<FileAssociationExtended>() + " WHERE ToIT.MasterID IN @parm";
            List<FileAssociationExtended> toFileAssocExtendList = vaultCache.m_CacheLoader.LoadVaultEntities<FileAssociationExtended>(sSelectString, new { parm = masterIDArray });
            Logger.Debug(string.Format("FileAssociationExtended Found As ToMasterID List: {0:n0} {1}", toFileAssocExtendList.Count(), swp.LapTimeString()));

            List<FileAssociationExtended> totalFileAssocExtendList = fromFileAssocExtendList.Union(toFileAssocExtendList).ToList();
            totalFileAssocExtendList = totalFileAssocExtendList.Distinct().ToList();

            long nOverlap = fromFileAssocExtendList.Count() + toFileAssocExtendList.Count() - totalFileAssocExtendList.Count();
            Logger.Debug(string.Format("GetFileAssociationExtendedList(): Found/Overlap: {0:n0}/{1:n0} {1}", totalFileAssocExtendList.Count(), nOverlap, swp.LapTimeString()));


            return (totalFileAssocExtendList);
        }

        #endregion FinalizeFileToFileRelationships

        #region FinalizeItemCache

        // build the VaultItemMaster, VaultItemRevision, and VaultItemIteration objects
        protected static void FinalizeItemCache(VaultCache vaultCache)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("FinalizeItemCache(): Start");


            // build the VaultItemMaster, VaultItemRevision, and VaultItemIteration objects
            VaultItemMaster.BuildVaultItemMasters(vaultCache.m_ItemMaster_Dict,
                                                    vaultCache.m_CategoryDefs_Dict,
                                                    vaultCache.m_CategoriesOnEntitiesForItemMasters_Dict,
                                                    vaultCache.m_MastersForItems_Dict,
                                                    ref vaultCache.m_VaultItemMasterDict);

            VaultItemRevision.BuildVaulItemRevisions(vaultCache.m_RevisionsForItems_Dict,
                                                        vaultCache.m_VaultItemMasterDict,
                                                        ref vaultCache.m_VaultItemRevisionDict);

            VaultItemIteration.BuildVaulItemIterations(vaultCache.m_IterationsForItems_Dict,
                                                        vaultCache.m_EntitiesForItems_Dict,
                                                        vaultCache.m_VaultItemRevisionDict,
                                                        ref vaultCache.m_VaultItemIterationDict);


            // log total time taken
            Logger.Debug(string.Format("FinalizeItemCache(): End {0}", swp.ElapsedTimeString()));
        }

        #endregion FinalizeItemCache


        #region LogTableCounts

        // get the number of records from the original tables
        public static void LogTableCounts(VaultCache vaultCache)
        {

            if (vaultCache.m_CacheLoader == null)
            {
                vaultCache.m_CacheLoader = new MSSqlVaultCacheLoader(vaultCache.m_VaultConnectionString);
            }

            if (vaultCache.m_KvmCacheLoader == null)
            {
                vaultCache.m_KvmCacheLoader = new MSSqlVaultCacheLoader(vaultCache.m_KvmConnectionString);
            }

            // get the database size information
            SpaceUsedDatabaseInfo sudi = vaultCache.m_CacheLoader.GetDatabaseSizeInfo();

            // generate the log output for the database size
            string sDatabaseSize = string.Format("Database Size For: \"{0}\"", sudi.database_name) + Environment.NewLine + "DatabaseSize\tUnallSpace\tReserved\tData\tIndexSize\tUnused" + Environment.NewLine +
                                            string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", sudi.database_size, sudi.unallocated_space, sudi.reserved, sudi.data, sudi.index_size, sudi.unused);


            Logger.Debug(sDatabaseSize);


            // get the table information
            List<SpaceUsedTableInfo> sutiList = GetTableSizeInfo(vaultCache);


            // generate the log output for the table sizes
            string sTableSizes = "Table Sizes for Database" + Environment.NewLine + "TableName\tNumberRows\tReserved\tData\tIndexSize\tUnused" + Environment.NewLine +
                                        string.Join(Environment.NewLine, sutiList.Select(suti => string.Format("{0}\t{1:n0}\t{2}\t{3}\t{4}\t{5}", suti.name, suti.rows, suti.reserved, suti.data, suti.index_size, suti.unused)));

            Logger.Debug(sTableSizes);
        }

        // get the number of records from the original tables
        public static List<SpaceUsedTableInfo> GetTableSizeInfo(VaultCache vaultCache)
        {
            if (vaultCache.m_CacheLoader == null)
            {
                vaultCache.m_CacheLoader = new MSSqlVaultCacheLoader(vaultCache.m_VaultConnectionString);
            }

            if (vaultCache.m_KvmCacheLoader == null)
            {
                vaultCache.m_KvmCacheLoader = new MSSqlVaultCacheLoader(vaultCache.m_KvmConnectionString);
            }

            // setup a list of the table names
            string[] tableNames = new string[] { "Acl", "CategoryDef", "CategoryOnEntity", "Entity", "EntityLifeCycleState",
                                                "FileAssociation", "FileIteration", "FileMaster", "FileResource", "Folder", "Iteration",
                                                "LifeCycleDefinition", "LifeCycleState", "Master", "ItemMaster", "Property", "PropertyDef",
                                                "Revision", "RevisionDefinition" };

            // get the table information
            List<SpaceUsedTableInfo> sutiList = vaultCache.m_CacheLoader.GetTableSizeInfo(tableNames);

            return (sutiList);
        }


        // get the number of records from the original tables
        public static void LogTableCountsOld(VaultCache vaultCache)
        {

            if (vaultCache.m_CacheLoader == null)
            {
                vaultCache.m_CacheLoader = new MSSqlVaultCacheLoader(vaultCache.m_VaultConnectionString);
            }

            if (vaultCache.m_KvmCacheLoader == null)
            {
                vaultCache.m_KvmCacheLoader = new MSSqlVaultCacheLoader(vaultCache.m_KvmConnectionString);
            }


            // get the GetTableCount method from the MSSqlVaultCacheLoader object
            // EntityToAcl has been removed
            Type[] types = new Type[] { typeof(Acl), typeof(CategoryDef), typeof(CategoryOnEntity), typeof(Entity), typeof(EntityLifeCycleState), /* typeof(EntityToAcl), */
                                        typeof(FileAssociation), typeof(FileIteration), typeof(FileMaster), typeof(FileResource), typeof(Folder), typeof(Iteration),
                                        typeof(LifeCycleDefinition), typeof(LifeCycleState), typeof(Master), typeof(Property), typeof(PropertyDef),
                                        typeof(Revision), typeof(RevisionDefinition) };

            MethodInfo methodGetTableCount = typeof(MSSqlVaultCacheLoader).GetMethod("GetTableCount");
            var tableCounts = types.Select(typ => new { tableName = typ.Name, tableCount = (int)(methodGetTableCount.MakeGenericMethod(typ).Invoke(vaultCache.m_CacheLoader, null)) }).ToList();

            Type userGroupType = typeof(UserGroup);
            string sTableSizes = "Number of Records in SQL Tables: " + Environment.NewLine +
                String.Join(Environment.NewLine, tableCounts.Select(tntc => string.Format("Table Name: \"{0}\"\tNumber of Records: {1:n0}", tntc.tableName, tntc.tableCount))) + Environment.NewLine +
                string.Format("Table Name: \"{0}\"\tNumber of Records: {1:n0}", userGroupType.Name, ((int)(methodGetTableCount.MakeGenericMethod(userGroupType).Invoke(vaultCache.m_KvmCacheLoader, null))));

            Logger.Debug(sTableSizes);
        }


        #endregion LogTableCounts

        #region LoadSubsetFilesAndFinalizeCache

        // method to load just a subset and finalize
        static public void LoadSubsetFilesAndFinalizeCache(VaultCache vaultCache, string vaultSearchFolders, bool bIncludeSubFolders, string vaultExtensionFilters, bool bDiscoverChildren, bool bDiscoverParents)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("VaultCache LoadSubsetFilesAndFinalizeCache(): start");

            LoadInitialCache(vaultCache);

            // setup the folder hierarchy
            SetupFolderHierarchies(vaultCache);

            // get the subset of files to process
            vaultCache.m_FileAssociationIdList = new List<long>();
            LoadCacheSubsetFiles(vaultCache, vaultSearchFolders, bIncludeSubFolders, vaultExtensionFilters, bDiscoverChildren, bDiscoverParents, ref vaultCache.m_FileAssociationIdList);

            // TODO: when Cameron gets more Items, do a subset based on the subset of files selected
            LoadCacheAllItems(vaultCache);

            // build the VaultFolder, VaultFileMaster, VaultFileRevision, and VaultFileIteration objects
            FinalizeFileCache(vaultCache);

            // build the file associations
            FinalizeFileToFileRelationships(vaultCache);

            // build the VaultItemMaster, VaultItemRevision, and VaultItemIteration objects
            FinalizeItemCache(vaultCache);


            Logger.Debug(string.Format("VaultCache LoadSubsetFilesAndFinalizeCache(): End {0}", swp.ElapsedTimeString()));
        }

        // discover the file Masters of interest with the folders/extensions filters, drill down into folders (OR NOT), discover children and parents
        // build remainder of cache
        protected static void LoadCacheSubsetFiles( VaultCache vaultCache,
                                                    string vaultSearchFolders,
                                                    bool bIncludeSubFolders,
                                                    string vaultExtensionFilters,
                                                    bool bDiscoverChildren,
                                                    bool bDiscoverParents,
                                                    ref List<long> fileAssociationIdList)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("VaultCache LoadCacheSubsetFiles(): Start");

            // check to see if the MSSqlVaultCacheLoader has been enabled
            if (vaultCache.m_CacheLoader == null)
            {
                vaultCache.m_CacheLoader = new MSSqlVaultCacheLoader(vaultCache.m_VaultConnectionString);
            }

            // get the subfolders of interest
            // this string should be pipe delimited
            // such as "$/Content Center Files|$/Libraries|$/Standard Parts"
            // there may be leading or trailing characters for ease of reading
            // trim off the whitespaces
            List<string> vaultFolderPathList = vaultSearchFolders.Split('|').Select(st => st.Trim()).ToList();
            Logger.Debug(string.Format("Getting Files from Vault Subfolders: \"{0}\"", vaultSearchFolders));

            vaultCache.m_SubsetVaultFolderList = new List<VaultFolder>();
            foreach (string vaultFolderPath in vaultFolderPathList)
            {
                // check to see if the VaultPath is in the Vault
                if (vaultCache.m_VaultFolderDictByVaultPath.ContainsKey(vaultFolderPath))
                {
                    VaultFolder vaultFolder = vaultCache.m_VaultFolderDictByVaultPath[vaultFolderPath];
                    vaultCache.m_SubsetVaultFolderList.Add(vaultFolder);

                    // if we are to include subfolders, get those as well
                    if (bIncludeSubFolders)
                    {
                        vaultCache.m_SubsetVaultFolderList.AddRange(vaultFolder.m_RolledUpFolders);
                    }
                }
            }

            // make sure they are distinct (overlap could have happened on selection)
            vaultCache.m_SubsetVaultFolderList = vaultCache.m_SubsetVaultFolderList.Distinct().ToList();
            Logger.Debug(string.Format("Number of folders to search: {0:n0}", vaultCache.m_SubsetVaultFolderList.Count()));


            // get a list of the remaining content taking in consideration we want to only look at the files in the given subfolders
            long[] folderIDArray = vaultCache.m_SubsetVaultFolderList.Select(vfl => vfl.m_FolderRecord.FolderID).ToArray();
            string sSelectString = VaultDbEntityHelper.GetSelectString<FileMaster>() + " WHERE FolderId IN @parm";
            vaultCache.m_InitialFileMaster_List = vaultCache.m_CacheLoader.LoadVaultEntities<FileMaster>(sSelectString, new { parm = folderIDArray });
            Logger.Debug(string.Format("Initial FileMaster List: {0:n0} {1}", vaultCache.m_InitialFileMaster_List.Count(), swp.LapTimeString()));


            // iterate through children and parents, find all up and down the hierarchy stack
            vaultCache.m_FileMaster_Dict = DiscoverFileMasterChildrenAndParents(vaultCache,
                                                                                vaultCache.m_InitialFileMaster_List,
                                                                                bDiscoverChildren,
                                                                                bDiscoverParents,
                                                                                ref fileAssociationIdList,
                                                                                out vaultCache.m_InitialIterationList,
                                                                                out vaultCache.m_ChildrenIterationList,
                                                                                out vaultCache.m_ParentsIterationList,
                                                                                out vaultCache.m_ChildrenFileMasterList,
                                                                                out vaultCache.m_ParentFileMasterList);

            Logger.Debug(string.Format("m_FileMaster_Dict: {0:n0}", vaultCache.m_FileMaster_Dict.Count));

            // once the file masters have been acquired, get the remainder of the information
            LoadRemainderOfFileCacheInformation(vaultCache);

            Logger.Debug(string.Format("VaultCache LoadCacheSubsetFiles(): End {0}", swp.ElapsedTimeString()));
        }

        // discover the FileIterations with children and parents
        protected static Dictionary<long, FileMaster> DiscoverFileMasterChildrenAndParents( VaultCache vaultCache,
                                                                                            List<FileMaster> initialFileMasterList,
                                                                                            bool bDiscoverChildren,
                                                                                            bool bDiscoverParents,
                                                                                            ref List<long> fileAssociationIdList,
                                                                                            out List<Iteration> initialIterationList,
                                                                                            out List<Iteration> childrenIterationList,
                                                                                            out List<Iteration> parentsIterationList,
                                                                                            out List<FileMaster> childrenFileMasterList,
                                                                                            out List<FileMaster> parentFileMasterList)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("VaultCache DiscoverFileMasterChildrenAndParents(): Start");

            // Sequence of hierarchical processing
            // we want to get all the children of the initial list, and their children
            // we DON'T want to get ALL Parents of those newly found children
            // we ONLY want to get all children and their children, then all parents and their parents

            // get the initial FileMaster Iterations
            // get the file iterations
            long[] fileMasterIDArray = initialFileMasterList.Select(fm => fm.FileMasterID).ToArray();
            string sIterationFromMasterIdSelectString = "SELECT IterationID, MasterID FROM dbo.Iteration WHERE MasterID IN @parm";
            initialIterationList = vaultCache.m_CacheLoader.LoadVaultEntities<Iteration>(sIterationFromMasterIdSelectString, new { parm = fileMasterIDArray });
            Logger.Debug(string.Format("initialIterationList: {0:n0} {1}", initialIterationList.Count(), swp.LapTimeString()));
            vaultCache.m_InitialFileIterationsDiscovered = initialIterationList.Count();

            // get all of the children for the initialIterationList
            // the return values will potetially include iterations from the original list
            childrenIterationList = DiscoverFileIterationChildrenORParents(vaultCache, initialIterationList, false, ref fileAssociationIdList);
            parentsIterationList = DiscoverFileIterationChildrenORParents(vaultCache, initialIterationList, true, ref fileAssociationIdList);

            // normalize the list
            List<Iteration> allFoundIterationList = initialIterationList.Union(childrenIterationList).Union(parentsIterationList).ToList();
            allFoundIterationList = allFoundIterationList.Distinct().ToList();

            // verify they are unique
            long nIterationIDs = allFoundIterationList.Select(it => it.IterationID).Distinct().Count();
            Debug.Assert(nIterationIDs == allFoundIterationList.Count());


            // get the FileMaster objects for all of these
            long[] masterIDArray = childrenIterationList.Select(it => it.MasterID).Distinct().ToArray();
            string sSelectString = VaultDbEntityHelper.GetSelectString<FileMaster>() + " WHERE FileMasterId IN @parm";
            childrenFileMasterList = vaultCache.m_CacheLoader.LoadVaultEntities<FileMaster>(sSelectString, new { parm = masterIDArray });
            Logger.Debug(string.Format("Children FileMasters Subset Found List: {0:n0} {1}", childrenFileMasterList.Count(), swp.LapTimeString()));

            masterIDArray = parentsIterationList.Select(it => it.MasterID).Distinct().ToArray();
            parentFileMasterList = vaultCache.m_CacheLoader.LoadVaultEntities<FileMaster>(sSelectString, new { parm = masterIDArray });
            Logger.Debug(string.Format("Parent FileMasters Subset Found List: {0:n0} {1}", childrenFileMasterList.Count(), swp.LapTimeString()));

            // combine the parents
            List<FileMaster> allFoundFileMasterList = initialFileMasterList.Union(childrenFileMasterList).Union(parentFileMasterList).ToList();
            allFoundFileMasterList = allFoundFileMasterList.Distinct().ToList();

            // verify they are unique
            long nFileMasterIDs = allFoundFileMasterList.Select(fm => fm.FileMasterID).Distinct().Count();
            Debug.Assert(nFileMasterIDs == allFoundFileMasterList.Count());


            Dictionary<long, FileMaster> fileMaster_Dict = allFoundFileMasterList.ToDictionary(fm => fm.FileMasterID, fm => fm);
            Logger.Debug(string.Format("Total FileMasters Subset Found List: {0:n0} {1}", fileMaster_Dict.Count(), swp.LapTimeString()));

            Logger.Debug(string.Format("VaultCache DiscoverFileMasterChildrenAndParents(): End {0}", swp.ElapsedTimeString()));


            return (fileMaster_Dict);
        }


        // discover the FileIteration children OR parents
        protected static List<Iteration> DiscoverFileIterationChildrenORParents(VaultCache vaultCache, List<Iteration> initialIterationList, bool bFindParents, ref List<long> fileAssociationIdList)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug(string.Format("VaultCache DiscoverFileIterationChildrenORParents({0}): Start", bFindParents ? "Find Parents" : "Find Children"));


            // create a list to manage what has been used for child and parent discovery and iterate until we have NO new children OR no parents discovered
            List<Iteration> iterationsToProcessList = new List<Iteration>();
            iterationsToProcessList.AddRange(initialIterationList);

            List<Iteration> iterationsDiscoveredForList = new List<Iteration>();
            List<Iteration> processedIterationList = new List<Iteration>();
            List<Iteration> foundIterationList = new List<Iteration>();
            do
            {
                // get the children OR parents to discover
                List<Iteration> discoverIterationsForList = iterationsToProcessList.Except(iterationsDiscoveredForList).ToList();

                // set those as already processed
                processedIterationList.AddRange(iterationsToProcessList);
                processedIterationList = processedIterationList.Distinct().ToList();

                // verify they are unique
                long nIterationIDs = processedIterationList.Select(it => it.IterationID).Distinct().Count();
                Debug.Assert(nIterationIDs == processedIterationList.Count());


                // check to see if we have more to discover
                List<Iteration> iterationsDiscovered = GetNewChildrenOrParentIterations(vaultCache, discoverIterationsForList, bFindParents, ref fileAssociationIdList);
                foundIterationList.AddRange(iterationsDiscovered);
                List <Iteration> newIterationsDiscovered = iterationsDiscovered.Except(processedIterationList).ToList();

                // set those as already discovered
                iterationsDiscoveredForList.AddRange(discoverIterationsForList);

                // reset the initialIterationList and add the newly found children and parents
                iterationsToProcessList = newIterationsDiscovered;
                iterationsToProcessList = iterationsToProcessList.Distinct().ToList();

            } while (iterationsToProcessList.Count() > 0);

            Logger.Debug(string.Format("VaultCache DiscoverFileIterationChildrenORParents({0}): End {1}", bFindParents ? "Find Parents" : "Find Children", swp.ElapsedTimeString()));

            // normalize the list
            // this will contain the initialIterationList
            foundIterationList = foundIterationList.Distinct().ToList();

            // verify they are unique
            long nIterationIDs2 = foundIterationList.Select(it => it.IterationID).Distinct().Count();
            Debug.Assert(nIterationIDs2 == foundIterationList.Count());

            return (iterationsDiscoveredForList);
        }


        // method to get either the children or parents of the given list
        protected static List<Iteration> GetNewChildrenOrParentIterations(VaultCache vaultCache, List<Iteration> discoverForList, bool bFindParents, ref List<long> fileAssociationIdList)
        {
            List<Iteration> newIterationList = new List<Iteration>();

            // the list to discover may be empty
            if (discoverForList.Any())
            {

                // setup the SELECT string
                string sFileAssociationSelectString = "SELECT FileAssociationId, FromId, ToId FROM dbo.FileAssociation WHERE ";
                sFileAssociationSelectString += bFindParents ? "ToId" : "FromId";
                sFileAssociationSelectString += " IN @parm";

                long[] fromToIdArray = discoverForList.Select(it => it.IterationID).ToArray();
                List<FileAssociation> associationList = vaultCache.m_CacheLoader.LoadVaultEntities<FileAssociation>(sFileAssociationSelectString, new { parm = fromToIdArray });

                // get the FileAssociationId information
                fileAssociationIdList.AddRange(associationList.Select(fa => fa.FileAssociationId));

                // identify the new Iterations we need to grab
                List<long> iterationIdList;
                if (bFindParents)
                {
                    iterationIdList = associationList.Select(fa => fa.FromId).Distinct().ToList();
                }
                else
                {
                    iterationIdList = associationList.Select(fa => fa.ToId).Distinct().ToList();
                }

                // if we have iterations to get, select them from the database
                if (iterationIdList.Any())
                {
                    string sIterationFromIterationIdSelectString = "SELECT IterationID, MasterID FROM dbo.Iteration WHERE IterationID IN @parm";
                    newIterationList = vaultCache.m_CacheLoader.LoadVaultEntities<Iteration>(sIterationFromIterationIdSelectString, new { parm = iterationIdList.ToArray() });
                }
            }


            return (newIterationList);
        }


        #endregion LoadSubsetFilesAndFinalizeCache


        #region LoadInitialSpecificFilesAndFinalizeCache

        // method to load just a subset and finalize
        static public void LoadInitialSpecificFilesAndFinalizeCache(List<string> sVaultFullFilePathNameList, VaultCache vaultCache, bool bDiscoverChildren, bool bGetAllGenerics)
        {
            LoadInitialCache(vaultCache);
            //LoadCacheFromSpecificFiles(sVaultFullFilePathNameList, vaultCache, bDiscoverChildren, bGetAllGenerics);
            //FinalizeFileCache(vaultCache);
            //BuildVaultFileMasterRelationshipsAndAnalyze(vaultCache, bGetAllGenerics);
        }


        #endregion LoadInitialSpecificFilesAndFinalizeCache


        #region GetDesignsFolderStatistics

        // get the statistics from the first level folder under the level given
        public static void GetDesignsFolderStatistics(VaultCache vaultCache, string sTopoLevelVaultFolderPath)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug(string.Format("GetDesignsFolderStatistics() Start: Top Level Folder: \"{0}\"", sTopoLevelVaultFolderPath));

            if (vaultCache.m_CacheLoader == null)
            {
                vaultCache.m_CacheLoader = new MSSqlVaultCacheLoader(vaultCache.m_VaultConnectionString);
            }

            if (vaultCache.m_KvmCacheLoader == null)
            {
                vaultCache.m_KvmCacheLoader = new MSSqlVaultCacheLoader(vaultCache.m_KvmConnectionString);
            }

            // get the initial cache
            LoadInitialCache(vaultCache);

            // setup the folder hierarchy
            SetupFolderHierarchies(vaultCache);

            // find the top folder
            VaultFolder topLevelVaultFolder = vaultCache.m_VaultFolderDictByVaultPath[sTopoLevelVaultFolderPath];

            List<string> sOutputStrings = new List<string>();
            sOutputStrings.Add("VaultFolder\tFolders\tFilesInFolder\tTotalFiles\tChildren\tChildrenOut\tParents\tParentsOut\tIterationsInFolder\tTotalIterations\tChildIterations\tChildIterOut\tParentIterations\tParentIterOut");

            // iterate through each of the folders and get their statistics
            // Master files, file iterations, associations
            int nLoop = 1;
            foreach (VaultFolder childVaultFolder in topLevelVaultFolder.m_ChildFolders)
            {
                Logger.Debug(string.Format("GetDesignsFolderStatistics(): Processing Folder {0} of {1}", nLoop++, topLevelVaultFolder.m_ChildFolders.Count()));

                string sOutputString = GetDesignsFolderStatistics(vaultCache, childVaultFolder);
                sOutputStrings.Add(sOutputString);
            }

            Logger.Debug(string.Format("GetDesignsFolderStatistics(): Results{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, sOutputStrings)));

            Logger.Debug(string.Format("GetDesignsFolderStatistics() End: Top Level Folder: \"{0}\" {1}", sTopoLevelVaultFolderPath, swp.ElapsedTimeString()));

        }

        // get the Master files, file iterations, associations for all folders under the given folder
        protected static string GetDesignsFolderStatistics(VaultCache vaultCache, VaultFolder vaultFolder)
        {
            Logger.Debug(string.Format("GetDesignsFolderStatistics(): {0}", vaultFolder.m_FolderRecord.VaultPath));


            List<VaultFolder> vaultFolderList = new List<VaultFolder>();
            vaultFolderList.Add(vaultFolder);
            vaultFolderList.AddRange(vaultFolder.m_RolledUpFolders);


            long[] folderIDArray = vaultFolderList.Select(vfl => vfl.m_FolderRecord.FolderID).ToArray();
            // don't use the base FileMaster query - use the reduced
            string sSelectString = VaultDbEntityHelper.GetSelectString<FileMaster>() + " WHERE FolderId IN @parm";
            List<FileMaster> fileMasterList = vaultCache.m_CacheLoader.LoadVaultEntities<FileMaster>(sSelectString, new { parm = folderIDArray });

            // get all of the children and parents
            vaultCache.m_FileAssociationIdList = new List<long>();
            Dictionary<long, FileMaster> fileMasterDictionary = DiscoverFileMasterChildrenAndParents(   vaultCache,
                                                                                                        fileMasterList,
                                                                                                        true,
                                                                                                        true,
                                                                                                        ref vaultCache.m_FileAssociationIdList,
                                                                                                        out vaultCache.m_InitialIterationList,
                                                                                                        out vaultCache.m_ChildrenIterationList,
                                                                                                        out vaultCache.m_ParentsIterationList,
                                                                                                        out vaultCache.m_ChildrenFileMasterList,
                                                                                                        out vaultCache.m_ParentFileMasterList);

            // build the return string
            List<Iteration> totalIterationList = vaultCache.m_InitialIterationList.Union(vaultCache.m_ChildrenIterationList).Union(vaultCache.m_ParentsIterationList).ToList();
            List<FileMaster> totalFileMasterList = fileMasterList.Union(vaultCache.m_ChildrenFileMasterList).Union(vaultCache.m_ParentFileMasterList).ToList();

            // string sHeaderString = "VaultFolder\tFolders\tFilesInFolder\tTotalFiles\tChildren\tChildrenOut\tParents\tParentsOut\tIterationsInFolder\tTotalIterations\tChildIterations\tChildIterOut\tParentIterations\tParentIterOut";
            string sOutputString = string.Format("{0}\t{1:n0}\t{2:n0}\t{3:n0}\t{4:n0}\t{5:n0}\t{6:n0}\t{7:n0}\t{8:n0}\t{9:n0}\t{10:n0}\t{11:n0}\t{12:n0}\t{13:n0}",
                                                        vaultFolder.m_FolderRecord.VaultPath, vaultFolder.m_RolledUpFolders.Count(),
                                                        fileMasterList.Count(), totalFileMasterList.Count(),
                                                        vaultCache.m_ChildrenFileMasterList.Count(), vaultCache.m_ChildrenFileMasterList.Except(fileMasterList).Count(),
                                                        vaultCache.m_ParentFileMasterList.Count(), vaultCache.m_ParentFileMasterList.Except(fileMasterList).Count(),
                                                        vaultCache.m_InitialIterationList.Count(), totalIterationList.Count(),
                                                        vaultCache.m_ChildrenIterationList.Count(), vaultCache.m_ChildrenIterationList.Except(vaultCache.m_InitialIterationList).Count(),
                                                        vaultCache.m_ParentsIterationList.Count(), vaultCache.m_ParentsIterationList.Except(vaultCache.m_InitialIterationList).Count());

            return (sOutputString);
        }

        #endregion GetDesignsFolderStatistics


        #region AnalyzeForPurge

        public static void AnalyzeForPurge(VaultCache vaultCache)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("AnalyzeForPurge(): Start");

            // set those files that are from the initial search list
            IdentifyOriginalFiles(vaultCache);

            // iterate up and down the relationships stack of the VaultFileIterations and analyze for purge
            AnalyzeIterationsForPurge(vaultCache);

            // log total time taken
            Logger.Debug(string.Format("AnalyzeForPurge(): End {0}", swp.ElapsedTimeString()));
        }

        // method to identify those files that are in original search folders
        // and those parents that are out of search folders but need to be used for Purge Analysis
        // and are in the running for purging
        protected static void IdentifyOriginalFiles(VaultCache vaultCache)
        {
            // set those files that are from the initial search list
            // get those that are from the original list
            List<long> baseFileMasterIdList = vaultCache.m_InitialFileMaster_List.Select(fm => fm.FileMasterID).ToList();
            List<VaultFileMaster> baseVaultFileMasterList = new List<VaultFileMaster>();
            foreach (long masterId in baseFileMasterIdList)
            {
                baseVaultFileMasterList.Add(vaultCache.m_VaultFileMasterDict[masterId]);
            }

            // set the originally found masters
            // if they are within the folders, they are up for Purging
            baseVaultFileMasterList.ForEach(vfm => vfm.bInOriginalFoundList = true);

            // find the parents outside of the folders that should be used for analysis
            // if they are parents and outside of folders, they are up for purging
            List<long> parentFileMasterIdList = vaultCache.m_ParentFileMasterList.Select(fm => fm.FileMasterID).ToList();
            List<VaultFileMaster> parentVaultFileMasterList = new List<VaultFileMaster>();
            foreach (long masterId in parentFileMasterIdList)
            {
                parentVaultFileMasterList.Add(vaultCache.m_VaultFileMasterDict[masterId]);
            }
            parentVaultFileMasterList = parentVaultFileMasterList.Except(baseVaultFileMasterList).ToList();

            // set the parent masters that are outside the search folders
            // if they are parents and outside of folders, they are up for purging
            parentVaultFileMasterList.ForEach(vfm => vfm.bIsParentForAnalysis = true);

            // the remainder children that are not in the original search folder AND are not from the FOUND parents
            // are used for Purge analysis, but are not to be purged
        }



        // method to iterate up and down the relationships stack of the VaultFileIterations and analyze for purge
        protected static void AnalyzeIterationsForPurge(VaultCache vaultCache)
        {
            // set the VaultFileIteration
            // _bCanBePurgedByLifecycleStateName, _bCanBePurgedByControlled, _bCanBePurgedByRevisionPosition, _bCanBePurgedByParentState
            VaultFileMaster.SetCanBePurgedOnOwnMeritStatuses(vaultCache.m_VaultFileMasterDict.Values);
        }



        #endregion AnalyzeForPurge

    }
}
