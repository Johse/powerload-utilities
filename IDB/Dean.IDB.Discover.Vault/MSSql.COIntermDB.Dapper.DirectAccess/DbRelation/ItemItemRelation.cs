using MSSql.COIntermDB.Dapper.DirectAccess.DbEntity;

namespace MSSql.COIntermDB.Dapper.DirectAccess.DbRelation
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


        public string GetTableName()
        {
            return "ItemItemRelations";
        }

        public string GetSelectString()
        {
            return "SELECT * FROM ItemItemRelations";
        }

        public IDbEntity GetNullEntity()
        {
            return null;
        }
    }
}