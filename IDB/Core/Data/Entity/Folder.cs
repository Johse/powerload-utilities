using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
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
            FolderName = "$";
            Path = "$";
            CreateDate = DateTime.Now;
        }

        public Folder(IDictionary<string, object> dapperRow) : base(dapperRow)
        {
        }

        public Folder(Folder parent, VaultBcp.Folder bcpFolder)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            if (bcpFolder == null)
                throw new ArgumentNullException(nameof(bcpFolder));

            FolderName = bcpFolder.Name;
            Path = parent.Path + "/" + bcpFolder.Name;
            IsLibrary = bcpFolder.IsLibrary?.ToUpper() == "TRUE";
            Category = bcpFolder.Category;
            LifecycleState = bcpFolder.State?.Name;
            LifecycleDefinition = bcpFolder.State?.Definition;
            CreateUser = bcpFolder.Created.User;
            CreateDate = bcpFolder.Created.Date;
        }

        public void Insert(SqlConnection connection)
        {
            var entity = this.GetDapperEntity<Folder>(nameof(FolderID));
            FolderID = connection.InsertEntityAndReturnId(entity);
        }

        public void Update(SqlConnection connection)
        {
            var entity = this.GetDapperEntity<Folder>(nameof(FolderID));
            connection.UpdateEntity(entity);
        }

        //public static Folder GetFolderByPath(SqlConnection connection, string path)
        //{
        //    return connection.SelectEntity<Folder>("SELECT * FROM Folders WHERE Path = @Path", new {Path = path});
        //}

        public static IEnumerable<dynamic> GetAllFolders(SqlConnection connection, string sql = @"SELECT * FROM Folders")
        {
            return connection.Query(sql);
        }
    }
}