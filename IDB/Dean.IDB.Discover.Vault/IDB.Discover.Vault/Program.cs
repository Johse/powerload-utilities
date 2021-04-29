using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IDB.Discover.Vault
{
    static class Program
    {

        static MainForm activeMainForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // timer thread is used for a heart beat on system processing
            // this has nothing to do with expiration of cababilities discussed above
            System.Threading.Timer timer = new System.Threading.Timer(OnTimeExpire, null, 5000, 1000);

            activeMainForm = new MainForm();

            Application.Run(activeMainForm);
        }

        private static void OnTimeExpire(object state)
        {
            // call the update mechanism
            if (activeMainForm != null)
            {
                activeMainForm.updateProgressStopWatchDisplay();
            }
            else
            {
            }
        }

    }
}
