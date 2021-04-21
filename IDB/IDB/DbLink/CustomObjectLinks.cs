namespace IDB.DbLink
{
    public class CustomObjectCustomObjectLink
    {
        public long ParentCustomObjectID { get; set; }
        public long ChildCustomObjectID { get; set; }
    }

    public class CustomObjectFolderLink
    {
        public long ParentCustomObjectID { get; set; }
        public long ChildFolderID { get; set; }
    }

    public class CustomObjectFileLink
    {
        public long ParentCustomObjectID { get; set; }
        public long ChildFileID { get; set; }
    }

    public class CustomObjectItemLink
    {
        public long ParentCustomObjectID { get; set; }
        public long ChildItemID { get; set; }
    }
}
