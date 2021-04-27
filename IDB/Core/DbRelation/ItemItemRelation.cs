using System.Collections.Generic;
using IDB.Core.DbEntity;

namespace IDB.Core.DbRelation
{
    public class ItemItemRelation : IDbEntity
    {
        // ReSharper disable once InconsistentNaming
        public long ParentItemID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ChildItemID { get; set; }
        public int Position { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public string LinkType { get; set; }
        public int InstanceCount { get; set; }
        public decimal UnitSize { get; set; }
        // ReSharper disable once InconsistentNaming
        public bool CAD { get; set; }
        public Dictionary<string, object> UserDefinedProperties { get; set; }
    }
}