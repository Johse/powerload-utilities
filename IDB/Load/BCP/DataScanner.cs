using System;
using System.ComponentModel;
using System.Windows.Forms;
using IDB.Core;
using IDB.Load.BCP.IDB.Content;
using IDB.Load.BCP.Utilities;

namespace IDB.Load.BCP
{
    public partial class DataScanner : Form
    {
        private static string _inputConnectionString;

        internal static string InputConnectionString
        {
            get { return _inputConnectionString; }
            set { _inputConnectionString = value; }
        }

        internal static System.Xml.XmlDocument xmlDocument;
        private static long _counter;

        public static long Counter
        {
            set { _counter = value; }
            get { return _counter; }
        }

        public DataScanner()
        {
            InitializeComponent();
            connectionTxtBox.Text = Settings.IdbConnectionString;
            dataPathTxtBox.Text = Settings.ImportPath;
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            try
            {
                Logger.Log.Debug(":Program was started with path: " + dataPathTxtBox.Text);
                CancelBtn.Enabled = true;
                chooseBtn.Enabled = false;
                startBtn.Enabled = false;
                var document = XmlReaderUtility.GetXmlDocument(dataPathTxtBox.Text + @"\Vault.xml");
                SQLEditor.DatabaseConnection(connectionTxtBox.Text);
                XmlReaderUtility.parentFolderControl = false;

                InputConnectionString = connectionTxtBox.Text;
                processPart.Text = "Xml file is scanned,please wait";
                Cursor.Current = Cursors.WaitCursor;
                xmlDocument = XmlReaderUtility.ToXmlDocument(document);
                var vaultElement = xmlDocument.FirstChild;
                var statics = vaultElement.FirstChild;
                _counter = Int64.Parse(XmlReaderUtility.GetProperty(statics, "TotalFiles")) +
                           xmlDocument.GetElementsByTagName("Association").Count;
                ScanedFilesQantity.Text = ScanedFilesQantity.Text + _counter.ToString();
                workerFolders.WorkerSupportsCancellation = true;
                workerFileFileRelations.WorkerSupportsCancellation = true;
                workerItems.WorkerSupportsCancellation = true;
                workerFolders.RunWorkerAsync();
            }
            catch (System.Data.SqlClient.SqlException connectionError)
            {
                CancelBtn.Enabled = false;
                chooseBtn.Enabled = true;
                startBtn.Enabled = true;
                Logger.Log.Error(connectionError);
            }
            catch (Exception exception)
            {
                CancelBtn.Enabled = false;
                chooseBtn.Enabled = true;
                startBtn.Enabled = true;
                Logger.Log.Error(exception);
            }
        }

        internal void RefreshControls()
        {
            FilesInfo.files.Clear();
            CancelBtn.Text = "Cancel";
            dataPathTxtBox.Text = "";
            processPart.Text = "";
            ScanedFilesQantity.Text = "Scaned Files: ";

            progressBar1.Value = 0;
            chooseBtn.Enabled = true;
            startBtn.Enabled = true;
            CancelBtn.Enabled = false;
            Counter = 0;
            SQLEditor.AssocCounter = 0;
            Item.Count = 0;

            XmlReaderUtility.ContentCounter = 0;
        }

