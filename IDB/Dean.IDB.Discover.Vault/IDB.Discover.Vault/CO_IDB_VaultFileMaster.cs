using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;
using BCPBuilderConfig;
using MCDD = MSSql.COIntermDB.Dapper.DirectAccess;
using MVDD = MSSql.Vault.Dapper.DirectAccess;


namespace IDB.Discover.Vault
{
    // object that manages and processes the CO_FileMaster and VaultFileMaster objects that match up
    // between the CO Intermediate Database and those files that exist in the Vault
    public class CO_IDB_VaultFileMaster
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(CO_IDB_VaultFileMaster));

        // manage the filename, extension, and full vault file path
        public string mName { get; private set; }
        public string mExtension { get; private set; }
        public string mFullVaultFilePath { get; private set; }
        public bool mExistsInVault { get; private set; }
        public bool mChecksumsMatch { get; private set; }

        // manage the CO_FileMaster and VaultFileMaster objects
        // associated with the CO IDB
        // should be a CO_FileMaster object
        // but it is possible that the VaultFileMaster object does not exist
        // especially when we are doing a delta load and the new file does not yet
        // exist in the Vault
        public MCDD.Hierarchy.CO_FileMaster mCO_FileMaster { get; private set; }
        public MVDD.VaultFileMaster mVaultFileMaster { get; private set; }

        // manage a list of "other" possible matches for the CO_FileMaster that may be found in the Vault
        // based only on Filename - mostly used for reporting so that we can find possible
        // alternates
        public List<MVDD.VaultFileMaster> mAlternateVaultFileMasterList { get; private set; }

        // static members that manage lists/dictionaries on the content processed
        public static List<CO_IDB_VaultFileMaster> mCO_IDB_VaultFileMaster_List { get; private set; }

        // manage the MCDD.CO_IntermediateDbSqlCache and MVDD.VaultCache used to combine information
        public static MCDD.CO_IntermediateDbSqlCache mCO_IntermediateDbSqlCache { get; private set; }
        public static MVDD.VaultCache mVaultCache { get; private set; }


        // constructor - expects that at least the mCO_FileMaster is given
        // mVaultFileMaster can be null
        public CO_IDB_VaultFileMaster(MCDD.Hierarchy.CO_FileMaster co_FileMaster, MVDD.VaultFileMaster vaultFileMaster, List<MVDD.VaultFileMaster> alternateVaultFileMasterList)
        {
            // set a reference to the primary objects
            mCO_FileMaster = co_FileMaster;
            mVaultFileMaster = vaultFileMaster;

            // set the alternates that have the same name
            mAlternateVaultFileMasterList = new List<MVDD.VaultFileMaster>();
            mAlternateVaultFileMasterList.AddRange(alternateVaultFileMasterList);
            if (mVaultFileMaster != null)
            {
                mAlternateVaultFileMasterList.Remove(mVaultFileMaster);
            }

            // manage the information about the file
            mName = co_FileMaster.mName;
            mExtension = co_FileMaster.mExtension;
            mFullVaultFilePath = co_FileMaster.mFullVaultFilePath;

            mExistsInVault = false;
            mChecksumsMatch = false;
        }

    // static method to process the MCDD.CO_IntermediateDbSqlCache and MVDD.VaultCache information
    // and create a set of CO_IDB_VaultFileMaster objects
    public static void Combine_CO_IDB_And_VaultCache(MCDD.CO_IntermediateDbSqlCache co_IntermediateDbSqlCache, MVDD.VaultCache vaultCache)
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("Combine_CO_IDB_And_VaultCache: Start");

            // manage the MCDD.CO_IntermediateDbSqlCache and MVDD.VaultCache used to combine information
            mCO_IntermediateDbSqlCache = co_IntermediateDbSqlCache;
            mVaultCache = vaultCache;


            // find those VaultFileMaster files that are already in the Vault
            Dictionary<string, MVDD.VaultFileMaster> vaultFileMasterDictionaryByVaultPath = mVaultCache.m_VaultFileMasterDict.Values.ToDictionary(  vfm => vfm.GetVaultFileMasterFullPathName(),
                                                                                                                                                    vfm => vfm,
                                                                                                                                                    StringComparer.InvariantCultureIgnoreCase);

            // the following is done so that we can identify those files potentially not found in the exact same location, but report on files with the same name
            ILookup<string, MVDD.VaultFileMaster> vaultFileMasterLookupByFileName = mVaultCache.m_VaultFileMasterDict.Values.ToLookup(vfm => vfm.GetVaultFileMasterTipName(), vfm => vfm, StringComparer.InvariantCultureIgnoreCase);

            // iterate through and build the dictionary
            mCO_IDB_VaultFileMaster_List = new List<CO_IDB_VaultFileMaster>();
            foreach (MCDD.Hierarchy.CO_FileMaster co_FileMaster in mCO_IntermediateDbSqlCache.m_CO_FileMasterList)
            {
                // going to build the CO_IDB_VaultFileMaster whether we find a corresponding VaultFileMaster
                MVDD.VaultFileMaster vaultFileMaster = null;
                List<MVDD.VaultFileMaster> alternateVaultFileMaster = new List<MVDD.VaultFileMaster>();


                // if we find a match, set the vaultFileMaster found
                if (vaultFileMasterDictionaryByVaultPath.ContainsKey(co_FileMaster.mFullVaultFilePath))
                {
                    vaultFileMaster = vaultFileMasterDictionaryByVaultPath[co_FileMaster.mFullVaultFilePath];
                }

                // it could be that the file exists in the Vault but not in the same location
                if (vaultFileMasterLookupByFileName.Contains(co_FileMaster.mName))
                {
                    alternateVaultFileMaster.AddRange(vaultFileMasterLookupByFileName[co_FileMaster.mName]);
                }

                // build the CO_IDB_VaultFileMaster
                CO_IDB_VaultFileMaster coIdbVFM = new CO_IDB_VaultFileMaster(co_FileMaster, vaultFileMaster, alternateVaultFileMaster);

                // add the new object to the list
                mCO_IDB_VaultFileMaster_List.Add(coIdbVFM);
            }

            // log total time taken
            Logger.Debug(string.Format("Combine_CO_IDB_And_VaultCache(): End {0}", swp.ElapsedTimeString()));
        }


        // static method to process the mCO_IDB_VaultFileMaster_List
        // assumes that the mCO_IDB_VaultFileMaster_List has already been created with the Combine_CO_IDB_And_VaultCache method

        // !!!IMPORTANT!!!
        // assumptions of this implementation
        // The target Vault ONLY has one version/file iteration of each file master
        // the local file checksums have already been acquired
        // the source coolOrange Intermediate Database has only one version of new Delta files and/or Multi-pass files
        // referencing already loaded files in the Vault
        // those files that are only being referenced in the target Vault, may or may not have a single delta
        // file iteration in the coolOrange Intermediate Database
        // IDB.Analyze.AutoCAD and IDB.Analyze.Inventor have been run to obtain the RefID information ONLY FOR THE SINGLE DELTA FILE
        // meaning, run it before adding IterationRef records using this method
        // the [IDB.Files] table has all original files set as version 1 of 1

        // meaning, the record is already in the Vault as a file
        // but may be different than that found in the Vault
        // !!!WARNING!!!
        // for now, we are only able to process a situation where there is 
        // only one file in the target Vault
        // and only one Delta file being introduced in the current load


        // static method to copy VaultFileIteration checksum information to 
        public static void CopyVaultFMElemInfoToIDBFile()
        {
            StopwatchPlus swp = new StopwatchPlus();

            Logger.Debug("CopyVaultFMElemInfoToIDBFile: Start");

            // validate that Vault Checksum and CreateDate columns are in Intermediate DB
            mCO_IntermediateDbSqlCache.SetupDatabaseColumnsForExistingVaultFileDiscovery();

            // get the list of files that are in the Vault
            List<CO_IDB_VaultFileMaster> filesMatchingFullPath = mCO_IDB_VaultFileMaster_List.Where(coivfm => ((coivfm.mVaultFileMaster != null))).ToList();

            // iterate through each, combine the EntityCreateDate and VaultFileCheckSum, and update the record
            foreach (CO_IDB_VaultFileMaster civfm in filesMatchingFullPath)
            {
                // copy the information to the File record
                MCDD.DbEntity.File idbFileRecord = civfm.mCO_FileMaster.LatestCO_FileIteration.mFileIterationRecord;
                MVDD.VaultFileIteration vaultFileIteration = civfm.mVaultFileMaster.GetTipVaultFileIteration();

                idbFileRecord.VaultFileExists = true;
                idbFileRecord.EntityCreateDate = vaultFileIteration.m_EntityRecord.CreateDate.Value;
                idbFileRecord.VaultFileCheckSum = vaultFileIteration.m_FileResourceRecord.Checksum;

                // set the primary object information on matches
                civfm.mExistsInVault = true;
                civfm.mChecksumsMatch = (idbFileRecord.LocalFileCheckSum == idbFileRecord.VaultFileCheckSum);
            }



            // log total time taken
            Logger.Debug(string.Format("CopyVaultFMElemInfoToIDBFile(): End {0}", swp.ElapsedTimeString()));
        }



    }
}
