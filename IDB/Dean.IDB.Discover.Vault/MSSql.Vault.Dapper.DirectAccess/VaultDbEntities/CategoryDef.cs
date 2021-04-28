using System;
using System.Data.SqlClient;


namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class CategoryDef : IVaultDbEntity
    {
        public long CategoryDefId { get; set; } // bigint, not null

        public string Name { get; set; } // nvarchar(256), not null

        public string DisplayName { get; set; } // nvarchar(256), not null

        public int Color { get; set; } // int, not null

        public string Description { get; set; } // nvarchar(256), null

        public DateTime LastUpdate { get; set; } // datetime, not null

        public long ParentCategoryDefId { get; set; } // bigint, not null

        public string SystemName { get; set; } // nvarchar(256), not null

        public bool IsSystem { get; set; } // bit, not null

        public Guid rowguid { get; set; } // uniqueidentifier, not null


        public long GetId()
        {
            return CategoryDefId;
        }

        public string GetSelectString()
        {
            return "SELECT CategoryDefId, Name, DisplayName, Color, Description, LastUpdate, ParentCategoryDefId, SystemName, IsSystem FROM dbo.CategoryDef";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }
      
    }
}