using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

using System.IO;


using log4net;
using BCPBuilderConfig;

using MSSql.COIntermDB.Dapper.DirectAccess.DbEntity;
using MSSql.COIntermDB.Dapper.DirectAccess.DbLink;
using MSSql.COIntermDB.Dapper.DirectAccess.DbRelation;
using MSSql.COIntermDB.Dapper.DirectAccess.Hierarchy;
using Dapper;

namespace MSSql.COIntermDB.Dapper.DirectAccess
{
    // coolOrange Intermedate Database SQL Cache object
    public class CO_IntermediateDbSqlCache
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(CO_IntermediateDbSqlCache));

        string m_ConnectionString;
        protected MSSqlCacheLoader m_CacheLoader;


        // Caches that have object IDs
        public Dictionary<int, CustomObject> m_CustomObjectsDict;
        public Dictionary<int, MSSql.COIntermDB.Dapper.DirectAccess.DbEntity.File> m_FilesDict;
        public Dictionary<int, Folder> m_FoldersDict;
        public Dictionary<int, Item> m_ItemsDict;

        // Custom Object Link Caches (don't have object IDs)
        public List<CustomObjectCustomObjectLink> m_CustomObjectCustomObjectLinkList;
        public List<CustomObjectFolderLink> m_CustomObjectFolderLinkList;
        public List<CustomObjectFileLink> m_CustomObjectFileLinkList;
        public List<CustomObjectItemLink> m_CustomObjectItemLinkList;

        // Folder Object Link Caches (don't have object IDs)
        public List<FolderCustomObjectLink> m_FolderCustomObjectLinkList;
        public List<FolderFolderLink> m_FolderFolderLinkList;
        public List<FolderFileLink> m_FolderFileLinkList;
        public List<FolderItemLink> m_FolderItemLinkList;

        // DBRelation Object Caches (don't have object IDs)
        public List<FileFileRelation> m_FileFileRelationList;
        public List<ItemFileRelation> m_ItemFileRelationList;
        public List<ItemItemRelation> m_ItemItemRelationList;

        // Hierarchy objects - maintains top level objects, relationships, etc
        // used to build the top down hierarchy of Items, Files, Folders, and their relationships
        public Dictionary<int, CO_Folder> m_CO_FolderDict;
        public Dictionary<int, CO_FileIteration> m_CO_FileIterationDict;
        public List<CO_FileRevision> m_CO_FileRevisionList;
        public List<CO_FileMaster> m_CO_FileMasterList;

        public Dictionary<int, CO_ItemIteration> m_CO_ItemIterationDict;
        public List<CO_ItemRevision> m_CO_ItemRevisionList;
        public List<CO_ItemMaster> m_CO_ItemMasterList;


        // constructor
        public CO_IntermediateDbSqlCache(string sConnectionString)
        {
            m_ConnectionString = sConnectionString;
        }


        // method to load all tables, files, relationships, and finalize cache
        public void LoadInitialAllFilesAndFinalizeCache()
        {
            LoadInitialCache();
            FinalizeFileCache();

            // TODO: we are not build file relationships for Deublin
            //BuildVaultFileMasterRelationshipsAndAnalyze(vaultCache, bGetAllGenerics);

            FinalizeItemCache();

            // make sure the CO_FileMaster objects have references to CO_ItemMasters
        }


        // method to setup the initial cache's
        public void LoadInitialCache()
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("LoadInitialCache(): Start");

            // check to see if the MSSqlVaultCacheLoader has been enabled
            if (m_CacheLoader == null)
            {
                m_CacheLoader = new MSSqlCacheLoader(m_ConnectionString);
            }


            // output the number of records in each table
            // TODO: !!!!! put this back in when ready to process for production - so we can log the metrics
            // LogTableCounts(vaultCache);

            #region DbEntity Cache Loading


            // load custom objects
            m_CustomObjectsDict = m_CacheLoader.LoadEntitiesWithUDPAsDictionary<CustomObject>();
            Logger.Debug(string.Format("m_CustomObjectsDict: {0:n0}", m_CustomObjectsDict.Count));

            // load Files
            m_FilesDict = m_CacheLoader.LoadEntitiesWithUDPAsDictionary<MSSql.COIntermDB.Dapper.DirectAccess.DbEntity.File>();
            Logger.Debug(string.Format("m_FilesDict: {0:n0}", m_FilesDict.Count));

            // load Folders
            m_FoldersDict = m_CacheLoader.LoadEntitiesWithUDPAsDictionary<Folder>();
            Logger.Debug(string.Format("m_FoldersDict: {0:n0}", m_FoldersDict.Count));

            // analyze and fix folders - don't update IDB, warn user
            RepairIDBFolders(false);

            // load Items
            m_ItemsDict = m_CacheLoader.LoadEntitiesWithUDPAsDictionary<Item>();
            Logger.Debug(string.Format("m_ItemsDict: {0:n0}", m_ItemsDict.Count));

            #endregion DbEntity Cache Loading

            #region DbLink Cache Loading

            // load custom object to custom object links
            m_CustomObjectCustomObjectLinkList = m_CacheLoader.LoadEntities<CustomObjectCustomObjectLink>();
            Logger.Debug(string.Format("m_CustomObjectCustomObjectLinkList: {0:n0}", m_CustomObjectCustomObjectLinkList.Count));

            // load custom object to folder object links
            m_CustomObjectFolderLinkList = m_CacheLoader.LoadEntities<CustomObjectFolderLink>();
            Logger.Debug(string.Format("m_CustomObjectFolderLinkList: {0:n0}", m_CustomObjectFolderLinkList.Count));

            // load custom object to file object links
            m_CustomObjectFileLinkList = m_CacheLoader.LoadEntities<CustomObjectFileLink>();
            Logger.Debug(string.Format("m_CustomObjectFileLinkList: {0:n0}", m_CustomObjectFileLinkList.Count));

            // load custom object to item object links
            m_CustomObjectItemLinkList = m_CacheLoader.LoadEntities<CustomObjectItemLink>();
            Logger.Debug(string.Format("m_CustomObjectItemLinkList: {0:n0}", m_CustomObjectItemLinkList.Count));


            // load folder object to custom object links
            m_FolderCustomObjectLinkList = m_CacheLoader.LoadEntities<FolderCustomObjectLink>();
            Logger.Debug(string.Format("m_FolderCustomObjectLinkList: {0:n0}", m_FolderCustomObjectLinkList.Count));

            // load folder object to folder object links
            m_FolderFolderLinkList = m_CacheLoader.LoadEntities<FolderFolderLink>();
            Logger.Debug(string.Format("m_FolderFolderLinkList: {0:n0}", m_FolderFolderLinkList.Count));

            // load folder object to file object links
            m_FolderFileLinkList = m_CacheLoader.LoadEntities<FolderFileLink>();
            Logger.Debug(string.Format("m_FolderFileLinkList: {0:n0}", m_FolderFileLinkList.Count));

            // load folder object to item object links
            m_FolderItemLinkList = m_CacheLoader.LoadEntities<FolderItemLink>();
            Logger.Debug(string.Format("m_FolderItemLinkList: {0:n0}", m_FolderItemLinkList.Count));


            #endregion DbLink Cache Loading

            #region DbRelation Cache Loading

            // load file to file relationships
            m_FileFileRelationList = m_CacheLoader.LoadEntities<FileFileRelation>();
            Logger.Debug(string.Format("m_FileFileRelationList: {0:n0}", m_FileFileRelationList.Count));

            // load item to file relationships
            m_ItemFileRelationList = m_CacheLoader.LoadEntities<ItemFileRelation>();
            Logger.Debug(string.Format("m_ItemFileRelationList: {0:n0}", m_ItemFileRelationList.Count));

            // load item to item relationships
            m_ItemItemRelationList = m_CacheLoader.LoadEntities<ItemItemRelation>();
            Logger.Debug(string.Format("m_ItemItemRelationList: {0:n0}", m_ItemItemRelationList.Count));

            #endregion DbRelation Cache Loading


            Logger.Debug(string.Format("LoadInitialCache(): End {0}", swp.LapTimeString()));
        }

        // method to build File and Folders and their Hierarchies
        public void FinalizeFileCache()
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("FinalizeFileCache(): Start");

            // build the folders
            // public Dictionary<int, CO_Folder> m_CO_FolderDict;
            IEnumerable<CO_Folder> coFolders = CO_Folder.SetupFoldersAndHierarchies(this.m_FoldersDict.Values);
            m_CO_FolderDict = coFolders.ToDictionary(cfld => cfld.mFolderID, cfld => cfld);
            Logger.Debug(string.Format("m_CO_FolderDict: {0:n0}", m_CO_FolderDict.Count));

            // build the CO_FileIteration objects
            CO_FileIteration.BuildFileIterations(m_FilesDict, ref m_CO_FileIterationDict);
            Logger.Debug(string.Format("m_CO_FileIterationDict: {0:n0}", m_CO_FileIterationDict.Count));

            // build the CO_FileMaster and CO_FileRevision objects
            CO_FileMaster.BuildFileMastersAndRevisions(m_CO_FolderDict, m_CO_FileIterationDict, ref m_CO_FileRevisionList, ref m_CO_FileMasterList);
            Logger.Debug(string.Format("m_CO_FileRevisionList: {0:n0}", m_CO_FileRevisionList.Count));
            Logger.Debug(string.Format("m_CO_FileMasterList: {0:n0}", m_CO_FileMasterList.Count));

            // set FileIteration relationships
            // NOTE: Deublin did not have file relationships
            CO_FileIteration.AssignFileIterationRelationships(m_CO_FileIterationDict, m_FileFileRelationList);

            // link files to folders
            CO_Folder.AssignCO_FileMasters(m_CO_FolderDict, m_FolderFileLinkList, m_CO_FileMasterList);

            Logger.Debug(string.Format("FinalizeFileCache(): End {0}", swp.LapTimeString()));

        }

        // method to build Items and their Hierarchies
        public void FinalizeItemCache()
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("FinalizeItemCache(): Start");


            // build the CO_ItemIteration objects
            CO_ItemIteration.BuildItemIterations(m_ItemsDict, ref m_CO_ItemIterationDict);
            Logger.Debug(string.Format("m_CO_ItemIterationDict: {0:n0}", m_CO_ItemIterationDict.Count));

            // build the CO_ItemMaster and CO_ItemRevision objects
            CO_ItemMaster.BuildItemMastersAndRevisions(m_CO_ItemIterationDict, ref m_CO_ItemRevisionList, ref m_CO_ItemMasterList);
            Logger.Debug(string.Format("m_CO_ItemRevisionList: {0:n0}", m_CO_ItemRevisionList.Count));
            Logger.Debug(string.Format("m_CO_ItemMasterList: {0:n0}", m_CO_ItemMasterList.Count));

            // set ItemIteration relationships
            CO_ItemIteration.AssignItemToItemIterationRelationships(m_CO_ItemIterationDict, m_ItemItemRelationList);
            CO_ItemIteration.AssignItemToFileIterationRelationships(m_CO_ItemIterationDict, m_CO_FileIterationDict, m_ItemFileRelationList);

            // set CO_FileMaster.mCO_ItemMaster
            CO_ItemMaster.AssignItemMasterToFileMaster(m_CO_ItemMasterList);


            // link Items to folders
            CO_Folder.AssignCO_ItemMasters(m_CO_FolderDict, m_FolderItemLinkList, m_CO_ItemMasterList);

            // roll up Item to Item relationships
            CO_ItemIteration.RollupItemToItemIterationRelationships(m_CO_ItemIterationDict);


            Logger.Debug(string.Format("FinalizeItemCache(): End {0}", swp.LapTimeString()));

        }

        #region LogTableCounts

        // get the number of records from the original tables
        public void LogTableCounts()
        {

            StopwatchPlus swp = new StopwatchPlus();

            // check to see if the MSSqlVaultCacheLoader has been enabled
            if (m_CacheLoader == null)
            {
                m_CacheLoader = new MSSqlCacheLoader(m_ConnectionString);
            }

            Logger.Debug(string.Format("LogTableCounts({0}): Start", m_CacheLoader.SqlConnStrBuilder.InitialCatalog));

            // get the database size information
            SpaceUsedDatabaseInfo sudi = this.m_CacheLoader.GetDatabaseSizeInfo();

            // generate the log output for the database size
            string sDatabaseSize = string.Format("Database Size For: \"{0}\"", sudi.database_name) + Environment.NewLine + "DatabaseSize\tUnallSpace\tReserved\tData\tIndexSize\tUnused" + Environment.NewLine +
                                            string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", sudi.database_size, sudi.unallocated_space, sudi.reserved, sudi.data, sudi.index_size, sudi.unused);


            Logger.Debug(sDatabaseSize);


            // get the table information
            List<SpaceUsedTableInfo> sutiList = GetTableSizeInfo();


            // generate the log output for the table sizes
            string sTableSizes = "Table Sizes for Database" + Environment.NewLine + "TableName\tNumberRows\tNumberColumns\tReserved\tData\tIndexSize\tUnused" + Environment.NewLine +
                                        string.Join(Environment.NewLine, sutiList.Select(suti => string.Format("{0}\t{1:n0}\t{2}\t{3}\t{4}\t{5}\t{6}", suti.name, suti.rows, suti.numColumns, suti.reserved, suti.data, suti.index_size, suti.unused)));

            Logger.Debug(sTableSizes);
        }

        // get the number of records from the original tables
        public List<SpaceUsedTableInfo> GetTableSizeInfo()
        {
            // check to see if the MSSqlVaultCacheLoader has been enabled
            if (this.m_CacheLoader == null)
            {
                this.m_CacheLoader = new MSSqlCacheLoader(m_ConnectionString);
            }

            // setup a list of the table names
            string[] tableNames = new string[] { "CustomObjectCustomObjectLinks", "CustomObjectFileLinks", "CustomObjectFolderLinks", "CustomObjectItemLinks", "CustomObjects",
                                                    "FileFileRelations", "Files", "FolderCustomObjectLinks", "FolderFileLinks", "FolderFolderLinks", "FolderItemLinks", "Folders",
                                                    "ItemFileRelations", "ItemItemRelations", "Items" };

            // get the table information
            List<SpaceUsedTableInfo> sutiList = this.m_CacheLoader.GetTableSizeInfo(tableNames);

            return (sutiList);
        }


        #endregion LogTableCounts



        #region local file processing

        // method to process local file information
        public void ProcessLocalFileInformation(bool bUpdateDatabase)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug(string.Format("ProcessLocalFileInformation(): Start {0:n0} files to process", this.m_FilesDict.Count()));

            // validate columns are in database
            SetupDatabaseColumnsForLocalFileProcessing();

            // iterate through the local files and process them for information
            int nLoopCount = 0;
            int nMissing = 0;
            foreach (MSSql.COIntermDB.Dapper.DirectAccess.DbEntity.File file in this.m_FilesDict.Values)
            {
                // check to see if the file exists
                if (!System.IO.File.Exists(file.LocalFullFileName))
                {
                    file.LocalFileExists = false;
                    nMissing++;
                }
                else
                {
                    file.LocalFileExists = true;

                    file.LocalFileCheckSum = GetcheckSum.CalcCRC32(file.LocalFullFileName);
                }

                nLoopCount++;

                if ((nLoopCount % 100) == 0)
                {
                    Logger.Debug(string.Format("ProcessLocalFileInformation(): {0:n0}/{1:n0} ({2:n0} missing) files to process", nLoopCount, this.m_FilesDict.Count(), nMissing));
                }

                // set the process date
                file.LocalFileProcessedDate = DateTime.Now;
            }

            Logger.Debug(string.Format("ProcessLocalFileInformation(): {0:n0}/{1:n0} ({2:n0} missing) files to process", nLoopCount, this.m_FilesDict.Count(), nMissing));

            // update the database with the checksum information
            if (bUpdateDatabase)
            {
                UpdateDBLocalFileChecksumStatistics();
            }

            Logger.Debug(string.Format("ProcessLocalFileInformation(): End {0}", swp.LapTimeString()));
        }

        // method to validate columns are in database
        public void SetupDatabaseColumnsForLocalFileProcessing()
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("SetupDatabaseColumnsForLocalFileProcessing(): Start");

            // check to see if the MSSqlVaultCacheLoader has been enabled
            if (this.m_CacheLoader == null)
            {
                this.m_CacheLoader = new MSSqlCacheLoader(m_ConnectionString);
            }


            // check to see if the LocalFileProcessedDate exists
            if (!this.m_CacheLoader.TableColumnExists<MSSql.COIntermDB.Dapper.DirectAccess.DbEntity.File>("LocalFileProcessedDate"))
            {
                Logger.Debug("[File].[LocalFileProcessedDate] column does not exist, adding...");

                // add the column
                this.m_CacheLoader.AddTableColumn<MSSql.COIntermDB.Dapper.DirectAccess.DbEntity.File>("LocalFileProcessedDate", "DATETIME");
            }
            else
            {
                Logger.Debug("[File].[LocalFileProcessedDate] column exists...");
            }

            // check to see if the LocalFileExists exists
            if (!this.m_CacheLoader.TableColumnExists<MSSql.COIntermDB.Dapper.DirectAccess.DbEntity.File>("LocalFileExists"))
            {
                Logger.Debug("[File].[LocalFileExists] column does not exist, adding...");

                // add the column
                this.m_CacheLoader.AddTableColumn<MSSql.COIntermDB.Dapper.DirectAccess.DbEntity.File>("LocalFileExists", "BIT");
            }
            else
            {
                Logger.Debug("[File].[LocalFileExists] column exists...");
            }

            // check to see if the LocalFileCheckSum exists
            if (!this.m_CacheLoader.TableColumnExists<MSSql.COIntermDB.Dapper.DirectAccess.DbEntity.File>("LocalFileCheckSum"))
            {
                Logger.Debug("[File].[LocalFileCheckSum] column does not exist, adding...");

                // add the column
                this.m_CacheLoader.AddTableColumn<MSSql.COIntermDB.Dapper.DirectAccess.DbEntity.File>("LocalFileCheckSum", "INT");
            }
            else
            {
                Logger.Debug("[File].[LocalFileCheckSum] column exists...");
            }


            Logger.Debug(string.Format("SetupDatabaseColumnsForLocalFileProcessing(): End {0}", swp.LapTimeString()));
        }

        // method to update the local file Checksum statistics in the database for all Files in m_FilesDict;
        public void UpdateDBLocalFileChecksumStatistics()
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug(string.Format("UpdateDBLocalFileChecksumStatistics(): Start {0:n0} files to process", this.m_FilesDict.Count()));

            // iterate through each file
            foreach (MSSql.COIntermDB.Dapper.DirectAccess.DbEntity.File file in this.m_FilesDict.Values)
            {
                // have the object update its checksum statistics
                file.UpdateDBLocalFileChecksumStatistics(m_CacheLoader);
            }

            Logger.Debug(string.Format("UpdateDBLocalFileChecksumStatistics(): End {0}", swp.LapTimeString()));
        }


        #endregion local file processing


        #region existing Vault file discovery processing

        // method to validate columns are in database
        public void SetupDatabaseColumnsForExistingVaultFileDiscovery()
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("SetupDatabaseColumnsForExistingVaultFileDiscovery(): Start");

            // check to see if the MSSqlVaultCacheLoader has been enabled
            if (this.m_CacheLoader == null)
            {
                this.m_CacheLoader = new MSSqlCacheLoader(m_ConnectionString);
            }


            // check to see if the EntityCreateDate exists
            if (!this.m_CacheLoader.TableColumnExists<MSSql.COIntermDB.Dapper.DirectAccess.DbEntity.File>("EntityCreateDate"))
            {
                Logger.Debug("[File].[EntityCreateDate] column does not exist, adding...");

                // add the column
                this.m_CacheLoader.AddTableColumn<MSSql.COIntermDB.Dapper.DirectAccess.DbEntity.File>("EntityCreateDate", "DATETIME");
            }
            else
            {
                Logger.Debug("[File].[EntityCreateDate] column exists...");
            }

            // check to see if the VaultFileExists exists
            if (!this.m_CacheLoader.TableColumnExists<MSSql.COIntermDB.Dapper.DirectAccess.DbEntity.File>("VaultFileExists"))
            {
                Logger.Debug("[File].[VaultFileExists] column does not exist, adding...");

                // add the column
                this.m_CacheLoader.AddTableColumn<MSSql.COIntermDB.Dapper.DirectAccess.DbEntity.File>("VaultFileExists", "BIT");
            }
            else
            {
                Logger.Debug("[File].[VaultFileExists] column exists...");
            }

            // check to see if the VaultFileCheckSum exists
            if (!this.m_CacheLoader.TableColumnExists<MSSql.COIntermDB.Dapper.DirectAccess.DbEntity.File>("VaultFileCheckSum"))
            {
                Logger.Debug("[File].[VaultFileCheckSum] column does not exist, adding...");

                // add the column
                this.m_CacheLoader.AddTableColumn<MSSql.COIntermDB.Dapper.DirectAccess.DbEntity.File>("VaultFileCheckSum", "INT");
            }
            else
            {
                Logger.Debug("[File].[VaultFileCheckSum] column exists...");
            }


            Logger.Debug(string.Format("SetupDatabaseColumnsForExistingVaultFileDiscovery(): End {0}", swp.LapTimeString()));
        }

        // method to update the Vault file Checksum statistics in the database for all Files in m_FilesDict;
        public void UpdateDBVaultFileChecksumStatistics()
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug(string.Format("UpdateDBVaultFileChecksumStatistics(): Start {0:n0} files to process", this.m_FilesDict.Count()));

            // iterate through each file
            foreach (MSSql.COIntermDB.Dapper.DirectAccess.DbEntity.File file in this.m_FilesDict.Values)
            {
                // have the object update its checksum statistics
                file.UpdateDBVaultFileChecksumStatistics(m_CacheLoader);
            }

            Logger.Debug(string.Format("UpdateDBVaultFileChecksumStatistics(): End {0}", swp.LapTimeString()));
        }



        #endregion existing Vault file discovery processing


        #region IDB Folders fix methods

        // some databases - those that are loaded by IDB.Load.BCP will have incorrect folder structures

        // load just the Folders table
        public void LoadInitialForFoldersRepair()
        {
            Logger.Debug("LoadInitialForFoldersRepair(): Start");

            // check to see if the MSSqlVaultCacheLoader has been enabled
            if (m_CacheLoader == null)
            {
                m_CacheLoader = new MSSqlCacheLoader(m_ConnectionString);
            }

            // load Folders
            m_FoldersDict = m_CacheLoader.LoadEntitiesWithUDPAsDictionary<Folder>();
            Logger.Debug(string.Format("m_FoldersDict: {0:n0}", m_FoldersDict.Count));

            Logger.Debug("LoadInitialForFoldersRepair(): End");
        }

        // load just the Folders table
        public void RepairIDBFolders(bool bCreateUpdateIDBRecords)
        {
            // WARNING!!! this does create folders in the database if they are needed, and adds to the VaultCache.m_FoldersDict
            // but it does not update the this.m_CO_FolderDict
            // it is expected that you use this as a one off in a utility to fix the folders

            if (bCreateUpdateIDBRecords)
            {
                Logger.Debug("RepairIDBFolders(): Start: Will Create/Update IDB Records");
            }
            else
            {
                Logger.Debug("RepairIDBFolders(): Start: Will NOT Create/Update IDB Records\n!!! WARNING !!! use GUI to fix database if you need to");
            }

            // create a dictionary of folders based on their path
            Dictionary<string, Folder> folderDictByVaultPath = m_FoldersDict.Values.ToDictionary(fld => fld.Path, fld => fld, StringComparer.InvariantCultureIgnoreCase);

            // iterate through each folder and verify that the parent exists
            // that the ParentFolderID element is correct
            // if not, create the parent/and/or fix the ParentFolderID reference

            // we need to create another list because we will be adding to m_FoldersDict.Values
            List<Folder> folderList = new List<Folder>();
            folderList.AddRange(m_FoldersDict.Values);
            foreach (Folder folder in folderList)
            {
                // if it is not the $, verify that the parents exist
                if (folder.FolderName != "$")
                {
                    string parentFolderPath = folder.GetParentFolderPathFromPath();

                    // get or create the parent folder in our management of those folders as well
                    Folder parentFolder = GetOrCreateIDBFolder(parentFolderPath, ref folderDictByVaultPath, bCreateUpdateIDBRecords);

                    // verify that the Folder.ParentFolderID is the same as the Parent.FolderID
                    if ((folder.ParentFolderID == null) || (folder.ParentFolderID != parentFolder.FolderID))
                    {
                        Logger.Debug(string.Format("Warning!!! ParentFolderID {0}, Now Fixing: \"{1}\"", (folder.ParentFolderID == null) ? "Was Null" : "Not Same As Parent", folder.Path));

                        // update folder ParentFolderID
                        folder.ParentFolderID = parentFolder.FolderID;

                        if (bCreateUpdateIDBRecords)
                        {
                            string updateQuery = "UPDATE [dbo].[Folders] SET ParentFolderID = @ParentFolderID WHERE FolderID = @FolderID";
                            int nUpdated = m_CacheLoader.UpdateDbEntity<Folder>(updateQuery, folder);
                        }
                    }
                }

                // verify that the FolderName is the same as the last folder in the path
                List<string> folderNames = folder.Path.Split('/').ToList();
                if (folder.FolderName != folderNames.Last())
                {
                    Logger.Debug(string.Format("Warning!!! FolderName Does Not Match Path, Now Fixing: \"{0}\" \"{1}\"", folder.FolderName, folder.Path));

                    // update folder ParentFolderID
                    folder.FolderName = folderNames.Last();

                    if (bCreateUpdateIDBRecords)
                    {
                        string updateQuery = "UPDATE [dbo].[Folders] SET FolderName = @FolderName WHERE FolderID = @FolderID";
                        int nUpdated = m_CacheLoader.UpdateDbEntity<Folder>(updateQuery, folder);
                    }
                }


            }


            Logger.Debug("RepairIDBFolders(): End");
        }


        // get or create the parent folder as needed
        public Folder GetOrCreateIDBFolder(string folderPath, ref Dictionary<string, Folder> folderDictByVaultPath, bool bCreateUpdateIDBRecords)
        {
            Folder folder = null;
            if (folderDictByVaultPath.ContainsKey(folderPath))
            {
                folder = folderDictByVaultPath[folderPath];
            }
            else if (folderPath == "$")
            {
                folder = new Folder();
                folder.FolderName = "$";
                folder.Path = "$";
                folder.IsLibrary = false;
                folder.CreateUser = "Administrator";
                folder.CreateDate = DateTime.Now;

                int folderID = this.m_FoldersDict.Values.Max(fld => fld.FolderID) + 1;
                if (bCreateUpdateIDBRecords)
                {
                    string insertQuery = "INSERT INTO [dbo].[Folders] (FolderName, Path, IsLibrary, CreateUser, CreateDate) VALUES (@FolderName, @Path, @IsLibrary, @CreateUser, @CreateDate)";
                    folderID = m_CacheLoader.InsertDbEntityReturnIdentiy<Folder>(insertQuery, folder);
                }

                // set the ID of the inserted folder
                folder.FolderID = folderID;

                // add it to the dictionaries
                folderDictByVaultPath.Add(folder.Path, folder);
                this.m_FoldersDict.Add(folder.FolderID, folder);
            }
            else
            {
                // get or create the parent
                string parentFolderPath = Folder.GetParentFolderPathFromPath(folderPath);

                // get or create the parentFolder
                Folder parentFolder = GetOrCreateIDBFolder(parentFolderPath, ref folderDictByVaultPath, bCreateUpdateIDBRecords);

                // since we know this folder did not exist
                // we need to create it in the database, add it to the folderDictByVaultPath, and to the VaultCache.m_FoldersDict
                List<string> folderNames = folderPath.Split('/').ToList();
                folder = new Folder();

                folder.ParentFolderID = parentFolder.FolderID;
                folder.FolderName = folderNames.Last();
                folder.Path = folderPath;
                folder.IsLibrary = false;
                folder.CreateUser = "Administrator";
                folder.CreateDate = DateTime.Now;

                int folderID = this.m_FoldersDict.Values.Max(fld => fld.FolderID) + 1;
                if (bCreateUpdateIDBRecords)
                {
                    string insertQuery = "INSERT INTO [dbo].[Folders] (ParentFolderID, FolderName, Path, IsLibrary, CreateUser, CreateDate) VALUES (@ParentFolderID, @FolderName, @Path, @IsLibrary, @CreateUser, @CreateDate)";
                    folderID = m_CacheLoader.InsertDbEntityReturnIdentiy<Folder>(insertQuery, folder);
                }

                // set the ID of the inserted folder
                folder.FolderID = folderID;

                // add it to the dictionaries
                folderDictByVaultPath.Add(folder.Path, folder);
                this.m_FoldersDict.Add(folder.FolderID, folder);
            }

            return (folder);
        }


        #endregion IDB Folders fix methods



    }
}
