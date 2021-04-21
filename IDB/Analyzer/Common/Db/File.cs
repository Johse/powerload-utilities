using System.Xml.Serialization;

namespace IDB.Analyzer.Common.Db
{
    public class File : IDbEntity
    {
        [XmlElement]
        public string LocalFullFileName { get; set; }
        [XmlElement]
        public long FileID { get; set; }
        [XmlElement]
        public string FileName { get; set; }
    }
}