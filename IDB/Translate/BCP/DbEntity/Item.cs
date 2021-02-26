using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDB.Translate.BCP.DbEntity
{
    public class Item : IDbEntity
    {
        public int ItemID { get; set; }
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
        public Dictionary<string, object> UserDefinedProperties { get; set; }
        public bcpDevKit.Entities.Items.ItemIteration BcpItemIteration { get; set; }
    }
}
