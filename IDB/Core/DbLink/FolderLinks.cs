namespace IDB.Core.DbLink
{
    public class FolderCustomObjectLink
    {
        // ReSharper disable once InconsistentNaming
        public long ParentFolderID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ChildCustomObjectID { get; set; }
    }

    public class FolderFolderLink
    {
        // ReSharper disable once InconsistentNaming
        public long ParentFolderID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ChildFolderID { get; set; }
    }

    public class FolderFileLink
    {
        // ReSharper disable once InconsistentNaming
        public long ParentFolderID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ChildFileID { get; set; }
    }

    public class FolderItemLink
    {
        // ReSharper disable once InconsistentNaming
        public long ParentFolderID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ChildItemID { get; set; }
    }
}