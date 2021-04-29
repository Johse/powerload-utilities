using MSSql.COIntermDB.Dapper.DirectAccess.DbEntity;

namespace MSSql.COIntermDB.Dapper.DirectAccess.DbRelation
{
    public class ItemFileRelation : IDbEntity
    {
        public int ItemID { get; set; }
        public int FileID { get; set; }
        public bool IsPrimaryLink { get; set; }
        public bool IsSecondaryLink { get; set; }
        public bool IsTertiaryLink { get; set; }


        public string GetTableName()
        {
            return "ItemFileRelations";
        }

        public string GetSelectString()
        {
            return "SELECT * FROM ItemFileRelations";
        }

        public IDbEntity GetNullEntity()
        {
            return null;
        }
    }
}