using System.Xml.Serialization;

namespace IDB.Analyzer.Common.Db
{
    public class File : IDbEntity
    {
        [XmlElement]
        public string LocalFullFileName { get; set; }
        [XmlElement]
        public int FileID { get; set; }
        [XmlElement]
        public string FileName { get; set; }
    }
}