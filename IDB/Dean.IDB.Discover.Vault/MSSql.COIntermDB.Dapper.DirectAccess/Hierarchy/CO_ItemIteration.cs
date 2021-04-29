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
    // class to manage the original CO Item record iteration
    // relationships to Item Revisions
    // relationship to its Item Master
    public class CO_ItemIteration
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(CO_ItemIteration));

        // flags managing the status of the file
        [Flags]
        public enum ItemIterationStatus : ulong
        {
            NoStatus = 0,

            // top level/lowest level
            IsTopLevel = 1L << 1,
            IsLowLevel = 1L << 2,

            // identify if there is a circular dependency
            CicularDepend = 1L << 3,
            DirectCircDepend = 1L << 4,
        }


        // manage the file Iteration record for this ItemIteration
        public Item mItemIterationRecord { get; private set; }

        public string mItemNumber { get; private set; }
        public int mItemId { get; private set; }

        // manage who owns this object
        public CO_ItemRevision mCO_ItemRevisionOwner { get; private set; }
        public CO_ItemMaster mCO_ItemMasterOwner { get; private set; }

        // manage the Item to Item relationships
        public List<CO_ItemIteration> mChild_CO_ItemIterations { get; private set; }
        public List<CO_ItemIteration> mGrandChild_CO_ItemIterations { get; private set; }
        public List<CO_ItemIteration> mParent_CO_ItemIterations { get; private set; }
        public List<CO_ItemIteration> mGrandParent_CO_ItemIterations { get; private set; }
        public Dictionary<int, ItemItemRelation> mItemItemRelationDictByChildId { get; private set; }

        // manage whether the FileMaster had it's hierarchy built
        public bool mHierarchyBuilt { get; private set; }
        public int mHierarchyDepth { get; private set; }


        // manage the status of the ItemIteration
        public ItemIterationStatus mItemIterationStatus { get; private set; }





        // manage the Item to File relationships
        public List<CO_FileIteration> mChild_CO_FileIterations { get; private set; }
        public Dictionary<int, ItemFileRelation> mItemFileRelationDictByChildId { get; private set; }

        // manage the VaultBCP content for building the data loader package
        public ItemIteration mBcpItemIteration { get; set; }



        // constructor
        public CO_ItemIteration(Item iterationRecord)
        {
            // assign the properties
            mItemIterationRecord = iterationRecord;
            mItemNumber = iterationRecord.ItemNumber;
            mItemId = iterationRecord.ItemID;

            mChild_CO_ItemIterations = new List<CO_ItemIteration>();
            mGrandChild_CO_ItemIterations = new List<CO_ItemIteration>();

            mParent_CO_ItemIterations = new List<CO_ItemIteration>();
            mGrandParent_CO_ItemIterations = new List<CO_ItemIteration>();

            mItemItemRelationDictByChildId = new Dictionary<int, ItemItemRelation>();

            mChild_CO_FileIterations = new List<CO_FileIteration>();
            mItemFileRelationDictByChildId = new Dictionary<int, ItemFileRelation>();

            mItemIterationStatus = ItemIterationStatus.NoStatus;

            mHierarchyBuilt = false;
            mHierarchyDepth = 0;
        }

        // method to build the CO_ItemIterations
        public static bool BuildItemIterations(Dictionary<int, Item> iterationsDict, ref Dictionary<int, CO_ItemIteration> coItemIterationDict)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("BuildItemIterations(): Start");

            bool bSuccess = false;

            // iterate each of these and build the Item Iterations
            List<CO_ItemIteration> coiList = new List<CO_ItemIteration>();
            foreach (Item itemIteration in iterationsDict.Values)
            {
                // create the new CO_ItemIteration
                CO_ItemIteration coi = new CO_ItemIteration(itemIteration);

                coiList.Add(coi);
            }

            // create the dictionary
            coItemIterationDict = coiList.ToDictionary(coi => coi.mItemId, coi => coi);

            // log total time taken
            Logger.Debug(string.Format("BuildItemIterations(): End {0}", swp.ElapsedTimeString()));

            bSuccess = true;

            return (bSuccess);
        }


        // method to set ItemItemRelation relationships
        public static void AssignItemToItemIterationRelationships(Dictionary<int, CO_ItemIteration> co_ItemIterationDict, List<ItemItemRelation> itemItemRelationList)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("AssignItemToItemIterationRelationships(): Start");

            // build lookup for parents grouping of ItemItemRelation
            ILookup<int, ItemItemRelation> associationParentLookup = itemItemRelationList.ToLookup(ass => ass.ParentItemID, ass => ass);

            // iterate through each, and assign relationships
            int nAssigned = 0;
            int nParents = 0;
            foreach (IGrouping<int, ItemItemRelation> groupItem in associationParentLookup)
            {
                // groupItem.Key = ParentItemID<int>
                // groupItem is IEnumerable<ItemItemRelation>

                nParents++;

                // get the parent CO_ItemIteration
                CO_ItemIteration parentCO_ItemIteration = co_ItemIterationDict[groupItem.Key];

                // iterate over the associations and get children
                foreach (ItemItemRelation iir in groupItem)
                {
                    nAssigned++;

                    // get the child CO_ItemIteration
                    CO_ItemIteration childCO_ItemIteration = co_ItemIterationDict[iir.ChildItemID];

                    // add the child to the list, and add the parent to the child
                    parentCO_ItemIteration.mChild_CO_ItemIterations.Add(childCO_ItemIteration);
                    childCO_ItemIteration.mParent_CO_ItemIterations.Add(parentCO_ItemIteration);

                    // add the Association record
                    parentCO_ItemIteration.mItemItemRelationDictByChildId.Add(iir.ChildItemID, iir);
                }
            }

            // log information
            Logger.Debug(string.Format("Parents/Children: {0:n0}/{1:n0}", nParents, nAssigned));

            // output a warning if no parents or children were assigned
            if ((nParents == 0) || (nAssigned == 0))
            {
                Logger.Debug("!!!Warning: No item parents or children were assigned");
            }

            // log total time taken
            Logger.Debug(string.Format("AssignItemToItemIterationRelationships(): End {0}", swp.ElapsedTimeString()));

        }

        // method to roll up ItemItemRelation relationships
        public static void RollupItemToItemIterationRelationships(Dictionary<int, CO_ItemIteration> co_ItemIterationDict)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("RollupItemToItemIterationRelationships(): Start");

            // iterate each of the types and rollup their information
            foreach (CO_ItemIteration co_ItemIteration in co_ItemIterationDict.Values)
            {
                co_ItemIteration.RollupIterationRelationships();
            }

            // set the status for the ItemIterations
            // identify if this is top level or bottom level files
            co_ItemIterationDict.Values.Where(ii => ii.mChild_CO_ItemIterations.Count() == 0).ToList().ForEach(ii => ii.mItemIterationStatus |= ItemIterationStatus.IsLowLevel);
            co_ItemIterationDict.Values.Where(ii => ii.mParent_CO_ItemIterations.Count() == 0).ToList().ForEach(ii => ii.mItemIterationStatus |= ItemIterationStatus.IsTopLevel);

            // get the list that has direct circular dependencies
            List<CO_ItemIteration> directCircDepList = co_ItemIterationDict.Values.Where(ii => (ii.mItemIterationStatus & CO_ItemIteration.ItemIterationStatus.DirectCircDepend) != CO_ItemIteration.ItemIterationStatus.NoStatus).ToList();
            List<CO_ItemIteration> inDirectCircDepList = co_ItemIterationDict.Values.Where(ii => (ii.mItemIterationStatus & CO_ItemIteration.ItemIterationStatus.CicularDepend) != CO_ItemIteration.ItemIterationStatus.NoStatus).ToList();

            // get the list of total effected
            List<CO_ItemIteration> totalEffected = new List<CO_ItemIteration>();
            totalEffected.AddRange(directCircDepList);
            totalEffected.AddRange(inDirectCircDepList);

            totalEffected.AddRange(inDirectCircDepList.SelectMany(ii => ii.mChild_CO_ItemIterations));
            totalEffected.AddRange(inDirectCircDepList.SelectMany(ii => ii.mGrandChild_CO_ItemIterations));

            totalEffected.AddRange(inDirectCircDepList.SelectMany(ii => ii.mChild_CO_ItemIterations));
            totalEffected.AddRange(inDirectCircDepList.SelectMany(ii => ii.mGrandChild_CO_ItemIterations));

            totalEffected = totalEffected.Distinct().ToList();

            // check to see if this contains ones we have seen before
            var results = totalEffected.Where(ii => ii.mItemNumber == "451-721");
            results = totalEffected.Where(ii => ii.mItemNumber == "5920N-U25RS-IC");


            // log total time taken
            Logger.Debug(string.Format("Total Items/Direct Circular/Indirect Circular: {0:n0}/{1:n0}/{2:n0}", co_ItemIterationDict.Values.Count(), directCircDepList.Count(), inDirectCircDepList.Count()));
            Logger.Debug(string.Format("Total Items in Circular Dependency Trees: {0:n0}", totalEffected.Count()));

            // output the items that are direct, indirect, and total
            string sOutputString = string.Format("Direct Circular References ({0}): {1}", directCircDepList.Count(), string.Join(",\t", directCircDepList.Select(coii => coii.mItemNumber)));
            Logger.Debug(sOutputString);

            sOutputString = string.Format("Indirect Circular References ({0}): {1}", inDirectCircDepList.Count(), string.Join(",\t", inDirectCircDepList.Select(coii => coii.mItemNumber)));
            Logger.Debug(sOutputString);

            sOutputString = string.Format("Total Circular References ({0}): {1}", totalEffected.Count(), string.Join(",\t", totalEffected.Select(coii => coii.mItemNumber)));
            Logger.Debug(sOutputString);

            // get those Items that don't have BOM
            List<CO_ItemIteration> lowLevelItems = co_ItemIterationDict.Values.Where(ii => (ii.mItemIterationStatus & CO_ItemIteration.ItemIterationStatus.IsLowLevel) != CO_ItemIteration.ItemIterationStatus.NoStatus).ToList();

            System.IO.File.WriteAllLines(@"C:\Temp\LowLevelItems.txt", lowLevelItems.Select(coii => coii.mItemNumber).Distinct());

            // get the direct parents of those low level items
            List <CO_ItemIteration> lowLevelItemsParents = lowLevelItems.SelectMany(coii => coii.mParent_CO_ItemIterations).Distinct().ToList();

            // write out a distribution of Items and number of components in their BOM
            System.IO.File.WriteAllLines(@"C:\Temp\LowLevelItemsParents.txt", lowLevelItemsParents.Select(coii => string.Format("{0}\t{1}", coii.mItemNumber, coii.mChild_CO_ItemIterations.Count())));



            // log total time taken
            Logger.Debug(string.Format("RollupItemToItemIterationRelationships(): End {0}", swp.ElapsedTimeString()));
        }

        // method to iterate stack of relationships and roll up iteration hierarchy
        protected void RollupIterationRelationships()
        {
            // check to see if it has been processed yet
            if (!mHierarchyBuilt)
            {
                // set indicating it has been built
                mHierarchyBuilt = true;

                // process the children
                foreach (CO_ItemIteration childCO_ItemIteration in this.mChild_CO_ItemIterations)
                {
                    childCO_ItemIteration.RollupIterationRelationships();

                    // set the hierarchy depth
                    mHierarchyDepth = Math.Max(mHierarchyDepth, childCO_ItemIteration.mHierarchyDepth + 1);


                    //mChildrenMasterFileStatus |= bfm.mMasterFileStatus;

                    //mGrandChildrenMasterFileStatus |= bfm.mChildrenMasterFileStatus;
                    //mGrandChildrenMasterFileStatus |= bfm.mGrandChildrenMasterFileStatus;
                }

                // check to see if their is a direct circular dependency
                if (mChild_CO_ItemIterations.SelectMany(ii => ii.mChild_CO_ItemIterations).Contains(this))
                {
                    this.mItemIterationStatus |= ItemIterationStatus.DirectCircDepend;
                }

                // get the grandchildren list
                // NOTE: we have not filtered out the list of potential circular dependencies
                mGrandChild_CO_ItemIterations.AddRange(mChild_CO_ItemIterations.SelectMany(ii => ii.mChild_CO_ItemIterations));

                mGrandChild_CO_ItemIterations.AddRange(mChild_CO_ItemIterations.SelectMany(ii => ii.mGrandChild_CO_ItemIterations));
                mGrandChild_CO_ItemIterations = mGrandChild_CO_ItemIterations.Distinct().ToList();

                // check to see if there is a circular dependency that is NOT direct
                // we are not filtering on those as of yet
                if (mGrandChild_CO_ItemIterations.Contains(this))
                {
                    this.mItemIterationStatus |= ItemIterationStatus.CicularDepend;
                }
            }
        }





        // method to set ItemFileRelation relationships
        public static void AssignItemToFileIterationRelationships(  Dictionary<int, CO_ItemIteration> co_ItemIterationDict,
                                                                    Dictionary<int, CO_FileIteration> co_FileIterationDict,
                                                                    List<ItemFileRelation> itemItemRelationList)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("AssignItemToFileIterationRelationships(): Start");

            // build lookup for parents grouping of ItemFileRelation
            ILookup<int, ItemFileRelation> associationParentItemLookup = itemItemRelationList.ToLookup(ass => ass.ItemID, ass => ass);

            // iterate through each, and assign relationships
            int nAssigned = 0;
            int nParents = 0;
            foreach (IGrouping<int, ItemFileRelation> groupItem in associationParentItemLookup)
            {
                // groupItem.Key = ItemID<int>
                // groupItem is IEnumerable<ItemFileRelation>

                nParents++;

                // get the parent CO_ItemIteration
                CO_ItemIteration parentCO_ItemIteration = co_ItemIterationDict[groupItem.Key];

                // iterate over the associations and get children
                foreach (ItemFileRelation ifr in groupItem)
                {
                    nAssigned++;

                    // get the child CO_FileIteration
                    CO_FileIteration childCO_FileIteration = co_FileIterationDict[ifr.FileID];

                    // add the child to the list, and add the parent to the child
                    parentCO_ItemIteration.mChild_CO_FileIterations.Add(childCO_FileIteration);
                    childCO_FileIteration.mParent_CO_ItemIterations.Add(parentCO_ItemIteration);

                    // add the Association record
                    parentCO_ItemIteration.mItemFileRelationDictByChildId.Add(ifr.FileID, ifr);
                }
            }

            // log information
            Logger.Debug(string.Format("Parents/Children: {0:n0}/{1:n0}", nParents, nAssigned));

            // output a warning if no parents or children were assigned
            if ((nParents == 0) || (nAssigned == 0))
            {
                Logger.Debug("!!!Warning: No item parents or children were assigned");
            }

            // log total time taken
            Logger.Debug(string.Format("AssignItemToFileIterationRelationships(): End {0}", swp.ElapsedTimeString()));

        }



        // set the CO_ItemRevision as owner of this object
        public void SetCO_ItemRevisionOwner(CO_ItemRevision co_ItemRevision)
        {
            mCO_ItemRevisionOwner = co_ItemRevision;
        }

        // set the CO_ItemMaster as owner of this object
        public void SetCO_ItemMasterOwner(CO_ItemMaster co_ItemMaster)
        {
            mCO_ItemMasterOwner = co_ItemMaster;
        }



    }
}
