namespace IDB.Core.DbLink
{
    public class CustomObjectCustomObjectLink
    {
        // ReSharper disable once InconsistentNaming
        public long ParentCustomObjectID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ChildCustomObjectID { get; set; }
    }

    public class CustomObjectFolderLink
    {
        // ReSharper disable once InconsistentNaming
        public long ParentCustomObjectID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ChildFolderID { get; set; }
    }

    public class CustomObjectFileLink
    {
        // ReSharper disable once InconsistentNaming
        public long ParentCustomObjectID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ChildFileID { get; set; }
    }

    public class CustomObjectItemLink
    {
        // ReSharper disable once InconsistentNaming
        public long ParentCustomObjectID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ChildItemID { get; set; }
    }
}
