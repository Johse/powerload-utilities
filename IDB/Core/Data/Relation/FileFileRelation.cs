using System.Collections.Generic;
using System.Data.SqlClient;
using IDB.Core.Data.Base;
using IDB.Core.Data.Interface;

namespace IDB.Core.Data.Relation
{
    public class FileFileRelation : RelationWithUdp, ICommands
    {
        // ReSharper disable once InconsistentNaming
        public long ParentFileID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long ChildFileID { get; set; }
        public bool IsAttachment { get; set; }
        public bool IsDependency { get; set; }
        public bool NeedsResolution { get; set; }
        public string Source { get; set; }
        public string RefId { get; set; }

        public FileFileRelation()
        {
        }

        public FileFileRelation(IDictionary<string, object> dapperRow) : base(dapperRow)
        {
        }

        public void Insert(SqlConnection connection)
        {
            throw new System.NotImplementedException();
        }

        public void Update(SqlConnection connection)
        {
            throw new System.NotImplementedException();
        }
    }
}