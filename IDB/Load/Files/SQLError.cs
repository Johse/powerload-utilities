using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace IDB.Load.Files

{
    public partial class SQLError : Form
    {
        
        private string _error;
        public string ErrorSql {
            set { _error=value; }
            get {return _error; }
        }
        public SQLError()
        {
            InitializeComponent();
        }
      
            private void SQLErrorlbl_Click(object sender, EventArgs e)
        {

        }

        private void detailsBtn_Click(object sender, EventArgs e)
        {
            this.Close();
            MessageBox.Show(MainForm.ErrorMsg);
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SQLError_Load(object sender, EventArgs e)
        {
            SQLErrorlbl.Text = ErrorSql;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string pathOfSolution = System.Reflection.Assembly.GetEntryAssembly().Location;
            string nameOfSolution = new DirectoryInfo(pathOfSolution).Name;
            string logFilePath = pathOfSolution.Replace(nameOfSolution, "bcpIDBMaker.log");
            Process.Start(logFilePath);
            
          

        }
    }
}
