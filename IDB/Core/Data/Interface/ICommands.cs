using System.Data.SqlClient;

namespace IDB.Core.Data.Interface
{
    interface ICommands
    {
        void Insert(SqlConnection connection);
        void Update(SqlConnection connection);
    }
}
