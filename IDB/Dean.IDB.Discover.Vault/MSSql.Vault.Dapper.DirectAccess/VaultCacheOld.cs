using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using MSSql.Vault.Dapper.DirectAccess.VaultDbEntities;
using System.Reflection;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Diagnostics;

using log4net;
using BCPBuilderConfig;

namespace MSSql.Vault.Dapper.DirectAccess
{
    public class VaultCacheOld
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(VaultCacheOld));

        string _connectionString;
        string _kvmConnectionString;

        // maintain the MSSqlVaultCacheLoader
        protected MSSqlVaultCacheLoader cacheLoader;
        protected MSSqlVaultCacheLoader kvmCacheLoader;   // this is for the KnowledgeVaultMaster

        // statistical information
        public string _KVMDatabaseName;
        public string _vaultSearchFolders;
        public string _vaultExtensionFilters;
        public List<string> _filesOfInterest;
        public long _initialFilesDiscovered;
        public long _initialFileIterationsDiscovered;
        public long _initialItemIterationsDiscovered;
        public long _vaultSearchFoldersCount;
        public long _totalFilesWithinSearchFolders;
        public long _totalIterationsWithinSearchFolders;


        //Caches
        public Dictionary<long, UserGroup> _userGroups;
        public Dictionary<long, CategoryDef> _categoryDefs;
        public Dictionary<long, RevisionDefinition> _revisionDefinitions;
        public Dictionary<long, LifeCycleState> _lifeCycleStates;
        public Dictionary<long, LifeCycleDefinition> _lifeCycleDefinitions;

        // file specific information
        public Dictionary<long, CategoryOnEntity> _categoriesOnEntitiesForFiles;
        public Dictionary<long, Revision> _revisionsForFiles;
        public Dictionary<long, FileResource> _fileResources;
        public Dictionary<long, FileIterationExtended> _fileIterationsExtended;
        public Dictionary<long, Iteration> _iterationsForFiles;
        public Dictionary<long, Entity> _entitiesForFiles;
        public Dictionary<long, FileMaster> _fileMasters;
        public Dictionary<long, Master> _mastersForFiles;
        public Dictionary<long, IterationProviderAndContentSource> _iterationProviderAndContentSource;

        // folder specific information
        public Dictionary<long, Folder> _folders;
        public Dictionary<long, Entity> _entitiesForFolders;
        public Dictionary<long, CategoryOnEntity> _categoriesOnEntitiesForFolders;



        // item specific information
        public Dictionary<long, CategoryOnEntity> _categoriesOnEntitiesForItems;
        public Dictionary<long, Revision> _revisionsForItems;
        public Dictionary<long, Iteration> _iterationsForItems;
        public Dictionary<long, Entity> _entitiesForItems;
        public Dictionary<long, ItemMaster> _itemMasters;
        public Dictionary<long, Master> _mastersForItems;


        // lists to hold file assocations
        // _fileAssociationsExtended will be all FileAssociationsExtended for non family assocations, as well as the most recent family association
        // _familyFileAssociationsExtended are all family file assocations - even for files we are not interested in
        //      therefore, it may contain references to files not in the FileMaster or VaultFileMasterOld lists
        // _supercededFamilyFileAssocExtended contains the superceded FileAssociationExtended, where a family instance pointed to multiple generic masters 
        public List<FileAssociationExtended> _fileAssociationsExtended;
        public List<FileAssociationExtended> _familyFileAssociationsExtended;
        public List<FileAssociationExtended> _supercededFamilyFileAssocExtended;
        public List<long> _fileMasterIDsPointingToMultipleGenerics;



        // ACL relative to folders
        public List<Acl> _folderACLList;
        public Dictionary<long, EntityToAcl> _folderEntityToAcl;

        // Entity Lifecycle management
        public List<EntityLifeCycleState> _entityLifeCycleStateList;

        // TODO: visit property definition. Where property is drop down list AND property default is non-blank, need to output a property value of blank when file has it as blank
        // otherwise, when setting as blank for zero length string, the value for the UDP will be set as the default from the drop down list, which may not be blank

        // list of property definitions configured for an individual
        // file MasterId.  To use - isolate the property definitions of interest
        // using these lists and lookup tables
        // the Properties themselves are then associated with the IterationId and ResourceId of the file
        public List<PropertyDefOnEntity> _propertyDefOnEntityList;  
        public ILookup<long, PropertyDefOnEntity> _propertyDefOnEntitiesByMasterIds;

        // properties specific to file iterations
        public Dictionary<long, Property> _fileIterationProperties;
        public Dictionary<long, PropertyDef> _fileIterationPropertyDefs;

        // properties specific to file resources
        public List<ContentSourceProperties> _contentSourcePropertiesList;
        public ILookup<long, ContentSourceProperties> _contentSourcePropertyByResourceIds;


        // Property Content Provider information
        // not all PropertyDefs will be represented
        // if they are not a system property, and not found in this list
        // they are purely a UDP, and can be imported through VaultBCP as a UDP
        public List<PropertyDefToContentSourceMapping> _propertyDefToContentSourceMappingList;
        public ILookup<long, PropertyDefToContentSourceMapping> _propertyDefToContentSourceMappingsByPropertyDefIds;


        // ACL relative to MasterFile
        public List<Acl> _masterFileACLList;
        public Dictionary<long, EntityToAcl> _masterFileEntityToAcl;

        // Dictionary Indexes
        public Dictionary<string, Folder> _foldersByPathDictionary;
        public Dictionary<string, VaultFileMasterOld> _vaultFileMastersByFullVaultFilePathDictionary;
        public Dictionary<long, VaultFileMasterOld> _vaultFileMastersByFileIterationIdDictionary;
        public Dictionary<long, VaultFileMasterOld> _vaultFileMastersByFileMasterIdDictionary;
        public Dictionary<long, EntityLifeCycleState> _entityLifeCyleStatesByFileMasterIdDictionary;


        // Lookup Indexes
        public ILookup<long, LifeCycleState> _lifeCycleStatesByLCDefId;

        // file Lookup Indexes
        public ILookup<long, FileMaster> _fileMastersByFolderId;
        public ILookup<long, Iteration> _fileIterationsByMasterId;
        public ILookup<long, FileIterationExtended> _fileIterExtndsByMasterId;
        public ILookup<long, Revision> _fileRevisionsByMasterId;
        public ILookup<long, FileResource> _fileResourcesByMasterId;
        public ILookup<long, Entity> _fileEntitiesByMasterId;
        public ILookup<long, Property> _propertiesByFileMasterId;
        public ILookup<long, PropertyDef> _propertDefsByFileMasterId;

        // item Lookup Indexes
        public ILookup<long, Iteration> _itemIterationsByMasterId;
        public ILookup<long, Revision> _itemRevisionsByMasterId;
        public ILookup<long, Entity> _itemEntitiesByMasterId;
        public ILookup<long, Property> _propertiesByItemMasterId;
        public ILookup<long, PropertyDef> _propertDefsByItemMasterId;



        // lists managing rename analysis results
        public List<VaultFileMasterOld> _vaultFileMasters;
        public List<VaultFileMasterOld> _outOfTurnParentsList;                     // whole list of files
        public List<VaultFileMasterOld> _outOfTurnTopLevelVFMParents;              // top level inclusive of others
        public List<VaultFileMasterOld> _outOfTurnParentsListFoundInTopLevels;     // those found within top levels

        public List<VaultFileMasterOld> _parentsWithMisclassifiedChildren;                 // whole list of files
        public List<VaultFileMasterOld> _topLevelParentsWithMisclassifiedChildren;         // top level inclusive of others
        public List<VaultFileMasterOld> _parentsWithMisclassifiedChildrenFoundInTopLevels; // those found within top levels


        // child finder helper class
        FileToFileChildFinder _childFinder;

        // constructor - careful, gets the whole vault
        public VaultCacheOld(string sConnectionString)
        {
            _connectionString = sConnectionString;

            // create the KnowledgeVaultMaster connection string
            // NOTE: user - if you want to have a different name for KVM, use the SetKVMDatabaseConnection(string sKVMDatabaseName) method
            SetKVMDatabaseConnection("KnowledgeVaultMaster");
        }


        // method to set the KVM "KnowledgeVaultMaster" to a different name
        // this is used when customer KVM and primary Vault databases are brought into SQL but not ADMS
        public void SetKVMDatabaseConnection(string sKVMDatabaseName)
        {
            SqlConnectionStringBuilder kvmBuilder = new SqlConnectionStringBuilder(_connectionString);

            // set the initial catalog to the KnowledgeVaultMaster
            kvmBuilder.InitialCatalog = sKVMDatabaseName;
            _kvmConnectionString = kvmBuilder.ConnectionString;
        }


        // method to load file masters only for file discovery
        static public void LoadCacheForFileDiscovery(VaultCacheOld vaultCache)
        {
            // LoadCacheAllFiles(vaultCache);
            Logger.Debug("VaultCacheOld LoadCacheForFileDiscovery: Start");

            // check to see if the MSSqlVaultCacheLoader has been enabled
            if (vaultCache.cacheLoader == null)
            {
                vaultCache.cacheLoader = new MSSqlVaultCacheLoader(vaultCache._connectionString);
            }

            vaultCache._folders = vaultCache.cacheLoader.LoadVaultEntityDictionary<Folder>();
            Logger.Debug(string.Format("_folders: {0:n0}", vaultCache._folders.Count));

            // build the folders dictionary
            vaultCache._foldersByPathDictionary = vaultCache._folders.Values.ToDictionary(f => f.VaultPath, f => f, StringComparer.CurrentCultureIgnoreCase);

            // output the paths to an export file
            // TODO: put this in a configuration?
            System.IO.File.WriteAllLines(@"C:\Temp\AllVaultFolders.txt", vaultCache._foldersByPathDictionary.Keys);

            // get the entities associated with the folders
            long[] folderIDArray = vaultCache._folders.Values.Select(fldr => fldr.FolderID).ToArray();
            string sSelectString = "SELECT EntityId, EntityClassID, CreateDate, CreateUserID, Active, IsConsumable FROM dbo.Entity WHERE EntityId IN @parm";
            vaultCache._entitiesForFolders = vaultCache.cacheLoader.LoadVaultEntityDictionary<Entity>(sSelectString, new { parm = folderIDArray });
            Logger.Debug(string.Format("_entitiesForFolders: {0:n0}", vaultCache._entitiesForFolders.Count));

            vaultCache._fileMasters = vaultCache.cacheLoader.LoadVaultEntityDictionary<FileMaster>();
            Logger.Debug(string.Format("_fileMasters: {0:n0}", vaultCache._fileMasters.Count));

            // finalize the cache for file discovery
            FinalizeFileCacheForFileDiscovery(vaultCache);

            // create the VaultFileMasters and build a dictionary on their Vault full path
            SetupVaultFileMasterListForFileDiscovery(vaultCache);

            Logger.Debug("VaultCacheOld LoadCacheForFileDiscovery: End");

        }


        // method to load all files and finalize cache
        static public void LoadInitialAllFilesAndFinalizeCache(VaultCacheOld vaultCache, bool bGetAllGenerics)
        {
            LoadInitialCache(vaultCache);

            LoadCacheAllFiles(vaultCache);

            LoadCacheAllItems(vaultCache);

            FinalizeFileCache(vaultCache);

            FinalizeItemCache(vaultCache);


            BuildVaultFileMasterRelationshipsAndAnalyze(vaultCache, bGetAllGenerics);
        }

        // method to load just a subset and finalize
        static public void LoadInitialSubsetFilesAndFinalizeCache(string vaultSearchFolders, string vaultExtensionFilters, VaultCacheOld vaultCache, bool bDiscoverChildren, bool bGetAllGenerics, string sInventorProjectFile = null)
        {
            StopwatchPlus swp = new StopwatchPlus();

            LoadInitialCache(vaultCache);
            LoadCacheFilesFromSubfolders(vaultSearchFolders, vaultExtensionFilters, vaultCache, bDiscoverChildren, bGetAllGenerics, sInventorProjectFile);
            FinalizeFileCache(vaultCache);
            BuildVaultFileMasterRelationshipsAndAnalyze(vaultCache, bGetAllGenerics);
        }


        // method to load just a subset and finalize
        static public void LoadInitialSpecificFilesAndFinalizeCache(List<string> sVaultFullFilePathNameList, VaultCacheOld vaultCache, bool bDiscoverChildren, bool bGetAllGenerics)
        {
            LoadInitialCache(vaultCache);
            LoadCacheFromSpecificFiles(sVaultFullFilePathNameList, vaultCache, bDiscoverChildren, bGetAllGenerics);
            FinalizeFileCache(vaultCache);
            BuildVaultFileMasterRelationshipsAndAnalyze(vaultCache, bGetAllGenerics);
        }

        // method to load just a subset and finalize
        static public void LoadInitialSpecificFilesFinalizeWithoutRelationships(List<string> sVaultFullFilePathNameList, VaultCacheOld vaultCache, bool bDiscoverChildren, bool bGetAllGenerics)
        {
            LoadInitialCache(vaultCache);
            LoadCacheFromSpecificFiles(sVaultFullFilePathNameList, vaultCache, bDiscoverChildren, bGetAllGenerics);
            FinalizeFileCache(vaultCache);

            // not performing the relationships build
            // BuildVaultFileMasterRelationshipsAndAnalyze(vaultCache, bGetAllGenerics);

            // in lieu of setting up relationships, perform finish up on the file masters
            // create the VaultFileMasters and build a dictionary on their Vault full path
            SetupVaultFileMasterList(vaultCache);

        }

        // method to load just a subset and finalize
        static public void LoadInitialSpecificFilesFinalizeWithoutRelationshipsAndFindMovedFiles(List<string> sVaultFullFilePathNameList, VaultCacheOld vaultCache, bool bDiscoverChildren, bool bGetAllGenerics)
        {
            LoadInitialCache(vaultCache);
            LoadCacheFromSpecificFilesAndFindMoved(sVaultFullFilePathNameList, vaultCache, bDiscoverChildren, bGetAllGenerics);


            FinalizeFileCache(vaultCache);

            // not performing the relationships build
            // BuildVaultFileMasterRelationshipsAndAnalyze(vaultCache, bGetAllGenerics);

            // in lieu of setting up relationships, perform finish up on the file masters
            // create the VaultFileMasters and build a dictionary on their Vault full path
            SetupVaultFileMasterList(vaultCache);

        }





        // method to copy the initial content from one VaultCacheOld to another
        // so we don't have to do it more than once
        public void CopyInitialCacheFrom(VaultCacheOld sourceVaultCache)
        {
            // this is not a deep copy, but it won't matter as these are used 
            // in a full capture always and only needs to be captured once
            _userGroups = sourceVaultCache._userGroups;
            _categoryDefs = sourceVaultCache._categoryDefs;
            _revisionDefinitions = sourceVaultCache._revisionDefinitions;
            _lifeCycleStates= sourceVaultCache._lifeCycleStates;
            _lifeCycleDefinitions = sourceVaultCache._lifeCycleDefinitions;
            _folders = sourceVaultCache._folders;
            _entitiesForFolders = sourceVaultCache._entitiesForFolders;
            _categoriesOnEntitiesForFolders = sourceVaultCache._categoriesOnEntitiesForFolders;
            _foldersByPathDictionary = sourceVaultCache._foldersByPathDictionary;
        }

        // method to merge the remainder of the content from one VaultCacheOld to another
        // mostly so that we can combine getting information from subfolders and specific files
        public void MergeCacheFrom(VaultCacheOld sourceVaultCache)
        {
            _fileMasters = MSSqlVaultCacheLoader.CombineDictionaries(_fileMasters, sourceVaultCache._fileMasters);
            _fileResources = MSSqlVaultCacheLoader.CombineDictionaries(_fileResources, sourceVaultCache._fileResources);
            _mastersForFiles = MSSqlVaultCacheLoader.CombineDictionaries(_mastersForFiles, sourceVaultCache._mastersForFiles);
            _categoriesOnEntitiesForFiles = MSSqlVaultCacheLoader.CombineDictionaries(_categoriesOnEntitiesForFiles, sourceVaultCache._categoriesOnEntitiesForFiles);
            _fileIterationsExtended = MSSqlVaultCacheLoader.CombineDictionaries(_fileIterationsExtended, sourceVaultCache._fileIterationsExtended);
            _iterationsForFiles = MSSqlVaultCacheLoader.CombineDictionaries(_iterationsForFiles, sourceVaultCache._iterationsForFiles);
            _revisionsForFiles = MSSqlVaultCacheLoader.CombineDictionaries(_revisionsForFiles, sourceVaultCache._revisionsForFiles);
            _entitiesForFiles = MSSqlVaultCacheLoader.CombineDictionaries(_entitiesForFiles, sourceVaultCache._entitiesForFiles);
            _fileAssociationsExtended = _fileAssociationsExtended.Concat(sourceVaultCache._fileAssociationsExtended).Distinct().ToList(); ;
        }



        // method to setup the initial cache's
        static public void LoadInitialCache(VaultCacheOld vaultCache)
        {
            // check to see if the MSSqlVaultCacheLoader has been enabled
            if (vaultCache.cacheLoader == null)
            {
                vaultCache.cacheLoader = new MSSqlVaultCacheLoader(vaultCache._connectionString);
            }

            if (vaultCache.kvmCacheLoader == null)
            {
                vaultCache.kvmCacheLoader = new MSSqlVaultCacheLoader(vaultCache._kvmConnectionString);
            }

            Logger.Debug("VaultCacheOld LoadInitialCache: Start");

            // output the number of records in each table
            // TODO: !!!!! put this back in when ready to process for production - so we can log the metrics
            // LogTableCounts(vaultCache);

            // don't load if already loaded
            if (vaultCache._userGroups == null)
            {
                vaultCache._userGroups = vaultCache.kvmCacheLoader.LoadVaultEntityDictionary<UserGroup>();
            }
            Logger.Debug(string.Format("_userGroups: {0:n0}", vaultCache._userGroups.Count));

            if (vaultCache._categoryDefs == null)
            {
                vaultCache._categoryDefs = vaultCache.cacheLoader.LoadVaultEntityDictionary<CategoryDef>();
            }
            Logger.Debug(string.Format("_categoryDefs: {0:n0}", vaultCache._categoryDefs.Count));

            if (vaultCache._revisionDefinitions == null)
            {
                vaultCache._revisionDefinitions = vaultCache.cacheLoader.LoadVaultEntityDictionary<RevisionDefinition>();
            }
            Logger.Debug(string.Format("_revisionDefinitions: {0:n0}", vaultCache._revisionDefinitions.Count));

            if (vaultCache._lifeCycleStates == null)
            {
                vaultCache._lifeCycleStates = vaultCache.cacheLoader.LoadVaultEntityDictionary<LifeCycleState>();
            }
            Logger.Debug(string.Format("_lifeCycleStates: {0:n0}", vaultCache._lifeCycleStates.Count));

            if (vaultCache._lifeCycleDefinitions == null)
            {
                vaultCache._lifeCycleDefinitions = vaultCache.cacheLoader.LoadVaultEntityDictionary<LifeCycleDefinition>();
            }
            Logger.Debug(string.Format("_lifeCycleDefinitions: {0:n0}", vaultCache._lifeCycleDefinitions.Count));

            if (vaultCache._folders == null)
            {
                vaultCache._folders = vaultCache.cacheLoader.LoadVaultEntityDictionary<Folder>();

                // build the folders dictionary
                vaultCache._foldersByPathDictionary = vaultCache._folders.Values.ToDictionary(f => f.VaultPath, f => f, StringComparer.CurrentCultureIgnoreCase);

                // get the entities associated with the folders
                long[] folderIDArray = vaultCache._folders.Values.Select(fldr => fldr.FolderID).ToArray();
                string sSelectString = "SELECT EntityId, EntityClassID, CreateDate, CreateUserID, Active, IsConsumable FROM dbo.Entity WHERE EntityId IN @parm";
                vaultCache._entitiesForFolders = vaultCache.cacheLoader.LoadVaultEntityDictionary<Entity>(sSelectString, new { parm = folderIDArray });

                sSelectString = "SELECT EntityId, CategoryDefId FROM dbo.CategoryOnEntity WHERE EntityId IN @parm";
                vaultCache._categoriesOnEntitiesForFolders = vaultCache.cacheLoader.LoadVaultEntityDictionary<CategoryOnEntity>(sSelectString, new { parm = folderIDArray });

            }
            Logger.Debug(string.Format("_folders: {0:n0}", vaultCache._folders.Count));
            Logger.Debug(string.Format("_foldersByPathDictionary: {0:n0}", vaultCache._foldersByPathDictionary.Count));

            Logger.Debug("VaultCacheOld LoadInitialCache: Finish");
        }


        // get the number of records from the original tables
        static protected void LogTableCounts(VaultCacheOld vaultCache)
        {
            // get the GetTableCount method from the MSSqlVaultCacheLoader object
            // EntityToAcl has been removed
            Type[] types = new Type[] { typeof(Acl), typeof(CategoryDef), typeof(CategoryOnEntity), typeof(Entity), typeof(EntityLifeCycleState), /* typeof(EntityToAcl), */
                                        typeof(FileAssociation), typeof(FileIteration), typeof(FileMaster), typeof(FileResource), typeof(Folder), typeof(Iteration),
                                        typeof(LifeCycleDefinition), typeof(LifeCycleState), typeof(Master), typeof(Property), typeof(PropertyDef),
                                        typeof(Revision), typeof(RevisionDefinition) };

            MethodInfo methodGetTableCount = typeof(MSSqlVaultCacheLoader).GetMethod("GetTableCount");
            var tableCounts = types.Select(typ => new { tableName = typ.Name, tableCount = (int)(methodGetTableCount.MakeGenericMethod(typ).Invoke(vaultCache.cacheLoader, null)) }).ToList();

            Type userGroupType = typeof(UserGroup);
            string sTableSizes = "Number of Records in SQL Tables: " + Environment.NewLine +
                String.Join(Environment.NewLine, tableCounts.Select(tntc => string.Format("Table Name: \"{0}\"\tNumber of Records: {1:n0}", tntc.tableName, tntc.tableCount))) + Environment.NewLine +
                string.Format("Table Name: \"{0}\"\tNumber of Records: {1:n0}", userGroupType.Name, ((int)(methodGetTableCount.MakeGenericMethod(userGroupType).Invoke(vaultCache.kvmCacheLoader, null))));

            Logger.Debug(sTableSizes);
        }




        // method to create the VaultFileMasters, build relationships, analyze issues
        static public void BuildVaultFileMasterRelationshipsAndAnalyze(VaultCacheOld vaultCache, bool bGetAllGenerics)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("BuildVaultFileMasterRelationshipsAndAnalyze(): Start");

            // check to see if the associations have any content
            if (vaultCache._fileAssociationsExtended == null)
            {
                vaultCache._fileAssociationsExtended = vaultCache.cacheLoader.LoadVaultEntities<FileAssociationExtended>();
                Logger.Debug(string.Format("vaultCache._fileAssociationsExtended: {0:n0} {1}", vaultCache._fileAssociationsExtended.Count, swp.LapTimeString()));
            }

            // child finder helper class
            Logger.Debug("FileToFileChildFinder build: Start");
            vaultCache._childFinder = new FileToFileChildFinder(vaultCache._fileAssociationsExtended, vaultCache._fileIterationsExtended);
            Logger.Debug("FileToFileChildFinder build: Finish");


            // create the VaultFileMasters and build a dictionary on their Vault full path
            SetupVaultFileMasterList(vaultCache);


            // build the VaultFileMasterOld relationships and analyze children that have been renamed
            VaultFileMasterOld.BuildVaultFileMasterRelationships(vaultCache);

            // setup the Solidworks family table information and analysis
            VaultFileMasterOld.AnalyzeCREOFamilyTableInstancesAndGenerics(vaultCache, bGetAllGenerics);
            Logger.Debug(string.Format("VaultFileMasterOld.AnalyzeCREOFamilyTableInstancesAndGenerics() {0}", swp.LapTimeString()));

            // get rolled up parents
            VaultFileMasterOld.RecurseAndIdentifyRolledUpParents(vaultCache);
            Logger.Debug(string.Format("VaultFileMasterOld.RecurseAndIdentifyRolledUpParents() {0}", swp.LapTimeString()));


            // method to analyze whether children have been renamed after parents have been checked in
            VaultFileMasterOld.AnalyzeOutOfTurnParents(vaultCache,
                                                        out vaultCache._outOfTurnParentsList,
                                                        out vaultCache._outOfTurnTopLevelVFMParents,
                                                        out vaultCache._outOfTurnParentsListFoundInTopLevels);
            Logger.Debug(string.Format("VaultFileMasterOld.AnalyzeOutOfTurnParents() {0}", swp.LapTimeString()));


            // assess the parents that have children with a file misclassification
            VaultFileMasterOld.AnalyzeParentsWithMisclassifiedChildren(vaultCache,
                                                                    out vaultCache._parentsWithMisclassifiedChildren,
                                                                    out vaultCache._topLevelParentsWithMisclassifiedChildren,
                                                                    out vaultCache._parentsWithMisclassifiedChildrenFoundInTopLevels);

            // get the FULL set of CREOFileStatus
            VaultFileMasterOld.SetAllFileStatus(vaultCache);


            Logger.Debug(string.Format("BuildVaultFileMasterRelationshipsAndAnalyze(): End {0}", swp.LapTimeString()));
        }

        // method to finalize cache and setup indexes
        static public void FinalizeFileCacheForFileDiscovery(VaultCacheOld vaultCache)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("FinalizeFileCacheForFileDiscovery(): Start");

            // NOTE: allow for these to be rebuilt because calling the LoadInitialCache() multiple times
            // with different criteria will allow us to get subsets of folders, and specific files selected

            vaultCache._fileMastersByFolderId = vaultCache._fileMasters.Values.ToLookup(fm => fm.FolderId, fm => fm);
            Logger.Debug(string.Format("_fileMastersByFolderId: {0:n0}", vaultCache._fileMastersByFolderId.Count));

            // get the masters table
            long[] fileMasterIDArray = vaultCache._fileMasters.Keys.ToArray();
            string sSelectString = "SELECT MasterID, TipIterationID, TipCommittedIterationID, MaxRevisionId, MaxConsumableRevisionId, ControlledByChangeOrder FROM dbo.Master WHERE MasterID IN @parm";
            vaultCache._mastersForFiles = vaultCache.cacheLoader.LoadVaultEntityDictionary<Master>(sSelectString, new { parm = fileMasterIDArray });
            Logger.Debug(string.Format("masters: {0:n0} {1}", vaultCache._mastersForFiles.Count, swp.LapTimeString()));


            // get the file iterations
            sSelectString = "SELECT IterationID, MasterID, IterationNumber, RevisionId, Comment FROM dbo.Iteration WHERE MasterID IN @parm";
            vaultCache._iterationsForFiles = vaultCache.cacheLoader.LoadVaultEntityDictionary<Iteration>(sSelectString, new { parm = vaultCache._fileMasters.Keys.ToArray() });
            Logger.Debug(string.Format("Total file iterations from FileMasterId: {0:n0} {1}", vaultCache._iterationsForFiles.Count, swp.LapTimeString()));
            vaultCache._initialFileIterationsDiscovered = vaultCache._iterationsForFiles.Count;



            // Controlled is no longer in table
            // remove the BOMBlob
            // string sSelectString = "SELECT FileIterationId, FileName, ModDate, CheckinDate, BOMBlob, DesignVisualizationAttachmentStatus, IsParsed, Controlled, LifeCycleStateName, CheckoutDate, ResourceId FROM dbo.FileIteration WHERE FileIterationId IN @parm";
            sSelectString = VaultDbEntityHelper.GetSelectString<FileIterationExtended>() + " WHERE FileIterationId IN @parm";
            vaultCache._fileIterationsExtended = vaultCache.cacheLoader.LoadVaultEntityDictionary<FileIterationExtended>(sSelectString, new { parm = vaultCache._iterationsForFiles.Keys.ToArray() });
            Logger.Debug(string.Format("fileIterations: {0:n0} {1}", vaultCache._fileIterationsExtended.Count, swp.LapTimeString()));


            vaultCache._fileIterationsByMasterId = vaultCache._iterationsForFiles.Values.ToLookup(it => it.MasterID, it => it);
            Logger.Debug(string.Format("_fileIterationsByMasterId: {0:n0}", vaultCache._fileIterationsByMasterId.Count));

            // get the MasterId associated with FileIterations 
            // by creating an anonymous object dynamically to facilitate the ToLookup on resultant list
            var masterIdFileIterList = (from iterKVP in vaultCache._iterationsForFiles
                                        join fileIterKVP in vaultCache._fileIterationsExtended
                                        on iterKVP.Key equals fileIterKVP.Key
                                        select new { masterId = iterKVP.Value.MasterID, fileIteration = fileIterKVP.Value });

            int nCount = masterIdFileIterList.Count();

            // create the lookup
            vaultCache._fileIterExtndsByMasterId = masterIdFileIterList.ToLookup(mifi => mifi.masterId, mifi => mifi.fileIteration);
            Logger.Debug(string.Format("_fileIterExtndsByMasterId: {0:n0}", vaultCache._fileIterExtndsByMasterId.Count));

            Logger.Debug(string.Format("FinalizeFileCacheForFileDiscovery(): Start {0}", swp.LapTimeString()));
        }




        // method to finalize cache and setup indexes
        static public void FinalizeFileCache(VaultCacheOld vaultCache)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("FinalizeFileCache(): Start");

            // NOTE: allow for these to be rebuilt because calling the LoadInitialCache() multiple times
            // with different criteria will allow us to get subsets of folders, and specific files selected

            vaultCache._fileMastersByFolderId = vaultCache._fileMasters.Values.ToLookup(fm => fm.FolderId, fm => fm);
            Logger.Debug(string.Format("_fileMastersByFolderId: {0:n0}", vaultCache._fileMastersByFolderId.Count));

            vaultCache._fileIterationsByMasterId = vaultCache._iterationsForFiles.Values.ToLookup(it => it.MasterID, it => it);
            Logger.Debug(string.Format("_fileIterationsByMasterId: {0:n0}", vaultCache._fileIterationsByMasterId.Count));

            // get the MasterId associated with FileIterations 
            // by creating an anonymous object dynamically to facilitate the ToLookup on resultant list
            var masterIdFileIterList = (from iterKVP in vaultCache._iterationsForFiles
                                        join fileIterKVP in vaultCache._fileIterationsExtended
                                        on iterKVP.Key equals fileIterKVP.Key
                                        select new { masterId = iterKVP.Value.MasterID, fileIteration = fileIterKVP.Value });

            int nCount = masterIdFileIterList.Count();

            // create the lookup
            vaultCache._fileIterExtndsByMasterId = masterIdFileIterList.ToLookup(mifi => mifi.masterId, mifi => mifi.fileIteration);
            Logger.Debug(string.Format("_fileIterExtndsByMasterId: {0:n0}", vaultCache._fileIterExtndsByMasterId.Count));


            //public ILookup<long, Revision> _fileRevisionsByMasterId;
            //public ILookup<long, FileResource> _fileResourcesByMasterId;
            //public ILookup<long, Entity> _fileEntitiesByMasterId;

            vaultCache._fileRevisionsByMasterId = vaultCache._revisionsForFiles.ToLookup(rev => rev.Value.MasterId, rev => rev.Value);
            Logger.Debug(string.Format("_fileRevisionsByMasterId: {0:n0}", vaultCache._fileRevisionsByMasterId.Count));

            vaultCache._fileResourcesByMasterId = vaultCache._fileResources.ToLookup(fr => fr.Value.FileMasterId, fr => fr.Value);
            Logger.Debug(string.Format("_fileResourcesByMasterId: {0:n0}", vaultCache._fileResourcesByMasterId.Count));


            // get the MasterId associated with Entities
            // by creating an anonymous object dynamically to facilitate the ToLookup on resultant list
            var masterIdEntitiesList = (from iterKVP in vaultCache._iterationsForFiles
                                        join entitiesKVP in vaultCache._entitiesForFiles
                                        on iterKVP.Key equals entitiesKVP.Key
                                        select new { masterId = iterKVP.Value.MasterID, entity = entitiesKVP.Value });

            // create the lookup
            vaultCache._fileEntitiesByMasterId = masterIdEntitiesList.ToLookup(mie => mie.masterId, mie => mie.entity);
            Logger.Debug(string.Format("entitiesByMasterId: {0:n0}", vaultCache._fileEntitiesByMasterId.Count));


            // associate properties specific to file iterations using
            // get the MasterId associated with Properties and PropertyDefs
            // by creating an anonymous object dynamically to facilitate the ToLookup on resultant list
            //var masterIdPropertyAndDefList = (from iterKVP in vaultCache._iterationsForFiles
            //                                    join propertiesKVP in vaultCache._fileIterationProperties
            //                                    on iterKVP.Key equals propertiesKVP.Value.EntityID
            //                                        join propertyDefKVP in vaultCache._fileIterationPropertyDefs
            //                                        on propertiesKVP.Value.PropertyDefID equals propertyDefKVP.Key
            //                                    select new { masterId = iterKVP.Value.MasterID, prop = propertiesKVP.Value, propDef = propertyDefKVP.Value });

            // setup the lookup tables
            //vaultCache._propertiesByFileMasterId = masterIdPropertyAndDefList.ToLookup(mipad => mipad.masterId, mipad => mipad.prop);
            //vaultCache._propertDefsByFileMasterId = masterIdPropertyAndDefList.ToLookup(mipad => mipad.masterId, mipad => mipad.propDef);
            //Logger.Debug(string.Format("propertiesByMasterId: {0:n0}", vaultCache._propertiesByFileMasterId.Count));
            //Logger.Debug(string.Format("propertDefsByMasterId: {0:n0}", vaultCache._propertDefsByFileMasterId.Count));


            // create the lifecycle lookup
            // vaultCache._lifeCycleStatesByLCDefId = vaultCache._lifeCycleStates.Values.ToLookup(lcs => lcs.LifeCycleDefId, lcs => lcs);

            Logger.Debug(string.Format("FinalizeFileCache(): Start {0}", swp.LapTimeString()));
        }

        // method to finalize cache and setup indexes
        static public void FinalizeItemCache(VaultCacheOld vaultCache)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("FinalizeItemCache(): Start");

            // NOTE: TODO: allow for these to be rebuilt because calling the LoadInitialCache() multiple times
            // with different criteria will allow us to get subsets of folders, and specific files selected

            vaultCache._itemIterationsByMasterId = vaultCache._iterationsForItems.Values.ToLookup(it => it.MasterID, it => it);
            Logger.Debug(string.Format("_itemIterationsByMasterId: {0:n0}", vaultCache._itemIterationsByMasterId.Count));


            //public ILookup<long, Revision> _fileRevisionsByMasterId;
            //public ILookup<long, FileResource> _fileResourcesByMasterId;
            //public ILookup<long, Entity> _fileEntitiesByMasterId;

            vaultCache._itemRevisionsByMasterId = vaultCache._revisionsForItems.ToLookup(rev => rev.Value.MasterId, rev => rev.Value);
            Logger.Debug(string.Format("_fileRevisionsByMasterId: {0:n0}", vaultCache._fileRevisionsByMasterId.Count));

            // get the MasterId associated with Entities
            // by creating an anonymous object dynamically to facilitate the ToLookup on resultant list
            var masterIdEntitiesList = (from iterKVP in vaultCache._iterationsForItems
                                        join entitiesKVP in vaultCache._entitiesForItems
                                        on iterKVP.Key equals entitiesKVP.Key
                                        select new { masterId = iterKVP.Value.MasterID, entity = entitiesKVP.Value });

            // create the lookup
            vaultCache._itemEntitiesByMasterId = masterIdEntitiesList.ToLookup(mie => mie.masterId, mie => mie.entity);
            Logger.Debug(string.Format("_itemEntitiesByMasterId: {0:n0}", vaultCache._itemEntitiesByMasterId.Count));


            // associate properties specific to file iterations using
            // get the MasterId associated with Properties and PropertyDefs
            // by creating an anonymous object dynamically to facilitate the ToLookup on resultant list
            //var masterIdPropertyAndDefList = (from iterKVP in vaultCache._iterationsForFiles
            //                                    join propertiesKVP in vaultCache._fileIterationProperties
            //                                    on iterKVP.Key equals propertiesKVP.Value.EntityID
            //                                        join propertyDefKVP in vaultCache._fileIterationPropertyDefs
            //                                        on propertiesKVP.Value.PropertyDefID equals propertyDefKVP.Key
            //                                    select new { masterId = iterKVP.Value.MasterID, prop = propertiesKVP.Value, propDef = propertyDefKVP.Value });

            // setup the lookup tables
            //vaultCache._propertiesByFileMasterId = masterIdPropertyAndDefList.ToLookup(mipad => mipad.masterId, mipad => mipad.prop);
            //vaultCache._propertDefsByFileMasterId = masterIdPropertyAndDefList.ToLookup(mipad => mipad.masterId, mipad => mipad.propDef);
            //Logger.Debug(string.Format("propertiesByMasterId: {0:n0}", vaultCache._propertiesByFileMasterId.Count));
            //Logger.Debug(string.Format("propertDefsByMasterId: {0:n0}", vaultCache._propertDefsByFileMasterId.Count));


            // create the lifecycle lookup
            // vaultCache._lifeCycleStatesByLCDefId = vaultCache._lifeCycleStates.Values.ToLookup(lcs => lcs.LifeCycleDefId, lcs => lcs);

            Logger.Debug(string.Format("FinalizeItemCache(): Start {0}", swp.LapTimeString()));
        }


        // setup VaultFileMasterOld(s) for given information
        static protected void SetupVaultFileMasterListForFileDiscovery(VaultCacheOld vaultCache)
        {
            Logger.Debug(string.Format("SetupVaultFileMasterListForFileDiscovery() start"));

            vaultCache._vaultFileMasters = new List<VaultFileMasterOld>();
            foreach (FileMaster fm in vaultCache._fileMasters.Values)
            {
                VaultFileMasterOld vfm = new VaultFileMasterOld(fm, vaultCache, true);
                vaultCache._vaultFileMasters.Add(vfm);
            }


            // create the dictionarys
            vaultCache._vaultFileMastersByFullVaultFilePathDictionary = vaultCache._vaultFileMasters.ToDictionary(vfm => vfm.VaultFullFilePathName, vfm => vfm, StringComparer.CurrentCultureIgnoreCase);
            vaultCache._vaultFileMastersByFileMasterIdDictionary = vaultCache._vaultFileMasters.ToDictionary(vfm => vfm.mFileMasterID, vfm => vfm);


            Logger.Debug(string.Format("_vaultFileMastersByFullVaultFilePathDictionary: {0:n0}", vaultCache._vaultFileMastersByFullVaultFilePathDictionary.Count));
        }



        // setup VaultFileMasterOld(s) for given information
        static protected void SetupVaultFileMasterList(VaultCacheOld vaultCache)
        {
            Logger.Debug(string.Format("SetupVaultFileMasterList() start"));

            vaultCache._vaultFileMasters = new List<VaultFileMasterOld>();
            foreach (FileMaster fm in vaultCache._fileMasters.Values)
            {
                VaultFileMasterOld vfm = new VaultFileMasterOld(fm, vaultCache);
                vaultCache._vaultFileMasters.Add(vfm);
            }


            // create the dictionarys
            vaultCache._vaultFileMastersByFullVaultFilePathDictionary = vaultCache._vaultFileMasters.ToDictionary(vfm => vfm.VaultFullFilePathName, vfm => vfm, StringComparer.CurrentCultureIgnoreCase);
            vaultCache._vaultFileMastersByFileMasterIdDictionary = vaultCache._vaultFileMasters.ToDictionary(vfm => vfm.mFileMasterID, vfm => vfm);

            // Create a collection FileIterationIds, Iterations, and VaultFileMasters so that we can generate the _vaultFileMastersByFileIterationIdDictionary
            var query = from fie in vaultCache._fileIterationsExtended.Values
                        join vfm in vaultCache._vaultFileMastersByFileMasterIdDictionary.Values on fie.FileMasterId equals vfm.mFileMasterID
                        select new { fileIterationId = fie.FileIterationId, vaultFileMaster = vfm };


            vaultCache._vaultFileMastersByFileIterationIdDictionary = query.ToDictionary(qu => qu.fileIterationId, qu => qu.vaultFileMaster);

            // identify those that are pointing to multiple generics
            if (vaultCache._fileMasterIDsPointingToMultipleGenerics != null)
            {
                vaultCache._fileMasterIDsPointingToMultipleGenerics.Select(fmi => vaultCache._vaultFileMastersByFileMasterIdDictionary[fmi]).ToList().ForEach(vfm => vfm.mPointsToMoreThanOneGeneric = true);
            }


            Logger.Debug(string.Format("_vaultFileMastersByFullVaultFilePathDictionary: {0:n0}", vaultCache._vaultFileMastersByFullVaultFilePathDictionary.Count));
        }


        // load all files specifically given
        static public void GetFileMastersAndMissingFromSpecificFiles(   List<string> sVaultFullFilePathNameList,
                                                                        VaultCacheOld vaultCache,
                                                                        out List<FileMaster> fileMasterList,
                                                                        out List<string> missingFilesList)
        {
            Logger.Debug("GetFileMastersAndMissingFromSpecificFiles: Start");

            StopwatchPlus swp = new StopwatchPlus();

            // check to see if the CachLoader has been built yet
            if (vaultCache.cacheLoader == null)
            {
                vaultCache.cacheLoader = new MSSqlVaultCacheLoader(vaultCache._connectionString);
            }

            // get the subfolders of interest by finding the common folders, make sure they are distinct case insensitive
            List<string> sVaultPathList = sVaultFullFilePathNameList.Select(vffpn => GetVaultPath(vffpn))
                                                .GroupBy(g => g, StringComparer.CurrentCultureIgnoreCase)
                                                .Select(grp => grp.First()).ToList();



            // get the folders from the resultant list
            List<Folder> selectFolders = vaultCache._folders.Values.Where(fldr => sVaultPathList.Contains(fldr.VaultPath, StringComparer.CurrentCultureIgnoreCase)).ToList();
            Logger.Debug(string.Format("Total Subfolders Found: {0:n0}{1}{2}", selectFolders.Count, Environment.NewLine, string.Join(Environment.NewLine, selectFolders.Select(vf => vf.VaultPath))));
                                            

            // identify the file masters in those folders
            long[] folderIDArray = selectFolders.Select(fldr => fldr.FolderID).ToArray();
            string sSelectString = "SELECT FileMasterID, CheckedOut, CheckoutLocalSpec, CheckoutMachine, FileClassification, Hidden, FileStatus, Unmanaged, FolderId, TipFileBaseName FROM dbo.FileMaster WHERE FolderId IN @parm";
            Dictionary<long, FileMaster> fileMasters = vaultCache.cacheLoader.LoadVaultEntityDictionary<FileMaster>(sSelectString, new { parm = folderIDArray });
            Logger.Debug(string.Format("total fileMasters in folders: {0:n0}", fileMasters.Count));

            // create a lookup to quickly get the subset of files, TipFileBaseName is the filename without extension
            ILookup<string, FileMaster> fileMasterByBaseFileNameLookup = fileMasters.Values.ToLookup(fm => fm.TipFileBaseName, fm => fm, StringComparer.CurrentCultureIgnoreCase);

            // for speed, get the subset of file masters that may fit the files of interest
            // can only filter on the base filename, as the master does not have the extension
            List<string> sUniqueBaseFilenames = sVaultFullFilePathNameList.Select(fn => Path.GetFileNameWithoutExtension(fn)).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();

            // get the subset of FileMasters
            Dictionary<long, FileMaster> fileMastersSubset = sUniqueBaseFilenames.SelectMany(bn => fileMasterByBaseFileNameLookup[bn]).ToDictionary(fm => fm.FileMasterID, fm => fm);

            // get the tip file iterations
            sSelectString = FileIterationExtended.GetTipCommitedSelectString() + " WHERE MasterId IN @parm";
            Dictionary<long, FileIterationExtended> fileIterExtended = vaultCache.cacheLoader.LoadVaultEntityDictionary<FileIterationExtended>(sSelectString, new { parm = fileMastersSubset.Keys.ToArray() });

            // get the full pathname for the master files
            // Folder.VaultPath, FileMaster.FolderId, Master.MasterID, FileIteration.FileName
            // FileIteration.FileIterationId == Master.TipTipCommittedIterationID
            // Master.MasterId == FileMaster.FileMasterId
            // FileMaster.FolderId = Folder.FolderId

            // Create a collection of the objects combined so that we can generate the vault full path
            var query = from fi in fileIterExtended.Values
                        join fm in fileMasters.Values on fi.FileMasterId equals fm.FileMasterID
                        join fldr in selectFolders on fm.FolderId equals fldr.FolderID
                        select new { fileIteration = fi, fileMaster = fm, folder = fldr};

            // from the query, make a dictionary of vault full path
            // make sure to use the "renamed" filename
            Dictionary<string, FileMaster> vaultPathFileMasterDictionary = query.ToDictionary(an => an.folder.VaultPath + "/" + an.fileMaster.TipFileBaseName + Path.GetExtension(an.fileIteration.FileName), an => an.fileMaster, StringComparer.CurrentCultureIgnoreCase);

            // now select those FileMaster objects that have the matching full path
            fileMasterList = new List<FileMaster>();
            missingFilesList = new List<string>();
            foreach (string sVaultPathFileName in sVaultFullFilePathNameList)
            {
                if (vaultPathFileMasterDictionary.ContainsKey(sVaultPathFileName))
                {
                    fileMasterList.Add(vaultPathFileMasterDictionary[sVaultPathFileName]);
                }
                else
                {
                    missingFilesList.Add(sVaultPathFileName);
                }
            }

            Logger.Debug(string.Format("GetFileMastersAndMissingFromSpecificFiles: End {0}", swp.ElapsedTimeString()));
        }

        // load all files specifically given
        static public void LoadCacheFromSpecificFiles(List<string> sVaultFullFilePathNameList, VaultCacheOld vaultCache, bool bDiscoverChildren, bool bGetAllGenerics)
        {
            Logger.Debug("VaultCacheOld LoadCacheFromSpecificFiles: Start");

            StopwatchPlus swp = new StopwatchPlus();

            // check to see if the CachLoader has been built yet
            if (vaultCache.cacheLoader == null)
            {
                vaultCache.cacheLoader = new MSSqlVaultCacheLoader(vaultCache._connectionString);
            }

            // get the FileMasters we can and identify those that could not be loaded
            List<FileMaster> fileMasterList;
            List<string> missingFilesList;
            GetFileMastersAndMissingFromSpecificFiles(sVaultFullFilePathNameList, vaultCache, out fileMasterList, out missingFilesList);


            // create the dictionare of files to process
            vaultCache._fileMasters = fileMasterList.ToDictionary(fm => fm.FileMasterID, fm => fm);

            Logger.Debug(string.Format("VaultCacheOld LoadCacheFromSpecificFiles: Initial Setup {0}", swp.LapTimeString()));


            // log the missing files
            Logger.Debug(string.Format("Files not found in Vault: {0:n0}{1}{2}", missingFilesList.Count, Environment.NewLine, string.Join(Environment.NewLine, missingFilesList)));

            // once the file masters have been acquired, get the remainder of the information
            LoadRemainderOfFileCacheInformation(vaultCache, "", bDiscoverChildren, bGetAllGenerics);

            Logger.Debug(string.Format("VaultCacheOld LoadCacheFromSpecificFiles: End {0}", swp.ElapsedTimeString()));
        }



        // load all files specifically given and then find those that MAY have been moved
        static public void LoadCacheFromSpecificFilesAndFindMoved(List<string> sVaultFullFilePathNameList, VaultCacheOld vaultCache, bool bDiscoverChildren, bool bGetAllGenerics)
        {
            Logger.Debug("LoadCacheFromSpecificFilesAndFindMoved: Start");

            StopwatchPlus swp = new StopwatchPlus();

            // check to see if the CachLoader has been built yet
            if (vaultCache.cacheLoader == null)
            {
                vaultCache.cacheLoader = new MSSqlVaultCacheLoader(vaultCache._connectionString);
            }

            // get the FileMasters we can and identify those that could not be loaded
            List<FileMaster> foundFileMasterList;
            List<string> missingFullFilePathNameList;
            GetFileMastersAndMissingFromSpecificFiles(sVaultFullFilePathNameList, vaultCache, out foundFileMasterList, out missingFullFilePathNameList);


            // from the missingFilesList, see if we can find them anywhere in the Vault
            List<string> missingFileNamesList = missingFullFilePathNameList.Select(st => Path.GetFileName(st)).ToList();
            missingFileNamesList = missingFileNamesList.Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();

            // get the tip file iterations where the filename matches that of the files we are missing
            // cannot do more than 2100 in a parameterized list
            string sSelectString = FileIterationExtended.GetTipCommitedSelectString() + " WHERE FI.FileName IN @parm";
            int nIter = 0;
            List<FileIterationExtended> missingFileIterExtendedList = new List<FileIterationExtended>();
            do
            {
                missingFileIterExtendedList.AddRange(vaultCache.cacheLoader.LoadVaultEntities<FileIterationExtended>(sSelectString, new { parm = missingFileNamesList.Skip(nIter*2000).Take(2000).ToArray() }));

                nIter++;
            } while ((nIter * 2000) < missingFileNamesList.Count());

            // get the file masters for these FileIterationExtended objects
            List<long> masterIdList = missingFileIterExtendedList.Select(fi => fi.FileMasterId).ToList();
            masterIdList = masterIdList.Distinct().ToList();

            sSelectString = (new FileMaster()).GetSelectString() + " WHERE FileMasterID IN @parm";
            List<FileMaster> missingFileMasterList = vaultCache.cacheLoader.LoadVaultEntities<FileMaster>(sSelectString, new { parm = masterIdList.ToArray() });

            // add them to the found list
            foundFileMasterList.AddRange(missingFileMasterList);
            foundFileMasterList = foundFileMasterList.Distinct().ToList();

            // find the non-unique FileMasterID
            ILookup<long, FileMaster> fileMasterLookupByMasterId = foundFileMasterList.ToLookup(fm => fm.FileMasterID, fm => fm);
            var duplicates = fileMasterLookupByMasterId.Where(grp => grp.Count() > 1);
            int nDuplicates = duplicates.Count();

            // verify that the folder ID's are all the same for the same MasterId
            foreach (IGrouping<long, FileMaster> groupItem in duplicates)
            {
                // groupItem.Key = FileMasterID
                // groupItem is IEnumerable<FileMaster>
                Debug.Assert(groupItem.Select(fm => fm.FolderId).Distinct().Count() == 1);
            }

            // get the first FileMaster from the lookup
            foundFileMasterList = fileMasterLookupByMasterId.Select(grp => grp.First()).ToList();

            // create the dictionary of files to process
            vaultCache._fileMasters = foundFileMasterList.ToDictionary(fm => fm.FileMasterID, fm => fm);

            Logger.Debug(string.Format("VaultCacheOld LoadCacheFromSpecificFiles: Initial Setup {0}", swp.LapTimeString()));


            // log the missing files
            Logger.Debug(string.Format("Files not found in Vault: {0:n0}{1}{2}", missingFullFilePathNameList.Count, Environment.NewLine, string.Join(Environment.NewLine, missingFullFilePathNameList)));

            // once the file masters have been acquired, get the remainder of the information
            LoadRemainderOfFileCacheInformation(vaultCache, "", bDiscoverChildren, bGetAllGenerics);

            Logger.Debug(string.Format("LoadCacheFromSpecificFilesAndFindMoved: End {0}", swp.ElapsedTimeString()));
        }




        // get ALL of the Generic parent associations to be inclusive of all potential generics
        static public Dictionary<long, FileMaster> GetAllFileMasterGenericInstances(VaultCacheOld vaultCache, bool bGetAllGenerics, List<FileMaster> fileMasterList)
        {
            Logger.Debug("VaultCacheOld GetAllFileMasterGenericInstances: Start");

            StopwatchPlus swp = new StopwatchPlus();

            // get all of the file associations where Source contains 'FamTableRelation:%'
            // we do not have vaultCache._fileAssociationsExtended populated yet, so we have to get it directly from the database
            string sSelectString;
            if (vaultCache._familyFileAssociationsExtended == null)
            {
                sSelectString = FileAssociationExtended.GetAllSelectionString() + " WHERE Source LIKE 'FamTableRelation:%'";
                vaultCache._familyFileAssociationsExtended = vaultCache.cacheLoader.LoadVaultEntities<FileAssociationExtended>(sSelectString);
            }

            // setup a lookup with the ToMasterId and FromMasterId as the key
            ILookup<long, FileAssociationExtended> faeByFromMasterIdLookup = vaultCache._familyFileAssociationsExtended.ToLookup(fae => fae.FromMasterID, fae => fae);
            ILookup<long, FileAssociationExtended> faeByToMasterIdLookup = vaultCache._familyFileAssociationsExtended.ToLookup(fae => fae.ToMasterID, fae => fae);

            // iterate through and get ALL of the family parent child relationships from these masters UP
            List<long> toMasterIds = new List<long>();
            toMasterIds.AddRange(fileMasterList.Select(fm => fm.FileMasterID));

            List<long> allMasterIds = new List<long>();
            allMasterIds.AddRange(vaultCache._fileMasters.Keys.Concat(toMasterIds).Distinct());

            int nLoop = 0;
            do
            {
                nLoop++;

                // get the FileAssocations where the ToId is found in the fromIdByToIdLookup
                var toFAEList = toMasterIds.SelectMany(mi => faeByToMasterIdLookup[mi]);

                // remove masters already found and group FileAssociationExtended by FromMasterId
                List<long>fromMasterIds = toFAEList.Select(fae => fae.FromMasterID).Distinct().Except(allMasterIds).ToList();

                // use all of these fromMasterIds if bGetAllGenerics is true
                var fromFAEList = fromMasterIds.SelectMany(mi => faeByFromMasterIdLookup[mi]);
                ILookup<long, FileAssociationExtended> faeLookup = fromFAEList.ToLookup(fae => fae.FromMasterID, fae => fae);

                // strip off all but the latest for each master
                var multipleToGenerics = faeLookup.Where(fas => fas.Count() > 1);
                if (multipleToGenerics.Any())
                {
                    foreach (IGrouping<long, FileAssociationExtended> ig in multipleToGenerics)
                    {
                        long fromFileMasterId = ig.Key;
                        FileAssociationExtended lastFAE = ig.OrderByDescending(fae => fae.FileAssociationId).First();

                        // check to see if the ToMaster is in the current toMasterIds list
                        if (!toMasterIds.Contains(lastFAE.ToMasterID))
                        {
                            vaultCache._fileMasterIDsPointingToMultipleGenerics.Add(fromFileMasterId);

                            // remove it from the list of files to process
                            if (!bGetAllGenerics)
                            {
                                fromMasterIds.Remove(fromFileMasterId);
                            }
                        }
                    }
                }

                // remaining list of fromMasterIds becomes the toMasterIds
                toMasterIds = fromMasterIds;
                allMasterIds.AddRange(toMasterIds);

            } while (toMasterIds.Count > 0);


            // make sure they are unique
            allMasterIds = allMasterIds.Distinct().ToList();
            vaultCache._fileMasterIDsPointingToMultipleGenerics = vaultCache._fileMasterIDsPointingToMultipleGenerics.Distinct().ToList();


            // find all of the FileMasters with these MasterIds
            sSelectString = "SELECT FileMasterID, CheckedOut, CheckoutLocalSpec, CheckoutMachine, FileClassification, Hidden, FileStatus, Unmanaged, FolderId, TipFileBaseName FROM dbo.FileMaster WHERE FileMasterID IN @parm";
            Dictionary<long, FileMaster> fileMasters = vaultCache.cacheLoader.LoadVaultEntityDictionary<FileMaster>(sSelectString, new { parm = allMasterIds.ToArray() });
            Logger.Debug(string.Format("fileMasters: {0:n0}", fileMasters.Count));


            Logger.Debug(string.Format("VaultCacheOld GetAllFileMasterGenericInstances: End {0}", swp.ElapsedTimeString()));

            return (fileMasters);
        }




        //// get ALL of the Generic parent associations to be inclusive of all potential generics
        //static public Dictionary<long, FileMaster> GetAllFileMasterGenericInstances(VaultCacheOld vaultCache, List<FileMaster> fileMasterList)
        //{
        //    Logger.Debug("VaultCacheOld GetAllFileMasterGenericInstances: Start");

        //    StopwatchPlus swp = new StopwatchPlus();

        //    // get all of the file associations where Source contains 'FamTableRelation:%'
        //    // we do not have vaultCache._fileAssociationsExtended populated yet, so we have to get it directly from the database
        //    FileAssociationExtended fae = new FileAssociationExtended();
        //    string sSelectString = fae.GetSelectString() + " WHERE Source LIKE 'FamTableRelation:%'";
        //    Dictionary<long, FileAssociationExtended> familyAssocExtended = vaultCache.cacheLoader.LoadVaultEntityDictionary<FileAssociationExtended>(sSelectString);

        //    // setup a lookup with the FromId as the key
        //    ILookup<long, FileAssociation> familyAssocByFromIdLookup = familyAssocations.Values.ToLookup(fa => fa.ToId, fa => fa);
        //    List<long> fromIterIdList = familyAssocations.Values.Select(fa => fa.FromId).Distinct().ToList();

        //    // setup a lookup with the ToId as the key
        //    ILookup<long, FileAssociation> familyAssocByToIdLookup = familyAssocations.Values.ToLookup(fa => fa.ToId, fa => fa);
        //    List<long> toIterIdList = familyAssocations.Values.Select(fa => fa.ToId).Distinct().ToList();


        //    // create a list to manage the file masters being processed
        //    Dictionary<long, FileMaster> allFileMastersDictionary = new Dictionary<long, FileMaster>;
        //    List<FileMaster> currentFileMasters = new List<FileMaster>();
        //    currentFileMasters.AddRange(fileMasterList);

        //    // create a dictionary of the FileMasters
        //    Dictionary<long, FileMaster> fileMasterDictionary = new Dictionary<long, FileMaster>();

        //    // create a list of the viable To FilterIterationIds
        //    List<long> viableToFileIterationIds = new List<long>();

        //    // iterate until we cannot find anymore instances
        //    do
        //    {
        //        // get all the iterations of the current file masters
        //        sSelectString = "SELECT IterationID, MasterID, IterationNumber, RevisionId, Comment FROM dbo.Iteration WHERE MasterID IN @parm";
        //        Dictionary<long, Iteration> iterationsDictionary = vaultCache.cacheLoader.LoadVaultEntityDictionary<Iteration>(sSelectString, new { parm = currentFileMasters.Select(fm => fm.FileMasterID).ToArray() });

        //        // create an iterations to MasterId dictionary
        //        Dictionary<long, long> masterIdByIterIdDictionary = iterationsDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.MasterID);

        //        // verify these are not pointing to multiple generics
        //        // and compare against the viable TO file iterations
        //        if (viableToFileIterationIds.Count() > 0)
        //        {
        //            List<long> fromFamilyIterIdList = masterIdByIterIdDictionary.Keys.Intersect(fromIterIdList).ToList();
        //            List<FileAssociation> fromFamAssocations = fromFamilyIterIdList.SelectMany(fit => familyAssocByFromIdLookup[fit]).ToList();

        //            ILookup<long, FileAssociation> famAssocByMasterIdLookup = fromFamAssocations.ToLookup(ffa => masterIdByIterIdDictionary[ffa.FromId], ffa => ffa);

        //            var multipleToGenerics = famAssocByMasterIdLookup.Where(fas => fas.Count() > 1);
        //            if (multipleToGenerics.Any())
        //            {
        //                foreach (IGrouping<long, FileAssociation> ig in multipleToGenerics)
        //                {
        //                    long fileMasterId = ig.Key;
        //                    FileAssociation lastFA = ig.OrderByDescending(fa => fa.FileAssociationId).First();

        //                    // check to see if the ToId is in the viable IterationId list
        //                    if (!viableToFileIterationIds.Contains(lastFA.ToId))
        //                    {
        //                    }
        //                }
        //            }
        //        }

        //        // add the remainder to the viable IterationsId list
        //        viableToFileIterationIds.AddRange(iterationsDictionary.Keys);


        //        // intersect the iterations with the familyAssocByToIdLookup
        //        List<long> familyIterIdList = masterIdByIterIdDictionary.Keys.Intersect(toIterIdList).ToList();
        //        familyIterIdList.Select()


        //        // verify these are not pointing to multiple generics before you add them
        //        currentFileMasters.ForEach(fm => allFileMastersDictionary.Add(fm.FileMasterID, fm));



        //    } while (currentFileMasters.Count() > 0);




        //    // build a lookup for the FileMasters and Iterations
        //    ILookup<FileMaster, Iteration> fileMasterLookup = fileMasterList.Intersect()

        //        var fileMasterIds = vaultCache._iterationsForFiles.Select(it => it.Value.MasterID).Distinct().ToList();
        //    vaultCache._fileMasters = fileMasterIds.Intersect(vaultCache._fileMasters.Keys)
        //                                .Select(k => vaultCache._fileMasters[k]).ToDictionary(fm => fm.FileMasterID, fm => fm);


        //    // iterate through and get ALL of the family parent child relationships from these masters UP
        //    List<long> allIterationIds = new List<long>();
        //    allIterationIds.AddRange(baseIterations.Keys);
        //    List<long> toIterationIds = new List<long>();
        //    toIterationIds.AddRange(baseIterations.Keys);
        //    do
        //    {
        //        // get the FileAssocations where the ToId is found in the fromIdByToIdLookup
        //        toIterationIds = toIterationIds.SelectMany(it => fromIdByToIdLookup[it]).Distinct().Except(allIterationIds).ToList();

        //        allIterationIds.AddRange(toIterationIds);

        //    } while (toIterationIds.Count > 0);


        //    // make sure they are unique
        //    allIterationIds = allIterationIds.Distinct().ToList();


        //    // find all of the Iterations with these IterationIds
        //    sSelectString = "SELECT IterationID, MasterID, IterationNumber, RevisionId, Comment FROM dbo.Iteration WHERE IterationID IN @parm";
        //    Dictionary<long, Iteration> allIterations = vaultCache.cacheLoader.LoadVaultEntityDictionary<Iteration>(sSelectString, new { parm = allIterationIds.ToArray() });

        //    // verify we got all
        //    var missingIterationIds = allIterationIds.Except(allIterations.Keys);
        //    long nMissingCount = missingIterationIds.Count();


        //    // !!! verify that all instances only point to one generic or nested
        //    var FromIterIdAndToIterIdAOList = familyAssocations.Where(kvp => (allIterationIds.Contains(kvp.Value.FromId) && allIterationIds.Contains(kvp.Value.ToId)))
        //                                            .Select(kvp => new { fromIterId = kvp.Value.FromId, toIterId = kvp.Value.ToId });
        //    long nFromToCount = FromIterIdAndToIterIdAOList.Count();

        //    var FromMasterIdAndToIterIdAOList =
        //        FromIterIdAndToIterIdAOList.Select(ao => new { fromMasterId = allIterations[ao.fromIterId].MasterID, toIterId = ao.toIterId }).Distinct();
        //    nFromToCount = FromMasterIdAndToIterIdAOList.Count();

        //    var FromMasterIdAndToMasterIdAOList =
        //        FromMasterIdAndToIterIdAOList.Select(ao => new { fromMasterId = ao.fromMasterId, toMasterId = allIterations[ao.toIterId].MasterID }).Distinct();
        //    nFromToCount = FromMasterIdAndToMasterIdAOList.Count();

        //    // make a lookup on the fromMasterId
        //    ILookup<long, long> fromMasterIdToMasterIdLookup = FromMasterIdAndToMasterIdAOList.ToLookup(ao => ao.fromMasterId, ao => ao.toMasterId);
        //    nFromToCount = fromMasterIdToMasterIdLookup.Count();

        //    var duplicates = fromMasterIdToMasterIdLookup.Where(kvp => kvp.Count() > 1);


        //    // find all of the FileMasters with these MasterIds
        //    sSelectString = "SELECT FileMasterID, CheckedOut, CheckoutLocalSpec, CheckoutMachine, FileClassification, Hidden, FileStatus, Unmanaged, FolderId, TipFileBaseName FROM dbo.FileMaster WHERE FileMasterID IN @parm";
        //    Dictionary<long, FileMaster> fileMasters = vaultCache.cacheLoader.LoadVaultEntityDictionary<FileMaster>(sSelectString, new { parm = allIterations.Select(it => it.Value.MasterID).Distinct().ToArray() });
        //    Logger.Debug(string.Format("fileMasters: {0:n0}", fileMasters.Count));


        //    Logger.Debug(string.Format("VaultCacheOld GetAllFileMasterGenericInstances: End {0}", swp.ElapsedTimeString()));

        //    return (fileMasters);
        //}



        // load all files found in specific folders
        // TODO: having an additional List<string> sVaultFullFilePathNameList just to handle "$/Designs_2014.ipj" is a little kludgy
        // re-design methods to allow for capturing all of the fileMasters separately and merge into one vaultCache._fileMasters list
        static public void LoadCacheFilesFromSubfolders(string vaultSearchFolders, string vaultExtensionFilters, VaultCacheOld vaultCache, bool bDiscoverChildren, bool bGetAllGenerics, string sInventorProjectFile = null)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("VaultCacheOld LoadCacheFilesFromSubfolders: Start");


            // check to see if the MSSqlVaultCacheLoader has been enabled
            if (vaultCache.cacheLoader == null)
            {
                vaultCache.cacheLoader = new MSSqlVaultCacheLoader(vaultCache._connectionString);
            }

            // get the subfolders of interest
            // this string should be pipe delimited
            // such as "$/Content Center Files|$/Libraries|$/Standard Parts"
            // there may be leading or trailing characters for ease of reading
            // trim off the whitespaces
            List<string> vaultFolderList = vaultSearchFolders.Split('|').Select(st => st.Trim()).ToList();

            // get the folders that look like these, and get the numbers
            Logger.Debug(string.Format("Getting Files from Vault Subfolders: \"{0}\"", vaultSearchFolders));
            List<Folder> vaultSubFoldersList = new List<Folder>();
            foreach (string sVaultFolder in vaultFolderList)
            {
                List<Folder> currentSubFoldersList = vaultCache._foldersByPathDictionary.Where(kvp => ((string.Compare(kvp.Key, sVaultFolder, true) == 0) ||
                                                                                    (kvp.Key.IndexOf(sVaultFolder + "/", 0, StringComparison.CurrentCultureIgnoreCase) == 0))).Select(kvp => kvp.Value).ToList();

                Logger.Debug(string.Format("Folders in \"{0}\": {1:n0} {2}", sVaultFolder, currentSubFoldersList.Count, swp.LapTimeString()));


                // make a union so that we can get the unique folders
                vaultSubFoldersList = vaultSubFoldersList.Union(currentSubFoldersList).ToList();
            }
            vaultCache._vaultSearchFoldersCount = vaultSubFoldersList.Count;
            Logger.Debug(string.Format("Total Subfolders Found: {0:n0} {1}", vaultSubFoldersList.Count, swp.LapTimeString()));


            // TODO: roll these common queries into their objects and/or into the CachLoader template mechanism
            // TODO: set the IVaultDbEntity.GetSelectString() method as static so we can get the select string without contructing an object

            // get a list of the remaining content taking in consideration we want to only look at
            // the files in the given subfolders
            long[] folderIDArray = vaultSubFoldersList.Select(fldr => fldr.FolderID).ToArray();
            string sSelectString = "SELECT FileMasterID, CheckedOut, CheckoutLocalSpec, CheckoutMachine, FileClassification, Hidden, FileStatus, Unmanaged, FolderId, TipFileBaseName FROM dbo.FileMaster WHERE FolderId IN @parm";
            vaultCache._fileMasters = vaultCache.cacheLoader.LoadVaultEntityDictionary<FileMaster>(sSelectString, new { parm = folderIDArray });
            Logger.Debug(string.Format("Total FileMasters: {0:n0} {1}", vaultCache._fileMasters.Count, swp.LapTimeString()));
            vaultCache._totalFilesWithinSearchFolders = vaultCache._fileMasters.Count;


            // get the Inventor project file if it has been supplied
            if (sInventorProjectFile != null)
            {
                FileMaster invProjectFileMaster = GetFileMaster(sInventorProjectFile, vaultCache);
                vaultCache._fileMasters.Add(invProjectFileMaster.FileMasterID, invProjectFileMaster);
            }


            // once the file masters have been acquired, get the remainder of the information
            LoadRemainderOfFileCacheInformation(vaultCache, vaultExtensionFilters, bDiscoverChildren, false);

            Logger.Debug("VaultCacheOld LoadCacheFilesFromSubfolders: End");
        }


        // get the FileMaster for a specific file
        static public FileMaster GetFileMaster(string sVaultFullFilePathName, VaultCacheOld vaultCache)
        {
            Logger.Debug("VaultCacheOld GetFileMaster: Start");

            // check to see if the CachLoader has been built yet
            if (vaultCache.cacheLoader == null)
            {
                vaultCache.cacheLoader = new MSSqlVaultCacheLoader(vaultCache._connectionString);
            }

            // get the folder
            string sVaultPath = GetVaultPath(sVaultFullFilePathName);
            Folder selectFolder = vaultCache._foldersByPathDictionary[sVaultPath];
            List<Folder> selectFolders = new List<Folder> { selectFolder };

            // identify the file masters in those folders
            long[] folderIDArray = new long[] { selectFolder.FolderID };
            string sSelectString = "SELECT FileMasterID, CheckedOut, CheckoutLocalSpec, CheckoutMachine, FileClassification, Hidden, FileStatus, Unmanaged, FolderId, TipFileBaseName FROM dbo.FileMaster WHERE FolderId IN @parm";
            Dictionary<long, FileMaster> fileMasters = vaultCache.cacheLoader.LoadVaultEntityDictionary<FileMaster>(sSelectString, new { parm = folderIDArray });
            Logger.Debug(string.Format("fileMasters: {0:n0}", fileMasters.Count));


            // get the Master objects for those FileMaster objects
            long[] fileMasterIDArray = fileMasters.Values.Select(fm => fm.FileMasterID).ToArray();
            sSelectString = "SELECT MasterID, TipIterationID, TipCommittedIterationID, MaxRevisionId, MaxConsumableRevisionId, ControlledByChangeOrder FROM dbo.Master WHERE MasterID IN @parm";
            Dictionary<long, Master> masters = vaultCache.cacheLoader.LoadVaultEntityDictionary<Master>(sSelectString, new { parm = fileMasterIDArray });
            Logger.Debug(string.Format("masters: {0:n0}", masters.Count));

            // get the tip file iterations
            long[] tipFileIterationIDArray = masters.Values.Select(mast => mast.TipCommittedIterationID.Value).ToArray();
            sSelectString = "SELECT FileIterationId, FileName, ModDate, CheckinDate, BOMBlob, DesignVisualizationAttachmentStatus, IsParsed, Controlled, LifeCycleStateName, CheckoutDate, ResourceId FROM dbo.FileIteration WHERE FileIterationId IN @parm";
            Dictionary<long, FileIteration> fileIterations = vaultCache.cacheLoader.LoadVaultEntityDictionary<FileIteration>(sSelectString, new { parm = tipFileIterationIDArray });

            // get the full pathname for the master files
            // Folder.VaultPath, FileMaster.FolderId, Master.MasterID, FileIteration.FileName
            // FileIteration.FileIterationId == Master.TipTipCommittedIterationID
            // Master.MasterId == FileMaster.FileMasterId
            // FileMaster.FolderId = Folder.FolderId

            // Create a collection of the objects combined so that we can generate the vault full path
            var query = from fi in fileIterations.Values
                        join mstr in masters.Values on fi.FileIterationId equals mstr.TipCommittedIterationID
                        join fm in fileMasters.Values on mstr.MasterID equals fm.FileMasterID
                        join fldr in selectFolders on fm.FolderId equals fldr.FolderID
                        select new { fileIteration = fi, master = mstr, fileMaster = fm, folder = fldr };

            // from the query, make a dictionary of vault full path
            Dictionary<string, FileMaster> vaultPathFileMasterDictionary = query.ToDictionary(an => an.folder.VaultPath + "/" + an.fileIteration.FileName, an => an.fileMaster, StringComparer.CurrentCultureIgnoreCase);

            // now select the FileMaster object matches full path
            FileMaster foundFileMaster = vaultPathFileMasterDictionary[sVaultFullFilePathName];

            Logger.Debug("VaultCacheOld GetFileMaster: End");

            return (foundFileMaster);
        }

        // filter the Iterations and FileMasters based on their extensions and 
        static public void FilterFileMasters(VaultCacheOld vaultCache, string vaultExtensionFilters)
        {
            StopwatchPlus swp = new StopwatchPlus();

            // apply the file extension filters
            if (!String.IsNullOrWhiteSpace(vaultExtensionFilters))
            {
                Logger.Debug(string.Format("Filtering On File Extensions: {0}", vaultExtensionFilters));

                // Controlled is no longer in table
                // remove the BOMBlob and anything else not needed
                // string sSelectString = "SELECT FileIterationId, FileName, ModDate, CheckinDate, BOMBlob, DesignVisualizationAttachmentStatus, IsParsed, Controlled, LifeCycleStateName, CheckoutDate, ResourceId FROM dbo.FileIteration WHERE FileIterationId IN @parm";
                string sSelectString = FileIterationExtended.GetTipCommitedSelectString() + " WHERE MasterId IN @parm";
                var fileIterExtended = vaultCache.cacheLoader.LoadVaultEntities<FileIterationExtended>(sSelectString, new { parm = vaultCache._fileMasters.Keys.ToArray() });
                Logger.Debug(string.Format("fileIterationsExtended: {0:n0} {1}", fileIterExtended.Count, swp.LapTimeString()));

                // get the file extensions of interest
                // this string should be pipe | delimited
                // such as ".asm|.prt"
                var fileExtensions = vaultExtensionFilters.Split('|');

                // get the filtered list of FileIterationIds
                var fileMasterIds = fileIterExtended.Where(fi => fileExtensions.Any(e => e == Path.GetExtension(fi.FileName))).Select(fi => fi.FileMasterId);

                // filter the file masters
                vaultCache._fileMasters = fileMasterIds.Intersect(vaultCache._fileMasters.Keys)
                                            .Select(k => vaultCache._fileMasters[k]).ToDictionary(fm => fm.FileMasterID, fm => fm);
                Logger.Debug(string.Format("Filtered FileMasters from FileIterations: {0:n0} {1}", vaultCache._fileMasters.Count, swp.LapTimeString()));
            }

            // set the statistics
            vaultCache._initialFilesDiscovered = vaultCache._fileMasters.Count;

            Logger.Debug(string.Format("VaultCacheOld.FilterFileMasters() Total Time {0}", swp.ElapsedTimeString()));
        }


        // filter the Iterations and FileMasters based on their extensions and 
        static public void SetupIterationsAndAdjustFileMastersOld(VaultCacheOld vaultCache, string vaultExtensionFilters)
        {
            StopwatchPlus swp = new StopwatchPlus();

            // get the iterations from the FileMasterID
            string sSelectString = "SELECT IterationID, MasterID, IterationNumber, RevisionId, Comment FROM dbo.Iteration WHERE MasterID IN @parm";
            vaultCache._iterationsForFiles = vaultCache.cacheLoader.LoadVaultEntityDictionary<Iteration>(sSelectString, new { parm = vaultCache._fileMasters.Keys.ToArray() });
            Logger.Debug(string.Format("Total file iterations from FileMasterId: {0:n0} {1}", vaultCache._iterationsForFiles.Count, swp.LapTimeString()));
            vaultCache._totalIterationsWithinSearchFolders = vaultCache._iterationsForFiles.Count;

            // apply the file extension filters
            if (!String.IsNullOrWhiteSpace(vaultExtensionFilters))
            {
                Logger.Debug(string.Format("Filtering On File Extensions: {0}", vaultExtensionFilters));

                // Controlled is no longer in table
                // remove the BOMBlob and anything else not needed
                // string sSelectString = "SELECT FileIterationId, FileName, ModDate, CheckinDate, BOMBlob, DesignVisualizationAttachmentStatus, IsParsed, Controlled, LifeCycleStateName, CheckoutDate, ResourceId FROM dbo.FileIteration WHERE FileIterationId IN @parm";
                sSelectString = "SELECT FileIterationId, FileName FROM dbo.FileIteration WHERE FileIterationId IN @parm";
                var fileIterations = vaultCache.cacheLoader.LoadVaultEntities<FileIteration>(sSelectString, new { parm = vaultCache._iterationsForFiles.Keys.ToArray() });
                Logger.Debug(string.Format("fileIterations: {0:n0} {1}", fileIterations.Count, swp.LapTimeString()));

                // get the file extensions of interest
                // this string should be pipe | delimited
                // such as ".asm|.prt"
                var fileExtensions = vaultExtensionFilters.Split('|');

                // get the filtered list of FileIterationIds
                var filteredFileIterationIds = fileIterations.Where(fi => fileExtensions.Any(e => e == Path.GetExtension(fi.FileName))).Select(fi => fi.FileIterationId);

                // filter the Iterations
                vaultCache._iterationsForFiles = filteredFileIterationIds.Intersect(vaultCache._iterationsForFiles.Keys)
                                            .Select(k => vaultCache._iterationsForFiles[k]).ToDictionary(it => it.IterationID, it => it);
                Logger.Debug(string.Format("Filtered Iterations from FileIterations: {0:n0} {1}", vaultCache._iterationsForFiles.Count, swp.LapTimeString()));



                // filter the file masters
                var fileMasterIds = vaultCache._iterationsForFiles.Select(it => it.Value.MasterID).Distinct().ToList();
                vaultCache._fileMasters = fileMasterIds.Intersect(vaultCache._fileMasters.Keys)
                                            .Select(k => vaultCache._fileMasters[k]).ToDictionary(fm => fm.FileMasterID, fm => fm);
                Logger.Debug(string.Format("Filtered FileMasters from FileIterations: {0:n0} {1}", vaultCache._fileMasters.Count, swp.LapTimeString()));
            }

            // set the statistics
            vaultCache._initialFilesDiscovered = vaultCache._fileMasters.Count;
            vaultCache._initialFileIterationsDiscovered = vaultCache._iterationsForFiles.Count;

            Logger.Debug(string.Format("VaultCacheOld.SetupIterationsAndAdjustFileMasters() Total Time {0}", swp.ElapsedTimeString()));
        }



        // once the file masters have been acquired, get the remainder of the information
        static public void DiscoverChildrenAndUpdateFileMasters(VaultCacheOld vaultCache, bool bGetAllGenerics, List<long> visitedFileMasterIdList = null)
        {
            StopwatchPlus swp = new StopwatchPlus();


            // get the child associations from the list of all files
            // NOTE: in the situation we are sniffing content that has been loaded
            //      this may not be as robust as you need if you are doing parent child analysis...
            // get those associations for all files that we have - children and parents
            // using fileIterationIDArray for parent and child
            // get parent associations
            // iterate through and get relationships
            List<FileAssociationExtended> asParentAssociations = new List<FileAssociationExtended>();
            List<long> currentFileMasterIDList = new List<long>();
            List<long> originalFileMasterIDList = new List<long>();
            List<long> totalFileMasterIDList = new List<long>();


            // get all of the file associations where Source contains 'FamTableRelation:%'
            // we do not have vaultCache._fileAssociationsExtended populated yet, so we have to get it directly from the database
            string sSelectString;
            if (vaultCache._familyFileAssociationsExtended == null)
            {
                sSelectString = FileAssociationExtended.GetAllSelectionString() + " WHERE Source LIKE 'FamTableRelation:%'";
                vaultCache._familyFileAssociationsExtended = vaultCache.cacheLoader.LoadVaultEntities<FileAssociationExtended>(sSelectString);
            }

            if (vaultCache._fileMasterIDsPointingToMultipleGenerics == null)
            {
                vaultCache._fileMasterIDsPointingToMultipleGenerics = new List<long>();
            }

            if (vaultCache._fileAssociationsExtended == null)
            {
                vaultCache._fileAssociationsExtended = new List<FileAssociationExtended>();
            }

            if (vaultCache._supercededFamilyFileAssocExtended == null)
            {
                vaultCache._supercededFamilyFileAssocExtended = new List<FileAssociationExtended>();
            }


            // setup a lookup with the ToMasterId and FromMasterId as the key
            ILookup<long, FileAssociationExtended> faeByFromMasterIdLookup = vaultCache._familyFileAssociationsExtended.ToLookup(fae => fae.FromMasterID, fae => fae);
            ILookup<long, FileAssociationExtended> faeByToMasterIdLookup = vaultCache._familyFileAssociationsExtended.ToLookup(fae => fae.ToMasterID, fae => fae);


            // make the current list contain those that have not been analyzed yet
            currentFileMasterIDList.AddRange(vaultCache._fileMasters.Keys.Except(visitedFileMasterIdList));
            originalFileMasterIDList.AddRange(vaultCache._fileMasters.Keys);
            totalFileMasterIDList.AddRange(vaultCache._fileMasters.Keys);
            int nChildDiscoveryLoopCount = 0;
            while (currentFileMasterIDList.Count > 0)
            {
                // get the relationships for all of the files
                // get the relationships that are NOT LIKE 'FamTableRelation:%' - going to special case family table files
                sSelectString = FileAssociationExtended.GetLatestFromSelectionString() + " WHERE Source NOT LIKE 'FamTableRelation:%' AND FromIT.MasterID IN @parm";
                Dictionary<long, FileAssociationExtended> nonFamilyAssociations = vaultCache.cacheLoader.LoadVaultEntityDictionary<FileAssociationExtended>(sSelectString, new { parm = currentFileMasterIDList.ToArray() });
                Logger.Debug(string.Format("associations (FromId): {0:n0} {1}", nonFamilyAssociations.Count, swp.LapTimeString()));

                // get the list of non family associations
                List<FileAssociationExtended> currentFileAssocations = new List<FileAssociationExtended>();
                currentFileAssocations.AddRange(nonFamilyAssociations.Values);

                // special handle family associations - to eliminate the possible duplicate generic children
                // unless the bGetAllGenerics flag is on
                List<FileAssociationExtended> familyAssociationList = currentFileMasterIDList.SelectMany(fmi => faeByFromMasterIdLookup[fmi]).ToList();


                // now check for duplications
                // group FileAssociationExtended by FromMasterId
                ILookup<long, FileAssociationExtended> faeLookup = familyAssociationList.ToLookup(fae => fae.FromMasterID, fae => fae);

                var singleToGenerics = faeLookup.Where(fael => fael.Count() == 1);
                currentFileAssocations.AddRange(singleToGenerics.SelectMany(ig => ig));


                // strip off all but the latest for each master
                // this does not mean that the FileMaster points to different generics
                // they could be different iterations of the same generic
                List<long> supercededMasterIdList = new List<long>();
                var multipleToGenerics = faeLookup.Where(fael => fael.Count() > 1);
                if (multipleToGenerics.Any())
                {
                    foreach (IGrouping<long, FileAssociationExtended> ig in multipleToGenerics)
                    {
                        FileAssociationExtended lastFAE = ig.OrderByDescending(fae => fae.FileAssociationId).First();
                        currentFileAssocations.Add(lastFAE);

                        // add the remaining assocations to the _supercededFamilyFileAssocExtended list
                        // MSSqlVaultCacheLoader.CombineDictionaries(vaultCache._fileMasters, newFileMasters);
                        List<FileAssociationExtended> thisSupercededFamAssExt = ig.ToList();
                        thisSupercededFamAssExt = thisSupercededFamAssExt.Where(fae => fae.ToMasterID != lastFAE.ToMasterID).ToList();
                        vaultCache._supercededFamilyFileAssocExtended.AddRange(thisSupercededFamAssExt);

                        // add the ToMasterIDs to the superceded list in case we want to include them for analysis
                        supercededMasterIdList.AddRange(thisSupercededFamAssExt.Select(fae => fae.ToMasterID));

                        // check to see if we are pointing to multiple ToFileMasterId in the family list
                        if (thisSupercededFamAssExt.Count() > 0)
                        {
                            vaultCache._fileMasterIDsPointingToMultipleGenerics.Add(lastFAE.FromMasterID);
                        }
                    }
                }


                // add them to the FileMaster Ids we are processing
                asParentAssociations.AddRange(currentFileAssocations);

                // group FileAssociationExtended by ToMasterId
                List<long> toMasterIdList = currentFileAssocations.Select(fae => fae.ToMasterID).Distinct().ToList();

                // add these to the current and total list
                currentFileMasterIDList = toMasterIdList.Except(totalFileMasterIDList).ToList();

                // add the superceded generics if we want to analyize all
                if (bGetAllGenerics)
                {
                    currentFileMasterIDList.AddRange(supercededMasterIdList.Except(totalFileMasterIDList));
                    currentFileMasterIDList = currentFileMasterIDList.Distinct().ToList();
                }

                // add these to the total list
                totalFileMasterIDList.AddRange(currentFileMasterIDList);

                nChildDiscoveryLoopCount++;
            }

            // create the dictionary of superceded FileAssociationExtended
            vaultCache._supercededFamilyFileAssocExtended = vaultCache._supercededFamilyFileAssocExtended.Distinct().ToList();
            vaultCache._fileMasterIDsPointingToMultipleGenerics = vaultCache._fileMasterIDsPointingToMultipleGenerics.Distinct().ToList();


            // get the distinct masters from these new iterations
            List<long> newMasterIds = totalFileMasterIDList.Except(originalFileMasterIDList).ToList();
            if (newMasterIds.Count() > 0)
            {
                sSelectString = "SELECT FileMasterID, CheckedOut, CheckoutLocalSpec, CheckoutMachine, FileClassification, Hidden, FileStatus, Unmanaged, FolderId, TipFileBaseName FROM dbo.FileMaster WHERE FileMasterID IN @parm";
                Dictionary<long, FileMaster> newFileMasters = vaultCache.cacheLoader.LoadVaultEntityDictionary<FileMaster>(sSelectString, new { parm = newMasterIds.ToArray() });
                Logger.Debug(string.Format("New fileMasters from Child Associations: {0:n0} {1}", newFileMasters.Count, swp.LapTimeString()));

                // combine them into the totals
                vaultCache._fileMasters = MSSqlVaultCacheLoader.CombineDictionaries(vaultCache._fileMasters, newFileMasters);
                Logger.Debug(string.Format("MSSqlVaultCacheLoader.CombineDictionaries(vaultCache._fileMasters, newFileMasters) {0:n0} {1}", vaultCache._fileMasters.Count, swp.LapTimeString()));
            }

            // assign the file associations
            // make sure we have a distinct list
            vaultCache._fileAssociationsExtended = vaultCache._fileAssociationsExtended.Concat(asParentAssociations).Distinct().ToList();
            Logger.Debug(string.Format("fileAssociations (Both): {0:n0} {1}", vaultCache._fileAssociationsExtended.Count, swp.LapTimeString()));

            Logger.Debug(string.Format("VaultCacheOld.DiscoverChildrenAndUpdateFileMasters() Total Time {0}", swp.ElapsedTimeString()));
        }







        // once the file masters have been acquired, get the remainder of the information
        static public void DiscoverChildrenAndUpdateFileMastersOld(VaultCacheOld vaultCache, string vaultExtensionFilters, bool bDiscoverChildren)
        {
            StopwatchPlus swp = new StopwatchPlus();

            // filter the Iterations and FileMasters based on their extensions and 
            SetupIterationsAndAdjustFileMastersOld(vaultCache, vaultExtensionFilters);
            Logger.Debug(string.Format("VaultCacheOld.SetupIterationsAndAdjustFileMasters() {0}", swp.LapTimeString()));


            // get the child associations from the list of all files
            // NOTE: in the situation we are sniffing content that has been loaded
            //      this may not be as robust as you need if you are doign parent child analysis...
            // get those associations for all files that we have - children and parents
            // using fileIterationIDArray for parent and child
            // get parent associations
            // iterate through and get relationships
            List<FileAssociationExtended> asParentAssociations = new List<FileAssociationExtended>();
            List<long> currentFileIterationIDList = new List<long>();
            List<long> originalFileIterationIDList = new List<long>();
            List<long> totalFileIterationIDList = new List<long>();


            // get all of the file associations where Source contains 'FamTableRelation:%'
            // we do not have vaultCache._fileAssociationsExtended populated yet, so we have to get it directly from the database
            string sSelectString;
            if (vaultCache._familyFileAssociationsExtended == null)
            {
                sSelectString = FileAssociationExtended.GetAllSelectionString() + " WHERE Source LIKE 'FamTableRelation:%'";
                vaultCache._familyFileAssociationsExtended = vaultCache.cacheLoader.LoadVaultEntities<FileAssociationExtended>(sSelectString);
            }


            currentFileIterationIDList.AddRange(vaultCache._iterationsForFiles.Keys);
            originalFileIterationIDList.AddRange(vaultCache._iterationsForFiles.Keys);
            totalFileIterationIDList.AddRange(vaultCache._iterationsForFiles.Keys);
            int nChildDiscoveryLoopCount = 0;
            while (currentFileIterationIDList.Count > 0)
            {
                // don't request the vault path information
                sSelectString = FileAssociationExtended.GetAllSelectionString() + " WHERE FromId IN @parm";
                List<FileAssociationExtended> associations = vaultCache.cacheLoader.LoadVaultEntities<FileAssociationExtended>(sSelectString, new { parm = currentFileIterationIDList.ToArray() });
                Logger.Debug(string.Format("associations (FromId): {0:n0} {1}", associations.Count, swp.LapTimeString()));

                // add them to the parent associations
                // TODO: decide if we should keep these separate for future reference
                asParentAssociations = asParentAssociations.Concat(associations).Distinct().ToList();
                Logger.Debug(string.Format("asParentAssociations.Conat(associations) {0:n0} {1}", asParentAssociations.Count, swp.LapTimeString()));

                // if we are to discover children of children (they may be outside of the original folders requested)
                currentFileIterationIDList.Clear();
                if (bDiscoverChildren && associations.Any())
                {
                    var distinctIds = associations.Select(fa => fa.ToId).Distinct();
                    var currentFileIterationIds = distinctIds.Except(totalFileIterationIDList);
                    currentFileIterationIDList = currentFileIterationIds.ToList();
                    if (currentFileIterationIDList.Count() > 0)
                    {
                        totalFileIterationIDList.AddRange(currentFileIterationIDList);
                        totalFileIterationIDList = totalFileIterationIDList.Distinct().ToList();

                        sSelectString = "SELECT IterationID, MasterID, IterationNumber, RevisionId, Comment FROM dbo.Iteration WHERE IterationID IN @parm";
                        Dictionary<long, Iteration> newIterations = vaultCache.cacheLoader.LoadVaultEntityDictionary<Iteration>(sSelectString, new { parm = currentFileIterationIDList.ToArray() });
                        Logger.Debug(string.Format("New iterations from Child Associations: {0:n0} {1}", newIterations.Count, swp.LapTimeString()));

                        // combine them into the totals
                        vaultCache._iterationsForFiles = MSSqlVaultCacheLoader.CombineDictionaries(vaultCache._iterationsForFiles, newIterations);
                        Logger.Debug(string.Format("MSSqlVaultCacheLoader.CombineDictionaries(vaultCache._iterationsForFiles, newIterations) {0:n0} {1}", vaultCache._iterationsForFiles.Count, swp.LapTimeString()));

                        // get the distinct masters from these new iterations
                        List<long> newMasterIds = newIterations.Values.Select(itr => itr.MasterID).Distinct().Except(vaultCache._fileMasters.Keys).ToList();
                        if (newMasterIds.Count() > 0)
                        {
                            sSelectString = "SELECT FileMasterID, CheckedOut, CheckoutLocalSpec, CheckoutMachine, FileClassification, Hidden, FileStatus, Unmanaged, FolderId, TipFileBaseName FROM dbo.FileMaster WHERE FileMasterID IN @parm";
                            Dictionary<long, FileMaster> newFileMasters = vaultCache.cacheLoader.LoadVaultEntityDictionary<FileMaster>(sSelectString, new { parm = newMasterIds.ToArray() });
                            Logger.Debug(string.Format("New fileMasters from Child Associations: {0:n0} {1}", newFileMasters.Count, swp.LapTimeString()));

                            // combine them into the totals
                            vaultCache._fileMasters = MSSqlVaultCacheLoader.CombineDictionaries(vaultCache._fileMasters, newFileMasters);
                            Logger.Debug(string.Format("MSSqlVaultCacheLoader.CombineDictionaries(vaultCache._fileMasters, newFileMasters) {0:n0} {1}", vaultCache._fileMasters.Count, swp.LapTimeString()));
                        }
                    }
                }

                nChildDiscoveryLoopCount++;
            }


            // get files as child associations from original ID's (Not going backup up the tree to other parents)
            // TODO: decide if we should keep these separate for future reference
            sSelectString = FileAssociationExtended.GetAllSelectionString() + " WHERE FromId IN @parm";
            List<FileAssociationExtended> asChildAssociations = vaultCache.cacheLoader.LoadVaultEntities<FileAssociationExtended>(sSelectString, new { parm = originalFileIterationIDList.ToArray() });
            Logger.Debug(string.Format("associations (ToId): {0:n0} {1}", asChildAssociations.Count, swp.LapTimeString()));


            // assign the file associations
            // if the count is the same as the largest FromId or ToId dictionary found, then we have all of the parent child relationships
            // no files peirce outside of the subfolders that we have established
            vaultCache._fileAssociationsExtended = asParentAssociations.Concat(asChildAssociations).Distinct().ToList();
            Logger.Debug(string.Format("fileAssociations (Both): {0:n0} {1}", vaultCache._fileAssociationsExtended.Count, swp.LapTimeString()));

            Logger.Debug(string.Format("VaultCacheOld.SetupIterationsAndAdjustFileMasters() Total Time {0}", swp.ElapsedTimeString()));
        }



        // get the parents and grandparents of the original files
        static public void DiscoverParentsAndUpdateFileMasters(VaultCacheOld vaultCache, List<long> visitedFileMasterIdList = null)
        {
            StopwatchPlus swp = new StopwatchPlus();


            // get the parent associations from the list of all files
            // NOTE: in the situation we are sniffing content that has been loaded
            //      this may not be as robust as you need if you are doing parent child analysis...
            // get those associations for all files that we have - children and parents
            // using fileIterationIDArray for parent and child
            // get parent associations
            // iterate through and get relationships
            List<FileAssociationExtended> asChildrenAssociations = new List<FileAssociationExtended>();
            List<long> currentFileMasterIDList = new List<long>();
            List<long> originalFileMasterIDList = new List<long>();
            List<long> totalFileMasterIDList = new List<long>();


            // get all of the file associations where Source contains 'FamTableRelation:%'
            // we do not have vaultCache._fileAssociationsExtended populated yet, so we have to get it directly from the database
            string sSelectString;
            if (vaultCache._familyFileAssociationsExtended == null)
            {
                vaultCache._familyFileAssociationsExtended = new List<FileAssociationExtended>();
            }

            if (vaultCache._fileMasterIDsPointingToMultipleGenerics == null)
            {
                vaultCache._fileMasterIDsPointingToMultipleGenerics = new List<long>();
            }

            if (vaultCache._fileAssociationsExtended == null)
            {
                vaultCache._fileAssociationsExtended = new List<FileAssociationExtended>();
            }

            if (vaultCache._supercededFamilyFileAssocExtended == null)
            {
                vaultCache._supercededFamilyFileAssocExtended = new List<FileAssociationExtended>();
            }


            // setup a lookup with the ToMasterId and FromMasterId as the key
            ILookup<long, FileAssociationExtended> faeByFromMasterIdLookup = vaultCache._familyFileAssociationsExtended.ToLookup(fae => fae.FromMasterID, fae => fae);
            ILookup<long, FileAssociationExtended> faeByToMasterIdLookup = vaultCache._familyFileAssociationsExtended.ToLookup(fae => fae.ToMasterID, fae => fae);


            // make the current list contain those that have not been analyzed yet
            currentFileMasterIDList.AddRange(vaultCache._fileMasters.Keys.Except(visitedFileMasterIdList));
            originalFileMasterIDList.AddRange(vaultCache._fileMasters.Keys);
            totalFileMasterIDList.AddRange(vaultCache._fileMasters.Keys);
            int nParentDiscoveryLoopCount = 0;
            while (currentFileMasterIDList.Count > 0)
            {
                // get the relationships for all of the files
                sSelectString = FileAssociationExtended.GetLatestFromSelectionString() + " WHERE ToIT.MasterID IN @parm";
                Dictionary<long, FileAssociationExtended> associations = vaultCache.cacheLoader.LoadVaultEntityDictionary<FileAssociationExtended>(sSelectString, new { parm = currentFileMasterIDList.ToArray() });
                Logger.Debug(string.Format("associations (FromId): {0:n0} {1}", associations.Count, swp.LapTimeString()));

                // get the list of associations
                List<FileAssociationExtended> currentFileAssocations = new List<FileAssociationExtended>();
                currentFileAssocations.AddRange(associations.Values);

                // special handle family associations - to eliminate the possible duplicate generic children
                // unless the bGetAllGenerics flag is on
                List<FileAssociationExtended> familyAssociationList = currentFileMasterIDList.SelectMany(fmi => faeByFromMasterIdLookup[fmi]).ToList();


                // now check for duplications
                // group FileAssociationExtended by FromMasterId
                ILookup<long, FileAssociationExtended> faeLookup = familyAssociationList.ToLookup(fae => fae.FromMasterID, fae => fae);

                var singleToGenerics = faeLookup.Where(fael => fael.Count() == 1);
                currentFileAssocations.AddRange(singleToGenerics.SelectMany(ig => ig));


                // strip off all but the latest for each master
                // this does not mean that the FileMaster points to different generics
                // they could be different iterations of the same generic
                List<long> supercededMasterIdList = new List<long>();
                var multipleToGenerics = faeLookup.Where(fael => fael.Count() > 1);
                if (multipleToGenerics.Any())
                {
                    foreach (IGrouping<long, FileAssociationExtended> ig in multipleToGenerics)
                    {
                        FileAssociationExtended lastFAE = ig.OrderByDescending(fae => fae.FileAssociationId).First();
                        currentFileAssocations.Add(lastFAE);

                        // add the remaining assocations to the _supercededFamilyFileAssocExtended list
                        // MSSqlVaultCacheLoader.CombineDictionaries(vaultCache._fileMasters, newFileMasters);
                        List<FileAssociationExtended> thisSupercededFamAssExt = ig.ToList();
                        thisSupercededFamAssExt = thisSupercededFamAssExt.Where(fae => fae.ToMasterID != lastFAE.ToMasterID).ToList();
                        vaultCache._supercededFamilyFileAssocExtended.AddRange(thisSupercededFamAssExt);

                        // add the ToMasterIDs to the superceded list in case we want to include them for analysis
                        supercededMasterIdList.AddRange(thisSupercededFamAssExt.Select(fae => fae.ToMasterID));

                        // check to see if we are pointing to multiple ToFileMasterId in the family list
                        if (thisSupercededFamAssExt.Count() > 0)
                        {
                            vaultCache._fileMasterIDsPointingToMultipleGenerics.Add(lastFAE.FromMasterID);
                        }
                    }
                }


                // add them to the FileMaster Ids we are processing
                asChildrenAssociations.AddRange(currentFileAssocations);

                // group FileAssociationExtended by fromMasterId
                List<long> fromMasterIdList = currentFileAssocations.Select(fae => fae.FromMasterID).Distinct().ToList();

                // add these to the current and total list
                currentFileMasterIDList = fromMasterIdList.Except(totalFileMasterIDList).ToList();

                // add these to the total list
                totalFileMasterIDList.AddRange(currentFileMasterIDList);

                nParentDiscoveryLoopCount++;
            }

            // create the dictionary of superceded FileAssociationExtended
            vaultCache._supercededFamilyFileAssocExtended = vaultCache._supercededFamilyFileAssocExtended.Distinct().ToList();
            vaultCache._fileMasterIDsPointingToMultipleGenerics = vaultCache._fileMasterIDsPointingToMultipleGenerics.Distinct().ToList();


            // get the distinct masters from these new iterations
            List<long> newMasterIds = totalFileMasterIDList.Except(originalFileMasterIDList).ToList();
            if (newMasterIds.Count() > 0)
            {
                sSelectString = "SELECT FileMasterID, CheckedOut, CheckoutLocalSpec, CheckoutMachine, FileClassification, Hidden, FileStatus, Unmanaged, FolderId, TipFileBaseName FROM dbo.FileMaster WHERE FileMasterID IN @parm";
                Dictionary<long, FileMaster> newFileMasters = vaultCache.cacheLoader.LoadVaultEntityDictionary<FileMaster>(sSelectString, new { parm = newMasterIds.ToArray() });
                Logger.Debug(string.Format("New fileMasters from Child Associations: {0:n0} {1}", newFileMasters.Count, swp.LapTimeString()));

                // combine them into the totals
                vaultCache._fileMasters = MSSqlVaultCacheLoader.CombineDictionaries(vaultCache._fileMasters, newFileMasters);
                Logger.Debug(string.Format("MSSqlVaultCacheLoader.CombineDictionaries(vaultCache._fileMasters, newFileMasters) {0:n0} {1}", vaultCache._fileMasters.Count, swp.LapTimeString()));
            }

            // assign the file associations
            // make sure we have a distinct list
            // these are only for the TIP versions of the file masters
            // if we wanted to get ALL of the associations for previous versions, we would have to build them
            // probably a good idea if we are building these for fixing ALL FileAssociation records
            vaultCache._fileAssociationsExtended = vaultCache._fileAssociationsExtended.Concat(asChildrenAssociations).Distinct().ToList();
            Logger.Debug(string.Format("fileAssociations (Both): {0:n0} {1}", vaultCache._fileAssociationsExtended.Count, swp.LapTimeString()));

            Logger.Debug(string.Format("VaultCacheOld.DiscoverChildrenAndUpdateFileMasters() Total Time {0}", swp.ElapsedTimeString()));
        }






        // once the file masters have been acquired, get the remainder of the information
        static public void LoadRemainderOfFileCacheInformation(VaultCacheOld vaultCache, string vaultExtensionFilters, bool bDiscoverChildren, bool bGetAllGenerics)
        {
            StopwatchPlus swp = new StopwatchPlus();

            // filter the Iterations and FileMasters based on their extensions and 
            FilterFileMasters(vaultCache, vaultExtensionFilters);
            Logger.Debug(string.Format("VaultCacheOld.FilterFileMasters() {0}", swp.LapTimeString()));


            // discover the parents of the initial list of files
            // note, this only gets the parents, and parents of the parents of the initial list, 
            // not the parents of all files that may be found in the children and instances of generics
            // If the initial list IS generics, then we are guaranteed that we will be visiting ALL of the files
            // that reference the generics given or ANY of their instances in any instance hierarchy
            // TODO: potentially implement this


            // TODO:
            // for Honeywell - NOT CAPTURING PARENTS
            if (false)
            {
                int nLoop = 1;
                List<long> visitedFileMasterIdList = new List<long>();
                DiscoverParentsAndUpdateFileMasters(vaultCache, visitedFileMasterIdList);
                do
                {
                    // log that we have completed the loop
                    Logger.Debug(string.Format("Loop{0} Discover Children and Generics Start:", nLoop));

                    // discover children
                    if (bDiscoverChildren)
                    {
                        DiscoverChildrenAndUpdateFileMasters(vaultCache, bGetAllGenerics, visitedFileMasterIdList);
                        Logger.Debug(string.Format("VaultCacheOld.DiscoverChildrenAndUpdateFileMasters() {0}", swp.LapTimeString()));
                    }

                    // get the list of new file masters
                    List<long> newFileMasterIdList = vaultCache._fileMasters.Keys.Except(visitedFileMasterIdList).ToList();
                    List<FileMaster> newFileMastersToVistGenerics = newFileMasterIdList.Select(fmi => vaultCache._fileMasters[fmi]).ToList();

                    // what we visted to the list
                    visitedFileMasterIdList.AddRange(newFileMastersToVistGenerics.Select(fm => fm.FileMasterID));


                    // get ALL of the Generic parent associations to be inclusive of all potential generics
                    if (newFileMastersToVistGenerics.Count() > 0)
                    {
                        vaultCache._fileMasters = GetAllFileMasterGenericInstances(vaultCache, bGetAllGenerics, newFileMastersToVistGenerics);
                        Logger.Debug(string.Format("fileMasters: {0:n0}", vaultCache._fileMasters.Count));
                    }

                    // log that we have completed the loop
                    Logger.Debug(string.Format("Loop{0} DiscoverChildren and Generics End: {1}", nLoop, swp.LapTimeString()));

                    // check if we should stop
                    if (visitedFileMasterIdList.Count() == vaultCache._fileMasters.Count())
                    {
                        break;
                    }

                    nLoop++;

                } while (true);
            }



            // get the file iterations
            string sSelectString = "SELECT IterationID, MasterID, IterationNumber, RevisionId, Comment FROM dbo.Iteration WHERE MasterID IN @parm";
            vaultCache._iterationsForFiles = vaultCache.cacheLoader.LoadVaultEntityDictionary<Iteration>(sSelectString, new { parm = vaultCache._fileMasters.Keys.ToArray() });
            Logger.Debug(string.Format("Total file iterations from FileMasterId: {0:n0} {1}", vaultCache._iterationsForFiles.Count, swp.LapTimeString()));
            vaultCache._initialFileIterationsDiscovered = vaultCache._iterationsForFiles.Count;



            // Controlled is no longer in table
            // remove the BOMBlob
            // string sSelectString = "SELECT FileIterationId, FileName, ModDate, CheckinDate, BOMBlob, DesignVisualizationAttachmentStatus, IsParsed, Controlled, LifeCycleStateName, CheckoutDate, ResourceId FROM dbo.FileIteration WHERE FileIterationId IN @parm";
            sSelectString = VaultDbEntityHelper.GetSelectString<FileIterationExtended>() + " WHERE FileIterationId IN @parm";
            vaultCache._fileIterationsExtended = vaultCache.cacheLoader.LoadVaultEntityDictionary<FileIterationExtended>(sSelectString, new { parm = vaultCache._iterationsForFiles.Keys.ToArray() });
            Logger.Debug(string.Format("fileIterations: {0:n0} {1}", vaultCache._fileIterationsExtended.Count, swp.LapTimeString()));


            // get the file iteration properties
            //long[] fileIterationIdArray = vaultCache.m_FileIterationsExtended_Dict.Keys.ToArray();
            //sSelectString = "SELECT PropertyID, PropertyDefID, EntityID, Value FROM dbo.Property WHERE EntityID IN @parm";
            //vaultCache._fileIterationProperties = vaultCache.cacheLoader.LoadVaultEntityDictionary<Property>(sSelectString, new { parm = fileIterationIdArray });
            //Logger.Debug(string.Format("fileIterationProperties: {0:n0}", vaultCache._fileIterationProperties.Count));



            // get the file iteration property definitions
            //long[] propertyDefIDArray = vaultCache._fileIterationProperties.Select(kvp => kvp.Value.PropertyDefID).Distinct().ToArray();
            //sSelectString = "SELECT PropertyDefID, FriendlyName, SystemName, DataType, Active, BasicSearch, IsSystem, LastCfgUpdate, InitialValue FROM dbo.PropertyDef WHERE PropertyDefID IN @parm";
            //vaultCache._fileIterationPropertyDefs = vaultCache.cacheLoader.LoadVaultEntityDictionary<PropertyDef>(sSelectString, new { parm = propertyDefIDArray });
            //Logger.Debug(string.Format("fileIterationPropertyDefs: {0:n0}", vaultCache._fileIterationPropertyDefs.Count));


            // get the file resources
            long[] fileMasterIDArray = vaultCache._fileMasters.Keys.ToArray();
            sSelectString = "SELECT ResourceId, FileMasterId, FileSize, Checksum, Version, Extension, ResourceGuid FROM dbo.FileResource WHERE FileMasterId IN @parm";
            vaultCache._fileResources = vaultCache.cacheLoader.LoadVaultEntityDictionary<FileResource>(sSelectString, new { parm = fileMasterIDArray });
            Logger.Debug(string.Format("fileResources: {0:n0} {1}", vaultCache._fileResources.Count, swp.LapTimeString()));


            // properties specific to file resources
            // get the properties specific to file Resources
            //long[] fileResourcesIdArray = vaultCache.m_FileResources_Dict.Keys.ToArray();
            //sSelectString = "SELECT ContentSourceID, ContentSourcePropertyMoniker, EntityId, EntityClassId, DataType, Value  FROM dbo.ContentSourceProperties WHERE EntityId IN @parm";
            //vaultCache._contentSourcePropertiesList = vaultCache.cacheLoader.LoadVaultEntities<ContentSourceProperties>(sSelectString, new { parm = fileResourcesIdArray });
            //Logger.Debug(string.Format("contentSourcePropertiesList: {0:n0}", vaultCache._contentSourcePropertiesList.Count));

            // build the lookup for the resource to ContentSourceProperties
            //vaultCache._contentSourcePropertyByResourceIds = vaultCache._contentSourcePropertiesList.ToLookup(csp => csp.EntityId, csp => csp);
            //Logger.Debug(string.Format("contentSourcePropertyByResourceIds: {0:n0}", vaultCache._contentSourcePropertyByResourceIds.Count));


            // get the masters table
            sSelectString = "SELECT MasterID, TipIterationID, TipCommittedIterationID, MaxRevisionId, MaxConsumableRevisionId, ControlledByChangeOrder FROM dbo.Master WHERE MasterID IN @parm";
            vaultCache._mastersForFiles = vaultCache.cacheLoader.LoadVaultEntityDictionary<Master>(sSelectString, new { parm = fileMasterIDArray });
            Logger.Debug(string.Format("masters: {0:n0} {1}", vaultCache._mastersForFiles.Count, swp.LapTimeString()));

            // get the categories
            // TODO: get categories on other entities? files, folders, etc
            sSelectString = "SELECT EntityId, CategoryDefId FROM dbo.CategoryOnEntity WHERE EntityId IN @parm";
            vaultCache._categoriesOnEntitiesForFiles = vaultCache.cacheLoader.LoadVaultEntityDictionary<CategoryOnEntity>(sSelectString, new { parm = fileMasterIDArray });
            Logger.Debug(string.Format("categoriesOnEntities: {0:n0} {1}", vaultCache._categoriesOnEntitiesForFiles.Count, swp.LapTimeString()));


            // get the Revision table
            // LifeCycleStateId not in Revision table anymore
            // sSelectString = "SELECT RevisionId, RevisionDefinitionId, MasterId, RevisionLabel, RevisionOrder, MaxIterationId, MaxConsumableIterationId, LifeCycleStateId FROM dbo.Revision WHERE MasterId IN @parm";
            sSelectString = "SELECT RevisionId, RevisionDefinitionId, MasterId, RevisionLabel, RevisionOrder, MaxIterationId, MaxConsumableIterationId FROM dbo.Revision WHERE MasterId IN @parm";
            vaultCache._revisionsForFiles = vaultCache.cacheLoader.LoadVaultEntityDictionary<Revision>(sSelectString, new { parm = fileMasterIDArray });
            Logger.Debug(string.Format("revisions: {0:n0} {1}", vaultCache._revisionsForFiles.Count, swp.LapTimeString()));

            // get the file master ACL Information
            //sSelectString = "SELECT EntityId, EntityClassId, AclId, AclType FROM dbo.EntityToAcl WHERE AclType = 1 AND EntityId IN @parm";
            //vaultCache._masterFileEntityToAcl = vaultCache.cacheLoader.LoadVaultEntityDictionary<EntityToAcl>(sSelectString, new { parm = fileMasterIDArray });
            //Logger.Debug(string.Format("masterFileEntityToAcl: {0:n0}", vaultCache._masterFileEntityToAcl.Count));

            //long[] aclIdArray = vaultCache._masterFileEntityToAcl.Select(kvp => kvp.Value.AclId).Distinct().ToArray();
            //sSelectString = "SELECT AclId, UserGroupId, AllowRead, AllowWrite, AllowDelete, GC FROM dbo.Acl WHERE AclId IN @parm";
            //vaultCache._masterFileACLList = vaultCache.cacheLoader.LoadVaultEntities<Acl>(sSelectString, new { parm = aclIdArray });
            //Logger.Debug(string.Format("masterFileACLList: {0:n0}", vaultCache._masterFileACLList.Count));


            // list of property definitions configured for an individual
            // file MasterId.  To use - isolate the property definitions of interest
            // using these lists and lookup tables
            // the Properties themselves are then associated with the IterationId and ResourceId of the file
            //sSelectString = "SELECT EntityId, PropertyDefId, Uncommitted FROM dbo.PropertyDefOnEntity WHERE EntityId IN @parm";
            //vaultCache._propertyDefOnEntityList = vaultCache.cacheLoader.LoadVaultEntities<PropertyDefOnEntity>(sSelectString, new { parm = fileMasterIDArray });
            //Logger.Debug(string.Format("propertyDefOnEntityList: {0:n0}", vaultCache._propertyDefOnEntityList.Count));

            // generate an ILookup for this set
            //vaultCache._propertyDefOnEntitiesByMasterIds = vaultCache._propertyDefOnEntityList.ToLookup(pdoe => pdoe.EntityId, pdoe => pdoe);
            //Logger.Debug(string.Format("propertyDefOnEntitiesByMasterIds: {0:n0}", vaultCache._propertyDefOnEntitiesByMasterIds.Count));


            // Property Content Provider information
            // not all PropertyDefs will be represented
            // if they are not a system property, and not found in this list
            // they are purely a UDP, and can be imported through VaultBCP as a UDP
            //vaultCache._propertyDefToContentSourceMappingList = vaultCache.cacheLoader.LoadVaultEntities<PropertyDefToContentSourceMapping>();
            //Logger.Debug(string.Format("propertyDefToContentSourceMappingList: {0:n0}", vaultCache._propertyDefToContentSourceMappingList.Count));

            // generate a lookup 
            //vaultCache._propertyDefToContentSourceMappingsByPropertyDefIds = vaultCache._propertyDefToContentSourceMappingList.ToLookup(pdcsm => pdcsm.PropertyDefID, pdcsm => pdcsm);
            //Logger.Debug(string.Format("propertyDefOnEntityList: {0:n0}", vaultCache._propertyDefToContentSourceMappingsByPropertyDefIds.Count));



            // get the Folder ACL Information
            //long[] folderIdArray = vaultCache._folders.Select(kvp => kvp.Key).ToArray();
            //sSelectString = "SELECT EntityId, EntityClassId, AclId, AclType FROM dbo.EntityToAcl WHERE AclType = 1 AND EntityId IN @parm";
            //vaultCache._folderEntityToAcl = vaultCache.cacheLoader.LoadVaultEntityDictionary<EntityToAcl>(sSelectString, new { parm = folderIdArray });
            //Logger.Debug(string.Format("folderEntityToAcl: {0:n0}", vaultCache._folderEntityToAcl.Count));

            //aclIdArray = vaultCache._folderEntityToAcl.Select(kvp => kvp.Value.AclId).Distinct().ToArray();
            //sSelectString = "SELECT AclId, UserGroupId, AllowRead, AllowWrite, AllowDelete, GC FROM dbo.Acl WHERE AclId IN @parm";
            //vaultCache._folderACLList = vaultCache.cacheLoader.LoadVaultEntities<Acl>(sSelectString, new { parm = aclIdArray });
            //Logger.Debug(string.Format("folderACLList: {0:n0}", vaultCache._folderACLList.Count));

            // EntityLifeCycleState
            //sSelectString = "SELECT LifeCycleStateId, EntityID FROM dbo.EntityLifeCycleState WHERE EntityID IN @parm";
            //vaultCache._entityLifeCycleStateList = vaultCache.cacheLoader.LoadVaultEntities<EntityLifeCycleState>(sSelectString, new { parm = fileMasterIDArray });
            //vaultCache._entityLifeCyleStatesByFileMasterIdDictionary = vaultCache._entityLifeCycleStateList.ToDictionary(els => els.EntityID, els => els);
            //Logger.Debug(string.Format("entityLifeCycleStateList: {0:n0}", vaultCache._entityLifeCycleStateList.Count));

            // method to get providers for Iterations input
            //sSelectString = IterationProviderAndContentSource.SelectStatement() + " WHERE FileIteration.FileIterationId IN @parm";
            //vaultCache._iterationProviderAndContentSource = vaultCache.cacheLoader.LoadVaultEntityDictionary<IterationProviderAndContentSource>(sSelectString, new { parm = iterationIdArray });
            //Logger.Debug(string.Format("iterationProviderAndContentSource: {0:n0}", vaultCache._iterationProviderAndContentSource.Count));



            // get entities
            // WARNING - MUST USE Entity.CreateDate when generating FileRevisionIterationRef to already loaded files
            // instead of FileIteration.CheckinDate - they may be off by milliseconds
            sSelectString = "SELECT EntityId, EntityClassID, CreateDate, CreateUserID, Active, IsConsumable FROM dbo.Entity WHERE EntityId IN @parm";
            vaultCache._entitiesForFiles = vaultCache.cacheLoader.LoadVaultEntityDictionary<Entity>(sSelectString, new { parm = vaultCache._iterationsForFiles.Keys.ToArray() });
            Logger.Debug(string.Format("entities: {0:n0} {1}", vaultCache._entitiesForFiles.Count, swp.LapTimeString()));

            Logger.Debug(string.Format("VaultCacheOld LoadRemainderOfFileCacheInformation: End {0}", swp.ElapsedTimeString()));
        }

        // once the item masters have been acquired, get the remainder of the information
        static public void LoadRemainderOfItemCacheInformation(VaultCacheOld vaultCache, bool bDiscoverChildren)
        {
            StopwatchPlus swp = new StopwatchPlus();


            // discover the parents of the initial list of files
            // note, this only gets the parents, and parents of the parents of the initial list, 
            // not the parents of all files that may be found in the children and instances of generics
            // If the initial list IS generics, then we are guaranteed that we will be visiting ALL of the files
            // that reference the generics given or ANY of their instances in any instance hierarchy
            // TODO: potentially implement this


            // TODO:
            // for Honeywell - NOT CAPTURING PARENTS
            //if (false)
            //{
            //int nLoop = 1;
            //List<long> visitedFileMasterIdList = new List<long>();
            //DiscoverParentsAndUpdateFileMasters(vaultCache, visitedFileMasterIdList);
            //do
            //{
            //    // log that we have completed the loop
            //    Logger.Debug(string.Format("Loop{0} Discover Children and Generics Start:", nLoop));

            //    // discover children
            //    if (bDiscoverChildren)
            //    {
            //        DiscoverChildrenAndUpdateFileMasters(vaultCache, bGetAllGenerics, visitedFileMasterIdList);
            //        Logger.Debug(string.Format("VaultCacheOld.DiscoverChildrenAndUpdateFileMasters() {0}", swp.LapTimeString()));
            //    }

            //    // get the list of new file masters
            //    List<long> newFileMasterIdList = vaultCache._fileMasters.Keys.Except(visitedFileMasterIdList).ToList();
            //    List<FileMaster> newFileMastersToVistGenerics = newFileMasterIdList.Select(fmi => vaultCache._fileMasters[fmi]).ToList();

            //    // what we visted to the list
            //    visitedFileMasterIdList.AddRange(newFileMastersToVistGenerics.Select(fm => fm.FileMasterID));


            //    // get ALL of the Generic parent associations to be inclusive of all potential generics
            //    if (newFileMastersToVistGenerics.Count() > 0)
            //    {
            //        vaultCache._fileMasters = GetAllFileMasterGenericInstances(vaultCache, bGetAllGenerics, newFileMastersToVistGenerics);
            //        Logger.Debug(string.Format("fileMasters: {0:n0}", vaultCache._fileMasters.Count));
            //    }

            //    // log that we have completed the loop
            //    Logger.Debug(string.Format("Loop{0} DiscoverChildren and Generics End: {1}", nLoop, swp.LapTimeString()));

            //    // check if we should stop
            //    if (visitedFileMasterIdList.Count() == vaultCache._fileMasters.Count())
            //    {
            //        break;
            //    }

            //    nLoop++;

            //} while (true);
            //            }



            // get the file iterations
            long[] itemMasterIDArray = vaultCache._itemMasters.Keys.ToArray();
            string sSelectString = "SELECT IterationID, MasterID, IterationNumber, RevisionId, Comment FROM dbo.Iteration WHERE MasterID IN @parm";
            vaultCache._iterationsForItems = vaultCache.cacheLoader.LoadVaultEntityDictionary<Iteration>(sSelectString, new { parm = itemMasterIDArray });
            Logger.Debug(string.Format("Total item iterations from ItemMasterId: {0:n0} {1}", vaultCache._iterationsForItems.Count, swp.LapTimeString()));
            vaultCache._initialItemIterationsDiscovered = vaultCache._iterationsForItems.Count;


            // get the file iteration properties
            //long[] fileIterationIdArray = vaultCache.m_FileIterationsExtended_Dict.Keys.ToArray();
            //sSelectString = "SELECT PropertyID, PropertyDefID, EntityID, Value FROM dbo.Property WHERE EntityID IN @parm";
            //vaultCache._fileIterationProperties = vaultCache.cacheLoader.LoadVaultEntityDictionary<Property>(sSelectString, new { parm = fileIterationIdArray });
            //Logger.Debug(string.Format("fileIterationProperties: {0:n0}", vaultCache._fileIterationProperties.Count));



            // get the file iteration property definitions
            //long[] propertyDefIDArray = vaultCache._fileIterationProperties.Select(kvp => kvp.Value.PropertyDefID).Distinct().ToArray();
            //sSelectString = "SELECT PropertyDefID, FriendlyName, SystemName, DataType, Active, BasicSearch, IsSystem, LastCfgUpdate, InitialValue FROM dbo.PropertyDef WHERE PropertyDefID IN @parm";
            //vaultCache._fileIterationPropertyDefs = vaultCache.cacheLoader.LoadVaultEntityDictionary<PropertyDef>(sSelectString, new { parm = propertyDefIDArray });
            //Logger.Debug(string.Format("fileIterationPropertyDefs: {0:n0}", vaultCache._fileIterationPropertyDefs.Count));


            // properties specific to file resources
            // get the properties specific to file Resources
            //long[] fileResourcesIdArray = vaultCache.m_FileResources_Dict.Keys.ToArray();
            //sSelectString = "SELECT ContentSourceID, ContentSourcePropertyMoniker, EntityId, EntityClassId, DataType, Value  FROM dbo.ContentSourceProperties WHERE EntityId IN @parm";
            //vaultCache._contentSourcePropertiesList = vaultCache.cacheLoader.LoadVaultEntities<ContentSourceProperties>(sSelectString, new { parm = fileResourcesIdArray });
            //Logger.Debug(string.Format("contentSourcePropertiesList: {0:n0}", vaultCache._contentSourcePropertiesList.Count));

            // build the lookup for the resource to ContentSourceProperties
            //vaultCache._contentSourcePropertyByResourceIds = vaultCache._contentSourcePropertiesList.ToLookup(csp => csp.EntityId, csp => csp);
            //Logger.Debug(string.Format("contentSourcePropertyByResourceIds: {0:n0}", vaultCache._contentSourcePropertyByResourceIds.Count));


            // get the masters table
            sSelectString = "SELECT MasterID, TipIterationID, TipCommittedIterationID, MaxRevisionId, MaxConsumableRevisionId, ControlledByChangeOrder FROM dbo.Master WHERE MasterID IN @parm";
            vaultCache._mastersForItems = vaultCache.cacheLoader.LoadVaultEntityDictionary<Master>(sSelectString, new { parm = itemMasterIDArray });
            Logger.Debug(string.Format("masters: {0:n0} {1}", vaultCache._mastersForItems.Count, swp.LapTimeString()));

            // get the categories
            // TODO: get categories on other entities? files, items, folders, etc
            sSelectString = "SELECT EntityId, CategoryDefId FROM dbo.CategoryOnEntity WHERE EntityId IN @parm";
            vaultCache._categoriesOnEntitiesForItems = vaultCache.cacheLoader.LoadVaultEntityDictionary<CategoryOnEntity>(sSelectString, new { parm = itemMasterIDArray });
            Logger.Debug(string.Format("categoriesOnEntities: {0:n0} {1}", vaultCache._categoriesOnEntitiesForItems.Count, swp.LapTimeString()));


            // get the Revision table
            // LifeCycleStateId not in Revision table anymore
            // sSelectString = "SELECT RevisionId, RevisionDefinitionId, MasterId, RevisionLabel, RevisionOrder, MaxIterationId, MaxConsumableIterationId, LifeCycleStateId FROM dbo.Revision WHERE MasterId IN @parm";
            sSelectString = "SELECT RevisionId, RevisionDefinitionId, MasterId, RevisionLabel, RevisionOrder, MaxIterationId, MaxConsumableIterationId FROM dbo.Revision WHERE MasterId IN @parm";
            vaultCache._revisionsForItems = vaultCache.cacheLoader.LoadVaultEntityDictionary<Revision>(sSelectString, new { parm = itemMasterIDArray });
            Logger.Debug(string.Format("revisions: {0:n0} {1}", vaultCache._revisionsForItems.Count, swp.LapTimeString()));

            // get the file master ACL Information
            //sSelectString = "SELECT EntityId, EntityClassId, AclId, AclType FROM dbo.EntityToAcl WHERE AclType = 1 AND EntityId IN @parm";
            //vaultCache._masterFileEntityToAcl = vaultCache.cacheLoader.LoadVaultEntityDictionary<EntityToAcl>(sSelectString, new { parm = fileMasterIDArray });
            //Logger.Debug(string.Format("masterFileEntityToAcl: {0:n0}", vaultCache._masterFileEntityToAcl.Count));

            //long[] aclIdArray = vaultCache._masterFileEntityToAcl.Select(kvp => kvp.Value.AclId).Distinct().ToArray();
            //sSelectString = "SELECT AclId, UserGroupId, AllowRead, AllowWrite, AllowDelete, GC FROM dbo.Acl WHERE AclId IN @parm";
            //vaultCache._masterFileACLList = vaultCache.cacheLoader.LoadVaultEntities<Acl>(sSelectString, new { parm = aclIdArray });
            //Logger.Debug(string.Format("masterFileACLList: {0:n0}", vaultCache._masterFileACLList.Count));


            // list of property definitions configured for an individual
            // file MasterId.  To use - isolate the property definitions of interest
            // using these lists and lookup tables
            // the Properties themselves are then associated with the IterationId and ResourceId of the file
            //sSelectString = "SELECT EntityId, PropertyDefId, Uncommitted FROM dbo.PropertyDefOnEntity WHERE EntityId IN @parm";
            //vaultCache._propertyDefOnEntityList = vaultCache.cacheLoader.LoadVaultEntities<PropertyDefOnEntity>(sSelectString, new { parm = fileMasterIDArray });
            //Logger.Debug(string.Format("propertyDefOnEntityList: {0:n0}", vaultCache._propertyDefOnEntityList.Count));

            // generate an ILookup for this set
            //vaultCache._propertyDefOnEntitiesByMasterIds = vaultCache._propertyDefOnEntityList.ToLookup(pdoe => pdoe.EntityId, pdoe => pdoe);
            //Logger.Debug(string.Format("propertyDefOnEntitiesByMasterIds: {0:n0}", vaultCache._propertyDefOnEntitiesByMasterIds.Count));


            // Property Content Provider information
            // not all PropertyDefs will be represented
            // if they are not a system property, and not found in this list
            // they are purely a UDP, and can be imported through VaultBCP as a UDP
            //vaultCache._propertyDefToContentSourceMappingList = vaultCache.cacheLoader.LoadVaultEntities<PropertyDefToContentSourceMapping>();
            //Logger.Debug(string.Format("propertyDefToContentSourceMappingList: {0:n0}", vaultCache._propertyDefToContentSourceMappingList.Count));

            // generate a lookup 
            //vaultCache._propertyDefToContentSourceMappingsByPropertyDefIds = vaultCache._propertyDefToContentSourceMappingList.ToLookup(pdcsm => pdcsm.PropertyDefID, pdcsm => pdcsm);
            //Logger.Debug(string.Format("propertyDefOnEntityList: {0:n0}", vaultCache._propertyDefToContentSourceMappingsByPropertyDefIds.Count));



            // get the Folder ACL Information
            //long[] folderIdArray = vaultCache._folders.Select(kvp => kvp.Key).ToArray();
            //sSelectString = "SELECT EntityId, EntityClassId, AclId, AclType FROM dbo.EntityToAcl WHERE AclType = 1 AND EntityId IN @parm";
            //vaultCache._folderEntityToAcl = vaultCache.cacheLoader.LoadVaultEntityDictionary<EntityToAcl>(sSelectString, new { parm = folderIdArray });
            //Logger.Debug(string.Format("folderEntityToAcl: {0:n0}", vaultCache._folderEntityToAcl.Count));

            //aclIdArray = vaultCache._folderEntityToAcl.Select(kvp => kvp.Value.AclId).Distinct().ToArray();
            //sSelectString = "SELECT AclId, UserGroupId, AllowRead, AllowWrite, AllowDelete, GC FROM dbo.Acl WHERE AclId IN @parm";
            //vaultCache._folderACLList = vaultCache.cacheLoader.LoadVaultEntities<Acl>(sSelectString, new { parm = aclIdArray });
            //Logger.Debug(string.Format("folderACLList: {0:n0}", vaultCache._folderACLList.Count));

            // EntityLifeCycleState
            //sSelectString = "SELECT LifeCycleStateId, EntityID FROM dbo.EntityLifeCycleState WHERE EntityID IN @parm";
            //vaultCache._entityLifeCycleStateList = vaultCache.cacheLoader.LoadVaultEntities<EntityLifeCycleState>(sSelectString, new { parm = fileMasterIDArray });
            //vaultCache._entityLifeCyleStatesByFileMasterIdDictionary = vaultCache._entityLifeCycleStateList.ToDictionary(els => els.EntityID, els => els);
            //Logger.Debug(string.Format("entityLifeCycleStateList: {0:n0}", vaultCache._entityLifeCycleStateList.Count));

            // method to get providers for Iterations input
            //sSelectString = IterationProviderAndContentSource.SelectStatement() + " WHERE FileIteration.FileIterationId IN @parm";
            //vaultCache._iterationProviderAndContentSource = vaultCache.cacheLoader.LoadVaultEntityDictionary<IterationProviderAndContentSource>(sSelectString, new { parm = iterationIdArray });
            //Logger.Debug(string.Format("iterationProviderAndContentSource: {0:n0}", vaultCache._iterationProviderAndContentSource.Count));



            // get entities
            // WARNING - MUST USE Entity.CreateDate when generating FileRevisionIterationRef to already loaded files
            // instead of FileIteration.CheckinDate - they may be off by milliseconds
            sSelectString = "SELECT EntityId, EntityClassID, CreateDate, CreateUserID, Active, IsConsumable FROM dbo.Entity WHERE EntityId IN @parm";
            vaultCache._entitiesForItems = vaultCache.cacheLoader.LoadVaultEntityDictionary<Entity>(sSelectString, new { parm = vaultCache._iterationsForItems.Keys.ToArray() });
            Logger.Debug(string.Format("entities: {0:n0} {1}", vaultCache._entitiesForItems.Count, swp.LapTimeString()));

            Logger.Debug(string.Format("VaultCacheOld LoadRemainderOfFileCacheInformation: End {0}", swp.ElapsedTimeString()));
        }





        // load all files found in whole Vault
        static public void LoadCacheAllFiles(VaultCacheOld vaultCache)
        {
            Logger.Debug("VaultCacheOld LoadCacheAllFiles: Start");

            // check to see if the MSSqlVaultCacheLoader has been enabled
            if (vaultCache.cacheLoader == null)
            {
                vaultCache.cacheLoader = new MSSqlVaultCacheLoader(vaultCache._connectionString);
            }

            vaultCache._fileMasters = vaultCache.cacheLoader.LoadVaultEntityDictionary<FileMaster>();
            Logger.Debug(string.Format("_fileMasters: {0:n0}", vaultCache._fileMasters.Count));

            // once the file masters have been acquired, get the remainder of the information
            LoadRemainderOfFileCacheInformation(vaultCache, null, true, false);


            Logger.Debug("VaultCacheOld LoadCacheAllFiles: End");
        }

        // load all Items found in whole Vault
        static public void LoadCacheAllItems(VaultCacheOld vaultCache)
        {
            Logger.Debug("VaultCacheOld LoadCacheAllItems: Start");

            // check to see if the MSSqlVaultCacheLoader has been enabled
            if (vaultCache.cacheLoader == null)
            {
                vaultCache.cacheLoader = new MSSqlVaultCacheLoader(vaultCache._connectionString);
            }

            vaultCache._itemMasters = vaultCache.cacheLoader.LoadVaultEntityDictionary<ItemMaster>();
            Logger.Debug(string.Format("m_ItemMaster_Dict: {0:n0}", vaultCache._fileMasters.Count));

            // once the file masters have been acquired, get the remainder of the information
            LoadRemainderOfItemCacheInformation(vaultCache, true);


            Logger.Debug("VaultCacheOld LoadCacheAllItems: End");
        }


        // method to get the file path from the full Vault path
        static public string GetVaultPath(string sVaultFullFilePathName)
        {
            string sVaultPath = sVaultFullFilePathName;
            int nIndex = sVaultFullFilePathName.LastIndexOf("/");
            if (nIndex >= 0)
            {
                sVaultPath = sVaultFullFilePathName.Substring(0, nIndex);
            }

            return (sVaultPath);
        }

        // method to get the file path and filename from the full Vault path without extension
        static public string GetVaultPathFilenameWOExt(string sVaultFullFilePathName)
        {
            string sVaultPath = sVaultFullFilePathName;
            int nIndex = sVaultFullFilePathName.LastIndexOf(".");
            if (nIndex >= 0)
            {
                sVaultPath = sVaultFullFilePathName.Substring(0, nIndex);
            }

            return (sVaultPath);
        }



        public Folder GetFolderIdByPath(string vaultPath)
        {
            return _foldersByPathDictionary.ContainsKey(vaultPath) ? _foldersByPathDictionary[vaultPath] : null;
        }

        public IEnumerable<FileMaster> GetFileMastersByFolder(Folder folder)
        {
            return _fileMastersByFolderId[folder.FolderID];
        }

        // get folders from folder names
        // this will get the folders that match the StartsWith (case insensitive), such as 
        // "$/Workspace" will find $/Workspace and all of it's subsequent subfolders
        public IEnumerable<Folder> GetFoldersByFolderNames(List<string> sFolderNames)
        {
            Logger.Debug("GetFoldersByFolderNames(" + sFolderNames.Aggregate((i, j) => i + ";" + j) + ")");


            // make sure to explicitly compare strings (case insensitive) or append "/" to compare for subfolders
            // this will eliminate comparing "$/Workspace" to "$/WorkspaceBob"
            var folders = sFolderNames.SelectMany(
                                    fn => _folders.Values.Where(
                                                fld => (string.Compare(fld.VaultPath, fn, true) == 0) || 
                                                        fld.VaultPath.StartsWith(fn + "/", StringComparison.CurrentCultureIgnoreCase)
                                                )).Distinct();

            Logger.Debug("GetFoldersByFolderNames() Found " + folders.Count().ToString() + " Folders");


            // return the results
            return folders;
        }

        // get folder from the full Vault path name
        public Folder GetFolderByFolderName(string sFolderName)
        {
            // Logger.Debug("GetFolderByFolderName(" + sFolderName + ")"); - takes too much time during the process


            // make sure to explicitly compare strings (case insensitive) or append "/" to compare for subfolders
            // this will eliminate comparing "$/Workspace" to "$/WorkspaceBob"
            Folder folderFound = _folders.Values.FirstOrDefault(fld => (string.Compare(fld.VaultPath, sFolderName, true) == 0));

            // Logger.Debug("GetFolderByFolderName() " + ((folderFound == null) ? "Did not find folder in Vault" : "Found folder in Vault")); - takes too much time during the process

            // return the results
            return folderFound;
        }


        // get filemasters that are found in a list of folders
        public IEnumerable<FileMaster> GetFileMastersByFolderNames(List<string> sFolderNames)
        {
            Logger.Debug("GetFileMastersByFolderNames(" + sFolderNames.Aggregate((i, j) => i + ";" + j) + ")");

            // get the folders from the list of names
            var folders = GetFoldersByFolderNames(sFolderNames);

            var fileMasters = folders.SelectMany(fld => _fileMastersByFolderId[fld.FolderID]);

            Logger.Debug("GetFileMastersByFolderNames() Found " + fileMasters.Count().ToString() + " FileMasters");


            // return the results
            return fileMasters;
        }


        public string GetTipFileNameByFileMaster(FileMaster fileMaster)
        {
            // get the master, then tip
            Master master = _mastersForFiles[fileMaster.FileMasterID];
            FileIterationExtended tipFileIteration = _fileIterationsExtended[master.TipIterationID.Value];

            return (tipFileIteration.FileName);
        }

        public string GetTipFileNameByFileIteration(FileIterationExtended fileIteration)
        {
            // get the iteration, then master, the tip
            Iteration iteration = _iterationsForFiles[fileIteration.FileIterationId];
            Master master = _mastersForFiles[iteration.MasterID];
            FileIterationExtended tipFileIteration = _fileIterationsExtended[master.TipIterationID.Value];

            return (tipFileIteration.FileName);
        }


        // get the FileMaster by FileIteration
        public FileMaster GetFileMasterByFileIteration(FileIterationExtended fileIteration)
        {
            // try two ways of getting FileMaster
            FileMaster fileMaster1 = _fileMasters[_fileResources[fileIteration.ResourceId].FileMasterId];

            // get the iteration, then master
            Iteration iteration = _iterationsForFiles[fileIteration.FileIterationId];
            FileMaster fileMaster2 = _fileMasters[iteration.MasterID];

            return (fileMaster1);
        }


        /// <summary>
        /// Test Method, can be deleted after we are sure of performance
        /// </summary>
        public void TestAllMasterIds()
        {
            var itss = new List<VaultFileIterationOld>();
            foreach (var fileMaster in _fileMasters.Values)
            {
                var its = GetVaultFileIterationsFromVaultMasterId(fileMaster.FileMasterID);
                itss.AddRange(its);
            }
        }

        public IList<VaultFileIterationOld> GetVaultFileIterationsFromVaultMasterId(long vaultMasterId)
        {
            var vfis = new List<VaultFileIterationOld>();

            var iterations = _fileIterationsByMasterId[vaultMasterId];

            foreach (var it in iterations)
            {
                var vfi = new VaultFileIterationOld();
                var fi = _fileIterationsExtended[it.IterationID];
                var fr = _fileResources[fi.ResourceId];
                var ent = _entitiesForFiles[it.IterationID];
                var coe = _categoriesOnEntitiesForFiles[it.MasterID];
                var cd = _categoryDefs[coe.CategoryDefId];
                var rev = _revisionsForFiles[it.RevisionId.Value];
                var rd = _revisionDefinitions[rev.RevisionDefinitionId.Value];
                // var lcs = _lifeCycleStates[rev.LifeCycleStateId.Value];
                //var lcd = _lifeCycleDefinitions[lcs.LifeCycleDefId];
                
                
                vfi.IterationId = it.IterationID;
                vfi.MasterId = it.MasterID;
                vfi.IterationNumber = it.IterationNumber;
                vfi.RevisionId = it.RevisionId.Value;
                vfi.Comment = it.Comment;
                vfi.Checksum = fr.Checksum;
                vfi.CreateDate = ent.CreateDate.Value;
                vfi.ModDate = fi.ModDate;
                vfi.CheckinDate = fi.CheckinDate.Value;
                vfi.LifeCycleStateName = fi.LifeCycleStateName;
                vfi.CategoryDefId = coe.CategoryDefId;
                vfi.CategoryDefName = cd.Name;
                vfi.RevisionDefinitionId = rev.RevisionDefinitionId.Value;
                vfi.RevisionLabel = rev.RevisionLabel;
                vfi.RevisionDefinitionName = rd.DisplayName;
                // vfi.LifeCycleStateId = rev.LifeCycleStateId.Value;
                vfi.LifeCycleStateDisplay = rd.Description;
                //vfi.LifeCycleDefId = lcs.LifeCycleDefId;
                //vfi.LifeCycleStatName = lcs.DisplayName ;
                //vfi.IsReleasedState = lcs.IsReleasedState;
                //vfi.LifeCycleDefName = lcd.DisplayName;

                vfis.Add(vfi);
            }

            return vfis;
        }


        public IEnumerable<FileIterationExtended> GetFileIterationsByPathAndName(Folder vaultFolder, string fileMasterName)
        {
            var mastersInFolder = GetFileMastersByFolder(vaultFolder);

            var iterations = mastersInFolder.SelectMany(m => _fileIterationsByMasterId[m.FileMasterID]);
            var fileIterations = iterations.Select(i => _fileIterationsExtended[i.IterationID]);
            return fileIterations.Where(fi => GetTipFileNameByFileIteration(fi).Equals(fileMasterName, StringComparison.CurrentCultureIgnoreCase));
        }

        public IEnumerable<Revision> GetRevisions(IEnumerable<FileIteration> fileIterations)
        {
            if (!fileIterations.Any())
            {
                return new Revision[0];
            }

            var fileMasterIds = fileIterations.Select(fi => _fileResources[fi.ResourceId].FileMasterId);
            var fileMasterId = fileMasterIds.Distinct().Single();

            // just because a revision label on the Vault.RevisionTable has a record, does not mean that
            // there will be iterations in that list
            return _fileRevisionsByMasterId[fileMasterId];
        }

        public int GetChecksumByFileIteration(FileIteration fileIteration)
        {
            return _fileResources[fileIteration.ResourceId].Checksum;
        }

        public RevisionDefinition GetRevisionDefinition(Revision revision)
        {
            return _revisionDefinitions[revision.RevisionDefinitionId.Value];
        }

        public Folder GetFolderById(long vaultFolderId)
        {
            return _folders[vaultFolderId];
        }

        // FileToFileChildFinder helper class
        public IEnumerable<FileIterationExtended> FindChildren(FileIterationExtended fi)
        {
            return (_childFinder.FindChildren(fi));
        }

        // FileToFileChildFinder helper class
        public IEnumerable<FileIterationExtended> FindChildren(long fileIterationId)
        {
            return (_childFinder.FindChildren(fileIterationId));

        }

        // FileToFileChildFinder helper class
        public IEnumerable<FileIterationExtended> FindParents(IEnumerable<FileIterationExtended> fileIterations)
        {
            List<FileIterationExtended> fileIterationsFound = new List<FileIterationExtended>();

            foreach (FileIterationExtended childIteration in fileIterations)
            {
                fileIterationsFound.AddRange(_childFinder.FindParents(childIteration));
            }

            fileIterationsFound = fileIterationsFound.Distinct().ToList();

            return (fileIterationsFound);
        }


        // FileToFileChildFinder helper class
        public IEnumerable<FileIterationExtended> FindParents(FileIterationExtended fi)
        {
            return (_childFinder.FindParents(fi));
        }

        // FileToFileChildFinder helper class
        public IEnumerable<FileIterationExtended> FindParents(long fileIterationId)
        {
            return (_childFinder.FindParents(fileIterationId));

        }


        // get folder create user list
        public List<long> GetFoldersCreatedByUserIds(List<string> sTopLevelFolderNamesList, bool bIncludeSubfolders, bool bIncludeParentFolders)
        {
            List<long> foldersCreatedByUserIds = new List<long>();
            foreach (string sTopLevelFolderName in sTopLevelFolderNamesList)
            {
                foldersCreatedByUserIds.AddRange(GetFoldersCreatedByUserIds(sTopLevelFolderName, bIncludeSubfolders, bIncludeParentFolders));
            }

            return (foldersCreatedByUserIds.Distinct().ToList());
        }


        // get folder create user list
        public List<long> GetFoldersCreatedByUserIds(string sTopLevelFolderName, bool bIncludeSubfolders, bool bIncludeParentFolders)
        {
            // get the folders
            List<Folder> foldersFound = _foldersByPathDictionary.Where(kvp => ((string.Compare(kvp.Key, sTopLevelFolderName, true) == 0) ||
                                                                      (bIncludeSubfolders && (string.Compare(kvp.Key, sTopLevelFolderName + "/", true) == 0)))).Select(kvp => kvp.Value).ToList();

            // get the parent folders as well
            if (bIncludeParentFolders)
            {
                foldersFound.AddRange(_foldersByPathDictionary.Where(kvp => (sTopLevelFolderName.IndexOf(kvp.Key, StringComparison.InvariantCultureIgnoreCase) == 0)).Select(kvp => kvp.Value));
                foldersFound = foldersFound.Distinct().ToList();
            }

            // get the folder entities
            var folderIdsFound = foldersFound.Select(fld => fld.FolderID);

            // get the Entities
            var entitiesFound = _entitiesForFolders.Where(kvp => folderIdsFound.Contains(kvp.Key)).Select(kvp => kvp.Value);

            // return the unique create user ids
            return (entitiesFound.Select(ent => ent.CreateUserID.Value).Distinct().ToList());
        }


    }
}