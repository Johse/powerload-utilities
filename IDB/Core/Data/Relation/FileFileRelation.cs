using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using IDB.Core.Data.Base;
using IDB.Core.Data.Interface;
using IDB.Core.Extensions;

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

        public FileFileRelation(long parentFileId, long childFileId, bool isAttachment, bool isDependency, string source, string refId)
        {
            ParentFileID = parentFileId;
            ChildFileID = childFileId;
            IsAttachment = isAttachment;
            IsDependency = isDependency;
            NeedsResolution = isDependency;
            Source = source;
            RefId = refId;
        }

        public void Insert(SqlConnection connection)
        {
            var entity = this.GetDapperEntity<FileFileRelation>(new[] { nameof(ParentFileID), nameof(ChildFileID) });
            connection.InsertEntity(entity);
        }

        public void Update(SqlConnection connection)
        {
            var entity = this.GetDapperEntity<FileFileRelation>(new[] {nameof(ParentFileID), nameof(ChildFileID)});
            connection.UpdateEntity(entity);
        }

        public static IEnumerable<dynamic> GetAllFileFileRelations(SqlConnection connection, string sql = @"SELECT * FROM FileFileRelations")
        {
            return connection.Query(sql);
        }
    }
}