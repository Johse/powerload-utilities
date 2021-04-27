using IDB.Core.DapperHelper;
using IDB.Core.DbEntity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;

namespace IDB.Core.DynamicTypes
{
    public class DynamicTypeInstance
    {
        public static Dictionary<Type, Type> ExtendedTypes;

        public class Column
        {
            public string ColumnName { get; set; }

            public string SqlColumnType { get; set; }

            public string ColumnType
            {
                get
                {
                    switch (SqlColumnType)
                    {
                        case "nvarchar":
                            return "System.String";
                        case "bit":
                            return "System.Boolean";
                        case "int":
                            return "System.Int32";
                        case "bigint":
                            return "System.Int64";
                        case "datetime":
                            return "System.DateTime";
                        default:
                            throw new NotSupportedException($"The SQL column {SqlColumnType} is not supported!");
                    }
                }
            }
        }

        public static void Create(string connectionString)
        {
            ExtendedTypes = new Dictionary<Type, Type>();
            var factory = new DynamicTypeFactory();

            var dynamicProperties = new List<DynamicProperty>();

            using (var connection = new SqlConnection(connectionString))
            {
                var sql = "SELECT column_name AS ColumnName, DATA_TYPE AS SqlColumnType FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Files' AND LEFT(column_name,4) = 'UDP_'";
                var columns = connection.Query<Column>(sql);

                foreach (var column in columns)
                {
                    dynamicProperties.Add(new DynamicProperty
                    {
                        PropertyName = column.ColumnName,
                        SystemTypeName = column.ColumnType
                    });
                }
            }

            var extendedType = factory.CreateNewTypeWithDynamicProperties(typeof(File), "Files", dynamicProperties);
            ExtendedTypes.Add(typeof(File), extendedType);

            Dapper.SqlMapper.SetTypeMap(extendedType, new ColumnAttributeTypeMapper(extendedType));
        }
    }
}
