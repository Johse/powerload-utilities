using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Xml;
using IDB.Load.BCP.IDB.Content;
using log4net;

namespace IDB.Load.BCP.Utilities
{
    static class SQLEditor
    {
        private static readonly ILog Log = LogManager.GetLogger("IDBLoadBCP");

        private static int _assocCounter = 0;
        internal static int AssocCounter
        {
            set { _assocCounter = value; }
            get { return _assocCounter; }
        }

        internal static void UdpAdder(string udpName, string udpTable)
        {
            var connectionString = MainForm.InputConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();


                using (var cmd = new SqlCommand("alter Table " + udpTable + " ADD [" + udpName + "] nvarchar(MAX)", connection))
                {
                    cmd.CommandTimeout = 600;
                    cmd.ExecuteScalar();
                }

            }

        }
        internal static int UdpCheck(string udpName, string udpTable)
        {
            var connectionString = MainForm.InputConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {

                connection.Open();
                int count;

                using (var cmd = new SqlCommand("use [" + SQLEditor.getBetween(MainForm.InputConnectionString, "Catalog=", ";") + "] select count(*) from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = '" + udpTable + "' AND COLUMN_NAME = '" + udpName + "'", connection))
                {
                    cmd.CommandTimeout = 600;
                    count = (int)cmd.ExecuteScalar();
                }
                return count;
            }
        }

