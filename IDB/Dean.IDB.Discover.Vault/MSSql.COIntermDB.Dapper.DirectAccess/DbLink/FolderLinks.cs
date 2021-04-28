using MSSql.COIntermDB.Dapper.DirectAccess.DbEntity;

namespace MSSql.COIntermDB.Dapper.DirectAccess.DbLink
{
    public class FolderCustomObjectLink : IDbEntity
    {
        public int ParentFolderID { get; set; }
        public int ChildCustomObjectID { get; set; }


        public string GetTableName()
        {
            return "FolderCustomObjectLinks";
        }

        public string GetSelectString()
        {
            return "SELECT * FROM FolderCustomObjectLinks";
        }

        public IDbEntity GetNullEntity()
        {
            return null;
        }
    }

    public class FolderFolderLink : IDbEntity
    {
        public int ParentFolderID { get; set; }
        public int ChildFolderID { get; set; }


        public string GetTableName()
        {
            return "FolderFolderLinks";
        }

        public string GetSelectString()
        {
            return "SELECT * FROM FolderFolderLinks";
        }

        public IDbEntity GetNullEntity()
        {
            return null;
        }
    }

    public class FolderFileLink : IDbEntity
    {
        public int ParentFolderID { get; set; }
        public int ChildFileID { get; set; }


        public string GetTableName()
        {
            return "FolderFileLinks";
        }

        public string GetSelectString()
        {
            return "SELECT * FROM FolderFileLinks";
        }

        public IDbEntity GetNullEntity()
        {
            return null;
        }
    }

    public class FolderItemLink : IDbEntity
    {
        public int ParentFolderID { get; set; }
        public int ChildItemID { get; set; }


        public string GetTableName()
        {
            return "FolderItemLinks";
        }

        public string GetSelectString()
        {
            return "SELECT * FROM FolderItemLinks";
        }

        public IDbEntity GetNullEntity()
        {
            return null;
        }
    }
}