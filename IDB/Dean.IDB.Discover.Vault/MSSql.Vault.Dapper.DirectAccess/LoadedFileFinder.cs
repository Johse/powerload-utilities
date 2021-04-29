using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSSql.Vault.Dapper.DirectAccess.VaultDbEntities;
using Property = MSSql.Vault.Dapper.DirectAccess.VaultDbEntities.Property;

using log4net;
using BCPBuilderConfig;

namespace MSSql.Vault.Dapper.DirectAccess
{
    public class LoadedFileFinder
    {
        private static readonly ILog Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetLogger(typeof(LoadedFileFinder));
        private string _connectionString;
        private Dictionary<long, FileResource> _fileResources;
        private Dictionary<long, FileIteration> _fileIterations;
        private Dictionary<long, FileMaster> _fileMasters;
        ILookup<long, Property> propertiesByETLId;
        
        public bool Loaded { get; private set; }


        public LoadedFileFinder(string sConnectionString)
        {
            _connectionString = sConnectionString;
        }

        public void LoadCache(string CadFileIdUDF)
        {
            var cacheLoader = new MSSqlVaultCacheLoader(_connectionString);

            Logger.Debug("LoadedFileFinder Load: Start");

            var propertyDefintion = cacheLoader.LoadSingleVaultEntity<PropertyDef>("select PropertyDefID, FriendlyName FROM dbo.PropertyDef WHERE FriendlyName = @FriendlyName", new { FriendlyName = CadFileIdUDF});

            var cadFileIdProperties = cacheLoader.LoadVaultEntityDictionary<Property>("select PropertyId, PropertyDefId, EntityId, Value FROM dbo.Property WHERE PropertyDefId = @PropertyDefId", new { PropertyDefId = propertyDefintion.PropertyDefID });
            _fileResources = cacheLoader.LoadVaultEntityDictionary<FileResource>();
            _fileIterations = cacheLoader.LoadVaultEntityDictionary<FileIteration>();
            _fileMasters = cacheLoader.LoadVaultEntityDictionary<FileMaster>();

            Logger.Debug("LoadedFileFinder Load: Finish");
            Logger.Debug("VaultCacheOld Indexes Load: Start");
            
            propertiesByETLId = cadFileIdProperties.Values.ToLookup(p =>
                {
                    long id;
                    long.TryParse(p.Value.ToString(), out id);
                    return id;
                },p => p);

            Logger.Debug("VaultCacheOld Indexes Load: Finish");

            Loaded = true;
        }


        // check to see if the file existed in the Vault before we started processing
        // and verify that the file is not currently checked out
        // this is the quickest way to cross reference to a Vault FileMaster.Id
        public bool IsMasterChekedOut(long nVaultMasterId)
        {
            bool bMasterCheckedOut = false;

            if (_fileMasters.ContainsKey(nVaultMasterId))
            {
                FileMaster fm = _fileMasters[nVaultMasterId];
                bMasterCheckedOut = fm.CheckedOut;
            }

            return (bMasterCheckedOut);
        }

    }
}
