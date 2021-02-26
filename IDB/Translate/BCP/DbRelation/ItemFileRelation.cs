namespace IDB.Translate.BCP.DbRelation
{
    public class ItemFileRelation
    {
        public int ItemID { get; set; }
        public int FileID { get; set; }
        public bool IsPrimaryLink { get; set; }
        public bool IsSecondaryLink { get; set; }
        public bool IsStandardComponentLink { get; set; }
        public bool IsDesignDocument { get; set; }
        public bool IsAttachment { get; set; }
    }
}