        private void chooseBtn_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog {Description = "Select your path."})
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                    dataPathTxtBox.Text = fbd.SelectedPath;
            }
        }

        private void workerFolders_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            e.Result = XmlReaderUtility.GetVaultData(dataPathTxtBox.Text, worker, e, _counter);
        }

        private void WorkerFolders_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            processPart.Text = "Import records: " + XmlReaderUtility.ContentCounter.ToString();
            progressBar1.Maximum = (int) _counter;
            progressBar1.Step = 1;
            progressBar1.Minimum = 0;
            progressBar1.PerformStep();
        }

        private void WorkerFolders_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
                Logger.Log.Error(e.Error.Message);

            }

            if (e.Cancelled)
            {
                processPart.Text = "Cancelled";
            }
            else
            {
                workerFileFileRelations.RunWorkerAsync();
            }
        }



        private void WorkerFileFileRelations_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            e.Result = SQLEditor.RelationDataFounder(dataPathTxtBox.Text, worker, e, _counter);
        }

        private void WorkerFileFileRelations_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            processPart.Text =
                "Import records: " + (XmlReaderUtility.ContentCounter + SQLEditor.AssocCounter).ToString();
            progressBar1.Maximum = (int) _counter;
            progressBar1.Step = 1;
            progressBar1.Minimum = 0;
            progressBar1.PerformStep();
        }

        private void WorkerFileFileRelations_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            FilesInfo.files.Clear();

            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
                return;
            }

            if (e.Cancelled)
            {
                processPart.Text = "Cancelled";
                return;
            }

            processPart.Text = "Finished";
            CancelBtn.Text = "Reset";
        }

        private void workerItems_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            e.Result = Item.GetItemAtrrributes(dataPathTxtBox.Text, worker, e);
        }

        private void workerItems_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            processPart.Text = "Import records: " + Item.Count.ToString();
            ScanedFilesQantity.Text = "Scaned Items: " + Counter;
            progressBar1.Maximum = (int) Counter;
            progressBar1.Step = 1;
            progressBar1.Minimum = 0;
            progressBar1.PerformStep();
        }

        private void workerItems_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                MessageBox.Show(e.Error.Message);
            if (e.Cancelled)
            {
                processPart.Text = "Cancelled";
            }
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            if (this.workerFolders.IsBusy)
            {
                this.workerFolders.CancelAsync();
            }

            if (this.workerFileFileRelations.IsBusy)
            {
                this.workerFileFileRelations.CancelAsync();
            }

            if (this.workerFolders.IsBusy)
            {
                this.workerItems.CancelAsync();
            }

            RefreshControls();
        }

        private void ItemRelations_Click(object sender, EventArgs e)
        {
            CancelBtn.Enabled = true;
            var document = XmlReaderUtility.GetXmlDocument(dataPathTxtBox.Text + @"\Itemswrapper.xml");
            InputConnectionString = connectionTxtBox.Text;
            Item.itemXmlDocument = XmlReaderUtility.ToXmlDocument(document);
            Counter = Item.itemXmlDocument.GetElementsByTagName("bomLink").Count;
            ScanedFilesQantity.Text = "Scaned Files: " + Counter;
            workerItemItemsRelation.RunWorkerAsync();
        }

        private void workerItemItemsRelation_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            ItemItemRelations itemRelations = new ItemItemRelations();
            e.Result = itemRelations.RelationDataFounder(worker, e);
        }

        private void workerItemItemsRelation_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            processPart.Text = "Import records: " + ItemItemRelations.count.ToString();
            ScanedFilesQantity.Text = "Scaned ItemItemRelations: " + Counter;
            progressBar1.Maximum = (int) Counter;
            progressBar1.Step = 1;
            progressBar1.Minimum = 0;
            progressBar1.PerformStep();
        }

        private void workerItemItemsRelation_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                MessageBox.Show(e.Error.Message);
            if (e.Cancelled)
            {
                processPart.Text = "Cancelled";
            }
            else
            {
                Item.item4Relations.Clear();
                processPart.Text = "Finished";
                CancelBtn.Text = "Reset";
            }
        }

        private void ValidateBtn_Click(object sender, EventArgs e)
        {
            /*
             IDBValidator validator = new IDBValidator();
             string startupPath = System.IO.Directory.GetCurrentDirectory();
             string sqlScript = "";
             foreach (string line in System.IO.File.ReadLines(startupPath + @"\sqlScript.txt"))
             {
                 if (line.Contains("--Updating"))
                 {
                     validator.Validator(connectionTxtBox.Text, sqlScript);
                     sqlScript = "";
                     continue;
                 }
                 sqlScript = sqlScript + "\n" + line;

             }
             validator.Validator(connectionTxtBox.Text, sqlScript);
             var counter = validator.AnnotationCounter(connectionTxtBox.Text, "Folders") + validator.AnnotationCounter(connectionTxtBox.Text, "Files") + validator.AnnotationCounter(connectionTxtBox.Text, "FileFileRelations");
             MessageBox.Show("There are " + counter + " elements in the database ,that are false");*/
        }

        private void ItemsInsert_Click(object sender, EventArgs e)
        {
            CancelBtn.Enabled = true;
            InputConnectionString = connectionTxtBox.Text;
            var document = XmlReaderUtility.GetXmlDocument(dataPathTxtBox.Text + @"\Itemswrapper.xml");
            Item.itemXmlDocument = XmlReaderUtility.ToXmlDocument(document);
            Item.itemsCollection = Item.itemXmlDocument.GetElementsByTagName("ItemMaster");
            Counter = Item.itemsCollection.Count;
            ScanedFilesQantity.Text = "Scaned Files: " + Counter;
            workerItems.RunWorkerAsync();
        }

        private void dataPathTxtBox_TextChanged(object sender, EventArgs e)
        {
            Settings.ImportPath = dataPathTxtBox.Text;
        }

        private void connectionTxtBox_TextChanged(object sender, EventArgs e)
        {
            Settings.IdbConnectionString = connectionTxtBox.Text;
        }
    }
}