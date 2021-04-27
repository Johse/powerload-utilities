using log4net;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace IDB.Validate.BCP
{
    public partial class MainForm : Form
    {
        private static readonly ILog Log = LogManager.GetLogger("IDBValidateBCP");

        public MainForm()
        {
            InitializeLogging();
            InitializeComponent();

            txtConnectionString.Text = Core.Settings.IdbConnectionString;
        }

        private void InitializeLogging()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var fi = new FileInfo(thisAssembly.Location + ".log4net");
            log4net.Config.XmlConfigurator.Configure(fi);

            Log.Info($"powerLoad {Assembly.GetExecutingAssembly().GetName().Name} v{Assembly.GetExecutingAssembly().GetName().Version}");
        }

        private void OnTxtConnectionStringTextChanged(object sender, EventArgs e)
        {
            Core.Settings.IdbConnectionString = txtConnectionString.Text;
        }

        private void OnBtnValidateClick(object sender, EventArgs e)
        {
            var validator = new Validator();
            var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "SQL");
            var sqlScript = File.ReadAllText(Path.Combine(scriptPath, "Validate.IDB.sql"));

            try
            {
                validator.Validate(txtConnectionString.Text, sqlScript);
            }
            catch (System.Data.SqlClient.SqlException sqlScriptException)
            {
                Log.Error($"Error in SQL script: {sqlScriptException.Message}", sqlScriptException);
                return;
            }
            var counter = validator.GetErrorCount(txtConnectionString.Text, "Folders") + validator.GetErrorCount(txtConnectionString.Text, "Files") + validator.GetErrorCount(txtConnectionString.Text, "FileFileRelations");
            MessageBox.Show($"There are {counter} elements with an error. Details can be found in IDB (Validation_Comment).");
        }

        private void OnBtnCloseClick(object sender, EventArgs e)
        {
            Close();
        }
    }
}
