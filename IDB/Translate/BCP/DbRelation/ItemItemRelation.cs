using System.Collections.Generic;
using IDB.Translate.BCP.DbEntity;

namespace IDB.Translate.BCP.DbRelation
{
    public class ItemItemRelation : IDbEntity
    {
        public int ParentItemID { get; set; }
        public int ChildItemID { get; set; }
        public int Position { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public string LinkType { get; set; }
        public int InstanceCount { get; set; }
        public decimal UnitSize { get; set; }
        public bool CAD { get; set; }
        public Dictionary<string, object> UserDefinedProperties { get; set; }
    }
}