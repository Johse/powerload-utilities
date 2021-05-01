using System;

namespace IDB.Core.DapperHelper
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class ColumnAttribute : Attribute
	{
        public ColumnAttribute(string name)
        {
            Name = name;
        }

		public string Name { get; set; }
	}
}