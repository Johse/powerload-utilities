using System.Xml.Serialization;

namespace IDB.Analyzer.Common.Db
{
    public class FileFileRelation
    {
        [XmlElement]
        public long ParentFileID { get; set; }
        [XmlElement]
        public long ChildFileID { get; set; }
        [XmlElement]
        public bool IsAttachment { get; set; }
        [XmlElement]
        public bool IsDependency { get; set; }
        [XmlElement]
        public bool NeedsResolution { get; set; }
        [XmlElement]
        public string Source { get; set; }
        [XmlElement]
        public string RefId { get; set; }
        [XmlElement]
        public string IDBAnalyzeNotes { get; set; }

        // property to identify if this has been processed
        // during IDB.Analyze.AutoCAD and/or IDB.Analyze.Inventor
        // currently not stored in the IDB, only used during runtime processing
        [XmlElement]
        public bool IDBAnalyzed { get; set; }


    }
}