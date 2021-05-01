using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using IDB.Core.Data.Base;
using IDB.Core.Data.Interface;
using IDB.Core.Extensions;

namespace IDB.Core.Data.Entity
{
    public class Folder : BaseEntityUdp, ICommands
    {
        // ReSharper disable once InconsistentNaming
        public long FolderID { get; set; }
        // ReSharper disable once InconsistentNaming
        public long? ParentFolderID { get; set; }
        public string FolderName { get; set; }
        public string Path { get; set; }
        public bool IsLibrary { get; set; }
        public string Category { get; set; }
        public string LifecycleState { get; set; }
        public string LifecycleDefinition { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }

        public bcpDevKit.Entities.Vault.FolderObject BcpFolderObject { get; set; }


        public Folder()
        {
        }

        public Folder(IDictionary<string, object> dapperRow) : base(dapperRow)
        {
        }

        public void Insert(SqlConnection connection)
        {
            var entity = this.GetDapperEntity<Folder>(nameof(FolderID));
            FolderID = connection.InsertEntity(entity);
        }

        public void Update(SqlConnection connection)
        {
            var entity = this.GetDapperEntity<Folder>(nameof(FolderID));
            connection.UpdateEntity(entity);
        }
    }
}