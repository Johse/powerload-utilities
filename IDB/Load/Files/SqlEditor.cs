using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace IDB.Load.Files
{
    internal class SqlEditor
    {
        public static void InsertFolder(string fullFolderPath, string basePathReplacement)
        {
            var sqlExpression =
                "insert into Folders(FolderName,Path,Category,CreateUser,CreateDate)values(@FolderName,@Path,@Category,@CreateUser,@CreateDate)";

            var connectionString = XmlConfiguration.GetBehavior("ConnectionString");
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SqlCommand(sqlExpression, connection))
                {
                    cmd.Parameters.Add("@Path", SqlDbType.NChar);
                    cmd.Parameters["@Path"].Value = fullFolderPath.Replace(basePathReplacement, "$/").Replace(@"\", "/").Replace("//", "/");
                    cmd.Parameters.Add("@FolderName", SqlDbType.NChar);
                    cmd.Parameters["@FolderName"].Value = new DirectoryInfo(fullFolderPath).Name;
                    cmd.Parameters.Add("@Category", SqlDbType.NVarChar);
                    cmd.Parameters["@Category"].Value = XmlConfiguration.GetBehavior("Folders", "Category");
                    cmd.Parameters.Add("@CreateUser", SqlDbType.NVarChar);
                    cmd.Parameters["@CreateUser"].Value = XmlConfiguration.GetBehavior("Folders", "CreateUser");
                    cmd.Parameters.Add("@CreateDate", SqlDbType.DateTime);
                    cmd.Parameters["@CreateDate"].Value = Directory.GetCreationTime(fullFolderPath);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void InsertFile(string fullFileName)
        {
            var sqlExpression = 
                "insert into Files(FolderID,Filename,LocalFullFileName,RevisionLabel,Version,Category,LifeCycleState,LifeCycleDefinition,RevisionDefinition,CreateUser,CreateDate,Classification)values(@FolderId,@FileName,@LocalFullFileName,@RevisionLabel,@Version,@Category,@LifeCycleState,@LifeCycleDefinition,@RevisionDefinition,@CreateUser,@CreateDate,@Classification)";

            var connectionString = XmlConfiguration.GetBehavior("ConnectionString");
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                int folderId;
                using (var cmd = new SqlCommand("select max(FolderId) from Folders", connection))
                {
                    folderId = (int)cmd.ExecuteScalar();
                }

                using (var cmd = new SqlCommand(sqlExpression, connection))
                {
                    cmd.Parameters.Add("@folderId", SqlDbType.BigInt);
                    cmd.Parameters["@folderId"].Value = folderId;
                    cmd.Parameters.Add("@LocalFullFileName", SqlDbType.NVarChar);
                    cmd.Parameters["@LocalFullFileName"].Value = fullFileName;
                    cmd.Parameters.Add("@FileName", SqlDbType.NVarChar);
                    cmd.Parameters["@FileName"].Value = new DirectoryInfo(fullFileName).Name;
                    cmd.Parameters.Add("@Category", SqlDbType.NVarChar);
                    cmd.Parameters["@Category"].Value = XmlConfiguration.GetBehavior("Files", "Category");
                    cmd.Parameters.Add("@RevisionDefinition", SqlDbType.NVarChar);
                    cmd.Parameters["@RevisionDefinition"].Value = XmlConfiguration.GetBehavior("Files", "RevisionDefinition");
                    cmd.Parameters.Add("@LifeCycleState", SqlDbType.NVarChar);
                    cmd.Parameters["@LifeCycleState"].Value = XmlConfiguration.GetBehavior("Files", "LifeCycleState");
                    cmd.Parameters.Add("@LifeCycleDefinition", SqlDbType.NVarChar);
                    cmd.Parameters["@LifeCycleDefinition"].Value = XmlConfiguration.GetBehavior("Files", "LifeCycleDefinition");
                    cmd.Parameters.Add("@CreateUser", SqlDbType.NVarChar);
                    cmd.Parameters["@CreateUser"].Value = XmlConfiguration.GetBehavior("Files", "CreateUser");
                    cmd.Parameters.Add("@CreateDate", SqlDbType.DateTime);
                    cmd.Parameters["@CreateDate"].Value = Directory.GetCreationTime(fullFileName);
                    cmd.Parameters.Add("@RevisionLabel", SqlDbType.NVarChar);
                    cmd.Parameters["@RevisionLabel"].Value = XmlConfiguration.GetBehavior("Files", "RevisionLabel");
                    cmd.Parameters.Add("@Classification", SqlDbType.NVarChar);
                    cmd.Parameters["@Classification"].Value = XmlConfiguration.GetBehavior("Files", "Classification");
                    cmd.Parameters.Add("@Version", SqlDbType.NVarChar);
                    cmd.Parameters["@Version"].Value = "001";

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}