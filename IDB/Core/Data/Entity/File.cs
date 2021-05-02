using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
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
        public string ContentSource { get; set; }
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

        public File(Folder folder, VaultBcp.File bcpFile, VaultBcp.File.Revision bcpRevision, VaultBcp.File.Revision.Iteration bcpIteration, int version)
        {
            LocalFullFileName = bcpIteration.LocalPath;
            FolderID = folder.FolderID;
            FileName = bcpFile.Name;
            Category = bcpFile.Category;
            Classification = bcpFile.Classification;
            RevisionLabel = bcpRevision.Label;
            RevisionDefinition = bcpRevision.Definition;
            Version = version;
            LifecycleState = bcpIteration.State?.Name;
            LifecycleDefinition = bcpIteration.State?.Definition;
            Comment = bcpIteration.Comment;
            CreateUser = bcpIteration.Created.User;
            CreateDate = bcpIteration.Created.Date;
            ContentSource = bcpIteration.ContentSource;
            IsHidden = bcpFile.Hidden?.ToUpper() == "TRUE";
            Tag = bcpIteration.Id;

            foreach (var bcpUdp in bcpIteration.UDPs)
                UserDefinedProperties.Add(bcpUdp.Name, bcpUdp.Value);

            if (System.IO.File.Exists(LocalFullFileName))
                LocalFileChecksum = Core.File.Checksum.CalcChecksum(LocalFullFileName);
        }

        public void Insert(SqlConnection connection)
        {
            var entity = this.GetDapperEntity<File>(nameof(FileID));
            FileID = connection.InsertEntityAndReturnId(entity);
        }

        public void Update(SqlConnection connection)
        {
            var entity = this.GetDapperEntity<File>(nameof(FileID));
            connection.UpdateEntity(entity);
        }

        public static IEnumerable<dynamic> GetAllFiles(SqlConnection connection, string sql = @"SELECT * FROM Files")
        {
            return connection.Query(sql);
        }
    }
}