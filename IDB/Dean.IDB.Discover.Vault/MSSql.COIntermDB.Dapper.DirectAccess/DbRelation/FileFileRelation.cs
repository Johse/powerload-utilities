using MSSql.COIntermDB.Dapper.DirectAccess.DbEntity;

namespace MSSql.COIntermDB.Dapper.DirectAccess.DbRelation
{
    public class FileFileRelation : IDbEntity
    {
        public int ParentFileID { get; set; }
        public int ChildFileID { get; set; }
        public bool IsAttachment { get; set; }
        public bool IsDependency { get; set; }
        public bool NeedsResolution { get; set; }
        public string Source { get; set; } 
        public string RefId { get; set; }


        public string GetTableName()
        {
            return "FileFileRelations";
        }

        public string GetSelectString()
        {
            return "SELECT * FROM FileFileRelations";
        }

        public IDbEntity GetNullEntity()
        {
            return null;
        }
    }
}