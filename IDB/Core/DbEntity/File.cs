using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;

namespace IDB.Core.DbEntity
{
    public class File : IDbEntity
    {
        public string LocalFullFileName { get; set; }
        // ReSharper disable once InconsistentNaming
        public long FileID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long FolderID { get; set; }
        public string FileName { get; set; }
        public string Category { get; set; }
        public string Classification { get; set; }
        public string RevisionLabel { get; set; }
        public string RevisionDefinition { get; set; }
        public int Version { get; set; }
        public string LifecycleState { get; set; }
        public string LifecycleDefinition { get; set; }
        public string Comment { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsHidden { get; set; }
		public bool IsExcluded { get; set; }
        public object Tag { get; set; }
        public Dictionary<string, object> UserDefinedProperties { get; set; }
        public bcpDevKit.Entities.Vault.FileIteration BcpFileIteration { get; set; }

        private Dictionary<string, string> _propertyParameters;

        public File()
        {
            UserDefinedProperties = new Dictionary<string, object>();
        }

        public File(IDictionary<string, object> dapperRow) : this()
        {
            foreach (KeyValuePair<string, object> keyValuePair in dapperRow)
            {
                if (keyValuePair.Key.StartsWith("UDP_"))
                    UserDefinedProperties.Add(keyValuePair.Key.Substring(4), keyValuePair.Value);
                else
                    GetType().GetProperty(keyValuePair.Key)?.SetValue(this, keyValuePair.Value, null);
            }
        }

        private object GetExtendedObject()
        {
            _propertyParameters = new Dictionary<string, string>();

            var extendedType = DynamicTypes.DynamicTypeInstance.ExtendedTypes[typeof(File)];
            var extendedObject = Activator.CreateInstance(extendedType);

            var properties = this.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if(property.Name == nameof(File.UserDefinedProperties))
                    continue;

                if (property.Name == nameof(File.BcpFileIteration))
                    continue;

                extendedType.GetProperty(property.Name)?.SetValue(extendedObject, property.GetValue(this), null);
                _propertyParameters.Add(property.Name, $"{property.Name}");
            }

            foreach (var userDefinedProperty in UserDefinedProperties)
            {
                var cleanPropertyName = userDefinedProperty.Key.Replace(" ", "_");
                extendedType.GetProperty($"UDP_{cleanPropertyName}")?.SetValue(extendedObject, userDefinedProperty.Value, null);
                _propertyParameters.Add($"UDP_{userDefinedProperty.Key}", $"UDP_{cleanPropertyName}");
            }

            return extendedObject;
        }

        public bool Insert(SqlConnection connection)
        {
            try
            {
                var fileEx = GetExtendedObject();
                var sql = GetInsertSql();
                this.FileID = connection.QuerySingle<long>(sql, fileEx);
                return true;
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public bool Update(SqlConnection connection)
        {
            try
            {
                var fileEx = GetExtendedObject();
                var sql = GetUpdateSql();
                connection.Execute(sql, fileEx);
                return true;
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        private string GetInsertSql()
        {
            var sb = new StringBuilder();
            sb.Append("INSERT INTO [dbo].[Files]");
            sb.Append("(");
            sb.Append(string.Join(", ", _propertyParameters.
                Where(p => !p.Value.Equals(nameof(FileID))).
                OrderBy(p => p.Key).
                Select(p => $"[{p.Key}]")));
            sb.Append(") VALUES (");
            sb.Append(string.Join(", ", _propertyParameters.
                Where(p => !p.Value.Equals(nameof(FileID))).
                OrderBy(p => p.Key).
                Select(p => $"@{p.Value}")));
            sb.Append(")");
            sb.Append(";SELECT CAST(SCOPE_IDENTITY() as bigint)");

            return sb.ToString();
        }

        private string GetUpdateSql()
        {
            var sb = new StringBuilder();
            sb.Append("UPDATE [dbo].[Files] SET ");
            sb.Append(string.Join(", ", _propertyParameters.
                Where(p => !p.Value.Equals(nameof(FileID))).
                OrderBy(p => p.Key).
                Select(p => $"[{p.Key}] = @{p.Value}")));
            sb.Append($" WHERE [{nameof(FileID)}] = @{nameof(FileID)}");

            return sb.ToString();
        }
    }
}