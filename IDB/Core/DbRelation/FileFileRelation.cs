namespace IDB.Core.DbRelation
{
    public class FileFileRelation
    {
        public long ParentFileID { get; set; }
        public long ChildFileID { get; set; }
        public bool IsAttachment { get; set; }
        public bool IsDependency { get; set; }
        public bool NeedsResolution { get; set; }
        public string Source { get; set; }
        public string RefId { get; set; }
    }
}