using System.Data.SqlClient;

namespace MSSql.Vault.Dapper.DirectAccess
{
    public interface IVaultDbEntity
    {
        long GetId();
        string GetSelectString();
        IVaultDbEntity GetNullEntity();
    }

    public class VaultDbEntityHelper
    {
        // template method to return the SELECT string
        public static string GetSelectString<T>() where T : IVaultDbEntity, new()
        {
            // create a new object
            var tempT = new T();
            string selectQuery = tempT.GetSelectString();

            return (selectQuery);
        }
    }

}