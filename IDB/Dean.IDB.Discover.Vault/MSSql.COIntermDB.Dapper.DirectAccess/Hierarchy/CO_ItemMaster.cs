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
    // class to manage groupings of CO_ItemRevision objects
    public class CO_ItemMaster
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(CO_ItemMaster));

        // manage the Item table record for this file
        // there is NO file master record - this was only for the VaultBCP to SQLite conversion
        // public BSE.Item mItemRecord { get; private set; }

        public string mItemNumber { get; private set; }

        // list of BCPItemRevisions
        public List<CO_ItemRevision> mCO_ItemRevisionList { get; private set; }

        // list of BCPItemIterations
        public List<CO_ItemIteration> mCO_ItemIterationList { get; private set; }
        public CO_ItemIteration mLatestCO_ItemIteration { get; private set; }

        // manage the VaultBCP content for building the data loader package
        public ItemMaster mBcpItemMaster { get; set; }


        // constructor
        public CO_ItemMaster(string sItemNumber)
        {
            mItemNumber = sItemNumber;

            // setup the lists to manage objects
            mCO_ItemRevisionList = new List<CO_ItemRevision>();
            mCO_ItemIterationList = new List<CO_ItemIteration>();
        }


        // build the CO_ItemMaster and CO_ItemRevision objects
        public static bool BuildItemMastersAndRevisions(Dictionary<int, CO_ItemIteration> co_ItemIterationDict,
                                                        ref List<CO_ItemRevision> co_ItemRevisionList,
                                                        ref List<CO_ItemMaster> co_ItemMasterList)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("BuildItemMastersAndRevisions(): Start");

            bool bSuccess = false;

            // group the iterations by their ItemNumber
            ILookup<string, CO_ItemIteration> itemIterationLookup = co_ItemIterationDict.Values.ToLookup(cii => cii.mItemNumber, cii => cii, StringComparer.CurrentCultureIgnoreCase);
            int nGroups = itemIterationLookup.Count();
            Logger.Debug(string.Format("Number Item Iterations/Grouped As Masters: {0:n0}", co_ItemIterationDict.Count(), nGroups));

            // iterate through and build the Item masters
            co_ItemRevisionList = new List<CO_ItemRevision>();
            co_ItemMasterList = new List<CO_ItemMaster>();
            foreach (IGrouping<string, CO_ItemIteration> masterGroupItem in itemIterationLookup)
            {
                // masterGroupItem.Key = ItemNumber<string>
                // masterGroupItem is IEnumerable<CO_ItemIteration>

                // build the CO_ItemMaster
                CO_ItemMaster co_ItemMaster = new CO_ItemMaster(masterGroupItem.Key);
                co_ItemMasterList.Add(co_ItemMaster);

                // group the CO_ItemIteration by their revision labels (A, B, C) etc
                ILookup<string, CO_ItemIteration> itemMasterLookup = masterGroupItem.ToLookup(cfi => cfi.mItemIterationRecord.RevisionLabel, cfi => cfi, StringComparer.CurrentCultureIgnoreCase);

                // iterate the revisions and build the CO_ItemRevision for this master
                foreach (IGrouping<string, CO_ItemIteration> revisionGroupItem in itemMasterLookup)
                {
                    // revisionGroupItem.Key = RevisionLabel<string>
                    // revisionGroupItem is IEnumerable<CO_ItemIteration>

                    // build the CO_ItemRevision object
                    CO_ItemRevision co_ItemRevision = new CO_ItemRevision(revisionGroupItem.Key, revisionGroupItem);
                    co_ItemRevisionList.Add(co_ItemRevision);

                    // add the ItemRevision and ItemIterations to the master
                    co_ItemMaster.mCO_ItemRevisionList.Add(co_ItemRevision);
                    co_ItemMaster.mCO_ItemIterationList.AddRange(co_ItemRevision.mCO_ItemIterationList);

                    // set the master for these objects
                    co_ItemRevision.SetCO_ItemMasterOwner(co_ItemMaster);
                    co_ItemRevision.mCO_ItemIterationList.ForEach(cfi => cfi.SetCO_ItemMasterOwner(co_ItemMaster));
                }

            }

            // log total time taken
            Logger.Debug(string.Format("BuildItemMastersAndRevisions(): End {0}", swp.ElapsedTimeString()));

            bSuccess = true;

            return (bSuccess);
        }

        // method to set CO_FileMaster.mCO_ItemMaster
        public static void AssignItemMasterToFileMaster(List<CO_ItemMaster> co_ItemMasterList)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("AssignItemMasterToFileMaster(): Start");

            // build lookup of CO_ItemMaster to CO_FileMaster
            var itemMastersAndFileMasters = from im in co_ItemMasterList
                        from ii in im.mCO_ItemIterationList
                            from fi in ii.mChild_CO_FileIterations
                                select new { IM = im, FM = fi.mCO_FileMasterOwner };


            ILookup<CO_ItemMaster, CO_FileMaster> fileMasterByItemMasterLookup = itemMastersAndFileMasters.ToLookup(imfm => imfm.IM, imfm => imfm.FM);

            // iterate over the Lookup and assign the ItemMasters
            foreach (IGrouping<CO_ItemMaster, CO_FileMaster> groupItem in fileMasterByItemMasterLookup)
            {
                // groupItem.Key = CO_ItemMaster
                // groupItem is IEnumerable<CO_FileMaster>

                foreach (CO_FileMaster fm in groupItem.Distinct())
                {
                    fm.Set_CO_ItemMaster(groupItem.Key);
                }
            }

            // log total time taken
            Logger.Debug(string.Format("AssignItemMasterToFileMaster(): End {0}", swp.ElapsedTimeString()));

        }



    }
}
