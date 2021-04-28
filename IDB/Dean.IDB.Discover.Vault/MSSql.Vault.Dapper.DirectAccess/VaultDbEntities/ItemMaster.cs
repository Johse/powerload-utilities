using System;
using System.Data.SqlClient;


namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class ItemMaster : IVaultDbEntity
    {

        public long ItemMasterID { get; set; } // bigint, not null


        public long GetId()
        {
            return ItemMasterID;
        }

        public string GetSelectString()
        {
            return "SELECT ItemMasterID FROM dbo.ItemMaster";
        }

        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }

    }
}
