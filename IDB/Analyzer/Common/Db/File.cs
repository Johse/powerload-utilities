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

        [XmlElement]
        public string IDBAnalyzeNotes { get; set; }

        // actually stored as [UDP_Application Version] in the database with a space between "Application" and "Version"
        // should be able to read with Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        [XmlElement]
        public string UDP_Application_Version { get; set; }

        // property to identify if this has been processed
        // during IDB.Analyze.AutoCAD and/or IDB.Analyze.Inventor
        // currently not stored in the IDB, only used during runtime processing
        [XmlElement]
        public bool IDBAnalyzed { get; set; }

        // property to identify that this was found to have relationships issues
        // during IDB.Analyze.AutoCAD and/or IDB.Analyze.Inventor
        // currently not stored in the IDB, only used during runtime processing
        [XmlElement]
        public bool HasRelationshipIssues { get; set; }
    }
}