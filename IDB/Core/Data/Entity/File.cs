using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using IDB.Core.Data.Base;
using IDB.Core.Data.Interface;
using IDB.Core.Extensions;

namespace IDB.Core.Data.Entity
{
    public class File : BaseEntityUdp, ICommands
    {
        public string LocalFullFileName { get; set; }
        public int LocalFileChecksum { get; set; }
        // ReSharper disable once InconsistentNaming
        public long FileID { get; set; }
        // ReSharper disable once InconsistentNaming
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
        public int? VaultChecksum { get; set; }
        public DateTime? VaultCreateDate { get; set; }

        public bcpDevKit.Entities.Vault.FileIteration BcpFileIteration { get; set; }

        public File()
        {
        }

        public File(IDictionary<string, object> dapperRow) : base(dapperRow)
        {
        }

        public void Insert(SqlConnection connection)
        {
            var entity = this.GetDapperEntity<File>(nameof(FileID));
            FileID = connection.InsertEntity(entity);
        }

        public void Update(SqlConnection connection)
        {
            var entity = this.GetDapperEntity<File>(nameof(FileID));
            connection.UpdateEntity(entity);
        }
    }
}