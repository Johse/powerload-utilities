using System;
using System.Windows.Forms;
using log4net.Config;

[assembly: XmlConfigurator(Watch = true)]

namespace IDB.Load.Files
{
    internal static class Program
    {
        //   private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}