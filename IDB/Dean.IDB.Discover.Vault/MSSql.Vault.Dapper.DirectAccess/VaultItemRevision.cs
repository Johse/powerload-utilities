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
    public class VaultItemRevision
    {
        // setup the log Item information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(VaultItemRevision));

        // manage the Revision table record for this VaultItemRevision
        public Revision m_RevisionRecord { get; private set; }

        // manage the owner of this VaultItemRevision
        public VaultItemMaster m_VaultItemMaster { get; private set; }

        // manage the VaultItemIteration
        public List<VaultItemIteration> m_VaultItemIterationList { get; private set; }


        // constructor
        public VaultItemRevision(Revision revisionRecord, VaultItemMaster parentVaultItemMaster)
        {
            // assign the properties
            m_RevisionRecord = revisionRecord;
            m_VaultItemMaster = parentVaultItemMaster;

            m_VaultItemIterationList = new List<VaultItemIteration>();
        }


        // method to build the VaultItemRevisions
        public static bool BuildVaulItemRevisions(  Dictionary<long, Revision> revisionsForItems_Dict,
                                                    Dictionary<long, VaultItemMaster> vaultItemMasterDict,
                                                    ref Dictionary<long, VaultItemRevision> vaultItemRevisionDict)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("BuildVaulItemRevisions: Start");

            bool bSuccess = false;

            // iterate each of these and build the revisions
            List<VaultItemRevision> virList = new List<VaultItemRevision>();
            foreach (Revision rev in revisionsForItems_Dict.Values)
            {
                VaultItemMaster parentVaultItemMaster = vaultItemMasterDict[rev.MasterId];

                // create the new VaultItemRevision
                VaultItemRevision vir = new VaultItemRevision(rev, parentVaultItemMaster);
                virList.Add(vir);

                // add the VaultItemRevision to the parentVaultItemMaster
                parentVaultItemMaster.AddVaultItemRevision(vir);
            }

            // create the dictionary
            vaultItemRevisionDict = virList.ToDictionary(vir => vir.m_RevisionRecord.RevisionId, vir => vir);

            // log total time taken
            Logger.Debug(string.Format("BuildVaulItemRevisions(): End {0:n0} {1}", vaultItemRevisionDict.Count(), swp.ElapsedTimeString()));

            bSuccess = true;

            return (bSuccess);
        }

        // method to add a VaultItemIteration to the VaultItemRevision
        public void AddVaultItemIteration(VaultItemIteration vaultItemIteration)
        {
            m_VaultItemIterationList.Add(vaultItemIteration);
        }


    }
}
