using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using IDB.Core.Data.TargetVault;
using IDB.Core.Extensions;
using log4net;

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

            var table = "TargetVaultFiles";
            Log.Info($"Transferring Vault information to IDB table '{table}'");

            var rows = QueryTable<TargetVaultFile>(sql, table);
            using (var connection = new SqlConnection(_loadConnectionString))
            {
                connection.Open();
                connection.TruncateTable(table);
                connection.BulkInsert(table, rows);
            }
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

            var table = "TargetVaultProperties";
            Log.Info($"Transferring Vault information to IDB table '{table}'");

            var rows = QueryTable<TargetVaultProperty>(sql, table);
            using (var connection = new SqlConnection(_loadConnectionString))
            {
                connection.Open();
                connection.TruncateTable(table);
                connection.BulkInsert(table, rows);
            }
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

            var table = "TargetVaultLifeCycles";
            Log.Info($"Transferring Vault information to IDB table '{table}'");

            var rows = QueryTable<TargetVaultLifeCycle>(sql, table);
            using (var connection = new SqlConnection(_loadConnectionString))
            {
                connection.Open();
                connection.TruncateTable(table);
                connection.BulkInsert(table, rows);
            }
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

            var table = "TargetVaultCategories";
            Log.Info($"Transferring Vault information to IDB table '{table}'");

            var rows = QueryTable<TargetVaultCategory>(sql, table);
            using (var connection = new SqlConnection(_loadConnectionString))
            {
                connection.Open();
                connection.TruncateTable(table);
                connection.BulkInsert(table, rows);
            }
        }

        public static void TransferRevisions()
        {
            var sql = @"
                SELECT RD.DisplayName AS RevisionDefinition, RS.DisplayName AS PrimarySequence, RL.Label AS RevisionLabel FROM [dbo].[RevisionDefinition] RD
                INNER JOIN [dbo].[RevisionSequence] RS ON RD.PrimarySeqSchemeID = RS.SeqSchemeID
                INNER JOIN  [dbo].[RevisionSequenceLabel] RL ON RS.SeqSchemeID = RL.SeqSchemeID
                ORDER BY RD.DisplayName, RS.DisplayName, RL.Rank
            ";

            var table = "TargetVaultRevisions";
            Log.Info($"Transferring Vault information to IDB table '{table}'");

            var rows = QueryTable<TargetVaultRevision>(sql, table);
            using (var connection = new SqlConnection(_loadConnectionString))
            {
                connection.Open();
                connection.TruncateTable(table);
                connection.BulkInsert(table, rows);
            }
        }

        public static IEnumerable<T> QueryTable<T>(string sqlQuery, string loadTable)
        {
            Log.Debug(sqlQuery);

            using (var vaultConnection = new SqlConnection(_vaultConnectionString))
            {
                var rows = vaultConnection.Query<T>(sqlQuery).ToList();
                Log.Info($"{rows.Count} rows found");

                return rows;
            }
        }
    }
}