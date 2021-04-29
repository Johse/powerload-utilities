using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Windows;
using System.Windows.Forms;
using System.Diagnostics;

using MSSql.Vault.Dapper.DirectAccess.VaultDbEntities;

using log4net;
using BCPBuilderConfig;

namespace MSSql.Vault.Dapper.DirectAccess
{
    public class VaultFileIteration
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(VaultFileIteration));

        // manage the FileIteration table records for this VaultFileIteration
        public FileIterationExtended m_FileIterationExtendedRecord { get; private set; }
        public Iteration m_IterationRecord { get; private set; }
        public FileResource m_FileResourceRecord { get; private set; }
        public Entity m_EntityRecord { get; private set; }
        public List<Property> m_PropertyList { get; private set; }

        // manage the VaultFileRevision that owns this file iteration
        public VaultFileRevision m_VaultFileRevision { get; private set; }

        // manage the child iteration relationships
        public List<VaultFileIteration> m_ChildVaultFileIterationList { get; private set; }
        public List<VaultFileIteration> m_ParentVaultFileIterationList { get; private set; }

        // manage a dictionary of the MeridianReferenceData based on the child EntityId
        public Dictionary<long, FileAssociationExtended> m_ChildAssociationDictionary { get; private set; }

        // manage whether these can be purged based on multiple criteria
        protected bool _bCanBePurgedByLifecycleStateName;
        protected bool _bCanBePurgedByControlled;
        protected bool _bCanBePurgedByRevisionPosition;
        protected bool _bCanBePurgedByParentState;

        // static values may override the discovery
        // so that we can test mechanism when things are looser
        public static bool STPurgedByLifecycleStateName = true;
        public static bool STPurgedByControlled = true;
        public static bool STPurgedByRevisionPosition = true;
        public static bool STPurgedByParentState = true;

        // manage whether the VaultFileIteration has been visited to assess
        // if the _bCanBePurgedByParentState - requires walking UP the tree to all parents first
        public bool bHaveVisitedParents { get; private set; }

        // constructor
        public VaultFileIteration(  FileIterationExtended fileIterationExtendedRecord,
                                    Iteration iterationRecord,
                                    FileResource fileResourceRecord,
                                    Entity entityRecord,
                                    VaultFileRevision parentVaultFileRevision,
                                    List<Property> fileIterationProperties)
        {
            m_FileIterationExtendedRecord = fileIterationExtendedRecord;
            m_IterationRecord = iterationRecord;
            m_FileResourceRecord = fileResourceRecord;
            m_EntityRecord = entityRecord;
            m_PropertyList = fileIterationProperties;

            m_VaultFileRevision = parentVaultFileRevision;

            // setup the association lists and dictionary
            m_ChildVaultFileIterationList = new List<VaultFileIteration>();
            m_ParentVaultFileIterationList = new List<VaultFileIteration>();
            m_ChildAssociationDictionary = new Dictionary<long, FileAssociationExtended>();

            // manage whether these can be purged based on multiple criteria
            _bCanBePurgedByLifecycleStateName = false;
            _bCanBePurgedByControlled = false;
            _bCanBePurgedByRevisionPosition = false;
            _bCanBePurgedByParentState = false;

            bHaveVisitedParents = false;
        }


        // method to build the VaultFileIterations
        public static bool BuildVaulFileIterations(Dictionary<long, FileIterationExtended> fileIterationsExtended_Dict,
                                                    Dictionary<long, Iteration> iterationsForFiles_Dict,
                                                    Dictionary<long, FileResource> fileResources_Dict,
                                                    Dictionary<long, Entity> entitiesForFiles_Dict,
                                                    Dictionary<long, VaultFileRevision> vaultFileRevisionDict,
                                                    ILookup<long, Property> propertyLookupByEntityID,
                                                    ref Dictionary<long, VaultFileIteration> vaultFileIterationDict)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("BuildVaulFileIterations: Start");

            bool bSuccess = false;

            // iterate each of these and build the iterations
            List<VaultFileIteration> vfiList = new List<VaultFileIteration>();
            foreach (FileIterationExtended fie in fileIterationsExtended_Dict.Values)
            {
                // get the requisite database records
                Iteration iterationRecord = iterationsForFiles_Dict[fie.FileIterationId];
                FileResource fileResourceRecord = fileResources_Dict[fie.ResourceId];
                Entity entityRecord = entitiesForFiles_Dict[iterationRecord.IterationID];

                VaultFileRevision parentVaultFileRevision = vaultFileRevisionDict[iterationRecord.RevisionId.Value];

                // get the FileIteration properties
                List<Property> fileIterationProperties = new List<Property>();
                if (propertyLookupByEntityID.Contains(fie.FileIterationId))
                {
                    fileIterationProperties = propertyLookupByEntityID[fie.FileIterationId].ToList();
                }

                // create the new VaultFileIteration
                VaultFileIteration vfi = new VaultFileIteration(fie, iterationRecord, fileResourceRecord, entityRecord, parentVaultFileRevision, fileIterationProperties);
                vfiList.Add(vfi);

                // add the VaultFileIteration to the parentVaultFileRevision
                parentVaultFileRevision.AddVaultFileIteration(vfi);
            }

            // create the dictionary
            vaultFileIterationDict = vfiList.ToDictionary(vfi => vfi.m_FileIterationExtendedRecord.FileIterationId, vfi => vfi);

            // log total time taken
            Logger.Debug(string.Format("BuildVaulFileIterations(): End {0:n0} {1}", vaultFileIterationDict.Count(), swp.ElapsedTimeString()));

            bSuccess = true;

            return (bSuccess);
        }

        // method to set FileIteration relationships
        public static void AssignFileToFileIterationRelationships(Dictionary<long, VaultFileIteration> vaultFileIterationDict, IEnumerable<FileAssociationExtended> fileAssociationExtended)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("AssignFileToFileIterationRelationships(): Start");

            // build lookup for parents
            ILookup<long, FileAssociationExtended> associationParentLookup = fileAssociationExtended.ToLookup(ass => ass.FromId, ass => ass);

            // iterate through each, and assign relationships
            foreach (IGrouping<long, FileAssociationExtended> groupItem in associationParentLookup)
            {
                // get the parent VaultFileIteration
                // WARNING!!! we may have more relationships than we want to analyze
                // because of how we are discoverying the parents and children from the initial master list
                if (vaultFileIterationDict.ContainsKey(groupItem.Key))
                {
                    VaultFileIteration parentVaultFileIteration = vaultFileIterationDict[groupItem.Key];

                    // iterate over the associations and get children
                    foreach (FileAssociationExtended ass in groupItem)
                    {
                        // get the child VaultFileIteration
                        // WARNING!!! we may have more relationships than we want to analyze
                        // because of how we are discoverying the parents and children from the initial master list
                        if (vaultFileIterationDict.ContainsKey(ass.ToId))
                        {
                            VaultFileIteration childVaultFileIteration = vaultFileIterationDict[ass.ToId];

                            // add the child to the list, and add the parent to the child
                            parentVaultFileIteration.m_ChildVaultFileIterationList.Add(childVaultFileIteration);
                            childVaultFileIteration.m_ParentVaultFileIterationList.Add(parentVaultFileIteration);

                            // add the Association record
                            parentVaultFileIteration.m_ChildAssociationDictionary.Add(ass.ToId, ass);
                        }
                    }
                }
            }

            // log total time taken
            Logger.Debug(string.Format("AssignFileToFileIterationRelationships(): End {0}", swp.ElapsedTimeString()));
        }


        // set the VaultFileIteration
        // _bCanBePurgedByLifecycleStateName, _bCanBePurgedByControlled, _bCanBePurgedByRevisionPosition, _bCanBePurgedByParentState
        public void SetCanBePurgedOnOwnMeritStatuses()
        {
            _bCanBePurgedByLifecycleStateName = LifeCycleStateNameIsViableForPurge();

            // _bCanBePurgedByControlled = !this.m_IterationRecord.Controlled;

            // the remainder have to be set by logic
            //_bCanBePurgedByRevisionPosition = false;
            //_bCanBePurgedByParentState = false;
        }

        // set the VaultFileIteration
        // _bCanBePurgedByLifecycleStateName, _bCanBePurgedByControlled, _bCanBePurgedByRevisionPosition, _bCanBePurgedByParentState
        public void SetCanBePurgedByRevisionPosition()
        {
            _bCanBePurgedByRevisionPosition = true;
        }


        // method to identify that the LifeCycleStateName is viable for purge
        public bool LifeCycleStateNameIsViableForPurge()
        {
            bool bViableForPurge = (this.m_FileIterationExtendedRecord.LifeCycleStateName == null) ||
                                    (string.Compare(this.m_FileIterationExtendedRecord.LifeCycleStateName, "Work in Progress", true) == 0) ||
                                    (string.Compare(this.m_FileIterationExtendedRecord.LifeCycleStateName, "For Review", true) == 0) ||
                                    (string.Compare(this.m_FileIterationExtendedRecord.LifeCycleStateName, "Obsolete", true) == 0);

            return (bViableForPurge);
        }

        // return if it can be purged on its own merits
        // combine _bCanBePurgedByLifecycleStateName, _bCanBePurgedByControlled, _bCanBePurgedByRevisionPosition
        public bool CanBePurgedOnOwnMerits()
        {
            return (CanBePurgedByLifecycleStateName() && CanBePurgedByControlled() && CanBePurgedByRevisionPosition());
        }

        // return if it can be purged on its own merits
        // combine _bCanBePurgedByLifecycleStateName, _bCanBePurgedByControlled, _bCanBePurgedByRevisionPosition
        public bool CanBePurgedOnAllMerits()
        {
            return (CanBePurgedByLifecycleStateName() && CanBePurgedByControlled() && CanBePurgedByRevisionPosition() && CanBePurgedByParentState());
        }


        // visit all of the parents and identify if this can be purged
        public bool VisitAndSetCanBePurgedByParentState()
        {
            if (!this.bHaveVisitedParents)
            {
                // visit each of the parents
                // TODO: - there are circular dependencies
                // set this.bHaveVisitedParents now because we will get stuck in an endless loop
                this.bHaveVisitedParents = true;
                this._bCanBePurgedByParentState = true;
                foreach (VaultFileIteration parentVFI in this.m_ParentVaultFileIterationList)
                {
                    // let the parent process
                    if (!parentVFI.VisitAndSetCanBePurgedByParentState())
                    {
                        this._bCanBePurgedByParentState = false;
                    }
                }
            }

            // return the CanBePurgedOnAllMerits() for this object
            // so that children can be set properly
            return (this.CanBePurgedOnAllMerits());
        }

        // manage whether these can be purged based on multiple criteria
        //public bool _bCanBePurgedByLifecycleStateName;
        //public bool _bCanBePurgedByControlled;
        //public bool _bCanBePurgedByRevisionPosition;
        //public bool _bCanBePurgedByParentState;

        //// static values may override the discovery
        //// so that we can test mechanism when things are looser
        //public static bool STPurgedByLifecycleStateName = true;
        //public static bool STPurgedByControlled = true;
        //public static bool STPurgedByRevisionPosition = true;
        //public static bool STBePurgedByParentState = true;

        // return value or override
        public bool CanBePurgedByLifecycleStateName()
        {
            if (!STPurgedByLifecycleStateName)
            {
                // return true no matter what
                return (true);
            }

            return (_bCanBePurgedByLifecycleStateName);
        }

        public bool CanBePurgedByControlled()
        {
            if (!STPurgedByControlled)
            {
                // return true no matter what
                return (true);
            }

            return (_bCanBePurgedByControlled);
        }

        public bool CanBePurgedByRevisionPosition()
        {
            if (!STPurgedByRevisionPosition)
            {
                // return true no matter what
                return (true);
            }

            return (_bCanBePurgedByRevisionPosition);
        }

        public bool CanBePurgedByParentState()
        {
            if (!STPurgedByParentState)
            {
                // return true no matter what
                return (true);
            }

            return (_bCanBePurgedByParentState);
        }


    }
}

