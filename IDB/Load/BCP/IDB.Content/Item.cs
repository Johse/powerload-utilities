using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using IDB.Load.BCP.Utilities;

namespace IDB.Load.BCP.IDB.Content
{
    class Item
    {
        private static int _count;
        internal static int Count
        {
            set { _count = value; }
            get { return _count; }
        }
        private static XmlDocument _itemXmlDocument;
        internal static XmlDocument itemXmlDocument
        {
            set { _itemXmlDocument = value; }
            get { return _itemXmlDocument; }
        }
        internal static XmlNodeList itemsCollection;
        internal string catagory;
        internal string itemNumber;
        internal string revisionLabel;
        internal string revisionDefinition;
        internal int version;
        internal string lifecycleState;
        internal string lifecycleDefinition;
        internal string comment;
        internal string createUser;
        internal string createDate;
        internal string title;
        internal string unit;
        internal string description;
        internal string bomStructure;
        internal static List<ItemItemRelations> item4Relations = new List<ItemItemRelations>();


        internal static int GetItemAtrrributes(string path, BackgroundWorker worker, DoWorkEventArgs e)
        {
            int percentComplete;
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            if (worker.CancellationPending)
            {
                e.Cancel = true; return Count;

            }
            List<ItemUdp> itemUdps = new List<ItemUdp>();
            foreach (XmlElement element in itemsCollection)
            {
                Count++;
                Item item = new Item();
                item.catagory = element.GetAttribute("category");
                var masterId = element.GetAttribute("ItemMasterID");
                item.itemNumber = element["ItemNumber"].GetAttribute("ItemNumber");
                var revisions = element.GetElementsByTagName("Rev");
                item.version = 0;
                foreach (XmlElement revision in revisions)
                {
                    item.revisionLabel = revision.GetAttribute("Label");
                    item.revisionDefinition = revision.GetAttribute("DefName");
                    var iterations = revision.GetElementsByTagName("Iteration");

                    foreach (XmlElement iteration in iterations)
                    {
                        item.version++;
                        item.comment = iteration.GetAttribute("Comment");
                        item.createUser = iteration.GetAttribute("UserGroupName");
                        item.createDate = iteration.GetAttribute("CreateDate");
                        item.title = iteration.GetAttribute("ItemTitle");
                        item.unit = iteration.GetAttribute("Units");
                        item.description = iteration.GetAttribute("Description");
                        item.bomStructure = iteration.GetAttribute("BOMStructure");
                        var state = iteration["State"];
                        item.lifecycleState = state.GetAttribute("Name");
                        item.lifecycleDefinition = state.GetAttribute("Definition");
                        var insertAttributes = "insert into Items (ItemNumber,Category,RevisionLabel,RevisionDefinition,Version,LifecycleState,LifecycleDefinition,Comment,CreateUser,CreateDate,Title,Unit,Description,BomStructure)";
                        var insertValues = "values('" + item.itemNumber + "' ,'" + item.catagory + "' ,'" + item.revisionLabel + "', '" + item.revisionDefinition + "', " + item.version.ToString() + " ,'" + item.lifecycleState + "' ,'" + item.lifecycleDefinition + "', " + "'" + item.comment.Replace("'", "''") + "', '" + item.createUser + "', '" + item.createDate + "', '" + item.title + "' ,'" + item.unit + "', '" + item.description + "', '" + item.bomStructure + "')";
                        var udps = iteration.GetElementsByTagName("UDP");
                        foreach (XmlElement udpElement in udps)
                        {
                            if (worker.CancellationPending)
                            {
                                e.Cancel = true;
                                return Count;

                            }

                            if (udpElement.InnerText == null)
                            {
                                continue;
                            }


                            if (SQLEditor.UdpCheck("UDP_" + udpElement.GetAttribute("Name"), "Items") == 0)
                            {
                                SQLEditor.UdpAdder("UDP_" + udpElement.GetAttribute("Name"), "Items");
                            }
                            insertAttributes = insertAttributes.Remove(insertAttributes.Length - 1) + ",[" + "UDP_" + udpElement.GetAttribute("Name") + "] )";
                            insertValues = insertValues.Remove(insertValues.Length - 1) + ",'" + udpElement.InnerText.Replace("'", "''") + "')";

                        }
                        var dbItemId = item.InsertItem(insertAttributes + " output INSERTED.ItemID " + insertValues);
                        ItemItemRelations itemRelation = new ItemItemRelations();
                        itemRelation.id = masterId;
                        itemRelation.idDB = dbItemId;
                        itemRelation.unit = item.unit;
                        itemRelation.iterationID = Int32.Parse(iteration.GetAttribute("IterationID").Remove(0,1) );
                        Item.item4Relations.Add(itemRelation);
                    }
                }

                if (DataScanner.Counter == 0)
                {
                    percentComplete = 0;
                }
                else
                {
                    percentComplete = (int)(Count / DataScanner.Counter * 100);
                }


                worker.ReportProgress(percentComplete);
            }
            return Count;

        }
        private  int InsertItem(string sqlExpression)
        {
            var connectionString = DataScanner.InputConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SqlCommand(sqlExpression, connection))
                {
                    sqlExpression = sqlExpression + ";SET @newId = SCOPE_IDENTITY();";
                    SqlParameter outParam = cmd.Parameters.Add("@newId", SqlDbType.Int);
                    outParam.Direction = ParameterDirection.Output;


                    return (int)cmd.ExecuteScalar();
                }
            }
        }
    }


}
