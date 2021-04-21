namespace IDB.Data.DbRelation
{
    public class ItemFileRelation
    {
        public long ItemID { get; set; }
        public long FileID { get; set; }
        public bool IsPrimaryLink { get; set; }
        public bool IsSecondaryLink { get; set; }
        public bool IsStandardComponentLink { get; set; }
        public bool IsDesignDocument { get; set; }
        public bool IsAttachment { get; set; }
    }
}