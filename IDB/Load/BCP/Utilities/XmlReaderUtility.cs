using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using IDB.Load.BCP.IDB.Content;
using log4net;
using log4net.Repository.Hierarchy;

namespace IDB.Load.BCP.Utilities
{
    internal static class XmlReaderUtility
    {
        private static readonly ILog Log = LogManager.GetLogger("IDBLoadBCP");

        private static string _pathIDB = "$/";
        internal static bool parentFolderControl;
        public static long ContentCounter { get; set; }
        public static string PathIDB
        {
            set { _pathIDB = value; }
            get { return _pathIDB; }
        }
        internal static XDocument GetXmlDocument(string path)
        {
            try
            {
                var Document = new XDocument();
                Document = XDocument.Load(path);
                return Document;
            }
            catch (Exception exception) 
            {
                MessageBox.Show(exception.Message);
                Log.Error(exception.Message);
                throw;
            }
        }
        //internal static string GetIDBBehaviors(string node)
        //{
        //    try
        //    {
        //        var document = GetXmlDocument("IDB.Load.BCP.Behaviors.config.xml");
        //        var rootElement = document.Element("Root");
        //        var mainElement = rootElement.Element(node);
        //        return mainElement.Value;
        //    }
        //    catch (System.IO.FileNotFoundException fileNotFoundException) 
        //    {
        //        throw fileNotFoundException;
        //    }

        //}
        //internal static void SetBehavior(string node, string value)
        //{
        //    var document = GetXmlDocument("IDB.Load.BCP.Behaviors.config.xml");
        //    var rootElement = document.Element("Root");
        //    var mainElement = rootElement.Element(node);
        //    mainElement.RemoveAttributes();
        //    mainElement.SetValue(value);
        //    document.Save("IDB.Load.BCP.Behaviors.config.xml");
        //}

        internal static long GetVaultData(string path, BackgroundWorker worker, DoWorkEventArgs e, long counter)
        {
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;

            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return ContentCounter;

            }
            List<string> udpList = new List<string>();
            var vaultElement = MainForm.xmlDocument.FirstChild;
            var rootElement = vaultElement["Root"];
            var folders = rootElement.SelectNodes(@"//Folder");
            foreach (XmlElement folder in folders)
            {
                var foldersUdpElements = folder.SelectNodes(@"//UDP");
                foreach (XmlElement udpElement in foldersUdpElements)
                {
                    udpList.Add("UDP_" + udpElement.GetAttribute("Name"));
                }
            }
            List<string> uniqueFolderUps = udpList.Distinct().ToList();

            foreach (var fileUdp in uniqueFolderUps)
            {

                if (SQLEditor.UdpCheck(fileUdp, "Folders") == 0)
                {
                    SQLEditor.UdpAdder(fileUdp, "Folders");
                }
            }

            var files = rootElement.GetElementsByTagName("File");
            foreach (XmlElement file in files)
            {
                var filesUdpElements = file.GetElementsByTagName("UDP");
                foreach (XmlElement udpElement in filesUdpElements)
                {

                    udpList.Add("UDP_" + udpElement.GetAttribute("Name"));

                }
            }
            List<string> uniqueFileUps = udpList.Distinct().ToList();
            foreach (var fileUdp in uniqueFileUps)
            {
                if (SQLEditor.UdpCheck(fileUdp, "Files") == 0)
                {
                    SQLEditor.UdpAdder(fileUdp, "Files");
                }
            }
            var data = rootElement.ChildNodes;
            int percentComplete;

            foreach (XmlElement element in data)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return ContentCounter;

                }
                 if (element.Name == "File")
                {
                    PathIDB = "$/";
                    FilesInfo fileInfo = new FilesInfo();
                    fileInfo.GetAttributes4FileInsert(element, fileInfo,0);

                    Log.Info(":File was inserted:" + GetProperty(element, "Name"));

                    ContentCounter++;
                    percentComplete = (int)(ContentCounter / counter * 100);
                    worker.ReportProgress(percentComplete);
                }
            }
            foreach (XmlElement element in data)
            {
               // if (parentFolderControl == false) 
               // {
                    parentFolderControl = true;
               // }
                if (element.Name == "Folder")
                {
                    PathIDB = "$";
                    XmlScanner(element, worker, e, counter);

                }
            }
            return ContentCounter;
        }

        private static void XmlScanner(XmlElement pathOfFolder, BackgroundWorker worker, DoWorkEventArgs e, long counter)
        {
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return;

            }
            long parentID=0; 
            PathIDB = PathIDB + "/" + GetProperty(pathOfFolder, "Name");
            try
            {
                SQLEditor.InsertFolder(pathOfFolder);
                parentID = SQLEditor.getFolderId();
                Log.Info(":Folder was inserted:  " + PathIDB);

            }
            catch (SqlException exception)
            {
                Log.Error(exception.Message);
            }
            int percentComplete;
            if (counter == 0)
            {
                percentComplete = 100;
                worker.ReportProgress(percentComplete);
            }

            var childrenItems = pathOfFolder.ChildNodes;
            foreach (XmlElement child in childrenItems)
            {
                if (child.Name == "File")
                {
                    FilesInfo fileInfo = new FilesInfo();
                    fileInfo.GetAttributes4FileInsert(child, fileInfo, parentID);
                    Log.Info(":File was inserted:" + GetProperty(child, "Name"));
                    ContentCounter++;
                    percentComplete = (int)(ContentCounter / counter * 100);
                    worker.ReportProgress(percentComplete);
                }
                if (child.Name == "Folder")
                {
                    XmlScanner(child, worker, e, counter);
                    PathIDB = PathIDB.Remove(PathIDB.Length - GetProperty(child, "Name").Length - 1);
                }
            }




        }
        internal static string GetProperty(XmlNode item, string property)
        {

            if (item.Attributes.GetNamedItem(property) == null)
            {

                return null;
            }

            var itemProperty = item.Attributes.GetNamedItem(property);
            return itemProperty.Value;


        }
        internal static XmlDocument ToXmlDocument(XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using (var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

    }
}

