﻿using System.Collections.Generic;
using System.Reflection;
using IDB.Core.DapperHelper;
using IDB.Core.Data.Interface;

namespace IDB.Core.Extensions
{
    public static class DapperExtensions
    {
        public static DapperEntity GetDapperEntity<T>(this IUdp entity, string idColumn)
        {
            return GetDapperEntity<T>(entity, new[] {idColumn});
        }
    
        public static DapperEntity GetDapperEntity<T>(this IUdp entity, string[] idColumns)
        {
            var tableName = typeof(T).Name + "s";
            var dapperEntity = new DapperEntity(tableName, new List<string>(idColumns));

            var properties = typeof(T).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.Name == nameof(IUdp.UserDefinedProperties)) continue;
                if (property.PropertyType.Namespace != null && property.PropertyType.Namespace.StartsWith("bcpDevKit")) continue;

                dapperEntity.Add(property.Name, property.GetValue(entity));
            }

            foreach (var userDefinedProperty in entity.UserDefinedProperties)
                dapperEntity.Add($"UDP_{userDefinedProperty.Key}", userDefinedProperty.Value);

            return dapperEntity;
        }
    }
}
