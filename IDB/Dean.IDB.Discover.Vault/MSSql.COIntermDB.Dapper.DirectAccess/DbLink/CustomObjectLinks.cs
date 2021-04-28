using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSSql.COIntermDB.Dapper.DirectAccess.DbEntity;

namespace MSSql.COIntermDB.Dapper.DirectAccess.DbLink
{
    public class CustomObjectCustomObjectLink : IDbEntity
    {
        public int ParentCustomObjectID { get; set; }
        public int ChildCustomObjectID { get; set; }

        public string GetTableName()
        {
            return "CustomObjectCustomObjectLinks";
        }

        public string GetSelectString()
        {
            return "SELECT * FROM CustomObjectCustomObjectLinks";
        }

        public IDbEntity GetNullEntity()
        {
            return null;
        }
    }

    public class CustomObjectFolderLink : IDbEntity
    {
        public int ParentCustomObjectID { get; set; }
        public int ChildFolderID { get; set; }

        public string GetTableName()
        {
            return "CustomObjectFolderLinks";
        }

        public string GetSelectString()
        {
            return "SELECT * FROM CustomObjectFolderLinks";
        }

        public IDbEntity GetNullEntity()
        {
            return null;
        }

    }

    public class CustomObjectFileLink : IDbEntity
    {
        public int ParentCustomObjectID { get; set; }
        public int ChildFileID { get; set; }

        public string GetTableName()
        {
            return "CustomObjectFileLinks";
        }

        public string GetSelectString()
        {
            return "SELECT * FROM CustomObjectFileLinks";
        }

        public IDbEntity GetNullEntity()
        {
            return null;
        }

    }

    public class CustomObjectItemLink : IDbEntity
    {
        public int ParentCustomObjectID { get; set; }
        public int ChildItemID { get; set; }

        public string GetTableName()
        {
            return "CustomObjectItemLinks";
        }

        public string GetSelectString()
        {
            return "SELECT * FROM CustomObjectItemLinks";
        }

        public IDbEntity GetNullEntity()
        {
            return null;
        }
    }
}
