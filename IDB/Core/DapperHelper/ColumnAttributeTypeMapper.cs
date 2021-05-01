using System;
using System.Linq;
using Dapper;

namespace IDB.Core.DapperHelper
{
    public class ColumnAttributeTypeMapper : FallbackTypeMapper
    {
        public ColumnAttributeTypeMapper(Type t) : base(new SqlMapper.ITypeMap[]
        {
            new CustomPropertyTypeMap(
                t,
                (type, columnName) =>
                    type.GetProperties().FirstOrDefault(prop =>
                        prop.GetCustomAttributes(false)
                            .OfType<ColumnAttribute>()
                            .Any(attr => attr.Name == columnName)
                    )
            ),
            new DefaultTypeMap(t)
        })
        {
        }
    }
}