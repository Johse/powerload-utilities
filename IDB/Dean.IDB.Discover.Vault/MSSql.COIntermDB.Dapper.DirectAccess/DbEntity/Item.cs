using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSql.COIntermDB.Dapper.DirectAccess.DbEntity
{
    public class Item : IDbEntityWithIDAndUDPs
    {
        // public bcpDevKit.Entities.Items.ItemIteration mBcpItemIteration { get; set; }

        public int ItemID { get; set; }
        public string ItemNumber { get; set; }
        public string Category { get; set; }
        public string RevisionLabel { get; set; }
        public string RevisionDefinition { get; set; }
        public string Version { get; set; }
        public string LifecycleState { get; set; }
        public string LifecycleDefinition { get; set; }
        public string Comment { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public string Title { get; set; }
        public string Unit { get; set; }
        public string Description { get; set; }
        public string BomStructure { get; set; }
        public Dictionary<string, object> UserDefinedProperties { get; set; }

        public int GetId()
        {
            // NOTE: TODO: if all tables had an EntityId
            // move this into the base class

            return (ItemID);
        }

        public string GetTableName()
        {
            return "Items";
        }

        public string GetSelectString()
        {
            return "SELECT * FROM Items";
        }

        public IDbEntity GetNullEntity()
        {
            return null;
        }

    }
}
