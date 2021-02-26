namespace IDB.Translate.BCP.DbLink
{
    public class FolderCustomObjectLink
    {
        public int ParentFolderID { get; set; }
        public int ChildCustomObjectID { get; set; }
    }

    public class FolderFolderLink
    {
        public int ParentFolderID { get; set; }
        public int ChildFolderID { get; set; }
    }

    public class FolderFileLink
    {
        public int ParentFolderID { get; set; }
        public int ChildFileID { get; set; }
    }

    public class FolderItemLink
    {
        public int ParentFolderID { get; set; }
        public int ChildItemID { get; set; }
    }
}