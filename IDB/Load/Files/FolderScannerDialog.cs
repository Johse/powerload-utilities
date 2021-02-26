using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace IDB.Load.Files
{
    public partial class FolderScannerDialog : Form
    {
        private static long _counter;

        public static string ErrorMsg { get; set; }

        public FolderScannerDialog()
        {
            InitializeComponent();
            InitializeBackgroundWorker();
            RefreshControls();
        }

        private void InitializeBackgroundWorker()
        {
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            Logger.Log.Debug(":Program was started with path: " + input.Text);
            CancelBtn.Enabled = true;
            button2.Enabled = false;
            try
            {
                ScanedFilesQantity.Text = "Scan records: ";
                var files = Directory.GetFiles(input.Text, "*.*", SearchOption.AllDirectories);
                /* foreach (string file in files) For UI Update
                 {
                     _counter++;
                     record.Text = _counter.ToString();
                     System.Threading.Thread.Sleep(1);
                     Application.DoEvents();
                 }
                */
                _counter = files.Length;
                ScanedFilesQantity.Text = ScanedFilesQantity.Text+_counter.ToString();
                Cursor.Current = Cursors.WaitCursor;
                backgroundWorker1.RunWorkerAsync();
                Cursor.Current = Cursors.Default;
            }
            catch (UnauthorizedAccessException accesException)
            {
                MessageBox.Show("Program has not acces for this data!");
                processPart.Text = "";
                _counter = 0;
                Logger.Log.Error(":Folder acces Error.");
            }
            catch (SqlException insertException)
            {
                Logger.Log.Error(":These data are already exist in database.");

                var sqlError = new SQLError();
                sqlError.ErrorSql = "These data are already exist in database";
                ErrorMsg = insertException.ToString();
                sqlError.Show();
                processPart.Text = "";
                _counter = 0;
            }
            catch (ArgumentException argumentException)
            {
                processPart.Text = "";
                _counter = 0;
                Logger.Log.Error(":Connection String is false.");
                var sqlError = new SQLError();
                sqlError.ErrorSql = "Connection String is false.";
                ErrorMsg = argumentException.ToString();
                sqlError.Show();
            }
            catch (IOException pathException)
            {
                Logger.Log.Error(":Path was not found.");

                var sqlError = new SQLError();
                sqlError.ErrorSql = "Path was not found.";
                ErrorMsg = pathException.ToString();
                sqlError.Show();
                processPart.Text = "";
                _counter = 0;
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            RefreshControls();
            
        }

        private void RefreshControls()
        {
            input.Text = "";
            processPart.Text = "";
            ScanedFilesQantity.Text = " ";
            connectionString.Text = XmlConfiguration.GetBehavior("ConnectionString");
            input.Text = XmlConfiguration.GetBehavior("DataPath");
            progressBar1.Value = 0;
            button2.Enabled = true;
            FileSystemScanner.ContentCounter = 0;
            CancelBtn.Enabled = false;
        }

        private void chooseBtn_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog {Description = "Select your path."})
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                    input.Text = fbd.SelectedPath;
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            XmlConfiguration.SetBehavior("ConnectionString", connectionString.Text);
            XmlConfiguration.SetBehavior("DataPath", input.Text);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            
           if (worker.CancellationPending)
            {
                e.Cancel = true;
                return;
           }
            var scanner = new FileSystemScanner(Directory.GetParent(input.Text).FullName);
            e.Result = scanner.Execute(input.Text, worker, e, _counter);
           
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            processPart.Text = "Import records: " + FileSystemScanner.ContentCounter.ToString();
            //record.Text = FileSystemScanner.ContentCounter.ToString();
            //progressBar1.Value = e.ProgressPercentage;
            progressBar1.Maximum = (int)_counter;
            progressBar1.Step = 1;
            progressBar1.Minimum = 0;
            progressBar1.PerformStep();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                MessageBox.Show(e.Error.Message);
            else if (e.Cancelled)
                processPart.Text = "Finished";
                CancelBtn.Text = "Reset";

        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            //CancelBtn.Enabled = false;
            backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.CancelAsync();
            RefreshControls();
            CancelBtn.Text = "Cancel";

        }

        private void FolderScannerDialog_Load(object sender, EventArgs e)
        {
            input.Text = XmlConfiguration.GetBehavior("DataPath");
        }
    }
}