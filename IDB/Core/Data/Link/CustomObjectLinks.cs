using IDB.Core.Data.Base;

namespace IDB.Core.Data.Link
{
    public class CustomObjectCustomObjectLink : LinkBase
    {
        // ReSharper disable once InconsistentNaming
        public long ParentCustomObjectID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ChildCustomObjectID { get; set; }
    }

    public class CustomObjectFolderLink : LinkBase
    {
        // ReSharper disable once InconsistentNaming
        public long ParentCustomObjectID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ChildFolderID { get; set; }
    }

    public class CustomObjectFileLink : LinkBase
    {
        // ReSharper disable once InconsistentNaming
        public long ParentCustomObjectID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ChildFileID { get; set; }
    }

    public class CustomObjectItemLink : LinkBase
    {
        // ReSharper disable once InconsistentNaming
        public long ParentCustomObjectID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ChildItemID { get; set; }
    }
}
