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
    public class VaultFileRevision
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(VaultFileRevision));

        // manage the Revision table record for this VaultFileRevision
        public Revision m_RevisionRecord { get; private set; }

        // manage the owner of this VaultFileRevision
        public VaultFileMaster m_VaultFileMaster { get; private set; }

        // manage the VaultFileIteration
        public List<VaultFileIteration> m_VaultFileIterationList { get; private set; }


        // constructor
        public VaultFileRevision(Revision revisionRecord, VaultFileMaster parentVaultFileMaster)
        {
            // assign the properties
            m_RevisionRecord = revisionRecord;
            m_VaultFileMaster = parentVaultFileMaster;

            m_VaultFileIterationList = new List<VaultFileIteration>();
        }


        // method to build the VaultFileRevisions
        public static bool BuildVaulFileRevisions(  Dictionary<long, Revision> revisionsForFiles_Dict,
                                                    Dictionary<long, VaultFileMaster> vaultFileMasterDict,
                                                    ref Dictionary<long, VaultFileRevision> vaultFileRevisionDict)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("BuildVaulFileRevisions: Start");

            bool bSuccess = false;

            // iterate each of these and build the revisions
            List<VaultFileRevision> vfrList = new List<VaultFileRevision>();
            foreach (Revision rev in revisionsForFiles_Dict.Values)
            {
                VaultFileMaster parentVaultFileMaster = vaultFileMasterDict[rev.MasterId];

                // create the new VaultFileRevision
                VaultFileRevision vfr = new VaultFileRevision(rev, parentVaultFileMaster);
                vfrList.Add(vfr);

                // add the VaultFileRevision to the parentVaultFileMaster
                parentVaultFileMaster.AddVaultFileRevision(vfr);
            }

            // create the dictionary
            vaultFileRevisionDict = vfrList.ToDictionary(vfr => vfr.m_RevisionRecord.RevisionId, vfr => vfr);

            // log total time taken
            Logger.Debug(string.Format("BuildVaulFileRevisions(): End {0:n0} {1}", vaultFileRevisionDict.Count(), swp.ElapsedTimeString()));

            bSuccess = true;

            return (bSuccess);
        }

        // method to add a VaultFileIteration to the VaultFileRevision
        public void AddVaultFileIteration(VaultFileIteration vaultFileIteration)
        {
            m_VaultFileIterationList.Add(vaultFileIteration);
        }

        // set the VaultFileIteration
        // _bCanBePurgedByRevisionPosition
        public void SetCanBePurgedByRevisionPosition()
        {
            // set the _bCanBePurgedByRevisionPosition for all but last version
            foreach (VaultFileIteration vfi in this.m_VaultFileIterationList.Take(this.m_VaultFileIterationList.Count() -1))
            {
                vfi.SetCanBePurgedByRevisionPosition();
            }
        }


    }

}
