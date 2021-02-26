namespace IDB.Translate.BCP.DbRelation
{
    public class FileFileRelation
    {
        public int ParentFileID { get; set; }
        public int ChildFileID { get; set; }
        public bool IsAttachment { get; set; }
        public bool IsDependency { get; set; }
        public bool NeedsResolution { get; set; }
        public string Source { get; set; }
        public string RefId { get; set; }
    }
}