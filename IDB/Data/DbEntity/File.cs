using System;
using System.Collections.Generic;

namespace IDB.Data.DbEntity
{
    public class File : IDbEntity
    {
        public string LocalFullFileName { get; set; }
        public long FileID { get; set; }
        public long FolderID { get; set; }
        public string FileName { get; set; }
        public string Category { get; set; }
        public string Classification { get; set; }
        public string RevisionLabel { get; set; }
        public string RevisionDefinition { get; set; }
        public int Version { get; set; }
        public string LifecycleState { get; set; }
        public string LifecycleDefinition { get; set; }
        public string Comment { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsHidden { get; set; }
		public bool IsExcluded { get; set; }
		public Dictionary<string, object> UserDefinedProperties { get; set; }
        public bcpDevKit.Entities.Vault.FileIteration BcpFileIteration { get; set; }
    }
}