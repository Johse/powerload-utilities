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


namespace MSSql.COIntermDB.Dapper.DirectAccess.Hierarchy
{
    public class CO_Folder
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(CO_Folder));

        // manage the Folder table record for this folder
        public Folder mFolderRecord { get; private set; }

        public string mFolderName { get; private set; }
        public int mFolderID { get; private set; }
        public string mFullVaultFolderPath { get; private set; }

        // manage the parents and children
        public List<CO_Folder> mChildFolders { get; private set; }
        public CO_Folder mParentFolder { get; private set; }

        // list of CO_FileMaster - owned by folder
        public List<CO_FileMaster> mCO_FileMasterList { get; private set; }

        // list of Linked CO_FileMaster and CO_ItemMaster
        public List<CO_FileMaster> mLinkedmCO_FileMasterList { get; private set; }
        public List<CO_ItemMaster> mLinkedmCO_ItemMasterList { get; private set; }

        public int mFolderDepth { get; private set; }
        public int mMaxFolderChildDepth { get; private set; }


        // manage the properties for rolled up folder and files count
        public List<CO_Folder> mRolledUpFolders { get; private set; }
        public List<CO_FileMaster> mRolledUpFiles { get; private set; }


        // constructor
        public CO_Folder(Folder folderRecord)
        {
            // assign the properties
            mFolderRecord = folderRecord;

            // if the folderRecord is NULL, this is the root $ folder
            // because CO_IntermediateDbSqlCache.RepairIDBFolders() already fixed these,
            // we should have a record for each CO_Folder
            //if (mFolderRecord == null)
            //{
            //    mFolderName = "$";
            //    mFullVaultFolderPath = "$";
            //    mFolderID = 0;
            //}
            //else
            //{
            //    mFolderName = mFolderRecord.FolderName;
            //    mFolderID = mFolderRecord.FolderID;
            //}

            mFolderName = mFolderRecord.FolderName;
            mFolderID = mFolderRecord.FolderID;


            // assign the properties
            mChildFolders = new List<CO_Folder>();
            mCO_FileMasterList = new List<CO_FileMaster>();
            mLinkedmCO_FileMasterList = new List<CO_FileMaster>();
            mLinkedmCO_ItemMasterList = new List<CO_ItemMaster>();

            mFolderDepth = 0;
            mMaxFolderChildDepth = 0;

            mRolledUpFolders = new List<CO_Folder>();
            mRolledUpFiles = new List<CO_FileMaster>();
        }

        // method to build the hierarchy of folder objects
        // NO longer using this method because CO_IntermediateDbSqlCache.RepairIDBFolders() already fixed 
        // folder hierarchy
        //public static IEnumerable<CO_Folder> SetupFoldersAndHierarchiesOLD(IEnumerable<Folder> folders)
        //{
        //    StopwatchPlus swp = new StopwatchPlus();

        //    Logger.Debug("SetupFoldersAndHierarchies(): Start");

        //    // create the folder list
        //    List<CO_Folder> coFolderList = new List<CO_Folder>();

        //    // create the root documents $ folder
        //    CO_Folder rootCfld = new CO_Folder(null);
        //    coFolderList.Add(rootCfld);

        //    // iterate through the given folder list and build the folders
        //    foreach (Folder fld in folders)
        //    {
        //        CO_Folder cfld = new CO_Folder(fld);
        //        coFolderList.Add(cfld);
        //    }

        //    // create a Dictionary of the built folders
        //    Dictionary<int, CO_Folder> coFolderDictionary = coFolderList.ToDictionary(cfld => cfld.mFolderID, cfld => cfld);

        //    // the [Folder] table points the child back to the parent with the [ParentFolderID] element
        //    // use this to setup the hierarchies
        //    foreach (CO_Folder cfld in coFolderList)
        //    {
        //        // check to see if it had a record
        //        if (cfld.mFolderRecord != null)
        //        {
        //            // null ParentFolderID indicates that the parent should be the $
        //            CO_Folder parentCO_Folder = rootCfld;
        //            if (cfld.mFolderRecord.ParentFolderID.HasValue)
        //            {
        //                // get the parent folder
        //                parentCO_Folder = coFolderDictionary[cfld.mFolderRecord.ParentFolderID.Value];
        //            }

        //            // assign the parent
        //            cfld.mParentFolder = parentCO_Folder;

        //            // assign the children
        //            parentCO_Folder.mChildFolders.Add(cfld);
        //        }
        //    }

        //    // iterate through the root Documents folder and set all childrens mFullVaultFolderPath
        //    rootCfld.SetupChildrenFullFolderPath();

        //    // get the max folder depth
        //    Logger.Debug(string.Format("Number of Folders: {0:n0}", coFolderList.Count()));
        //    Logger.Debug(string.Format("Max Folder Depth: {0:n0}", rootCfld.mMaxFolderChildDepth));


        //    // log total time taken
        //    Logger.Debug(string.Format("SetupFoldersAndHierarchies(): End {0}", swp.ElapsedTimeString()));

        //    return (coFolderList);
        //}

        // method to build the hierarchy of folder objects
        // because CO_IntermediateDbSqlCache.RepairIDBFolders() already fixed folder hierarchy
        // we can blindly go without checking much
        public static IEnumerable<CO_Folder> SetupFoldersAndHierarchies(IEnumerable<Folder> folders)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("SetupFoldersAndHierarchies(): Start");

            // create the folder list
            List<CO_Folder> coFolderList = new List<CO_Folder>();

            // iterate through the given folder list and build the folders
            foreach (Folder fld in folders)
            {
                CO_Folder cfld = new CO_Folder(fld);
                coFolderList.Add(cfld);
            }

            // get the root folder
            CO_Folder rootCfld = coFolderList.Where(fld => fld.mFolderName == "$").First();
            rootCfld.mFullVaultFolderPath = "$";

            // create a Dictionary of the built folders
            Dictionary<int, CO_Folder> coFolderDictionary = coFolderList.ToDictionary(cfld => cfld.mFolderID, cfld => cfld);

            // the [Folder] table points the child back to the parent with the [ParentFolderID] element
            // use this to setup the hierarchies
            foreach (CO_Folder cfld in coFolderList)
            {
                // check to see if this is the root "$" folder
                if (cfld != rootCfld)
                {
                    // get the parent folder
                    CO_Folder parentCO_Folder = coFolderDictionary[cfld.mFolderRecord.ParentFolderID.Value];

                    // assign the parent
                    cfld.mParentFolder = parentCO_Folder;

                    // assign the children
                    parentCO_Folder.mChildFolders.Add(cfld);
                }
            }

            // iterate through the root Documents folder and set all childrens mFullVaultFolderPath
            rootCfld.SetupChildrenFullFolderPath();

            // get the max folder depth
            Logger.Debug(string.Format("Number of Folders: {0:n0}", coFolderList.Count()));
            Logger.Debug(string.Format("Max Folder Depth: {0:n0}", rootCfld.mMaxFolderChildDepth));


            // log total time taken
            Logger.Debug(string.Format("SetupFoldersAndHierarchies(): End {0}", swp.ElapsedTimeString()));

            return (coFolderList);
        }



        // method to iterate children and set their mFullVaultFolderPath
        protected void SetupChildrenFullFolderPath()
        {
            foreach (CO_Folder childCOF in mChildFolders)
            {
                // set the child mFullVaultFolderPath
                childCOF.mFullVaultFolderPath = this.mFullVaultFolderPath + "/" + childCOF.mFolderName;
                childCOF.mFolderDepth = this.mFolderDepth + 1;

                // iterate through the children of this child
                childCOF.SetupChildrenFullFolderPath();

                this.mMaxFolderChildDepth = Math.Max(this.mMaxFolderChildDepth, childCOF.mMaxFolderChildDepth + 1);
            }
        }

        // method to assign the child CO_FileMasters to folders as children AND as linked
        public static bool AssignCO_FileMasters(Dictionary<int, CO_Folder> co_FolderDict, List<FolderFileLink> folderFileLinkList, List<CO_FileMaster> co_FileMasterList)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("AssignCO_FileMasters(): Start");

            bool bSuccess = false;

            // create a lookup for all of the File Masters associated to folders by ParentID
            ILookup<CO_Folder, CO_FileMaster> fileMasterLookupByFolder = co_FileMasterList.ToLookup(cfm => cfm.mParentCO_Folder, cfm => cfm);

            // iterate through the folders
            foreach (IGrouping<CO_Folder, CO_FileMaster> groupItem in fileMasterLookupByFolder)
            {
                // groupItem.Key = CO_Folder
                // groupItem is IEnumerable<CO_FileMaster>
                groupItem.Key.mCO_FileMasterList.AddRange(groupItem);
            }



            // assign file masters that are linked to the folders
            // build a lookup of the File iterations linked to the FolderId
            ILookup<int, int> fileLinkToFolderLookup = folderFileLinkList.ToLookup(ffl => ffl.ParentFolderID, ffl => ffl.ChildFileID);

            // build a Dictionary of file iteration reference ID's to FileMasters
            Dictionary<int, CO_FileMaster> co_FileMasterDictByIterationId = co_FileMasterList.SelectMany(cfm => cfm.mCO_FileIterationList)
                                                                                                .ToDictionary(cfi => cfi.mFileId, cfi => cfi.mCO_FileMasterOwner);

            // iterate through each folder and identify if there are File Links (there may be Item links as well)
            // verify that the folder DOES NOT ALREADY OWN THE FILE AS A PRIMARY CHILD
            int nFileMastersToLink = 0;
            int nFileMastersLinked = 0;
            foreach (IGrouping<int, int> groupItem in fileLinkToFolderLookup)
            {
                // groupItem.Key = folder FolderID<int>
                // groupItem is IEnumerable<int> File IterationId(FileID)

                // get the folder
                CO_Folder co_Folder= co_FolderDict[groupItem.Key];

                // iterate through the Linked IterationId(s)
                List<CO_FileMaster> linkedCO_FileMasterList = new List<CO_FileMaster>();
                foreach (int fileID in groupItem)
                {
                    if (co_FileMasterDictByIterationId.ContainsKey(fileID))
                    {
                        linkedCO_FileMasterList.Add(co_FileMasterDictByIterationId[fileID]);
                    }
                }

                // make sure this is a distinct list
                linkedCO_FileMasterList = linkedCO_FileMasterList.Distinct().ToList();

                // verify that the folder does not ALREADY own the FileMaster as a primary child
                co_Folder.mLinkedmCO_FileMasterList = linkedCO_FileMasterList.Except(co_Folder.mCO_FileMasterList).ToList();

                // add the statistics
                nFileMastersToLink += linkedCO_FileMasterList.Count();
                nFileMastersLinked += co_Folder.mLinkedmCO_FileMasterList.Count();
            }

            // log any issues on not linking files
            if (nFileMastersToLink != nFileMastersLinked)
            {
                Logger.Debug(string.Format("!!!Warning - Could not link all File Masters (ToLink/CouldLink): {0:n0}/{1:n0}", nFileMastersToLink, nFileMastersLinked));
            }


            // log total time taken
            Logger.Debug(string.Format("AssignCO_FileMasters(): End {0}", swp.ElapsedTimeString()));

            bSuccess = true;

            return (bSuccess);
        }

        // method to assign the CO_ItemMasters to folders as ONLY linked
        public static bool AssignCO_ItemMasters(Dictionary<int, CO_Folder> co_FolderDict, List<FolderItemLink> folderItemLinkList, List<CO_ItemMaster> co_ItemMasterList)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("AssignCO_ItemMasters(): Start");

            bool bSuccess = false;

            // assign Item masters that are linked to the folders
            // build a lookup of the Item iterations linked to the FolderId
            ILookup<int, int> itemLinkToFolderLookup = folderItemLinkList.ToLookup(ffl => ffl.ParentFolderID, ffl => ffl.ChildItemID);

            // build a Dictionary of Item iteration reference ID's to ItemMasters
            Dictionary<int, CO_ItemMaster> co_ItemMasterDictByIterationId = co_ItemMasterList.SelectMany(cim => cim.mCO_ItemIterationList)
                                                                                                .ToDictionary(cii => cii.mItemId, cii => cii.mCO_ItemMasterOwner);

            // iterate through each folder and identify if there are Item Links
            int nItemMastersLinked = 0;
            foreach (IGrouping<int, int> groupItem in itemLinkToFolderLookup)
            {
                // groupItem.Key = folder FolderID<int>
                // groupItem is IEnumerable<int> Item IterationId(ItemID)

                // get the folder
                CO_Folder co_Folder = co_FolderDict[groupItem.Key];

                // iterate through the Linked IterationId(s)
                foreach (int itemID in groupItem)
                {
                    if (co_ItemMasterDictByIterationId.ContainsKey(itemID))
                    {
                        co_Folder.mLinkedmCO_ItemMasterList.Add(co_ItemMasterDictByIterationId[itemID]);
                    }
                }

                // make sure this is a distinct list
                co_Folder.mLinkedmCO_ItemMasterList = co_Folder.mLinkedmCO_ItemMasterList.Distinct().ToList();

                // add the statistics
                nItemMastersLinked += co_Folder.mLinkedmCO_ItemMasterList.Count();
            }

            // log any issues on not linking files
            if (folderItemLinkList.Count() != nItemMastersLinked)
            {
                Logger.Debug(string.Format("!!!Warning - Could not link all Item Masters (ToLink/CouldLink): {0:n0}/{1:n0}", folderItemLinkList.Count(), nItemMastersLinked));
            }


            // log total time taken
            Logger.Debug(string.Format("AssignCO_ItemMasters(): End {0}", swp.ElapsedTimeString()));

            bSuccess = true;

            return (bSuccess);
        }


    }
}
