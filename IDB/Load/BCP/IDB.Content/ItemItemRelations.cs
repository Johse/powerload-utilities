using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Xml;
using log4net;

namespace IDB.Load.BCP.IDB.Content
{
    class ItemItemRelations
    {
        private static readonly ILog Log = LogManager.GetLogger("IDBLoadBCP");

        internal long parentId;
        internal long childId;
        internal string id;
        internal long idDB;
        internal string unit;
        internal long iterationID;
        internal static int count;

        internal int RelationDataFounder(BackgroundWorker worker, DoWorkEventArgs e)
        {
            int percentComplete;
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            var items = Item.itemXmlDocument.GetElementsByTagName("ItemMaster");
            var bomLinksQuantity = MainForm.Counter;
            count = 0;
            foreach (XmlElement item in items)
            {
                if (item.GetElementsByTagName("bomLink").Count == 0)
                {
                    continue;
                }
                else
                {
                    var masterId = (string)item.GetAttribute("ItemMasterID");
                    foreach (XmlElement iteration in item.GetElementsByTagName("Iteration"))
                    {
                        var iterationID = Int32.Parse(iteration.GetAttribute("IterationID").Remove(0, 1));
                        parentId = ParentIdSelection(masterId,iterationID);
                        if (parentId == 0)
                        {
                            Log.Error(":ParentItem with ID " + Int32.Parse(item.GetAttribute("ItemMasterID").Remove(0, 1)) + " was not found ");
                            //worker.CancelAsync();
                            continue;
                        }
                        foreach (XmlElement bomLink in iteration.GetElementsByTagName("bomLink"))
                        {
                            //int b = Int32.Parse(bomLink.GetAttribute("ChildItemMasterID").Remove(0, 1));
                            childId = ChildIdSelection((string)bomLink.GetAttribute("ChildItemMasterID"));
                            if (childId == 0)
                            {
                                Log.Error(":ChildItem with ID " + Int32.Parse(bomLink.GetAttribute("ChildItemMasterID").Remove(0, 1)) + " was not found ");
                                continue;
                            }

                            var unitSize = bomLink.GetAttribute("UnitSize");
                            var quantity = bomLink.GetAttribute("Quantity");
                            var linkType = bomLink.GetAttribute("LinkType");
                            var instanceCount = Int32.Parse(bomLink.GetAttribute("InstanceCount"));
                            var cad = bomLink.GetAttribute("CAD");
                            int position;
                            if (bomLink.GetAttribute("PositionNumber") != null)
                            {
                                position = Int32.Parse(bomLink.GetAttribute("PositionNumber"));
                            }
                            else
                            {
                                position = Int32.Parse(null);
                            }
                            InsertItemItemRelation(parentId, childId, position, QuantityInterpreter(quantity), unit, linkType, instanceCount, QuantityInterpreter(unitSize), cad);
                            count++;

                            percentComplete = (int)((count) / bomLinksQuantity);
                            worker.ReportProgress(percentComplete);

                        }
                    }
                }
            }
            return count;
        }

        private long ChildIdSelection(string childId)
        {

            foreach (var item in Item.item4Relations)
            {
                if (item.id == childId )
                {
                    return item.idDB;
                }
            }
            return 0;
        }

        private long ParentIdSelection(string iterationId,int iterationID)
        {
            foreach (var item in Item.item4Relations)
            {
                if (item.id == iterationId && item.iterationID==iterationID)
                {
                    unit = item.unit;
                    return item.idDB;
                }

            }
            return 0;
        }
        private string QuantityInterpreter(string itemQuantity)
        {
            string factor = "";
            string exp = "";
            float ex = 0;
            float quantity;

            for (int i = 0; i < itemQuantity.Length; i++)
            {
                if (itemQuantity[i] == 'e')
                {
                    factor = itemQuantity.Remove(i, itemQuantity.Length - i);
                    exp = itemQuantity.Remove(0, i + 2);
                    break;
                }

            }
            int f = Int32.Parse(exp);
            for (int j = 0; j <= Int32.Parse(exp); j++)
            {
                if (j == 0) { ex = 1; }
                if (j == 1) { ex = (float)2.71828182846; }
                else if (j >= 2)
                {
                    ex = ex * (float)2.71828182846;
                }
            }
            quantity = (float)float.Parse(factor, CultureInfo.InvariantCulture) * ex;
            return quantity.ToString();

        }
        private void InsertItemItemRelation(long parentIdDB, long childId, int position, string quantity, string unit, string linkType, int instanceCount, string unitsize, string cad)
        {
            var sqlExpression = "insert into ItemItemRelations(ParentItemID,ChildItemID,Position,Quantity,Unit,LinkType,InstanceCount,UnitSize,CAD)values(@ParentItemID,@ChildItemID,@Position,@Quantity,@Unit,@LinkType,@InstanceCount,@UnitSize,@CAD)";
            var connectionString = MainForm.InputConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {

                connection.Open();
                using (var cmd = new SqlCommand(sqlExpression, connection))
                {
                    cmd.CommandTimeout = 600;
                    cmd.Parameters.Add("@ParentItemID", SqlDbType.BigInt);
                    cmd.Parameters["@ParentItemID"].Value = parentIdDB;
                    cmd.Parameters.Add("@ChildItemID", SqlDbType.BigInt);
                    cmd.Parameters["@ChildItemID"].Value = childId;
                    cmd.Parameters.Add("@Position", SqlDbType.Int);
                    cmd.Parameters["@Position"].Value = position;
                    cmd.Parameters.Add("@Quantity", SqlDbType.Decimal);
                    cmd.Parameters["@Quantity"].Value = decimal.Parse(quantity);
                    cmd.Parameters.Add("@Unit", SqlDbType.NVarChar);
                    cmd.Parameters["@Unit"].Value = unit;
                    cmd.Parameters.Add("@LinkType", SqlDbType.NVarChar);
                    cmd.Parameters["@LinkType"].Value = linkType;
                    cmd.Parameters.Add("@InstanceCount", SqlDbType.Int);
                    cmd.Parameters["@InstanceCount"].Value = instanceCount;
                    cmd.Parameters.Add("@UnitSize", SqlDbType.Decimal);
                    cmd.Parameters["@UnitSize"].Value = decimal.Parse(unitsize);
                    cmd.Parameters.Add("@CAD", SqlDbType.Bit);
                    if (cad == "1")
                    {
                        cmd.Parameters["@CAD"].Value = Convert.ToBoolean("true");
                    }
                    else
                        cmd.Parameters["@CAD"].Value = Convert.ToBoolean("false");

                    cmd.ExecuteNonQuery();
                }
            }

        }

    }
}
