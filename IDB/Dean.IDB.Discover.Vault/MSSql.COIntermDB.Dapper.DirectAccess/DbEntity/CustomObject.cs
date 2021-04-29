using System;
using System.Collections.Generic;

namespace MSSql.COIntermDB.Dapper.DirectAccess.DbEntity
{
    public class CustomObject : IDbEntityWithIDAndUDPs
    {
        public int CustomObjectID { get; set; }
        public string CustomObjectDefinition { get; set; }
        public string CustomObjectName { get; set; }
        public string Category { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public string LifecycleState { get; set; }
        public string LifecycleDefinition { get; set; }
        public Dictionary<string, object> UserDefinedProperties { get; set; }

        // public bcpDevKit.Entities.CustomObjects.CustomObject BcpCustomObject { get; set; }

        public int GetId()
        {
            // NOTE: TODO: if all tables had an EntityId
            // move this into the base class

            return (CustomObjectID);
        }

        public string GetTableName()
        {
            return "CustomObjects";
        }

        public string GetSelectString()
        {
            return "SELECT * FROM CustomObjects";
        }

        public IDbEntity GetNullEntity()
        {
            return null;
        }

    }
}