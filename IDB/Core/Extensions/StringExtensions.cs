using System;
using System.Globalization;
using System.Linq;

namespace IDB.Core.Extensions
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return string.Join(".", name.Split('.').Select(n => char.ToLower(n[0], CultureInfo.InvariantCulture) + n.Substring(1)));
        }
    }
}
