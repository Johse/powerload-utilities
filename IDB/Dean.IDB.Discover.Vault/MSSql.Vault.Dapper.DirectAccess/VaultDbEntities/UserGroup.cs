using System;
using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess.VaultDbEntities
{
    public class UserGroup : IVaultDbEntity
    {
        public long UserGroupID { get; set; } // bigint, not null

        public string UserGroupName { get; set; } // nvarchar(255), not null

        public string SystemName { get; set; } // nvarchar(50), null

        public string UserPassword { get; set; } // nvarchar(50), null

        public string Salt { get; set; } // nvarchar(20), null

        public byte IsGroup { get; set; } // tinyint, not null

        public bool SuperUser { get; set; } // bit, not null

        public string FirstName { get; set; } // nvarchar(50), null

        public string LastName { get; set; } // nvarchar(100), null

        public string Email { get; set; } // nvarchar(100), null

        public DateTime CreateDate { get; set; } // datetime, not null

        public long CreateUserID { get; set; } // bigint, not null

        public int AuthType { get; set; } // int, not null

        public bool Active { get; set; } // bit, not null

        public Guid rowguid { get; set; } // uniqueidentifier, not null



        public long GetId()
        {
            return UserGroupID;
        }

        public string GetSelectString()
        {
            return "SELECT UserGroupID, UserGroupName, SystemName, UserPassword, Salt, IsGroup, SuperUser, FirstName, LastName, Email, CreateDate, CreateUserID, AuthType, Active FROM dbo.UserGroup";
        }


        public IVaultDbEntity GetNullEntity()
        {
            return null;
        }
    }
}
