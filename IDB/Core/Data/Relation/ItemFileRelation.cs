using System.Collections.Generic;
using IDB.Core.Data.Base;

namespace IDB.Core.Data.Relation
{
    public class ItemFileRelation : RelationBase
    {
        // ReSharper disable once InconsistentNaming
        public long ItemID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long FileID { get; set; }
        public bool IsPrimaryLink { get; set; }
        public bool IsSecondaryLink { get; set; }
        public bool IsStandardComponentLink { get; set; }
        public bool IsDesignDocument { get; set; }
        public bool IsAttachment { get; set; }

        public ItemFileRelation()
        {
        }

        public ItemFileRelation(IDictionary<string, object> dapperRow) : base(dapperRow)
        {
        }
    }
}