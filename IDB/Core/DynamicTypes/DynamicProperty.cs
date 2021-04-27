using System;

namespace IDB.Core.DynamicTypes
{
    public class DynamicProperty
    {
        public string PropertyName { get; set; }

        //public string DisplayName { get; set; }

        public string SystemTypeName { get; set; }

        public Type SystemType => Type.GetType(SystemTypeName);
    }
}