        internal static void DatabaseConnection(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                }
                catch (System.Data.SqlClient.SqlException connectionError)
                {
                    MessageBox.Show("Connection is failed. Please check your connection string.");
                    throw connectionError;
                }
            }
        }


        private static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }

        private static long ChildIdSelection(string ChildId)//Nach XMLReader transportieren
        {

            foreach (var file in FilesInfo.files)
            {
                if (file.IterationId == ChildId)
                {
                    return file.Id;
                }

            }
            return 0;
        }
        internal static int RelationDataFounder(string path, BackgroundWorker worker, DoWorkEventArgs e, long counter)//Nach XMLReader transportieren
        {

            DateTime date;
            int version;
            int percentComplete = 0;
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            var files = MainForm.xmlDocument.GetElementsByTagName("File");
            foreach (XmlElement file in files)
            {
                if (file.GetElementsByTagName("Association").Count == 0)
                {
                    continue;
                }


                else
                {
                    version = 0;

                    var iterationElements = file.GetElementsByTagName("Iteration");
                    foreach (XmlElement iteration in iterationElements)
                    {
                        FilesInfo fileInfo = new FilesInfo();
                        version++;
                        XmlElement created = iteration["Created"];
                        date = DateTime.Parse(created.GetAttribute("Date"));
                        fileInfo.parentId = ChildIdSelection(iteration.GetAttribute("Id"));
                        if (fileInfo.parentId == 0)
                        {
                            worker.CancelAsync();
                        }
                        foreach (XmlElement association in iteration.GetElementsByTagName("Association"))
                        {
                            if (worker.CancellationPending)
                            {
                                e.Cancel = true;
                                return percentComplete;

                            }

                            fileInfo.childId = ChildIdSelection(association.GetAttribute("ChildId"));
                            if (fileInfo.childId == 0)
                            {
                                Log.Error(":ChildFile with ID " + association.GetAttribute("ChildId") + " was not found ");
                                worker.CancelAsync();
                            }
                            fileInfo.source = XmlReaderUtility.GetProperty(association, "Source");
                            fileInfo.refId = XmlReaderUtility.GetProperty(association, "RefId");
                            fileInfo.resolution = XmlReaderUtility.GetProperty(association, "NeedsResolution");
                            if (fileInfo.resolution == null)
                                fileInfo.resolution = "true";
                            if (XmlReaderUtility.GetProperty(association, "Type") == "Dependency")
                            {
                                fileInfo.isDependency = true;
                                fileInfo.isAttachement = false;
                            }
                            if (XmlReaderUtility.GetProperty(association, "Type") == "Attachment")
                            {
                                fileInfo.isDependency = false;
                                fileInfo.isAttachement = true;
                            }
                            InsertIntoFileFilesRelation(fileInfo);
                            AssocCounter++;
                            percentComplete = (int)((AssocCounter + XmlReaderUtility.ContentCounter) / counter * 100);
                            worker.ReportProgress(percentComplete);
                        }
                    }
                }
            }
            return AssocCounter;
        }

        internal static void InsertIntoFileFilesRelation(FilesInfo fileInfo)
        {
            var sqlExpression = "insert into FileFileRelations(ParentFileID,ChildFileID,IsAttachment,IsDependency,NeedsResolution,Source,RefId)values(@ParentFileID,@ChildFileID,@IsAttachment,@IsDependency,@NeedsResolution,@Source,@RefId)";
            var connectionString = MainForm.InputConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (var cmd = new SqlCommand(sqlExpression, connection))
                    {
                        cmd.CommandTimeout = 600;
                        cmd.Parameters.Add("@ParentFileID", SqlDbType.NVarChar);
                        cmd.Parameters["@ParentFileID"].Value = fileInfo.parentId;
                        cmd.Parameters.Add("@ChildFileID", SqlDbType.BigInt);
                        cmd.Parameters["@ChildFileID"].Value = fileInfo.childId;
                        cmd.Parameters.Add("@IsAttachment", SqlDbType.Bit);
                        cmd.Parameters["@IsAttachment"].Value = fileInfo.isAttachement;
                        cmd.Parameters.Add("@IsDependency", SqlDbType.Bit);
                        cmd.Parameters["@IsDependency"].Value = fileInfo.isDependency;
                        cmd.Parameters.Add("@NeedsResolution", SqlDbType.Bit);
                        cmd.Parameters["@NeedsResolution"].Value = Boolean.Parse(fileInfo.resolution);
                        cmd.Parameters.Add("@Source", SqlDbType.NVarChar);
                        if (fileInfo.source != null)
                        {
                            cmd.Parameters["@Source"].Value = fileInfo.source;
                        }
                        else
                        {
                            cmd.Parameters["@Source"].Value = DBNull.Value;
                        }
                        cmd.Parameters.Add("@RefId", SqlDbType.NVarChar);
                        if (fileInfo.refId != null)
                        {
                            cmd.Parameters["@RefId"].Value = fileInfo.refId;
                        }
                        else
                        {
                            cmd.Parameters["@RefId"].Value = DBNull.Value;
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error during inserting into FileFilerelations.Inserting data : ParentFileID: " + fileInfo.parentId + ", ChildFileID: " + fileInfo.childId);
                    Log.Error("Reason for an error:" + ex.Message);
                }
            }
        }
        internal static long InsertFile(XmlElement filePath, string insertStatement, FilesInfo fileInfo,long parentFolderId)
        {
            var connectionString = MainForm.InputConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                long folderId;
                connection.Open();
                if (XmlReaderUtility.parentFolderControl == false)
                {
                    using (var cmd = new SqlCommand("select FolderId from Folders where Path='$' ", connection))
                    {
                        var cm=cmd.ExecuteScalar();
                        if (cmd.ExecuteScalar() == null)
                        {
                            using (var command = new SqlCommand("insert into Folders(ParentFolderID,FolderName,Path,IsLibrary,Category,LifecycleState,LifecycleDefinition,CreateUser,CreateDate) values (Null,'$','$',0,'Folder',NULL,NULL,'Administrator',GETDATE());select max(FolderId) from Folders;", connection)) 
                            {
                                folderId = (long)command.ExecuteScalar();

                            }
                        }
                        else
                        {
                            folderId = (long)cmd.ExecuteScalar();
                        }
                    }
                }
                else
                {
                    // using (var cmd = new SqlCommand("select max(FolderId) from Folders", connection))
                    // {
                    folderId = parentFolderId; //(int)cmd.ExecuteScalar();
                    //}
                }
                using (var cmd = new SqlCommand(insertStatement, connection))
                {
                    insertStatement = insertStatement + ";SET @newId = SCOPE_IDENTITY();";

                    cmd.CommandTimeout = 600;
                    cmd.Parameters.Add("@folderId", SqlDbType.BigInt);
                    cmd.Parameters["@folderId"].Value = folderId;
                    cmd.Parameters.Add("@LocalFullFileName", SqlDbType.NVarChar);
                    cmd.Parameters["@LocalFullFileName"].Value = fileInfo.localFullFilNename;
                    cmd.Parameters.Add("@FileName", SqlDbType.NVarChar);
                    cmd.Parameters["@FileName"].Value = XmlReaderUtility.GetProperty(filePath, "Name");
                    cmd.Parameters.Add("@Category", SqlDbType.NVarChar);
                    cmd.Parameters["@Category"].Value = XmlReaderUtility.GetProperty(filePath, "Category");
                    cmd.Parameters.Add("@RevisionDefinition", SqlDbType.NVarChar);
                    if (fileInfo.revisionDefinition == null)
                    {
                        cmd.Parameters["@RevisionDefinition"].Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters["@RevisionDefinition"].Value = fileInfo.revisionDefinition;
                    }
                    cmd.Parameters.Add("@Comment", SqlDbType.NVarChar);
                    if (fileInfo.comment == null)
                    {
                        cmd.Parameters["@Comment"].Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters["@Comment"].Value = fileInfo.comment.Replace("'", "''");
                    }

                    cmd.Parameters.Add("@CreateUser", SqlDbType.NVarChar);
                    if (fileInfo.user == null)
                    {
                        cmd.Parameters["@CreateUser"].Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters["@CreateUser"].Value = fileInfo.user;
                    }

                    cmd.Parameters.Add("@CreateDate", SqlDbType.DateTime);
                    if (fileInfo.date == null)
                    {
                        cmd.Parameters["@CreateDate"].Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters["@CreateDate"].Value = fileInfo.date;
                    }
                    if (fileInfo.date == null)
                    {
                        cmd.Parameters["@CreateDate"].Value = DBNull.Value;
                    }
                    cmd.Parameters.Add("@LifeCycleState", SqlDbType.NVarChar);
                    if (fileInfo.stateName == null)
                    {
                        cmd.Parameters["@LifeCycleState"].Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters["@LifeCycleState"].Value = fileInfo.stateName;
                    }

                    cmd.Parameters.Add("@LifeCycleDefinition", SqlDbType.NVarChar);
                    if (fileInfo.stateDefinition == null)
                    {
                        cmd.Parameters["@LifeCycleDefinition"].Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters["@LifeCycleDefinition"].Value = fileInfo.stateDefinition;
                    }
                    cmd.Parameters.Add("@RevisionLabel", SqlDbType.NVarChar);
                    if (fileInfo.revisionLabel == null)
                    {
                        cmd.Parameters["@RevisionLabel"].Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters["@RevisionLabel"].Value = fileInfo.revisionLabel;
                    }
                    cmd.Parameters.Add("@Classification", SqlDbType.NVarChar);
                    if (fileInfo.classification == null)
                    {
                        cmd.Parameters["@Classification"].Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters["@Classification"].Value = fileInfo.classification;
                    }
                    cmd.Parameters.Add("@Version", SqlDbType.NVarChar);
                    cmd.Parameters["@Version"].Value = fileInfo.version;

                    var outParam = cmd.Parameters.Add("@newId", SqlDbType.BigInt);
                    outParam.Direction = ParameterDirection.Output;


                    var id = cmd.ExecuteScalar();
                    return (long)id;
                }
            }

        }
        internal static long getFolderId()
        {
            var connectionString = MainForm.InputConnectionString;
            var replacedName = XmlReaderUtility.PathIDB.Replace("'", "''");
            var sqlExpression = "select FolderId from Folders where Path=N'" + replacedName + "'";
            using (var connection = new SqlConnection(connectionString))
            {

                connection.Open();
                using (var cmd = new SqlCommand(sqlExpression, connection))
                {
                    var id = cmd.ExecuteScalar();
                    return (long)id;
                }
            }
        }
        internal static void InsertFolder(XmlElement folderPath)
        {
            var insertAttributes = "insert into Folders(FolderName,Path,Category,CreateUser,CreateDate,LifeCycleDefinition,LifeCycleState,IsLibrary)";
            var insertValues = "values(@FolderName, @Path, @Category, @CreateUser, @CreateDate, @LifeCycleDefinition, @LifeCycleState, @IsLibrary)";
            var currentFolderUdps = folderPath.ChildNodes;
            foreach (XmlElement folderUdp in currentFolderUdps)
            {
                if (folderUdp.InnerText == null || folderUdp.Name != "UDP")
                {
                    continue;
                }
                else
                {
                    insertAttributes = insertAttributes.Remove(insertAttributes.Length - 1) + ",[" + "UDP_" + folderUdp.GetAttribute("Name") + "] )";
                    insertValues = insertValues.Remove(insertValues.Length - 1) + ",'" + folderUdp.InnerText.Replace("'", "''") + "')";
                }
            }
            var sqlExpression = insertAttributes + insertValues;
            var connectionString = MainForm.InputConnectionString;

            using (var connection = new SqlConnection(connectionString))
            {

                connection.Open();
                using (var cmd = new SqlCommand(sqlExpression, connection))
                {
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.Add("@Path", SqlDbType.NChar);
                    cmd.Parameters["@Path"].Value = XmlReaderUtility.PathIDB;
                    cmd.Parameters.Add("@FolderName", SqlDbType.NChar);
                    cmd.Parameters["@FolderName"].Value = XmlReaderUtility.GetProperty(folderPath, "Name");
                    cmd.Parameters.Add("@Category", SqlDbType.NVarChar);
                    cmd.Parameters["@Category"].Value = XmlReaderUtility.GetProperty(folderPath, "Category");
                    cmd.Parameters.Add("@CreateUser", SqlDbType.NVarChar);
                    cmd.Parameters["@CreateUser"].Value = XmlReaderUtility.GetProperty(folderPath.FirstChild, "User");
                    cmd.Parameters.Add("@CreateDate", SqlDbType.DateTime);
                    cmd.Parameters["@CreateDate"].Value = XmlReaderUtility.GetProperty(folderPath.FirstChild, "Date");
                    cmd.Parameters.Add("@LifeCycleState", SqlDbType.NVarChar);
                    cmd.Parameters.Add("@LifeCycleDefinition", SqlDbType.NVarChar);
                    XmlElement state = folderPath["State"];
                    if (state == null)
                    {
                        cmd.Parameters["@LifeCycleDefinition"].Value = DBNull.Value;
                        cmd.Parameters["@LifeCycleState"].Value = DBNull.Value;
                    }
                    else
                    {
                        cmd.Parameters["@LifeCycleDefinition"].Value = XmlReaderUtility.GetProperty(state, "Definition");
                        cmd.Parameters["@LifeCycleState"].Value = XmlReaderUtility.GetProperty(state, "Name");
                    }
                    cmd.Parameters.Add("@IsLibrary", SqlDbType.Bit);
                    if (XmlReaderUtility.GetProperty(folderPath, "IsLibrary") == null)
                    {
                        cmd.Parameters["@IsLibrary"].Value = bool.Parse("false");
                    }
                    else
                    {

                        cmd.Parameters["@IsLibrary"].Value = bool.Parse(XmlReaderUtility.GetProperty(folderPath, "IsLibrary"));
                    }

                    cmd.Parameters.Add("@UDP_Title", SqlDbType.NVarChar);
                    cmd.Parameters.Add("@UDP_Description", SqlDbType.NVarChar);


                    if (cmd.Parameters["@UDP_Description"].Value == null)
                    {
                        cmd.Parameters["@UDP_Description"].Value = DBNull.Value;
                    }
                    if (cmd.Parameters["@UDP_Title"].Value == null)
                    {
                        cmd.Parameters["@UDP_Title"].Value = DBNull.Value;
                    }

                    cmd.ExecuteScalar();
                }


            }



        }









    }
}