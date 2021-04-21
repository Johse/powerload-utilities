using System.Collections.Generic;
using System.Data.SqlClient;
using IDB.Load.BCP.Utilities;

namespace IDB.Load.BCP
{
    class IDBValidator
    {
        internal void AnnotationFounder(string connectionString, string tableName)
        {
            var sqlExpression = "";
            switch (tableName)
            {
                case "Files":
                    sqlExpression = @"select FileID  from " + tableName + " where Validation_Comment IS NOT NULL";
                    break;
                case "Folders":
                    sqlExpression = @"select FolderID  from " + tableName + " where Validation_Comment IS NOT NULL";
                    break;
                case "FileFileRelations":
                    sqlExpression = @"select ParentFileID,ChildFileID  from " + tableName + " where Validation_Comment IS NOT NULL";
                    break;
            }
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var cmd = new SqlCommand(sqlExpression, connection))
                {
                    cmd.CommandTimeout = 60;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<int> ids = new List<int>();
                    while (reader.Read())
                    {
                        if (tableName == "FileFileRelations")
                        {
                            Logger.Log.Debug("Field in the table " + tableName + " with ParentFileID " + (int)reader.GetValue(0) + " and ChildFileID " + (int)reader.GetValue(1) + " is incorrect,details are in column Validation_Comment in the table " + tableName);
                        }
                        else
                        {
                            Logger.Log.Debug("Field in the table " + tableName + " with ID " + (int)reader.GetValue(0) + " is incorrect,details are in column Validation_Comment in the table " + tableName);
                        }
                    }
                }

            }

        }

        internal int AnnotationCounter(string connectionString, string tableName)
        {
            var sqlExpression = @"select count(*) from " + tableName + " where Validation_Comment IS NOT NULL";
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var cmd = new SqlCommand(sqlExpression, connection))
                {
                    cmd.CommandTimeout = 60;
                    var count = (int)cmd.ExecuteScalar();
                    if (count != 0) 
                    {
                        AnnotationFounder(connectionString, tableName);
                    }
                    return count;
                }

            }

        }


        internal void Validator(string connectionString, string sqlExpression)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var cmd = new SqlCommand(sqlExpression, connection))
                {
                    cmd.CommandTimeout = 60;
                    cmd.ExecuteScalar();
                }

            }
        }
    }
}
