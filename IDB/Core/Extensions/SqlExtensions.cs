using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace IDB.Core.Extensions
{
    public static class SqlExtensions
    {
        public static T FindSingle<T>(this SqlConnection connection, string sql, object param = null)
        {
            if (connection.State != ConnectionState.Open)
                return default;

            IDictionary<string, object> dapperRow = connection.QuerySingle(sql, param);
            return (T)Activator.CreateInstance(typeof(T), dapperRow);
        }

        public static IEnumerable<T> Find<T>(this SqlConnection connection, string sql, object param = null)
        {
            if (connection.State != ConnectionState.Open)
                yield return default;

            IEnumerable<dynamic> dapperRows = connection.Query(sql, param);
            foreach (IDictionary<string, object> dapperRow in dapperRows)
            {
                yield return (T)Activator.CreateInstance(typeof(T), dapperRow);
            }
        }

        public static void TruncateTable(this SqlConnection connection, string tableName)
        {
            if (connection.State != ConnectionState.Open)
                return;

            var cmd = new SqlCommand($"TRUNCATE TABLE dbo.{tableName}", connection);
            cmd.ExecuteNonQuery();
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
                    transaction.Rollback();
                    connection.Close();
                }
            }
            transaction.Commit();
        }
    }
}