using System;
using System.Collections.Generic;
using System.Linq;

namespace MSSql.COIntermDB.Dapper.DirectAccess.DbEntity
{
    public class Folder : IDbEntityWithIDAndUDPs
    {
        public int FolderID { get; set; }
        public int? ParentFolderID { get; set; }
        public string FolderName { get; set; }
        public string Path { get; set; }
        public bool IsLibrary { get; set; }
        public string Category { get; set; }
        public string LifecycleState { get; set; }
        public string LifecycleDefinition { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public Dictionary<string, object> UserDefinedProperties { get; set; }

        // public bcpDevKit.Entities.Vault.FolderObject BcpFolderObject { get; set; }

        public int GetId()
        {
            // NOTE: TODO: if all tables had an EntityId
            // move this into the base class

            return (FolderID);
        }

        public string GetTableName()
        {
            return "Folders";
        }

        public string GetSelectString()
        {
            return "SELECT * FROM Folders";
        }

        public IDbEntity GetNullEntity()
        {
            return null;
        }

        // method to return expected parent folder name from this.Path
        public string GetParentFolderPathFromPath()
        {
            return (GetParentFolderPathFromPath(this.Path));
        }

        // method to return expected parent folder name from this.Path
        public static string GetParentFolderPathFromPath(string vaultFolderPath)
        {
            List<string> folderNames = vaultFolderPath.Split('/').ToList();

            string expectedParentFolderPath = string.Join("/", folderNames.Take(folderNames.Count() - 1));

            return (expectedParentFolderPath);
        }


    }
}