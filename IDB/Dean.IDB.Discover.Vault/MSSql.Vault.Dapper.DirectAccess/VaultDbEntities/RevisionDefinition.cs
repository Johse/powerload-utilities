using System;
using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class RevisionDefinition : IVaultDbEntity
    {
        public long RevisionDefinitionID { get; set; } // bigint, not null

        public string Name { get; set; } // nvarchar(256), not null

        public string DisplayName { get; set; } // nvarchar(256), not null

        public string Description { get; set; } // nvarchar(256), null

        public long PrimarySeqSchemeID { get; set; } // bigint, not null

        public long SecondarySeqSchemeID { get; set; } // bigint, not null

        public long TertiarySeqSchemeID { get; set; } // bigint, not null

        public string Separator { get; set; } // nchar(1), not null

        public DateTime LastUpdate { get; set; } // datetime, not null

        public bool IsSystem { get; set; } // bit, not null

        public string SystemName { get; set; } // nvarchar(256), not null

        public Guid rowguid { get; set; } // uniqueidentifier, not null



        public long GetId()
        {
            return RevisionDefinitionID;
        }

        public string GetSelectString()
        {
            return "SELECT RevisionDefinitionID, Name, DisplayName, Description, PrimarySeqSchemeID, SecondarySeqSchemeID, TertiarySeqSchemeID, Separator, LastUpdate, IsSystem, SystemName FROM dbo.RevisionDefinition";
        }
        
        public IVaultDbEntity GetNullEntity()
        {
            var nullEntity = new RevisionDefinition();
            

            return nullEntity;
        }
    }
}