using System;
using System.Collections.Generic;

namespace IDB.Core.DbEntity
{
    public class CustomObject : IDbEntity
    {
        public long CustomObjectID { get; set; }
        public string CustomObjectDefinition { get; set; }
        public string CustomObjectName { get; set; }
        public string Category { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public string LifecycleState { get; set; }
        public string LifecycleDefinition { get; set; }
        public Dictionary<string, object> UserDefinedProperties { get; set; }
        public bcpDevKit.Entities.CustomObjects.CustomObject BcpCustomObject { get; set; }
    }
}