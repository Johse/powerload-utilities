using IDB.Core.Data.Base;

namespace IDB.Core.Data.Link
{
    public class FolderCustomObjectLink : LinkBase
    {
        // ReSharper disable once InconsistentNaming
        public long ParentFolderID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ChildCustomObjectID { get; set; }
    }

    public class FolderFolderLink : LinkBase
    {
        // ReSharper disable once InconsistentNaming
        public long ParentFolderID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ChildFolderID { get; set; }
    }

    public class FolderFileLink : LinkBase
    {
        // ReSharper disable once InconsistentNaming
        public long ParentFolderID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ChildFileID { get; set; }
    }

    public class FolderItemLink : LinkBase
    {
        // ReSharper disable once InconsistentNaming
        public long ParentFolderID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ChildItemID { get; set; }
    }
}