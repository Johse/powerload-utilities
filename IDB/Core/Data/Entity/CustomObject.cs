using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using IDB.Core.Data.Base;
using IDB.Core.Data.Interface;
using IDB.Core.Extensions;

namespace IDB.Core.Data.Entity
{
    public class CustomObject : BaseEntityUdp, ICommands
    {
        // ReSharper disable once InconsistentNaming
        public long CustomObjectID { get; set; }
        public string CustomObjectDefinition { get; set; }
        public string CustomObjectName { get; set; }
        public string Category { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public string LifecycleState { get; set; }
        public string LifecycleDefinition { get; set; }

        public bcpDevKit.Entities.CustomObjects.CustomObject BcpCustomObject { get; set; }

        public CustomObject()
        {
        }

        public CustomObject(IDictionary<string, object> dapperRow) : base(dapperRow)
        {
        }

        public void Insert(SqlConnection connection)
        {
            var entity = this.GetDapperEntity<Folder>(nameof(CustomObjectID));
            CustomObjectID = connection.InsertEntityAndReturnId(entity);
        }

        public void Update(SqlConnection connection)
        {
            var entity = this.GetDapperEntity<File>(nameof(CustomObjectID));
            connection.UpdateEntity(entity);
        }
    }
}