using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDB.Translate.BCP.DbLink
{
    public class CustomObjectCustomObjectLink
    {
        public int ParentCustomObjectID { get; set; }
        public int ChildCustomObjectID { get; set; }
    }

    public class CustomObjectFolderLink
    {
        public int ParentCustomObjectID { get; set; }
        public int ChildFolderID { get; set; }
    }

    public class CustomObjectFileLink
    {
        public int ParentCustomObjectID { get; set; }
        public int ChildFileID { get; set; }
    }

    public class CustomObjectItemLink
    {
        public int ParentCustomObjectID { get; set; }
        public int ChildItemID { get; set; }
    }
}
