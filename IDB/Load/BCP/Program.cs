using System;
using System.Windows.Forms;
using log4net.Config;
[assembly: XmlConfigurator(Watch = true)]
namespace IDB.Load.BCP
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DataScanner());
        }
    }
}
