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

        // Don't use [UDP_Application Version] as it will populate that in the Vault.xml file
        // and will cause a property compliance issue in Vault once data is loaded
        // we are using this property only to report information to the customer
        // about the histogram of application versions
        // should be able to read with Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true; -- potential fix for properties with spaces
        [XmlElement]
        public string ApplicationVersion { get; set; }

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