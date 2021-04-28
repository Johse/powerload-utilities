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
    public class VaultItemMaster
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(VaultItemMaster));

        // manage the ItemMaster table record for this VaultItemMaster
        public ItemMaster m_ItemMasterRecord { get; private set; }
        public Master m_MasterRecord { get; private set; }
        public CategoryDef m_CategoryDef { get; private set; }

        // manage the VaultItemRevisions for this master
        public List<VaultItemRevision> m_VaultItemRevisionList { get; private set; }

        // constructor
        public VaultItemMaster(ItemMaster itemMasterRecord, Master masterRecord, CategoryDef categoryDef)
        {
            // assign the properties
            m_ItemMasterRecord = itemMasterRecord;
            m_MasterRecord = masterRecord;

            m_CategoryDef = categoryDef;

            m_VaultItemRevisionList = new List<VaultItemRevision>();
        }

        // method to build the VaultItemMasters
        public static bool BuildVaultItemMasters(Dictionary<long, ItemMaster> itemMaster_Dict,
                                                    Dictionary<long, CategoryDef> categoryDefs_Dict,
                                                    Dictionary<long, CategoryOnEntity> categoriesOnEntitiesForItemMasters_Dict,
                                                    Dictionary<long, Master> mastersForItems,
                                                    ref Dictionary<long, VaultItemMaster> VaultItemMasterDict)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("BuildVaultItemMasters: Start");

            bool bSuccess = false;

            // iterate each of these and build the masters
            List<VaultItemMaster> vimList = new List<VaultItemMaster>();
            foreach (ItemMaster im in itemMaster_Dict.Values)
            {
                // get the CategoryDef for the file master
                // may not be defined for the file
                CategoryDef categoryDef = null;
                if (categoriesOnEntitiesForItemMasters_Dict.ContainsKey(im.ItemMasterID))
                {
                    categoryDef = categoryDefs_Dict[categoriesOnEntitiesForItemMasters_Dict[im.ItemMasterID].CategoryDefId];
                }

                Master masterRecord = mastersForItems[im.ItemMasterID];

                // create the new VaultItemMaster
                VaultItemMaster vim = new VaultItemMaster(im, masterRecord, categoryDef);
                vimList.Add(vim);
            }

            // create the dictionary
            VaultItemMasterDict = vimList.ToDictionary(vim => vim.m_ItemMasterRecord.ItemMasterID, vim => vim);

            // log total time taken
            Logger.Debug(string.Format("BuildVaultItemMasters(): End {0:n0} {1}", VaultItemMasterDict.Count(), swp.ElapsedTimeString()));

            bSuccess = true;

            return (bSuccess);
        }

        // method to add a VaultItemRevision to the VaultItemMaster
        public void AddVaultItemRevision(VaultItemRevision vaultItemRevision)
        {
            m_VaultItemRevisionList.Add(vaultItemRevision);
        }

    }
}
