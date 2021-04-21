namespace IDB.DbLink
{
    public class FolderCustomObjectLink
    {
        public long ParentFolderID { get; set; }
        public long ChildCustomObjectID { get; set; }
    }

    public class FolderFolderLink
    {
        public long ParentFolderID { get; set; }
        public long ChildFolderID { get; set; }
    }

    public class FolderFileLink
    {
        public long ParentFolderID { get; set; }
        public long ChildFileID { get; set; }
    }

    public class FolderItemLink
    {
        public long ParentFolderID { get; set; }
        public long ChildItemID { get; set; }
    }
}