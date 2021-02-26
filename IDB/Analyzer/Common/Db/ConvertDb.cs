using System.Linq;
using System.Reflection;

namespace IDB.Analyzer.Common.Db
{
    public static class ConvertDb
    {
        public static IDbEntity To<T>(dynamic p) where T : IDbEntity, new()
        {
            var entity = new T();

            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (var element in p)
            {
                var property = properties.SingleOrDefault(x => x.Name == element.Key);
                if (property != default(PropertyInfo))
                    property.SetValue(entity, element.Value);
            }

            return entity;
        }
    }
}
