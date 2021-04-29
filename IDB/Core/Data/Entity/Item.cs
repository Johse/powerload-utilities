using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using IDB.Core.Data.Base;
using IDB.Core.Data.Interface;
using IDB.Core.Extensions;

namespace IDB.Core.Data.Entity
{
    public class Item : BaseEntityUdp, ICommands
    {
        // ReSharper disable once InconsistentNaming
        public long ItemID { get; set; }
        public string ItemNumber { get; set; }
        public string Category { get; set; }
        public string RevisionLabel { get; set; }
        public string RevisionDefinition { get; set; }
        public int Version { get; set; }
        public string LifecycleState { get; set; }
        public string LifecycleDefinition { get; set; }
        public string Comment { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public string Title { get; set; }
        public string Unit { get; set; }
        public string Description { get; set; }
        public string BomStructure { get; set; }

        public bcpDevKit.Entities.Items.ItemIteration BcpItemIteration { get; set; }

        public Item()
        {
        }

        public Item(IDictionary<string, object> dapperRow) : base(dapperRow)
        {

        }

        public void Insert(SqlConnection connection)
        {
            var entity = this.GetDapperEntity<Folder>(nameof(ItemID));
            ItemID = connection.InsertEntity(entity);
        }

        public void Update(SqlConnection connection)
        {
            var entity = this.GetDapperEntity<File>(nameof(ItemID));
            connection.UpdateEntity(entity);
        }
    }
}