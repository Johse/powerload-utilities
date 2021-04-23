using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Xml;
using IDB.Load.BCP.Utilities;
using log4net;

namespace IDB.Load.BCP.IDB.Content
{
    class FilesInfo
    {
        private static readonly ILog Log = LogManager.GetLogger("IDBLoadBCP");

        internal long fileId;
        internal string localFullFilNename;
        internal string fileName;
        internal string category;
        internal string revisionDefinition;
        internal string revisionLabel;
        internal string comment;
        internal string user;
        internal string date;
        internal string isHidden;
        internal string stateDefinition;
        internal string stateName;
        internal string classification;
        internal int version;
        internal string udpPart;
        internal string udpTittle;
        internal string udpDescription;
        internal long parentId;
        internal long childId;
        internal string source;
        internal string resolution;
        internal string refId;
        internal bool isDependency;
        internal bool isAttachement;
        internal string iterationId;
        internal string insertAttributes_Original = "insert into Files(FolderID,Filename,LocalFullFileName,RevisionLabel,Version,Category,LifeCycleState,LifeCycleDefinition,RevisionDefinition,CreateUser,CreateDate,IsHidden,Classification,Comment)";
        internal string insertValues_Original = "values(@FolderId, @FileName, @LocalFullFileName, @RevisionLabel, @Version, @Category, @LifeCycleState, @LifeCycleDefinition, @RevisionDefinition, @CreateUser, @CreateDate, @IsHidden, @Classification, @Comment)";
        internal static List<File> files = new List<File>();
        internal void GetAttributes4FileInsert(XmlElement filePath, FilesInfo filesInfo,long parentFolderId)
        {
            version = 0;
            filesInfo.fileName = XmlReaderUtility.GetProperty(filePath, "Name");
            filesInfo.category = XmlReaderUtility.GetProperty(filePath, "Category");
            filesInfo.classification = XmlReaderUtility.GetProperty(filePath, "Classification");
            filesInfo.isHidden = XmlReaderUtility.GetProperty(filePath, "Hidden");
            var revisionElements = filePath.GetElementsByTagName("Revision");
            foreach (XmlElement revision in revisionElements)
            {
                filesInfo.revisionDefinition = XmlReaderUtility.GetProperty(revision, "Definition");
                filesInfo.revisionLabel = XmlReaderUtility.GetProperty(revision, "Label");
                var iterationElements = revision.GetElementsByTagName("Iteration");
                foreach (XmlElement iteration in iterationElements)
                {
	                filesInfo.version = version + 1;
                    filesInfo.udpPart = null;
                    filesInfo.udpTittle = null;
                    filesInfo.udpDescription = null;
                    filesInfo.iterationId = iteration.GetAttribute("Id");
                    filesInfo.localFullFilNename = XmlReaderUtility.GetProperty(iteration, "LocalPath");
                    filesInfo.comment = XmlReaderUtility.GetProperty(iteration, "Comment");
                    XmlElement created = iteration["Created"];
                    filesInfo.user = XmlReaderUtility.GetProperty(created, "User");
                    filesInfo.date = XmlReaderUtility.GetProperty(created, "Date");
                    XmlElement state = iteration["State"];
                    if (state == null)
                    {
                        filesInfo.stateDefinition = null;
                        filesInfo.stateName = null;
                    }
                    else
                    {
                        filesInfo.stateDefinition = XmlReaderUtility.GetProperty(state, "Definition");
                        filesInfo.stateName = XmlReaderUtility.GetProperty(state, "Name");
                    }

                    var udp = iteration.GetElementsByTagName("UDP");
                    List<FileUdp> udps = new List<FileUdp>();

                    var insertAttributes = filesInfo.insertAttributes_Original;
                    var insertValues = filesInfo.insertValues_Original;

                    foreach (XmlElement udpElement in udp)
                    {
                        if (udpElement.InnerText == null)
                        {
                            continue;
                        }
                        insertAttributes = insertAttributes.Remove(insertAttributes.Length - 1) + ",[" + "UDP_" + udpElement.GetAttribute("Name") + "] )";
                        insertValues = insertValues.Remove(insertValues.Length - 1) + ",'" + udpElement.InnerText.Replace("'", "''") + "')";
                    }
                    try
                    {
                        filesInfo.fileId = SQLEditor.InsertFile(filePath, insertAttributes + " output INSERTED.FileId " + insertValues, filesInfo, parentFolderId);
                        files.Add(new File(filesInfo.fileId, filesInfo.fileName, filesInfo.localFullFilNename, DateTime.Parse(filesInfo.date), filesInfo.iterationId));
                    }
                    catch (SqlException ex)
                    {
                        Log.Error("Error during file inserting with LocalPath: " + filesInfo.localFullFilNename + " ,and IterationId: " + filesInfo.iterationId);//+"("+ex.Message+")");
                    }
                }
            }
        }
    }
}
