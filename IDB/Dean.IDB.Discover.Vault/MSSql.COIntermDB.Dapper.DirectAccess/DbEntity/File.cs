using System;
using System.Collections.Generic;
using Dapper;


namespace MSSql.COIntermDB.Dapper.DirectAccess.DbEntity
{
    public class File : IDbEntityWithIDAndUDPs
    {
        public string LocalFullFileName { get; set; }
        public int FileID { get; set; }
        public int FolderID { get; set; }
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

        // properties for storing local file information
        public DateTime LocalFileProcessedDate { get; set; }
        public bool LocalFileExists { get; set; }
        public int LocalFileCheckSum { get; set; }


        // properties for storing Vault file information
        // meaning, the record is already in the Vault as a file
        // but may be different than that found in the Vault
        // !!!WARNING!!!
        // for now, we are only able to process a situation where there is 
        // only one file in the target Vault
        // and only one Delta file being introduced in the current load

        public DateTime EntityCreateDate { get; set; }
        public bool VaultFileExists { get; set; }
        public int VaultFileCheckSum { get; set; }


        public Dictionary<string, object> UserDefinedProperties { get; set; }

        // public bcpDevKit.Entities.Vault.FileIteration BcpFileIteration { get; set; }

        public int GetId()
        {
            // NOTE: TODO: if all tables had an EntityId
            // move this into the base class

            return (FileID);
        }

        public string GetTableName()
        {
            return "Files";
        }

        public string GetSelectString()
        {
            return "SELECT * FROM Files";
        }


        public IDbEntity GetNullEntity()
        {
            return null;
        }

        // method to update the local file Checksum statistics in the database
        public bool UpdateDBLocalFileChecksumStatistics(MSSqlCacheLoader cacheLoader)
        {
            bool bSuccess = false;

            // create the query string
            string sQueryString = @"UPDATE Files SET LocalFileProcessedDate = @LocalFileProcessedDate, LocalFileExists = @LocalFileExists, LocalFileCheckSum = @LocalFileCheckSum ";
            sQueryString += @"WHERE FileID = @FileID";

            // prepare the parameters
            var param = new DynamicParameters();
            param.Add("@LocalFileProcessedDate", this.LocalFileProcessedDate);
            param.Add("@LocalFileExists", this.LocalFileExists);
            param.Add("@LocalFileCheckSum", this.LocalFileCheckSum);
            param.Add("@FileID", this.FileID);

            // run the query to update the properties
            int nUpdated = cacheLoader.UpdateDbRecord(sQueryString, param);

            if (nUpdated > 0)
            {
                bSuccess = true;
            }

            return (bSuccess);
        }

        // method to update the Vault file Checksum statistics in the database
        public bool UpdateDBVaultFileChecksumStatistics(MSSqlCacheLoader cacheLoader)
        {
            bool bSuccess = false;

            // Date cannot be older than "1/1/1753 12:00:00 AM" in SQL
            if (this.EntityCreateDate == DateTime.MinValue)
            {
                this.EntityCreateDate = new DateTime(1900, 1, 1);
            }

            // create the query string
                string sQueryString = @"UPDATE Files SET EntityCreateDate = @EntityCreateDate, VaultFileExists = @VaultFileExists, VaultFileCheckSum = @VaultFileCheckSum ";
            sQueryString += @"WHERE FileID = @FileID";

            // prepare the parameters
            var param = new DynamicParameters();
            param.Add("@EntityCreateDate", this.EntityCreateDate);
            param.Add("@VaultFileExists", this.VaultFileExists);
            param.Add("@VaultFileCheckSum", this.VaultFileCheckSum);
            param.Add("@FileID", this.FileID);

            // run the query to update the properties
            int nUpdated = cacheLoader.UpdateDbRecord(sQueryString, param);

            if (nUpdated > 0)
            {
                bSuccess = true;
            }

            return (bSuccess);
        }

    }
}
