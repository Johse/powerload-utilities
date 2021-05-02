using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Dapper;
using IDB.Core.DapperHelper;

namespace IDB.Core.Extensions
{
    public static class SqlExtensions
    {
        public static long InsertEntityAndReturnId(this SqlConnection connection, DapperEntity dapperEntity)
        {
            if (connection.State != ConnectionState.Open)
                return -1;

            try
            {
                var sb = new StringBuilder();
                sb.Append($"INSERT INTO [dbo].[{dapperEntity.TableName}]");
                sb.Append("(");
                sb.Append(string.Join(", ", dapperEntity.ColumnsAndParams
                    .Where(p => !dapperEntity.IdColumns.Contains(p.ParamName))
                    .OrderBy(p => p.ColumnName)
                    .Select(p => $"[{p.ColumnName}]")));
                sb.Append(") VALUES (");
                sb.Append(string.Join(", ", dapperEntity.ColumnsAndParams
                    .Where(p => !dapperEntity.IdColumns.Contains(p.ParamName))
                    .OrderBy(p => p.ColumnName)
                    .Select(p => $"@{p.ParamName}")));
                sb.Append(")");
                sb.Append(";SELECT CAST(SCOPE_IDENTITY() as bigint)");
                var sql = sb.ToString();

                var entityId = connection.QuerySingle<long>(sql, dapperEntity.DynamicParameters);
                return entityId;
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex);
                return 0;
            }
        }

        public static bool InsertEntity(this SqlConnection connection, DapperEntity dapperEntity)
        {
            if (connection.State != ConnectionState.Open)
                return false;

            try
            {
                var sb = new StringBuilder();
                sb.Append($"INSERT INTO [dbo].[{dapperEntity.TableName}]");
                sb.Append("(");
                sb.Append(string.Join(", ", dapperEntity.ColumnsAndParams
                    .OrderBy(p => p.ColumnName)
                    .Select(p => $"[{p.ColumnName}]")));
                sb.Append(") VALUES (");
                sb.Append(string.Join(", ", dapperEntity.ColumnsAndParams
                    .OrderBy(p => p.ColumnName)
                    .Select(p => $"@{p.ParamName}")));
                sb.Append(")");
                var sql = sb.ToString();

                connection.Execute(sql, dapperEntity.DynamicParameters);
                return true;
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public static bool UpdateEntity(this SqlConnection connection, DapperEntity dapperEntity)
        {
            if (connection.State != ConnectionState.Open)
                return false;

            try
            {
                var sb = new StringBuilder();
                sb.Append($"UPDATE [dbo].[{dapperEntity.TableName}] SET ");
                sb.Append(string.Join(", ", dapperEntity.ColumnsAndParams
                    .Where(p => !dapperEntity.IdColumns.Contains(p.ParamName))
                    .OrderBy(p => p.ColumnName)
                    .Select(p => $"[{p.ColumnName}] = @{p.ParamName}")));
                
                var where = new List<string>();
                foreach (var idColumn in dapperEntity.IdColumns)
                    where.Add($"[{idColumn}] = @{idColumn}");
                sb.Append($" WHERE {string.Join(" AND ", where)}");

                var sql = sb.ToString();

                connection.Execute(sql, dapperEntity.DynamicParameters);
                return true;
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public static T SelectEntity<T>(this SqlConnection connection, string sql, object param = null) where T : class
        {
            if (connection.State != ConnectionState.Open)
                return null;

            try
            {
                IDictionary<string, object> dapperRow = connection.QuerySingle(sql, param);
                return (T)Activator.CreateInstance(typeof(T), dapperRow);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public static IEnumerable<T> SelectEntities<T>(this SqlConnection connection, string sql, object param = null)
        {
            var entities = new List<T>();
            if (connection.State != ConnectionState.Open)
                return entities;

            try
            {
                IEnumerable<dynamic> dapperRows = connection.Query(sql, param);
                foreach (IDictionary<string, object> dapperRow in dapperRows)
                    entities.Add((T)Activator.CreateInstance(typeof(T), dapperRow));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return entities;
        }

        public static void TruncateTable(this SqlConnection connection, string tableName)
        {
            if (connection.State != ConnectionState.Open)
                return;

            try
            {
                var cmd = new SqlCommand($"TRUNCATE TABLE dbo.{tableName}", connection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void BulkInsert<T>(this SqlConnection connection, string tableName, IEnumerable<T> data, int batchSize = 1000)
        {
            if (connection.State != ConnectionState.Open)
                return;

            var properties = TypeDescriptor.GetProperties(typeof(T));
            var table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            foreach (var item in data)
            {
                var row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;

                table.Rows.Add(row);
            }

            var transaction = connection.BeginTransaction();
            using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default | SqlBulkCopyOptions.KeepIdentity, transaction))
            {
                bulkCopy.BatchSize = batchSize;
                bulkCopy.DestinationTableName = tableName;

                foreach (DataColumn dataColumn in table.Columns)
                    bulkCopy.ColumnMappings.Add(dataColumn.ColumnName, dataColumn.ColumnName);

                try
                {
                    bulkCopy.WriteToServer(table);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);

                    transaction.Rollback();
                    connection.Close();
                }
            }
            transaction.Commit();
        }
    }
}