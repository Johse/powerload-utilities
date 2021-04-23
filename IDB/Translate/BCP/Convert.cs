using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using IDB.Core.DbEntity;

namespace IDB.Translate.BCP
{
    public static class Convert
    {
        public static IDbEntity To<T>(dynamic p) where T : IDbEntity, new()
        {
            var entity = new T { UserDefinedProperties = new Dictionary<string, object>() };

            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (var element in p)
            {
                var property = properties.SingleOrDefault(x => x.Name == element.Key);
                if (property != default(PropertyInfo))
                    property.SetValue(entity, element.Value);

                if (element.Key.StartsWith("UDP_"))
                    entity.UserDefinedProperties.Add(element.Key.Replace("UDP_", "").ToString(), element.Value);
            }

            return entity;
        }

        public static Dictionary<string, object> ToFlatDictionary(IDbEntity entity)
        {
            var dictionary = new Dictionary<string, object>();
            var properties = entity.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.PropertyType.IsEnum && property.Name == "UserDefinedProperties")
                {
                    Dictionary<string, object> props = (Dictionary<string, object>)property.GetValue(entity);
                    foreach (var prop in props)
                        dictionary["UDP_" + prop.Key] = prop.Value;
                }
                else
                    dictionary[property.Name] = property.GetValue(entity);
            }

            return dictionary;
        }
    }
}