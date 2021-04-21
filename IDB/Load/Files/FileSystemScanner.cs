using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;
namespace IDB.Load.Files
{
    internal class FileSystemScanner
    {
        private readonly string _basePath;
        private readonly string _connectionString;

        public static long ContentCounter { get; set; }

        public FileSystemScanner(string basePath, string connectionString)
        {
            _basePath = basePath;
            _connectionString = connectionString;
        }

        public long Execute(string localPath, BackgroundWorker worker, DoWorkEventArgs e, long counter)
        {
            worker.WorkerReportsProgress = true;
           
            if (worker.CancellationPending)
            {
                e.Cancel = true; return ContentCounter;
                
            }
            SqlEditor.InsertFolder(localPath, _basePath, _connectionString);
            Logger.Log.Info(":Folder was inserted:  " + localPath);
            int percentComplete;

            if (counter == 0)
            {
                percentComplete = 100;
                worker.ReportProgress(percentComplete);
                return ContentCounter;
            }

            var fileNames = Directory.GetFiles(localPath);
            foreach (var filename in fileNames)
            {
                ContentCounter++;
                percentComplete = (int) (ContentCounter / counter * 100);
                SqlEditor.InsertFile(filename, _connectionString);

                var file = new DirectoryInfo(filename).Name;
                Logger.Log.Info(":File was inserted:" + file);

                worker.ReportProgress(percentComplete);
            }

            var folderNames = Directory.GetDirectories(localPath);
            if (folderNames.Length == 0) e.Cancel = true;
            foreach (var folderName in folderNames) 
                Execute(folderName, worker, e, counter);

            return ContentCounter;
        }
    }
}