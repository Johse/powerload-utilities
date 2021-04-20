using System;
using System.Windows.Forms;

namespace IDB.Discover.Vault
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            //txtConnectionString.Text = "Properties.Settings.Default.ConnectionString";
            //txtVaultConnection.Text = "Properties.Settings.Default.ExportDirectory";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string loadConnectionString = txtConnectionString.Text;
                string vaultConnectionString = txtVaultConnection.Text;
                VaultDbExtractor.Transfer(vaultConnectionString, loadConnectionString);

                MessageBox.Show("Successfully transferred target Vault behaviors to IDB");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}