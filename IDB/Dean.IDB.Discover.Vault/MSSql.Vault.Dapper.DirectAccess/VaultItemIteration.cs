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
    public class VaultItemIteration
    {
        // setup the log Item information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(VaultItemIteration));

        // manage the ItemIteration table records for this VaultItemIteration
        public Iteration m_IterationRecord { get; private set; }
        public Entity m_EntityRecord { get; private set; }

        // manage the VaultItemRevision that owns this Item iteration
        public VaultItemRevision m_VaultItemRevision { get; private set; }


        // constructor
        public VaultItemIteration(  Iteration iterationRecord,
                                    Entity entityRecord,
                                    VaultItemRevision parentVaultItemRevision)
        {
            m_IterationRecord = iterationRecord;
            m_EntityRecord = entityRecord;

            m_VaultItemRevision = parentVaultItemRevision;
        }


        // method to build the VaultItemIterations
        public static bool BuildVaulItemIterations( Dictionary<long, Iteration> iterationsForItems_Dict,
                                                    Dictionary<long, Entity> entitiesForItems_Dict,
                                                    Dictionary<long, VaultItemRevision> vaultItemRevisionDict,
                                                    ref Dictionary<long, VaultItemIteration> vaultItemIterationDict)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("BuildVaulItemIterations: Start");

            bool bSuccess = false;

            // iterate each of these and build the iterations
            List<VaultItemIteration> virList = new List<VaultItemIteration>();
            foreach (Iteration iterationRecord in iterationsForItems_Dict.Values)
            {
                // get the requisite database records
                Entity entityRecord = entitiesForItems_Dict[iterationRecord.IterationID];

                VaultItemRevision parentVaultItemRevision = vaultItemRevisionDict[iterationRecord.RevisionId.Value];

                // create the new VaultItemIteration
                VaultItemIteration vir = new VaultItemIteration(iterationRecord, entityRecord, parentVaultItemRevision);
                virList.Add(vir);

                // add the VaultItemIteration to the parentVaultItemRevision
                parentVaultItemRevision.AddVaultItemIteration(vir);
            }

            // create the dictionary
            vaultItemIterationDict = virList.ToDictionary(vir => vir.m_IterationRecord.IterationID, vir => vir);

            // log total time taken
            Logger.Debug(string.Format("BuildVaulItemIterations(): End {0:n0} {1}", vaultItemIterationDict.Count(), swp.ElapsedTimeString()));

            bSuccess = true;

            return (bSuccess);
        }

    }
}
