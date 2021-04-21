using System;
using System.Windows.Forms;
using IDB.Core;

namespace IDB.Discover.Vault
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            txtConnectionString.Text = Settings.IdbConnectionString;
            txtVaultConnection.Text = Settings.VaultConnectionString;
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

        private void txtConnectionString_TextChanged(object sender, EventArgs e)
        {
            Settings.IdbConnectionString = txtConnectionString.Text;
            
        }

        private void txtVaultConnection_TextChanged(object sender, EventArgs e)
        {
            Settings.VaultConnectionString = txtVaultConnection.Text;
        }
    }
}