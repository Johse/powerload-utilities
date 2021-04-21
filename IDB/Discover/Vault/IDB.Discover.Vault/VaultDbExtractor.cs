using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using IDB.Core.DbTargetVault;
using log4net;
using Z.Dapper.Plus;

namespace IDB.Discover.Vault
{
    public class VaultDbExtractor
    {
        private static readonly ILog Log = LogManager.GetLogger("IDBDiscoverVault");

        private static string _vaultConnectionString;
        private static string _loadConnectionString;

        public static void Transfer(string vaultConnectionString, string loadConnectionString)
        {
            Log.Debug("Starting transfer");

            _vaultConnectionString = vaultConnectionString;
            _loadConnectionString = loadConnectionString;

            TransferFiles();
            TransferProperties();
            TransferLifeCycles();
            TransferCategories();
            TransferRevisions();
        }

        public static void TransferFiles()
        {
            var sql = @"
                SELECT FO.VaultPath AS Folder, FI.FileName, FI.FileIterationId, IT.MasterId AS FileMasterId, E.CreateDate, FR.Checksum
                FROM dbo.FileIteration FI
                INNER JOIN dbo.Iteration IT ON FI.FileIterationId = IT.IterationID
                INNER JOIN dbo.FileMaster FM ON IT.MasterID = FM.FileMasterID
                INNER JOIN dbo.Folder FO ON FM.FolderId = FO.FolderID
                INNER JOIN dbo.FileResource FR ON FR.ResourceId = FI.ResourceId
                INNER JOIN dbo.Entity E ON E.EntityId = IT.IterationID
                ORDER BY FO.VaultPath, FI.FileName, E.CreateDate
            ";

            TransferTable<TargetVaultFile>(sql, "TargetVaultFiles");
        }

        public static void TransferProperties()
        {
            var sql = @"
                SELECT E.BaseId AS EntityClassId, P.FriendlyName AS PropertyName, P.DataType, P.IsSystem, P.Active
                FROM  dbo.EntityClassPropertyDef EC 
                INNER JOIN dbo.EntityClass E ON EC.EntityClassID = E.EntityClassID
                INNER JOIN dbo.PropertyDef P ON EC.PropertyDefID = P.PropertyDefID
                ORDER BY E.BaseId,P.FriendlyName
            ";

            TransferTable<TargetVaultProperty>(sql, "TargetVaultProperties");
        }
        public static void TransferLifeCycles()
        {
            var sql = @"
                SELECT E.BaseId AS EntityClassId, LD.DisplayName AS LifeCycleDefinition, LC.DisplayName AS LifeCycleState, LC.IsObsoleteState, LC.IsReleasedState
                FROM dbo.BehaviorClass BC
                INNER JOIN dbo.EntityClassBehavior EC ON BC.BehaviorClassId = EC.BehaviorClassID
                INNER JOIN dbo.EntityClass E ON EC.EntityClassID = E.EntityClassID
                INNER JOIN dbo.LifeCycleDefinition LD ON EC.BehaviorID = LD.LifeCycleDefId
                INNER JOIN dbo.LifeCycleState LC ON LD.LifeCycleDefId = LC.LifeCycleDefId
                WHERE BC.DisplayName = 'LifeCycle' 
                ORDER BY E.BaseId,LD.DisplayName,LC.DisplayName
            ";

            TransferTable<TargetVaultLifeCycle>(sql, "TargetVaultLifeCycles");
        }

        public static void TransferCategories()
        {
            var sql = @"
                SELECT E.BaseId AS EntityClassId, C.DisplayName AS Category
                FROM dbo.BehaviorClass BC
                INNER JOIN dbo.EntityClassBehavior EC ON BC.BehaviorClassId = EC.BehaviorClassID
                INNER JOIN dbo.EntityClass E ON EC.EntityClassID = E.EntityClassID
                INNER JOIN dbo.CategoryDef C ON EC.BehaviorID = C.CategoryDefId
                WHERE BC.DisplayName = 'Category' 
                ORDER BY E.BaseId,C.DisplayName
            ";

            TransferTable<TargetVaultCategory>(sql, "TargetVaultCategories");
        }

        public static void TransferRevisions()
        {
            var sql = @"
                SELECT RD.DisplayName AS RevisionDefinition, RS.DisplayName AS PrimarySequence, RL.Label AS RevisionLabel FROM [dbo].[RevisionDefinition] RD
                INNER JOIN [dbo].[RevisionSequence] RS ON RD.PrimarySeqSchemeID = RS.SeqSchemeID
                INNER JOIN  [dbo].[RevisionSequenceLabel] RL ON RS.SeqSchemeID = RL.SeqSchemeID
                ORDER BY RD.DisplayName, RS.DisplayName, RL.Rank
            ";

            TransferTable<TargetVaultRevision>(sql, "TargetVaultRevisions");
        }

        public static void TransferTable<T>(string sqlQuery, string loadTable)
        {
            Log.Info($"Transferring Vault information to IDB table '{loadTable}'");
            Log.Debug(sqlQuery);

            List<T> files;
            using (var vaultConnection = new SqlConnection(_vaultConnectionString))
            {
                files = vaultConnection.Query<T>(sqlQuery).ToList();
                Log.Info($"{files.Count} rows found");
            }

            using (var loadConnection = new SqlConnection(_loadConnectionString))
            {
                DapperPlusManager.Entity(typeof(T)).Table(loadTable);

                loadConnection.Open();
                var cmd = new SqlCommand($"TRUNCATE TABLE dbo.{loadTable}", loadConnection);
                cmd.ExecuteNonQuery();

                loadConnection.BulkInsert(files);
            }
        }
    }
}