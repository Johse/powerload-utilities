using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Reflection;

using log4net;
using BCPBuilderConfig;
using MCDD = MSSql.COIntermDB.Dapper.DirectAccess;
using MVDD = MSSql.Vault.Dapper.DirectAccess;




namespace IDB.Discover.Vault
{
    public partial class MainForm : Form
    {
        // setup the log file information
        private static readonly ILog4NetExtender Logger = BCPBuilderConfig.BCPBuilderConfigurationManager.GetExtendedLogger(typeof(MainForm));

        // things to give user indication we are still moving forward
        private StopwatchPlus mProcessingStopwatch;
        private Thread mProcessWorkerThread;

        // delegate to handle the update to the RichTextBox
        protected UpdateTextDelegate mUpdateTextDelegate;

        // manage the CO_IntermediateDbSqlCache
        private MCDD.CO_IntermediateDbSqlCache mCOIntDbSqlCache;

        // manage MVDD.VaultCache
        private MVDD.VaultCache mVaultCache;


        public MainForm()
        {
            // initialize Log4Net logging
            InitializeLogging();

            InitializeComponent();

            GetPropertiesSettingsDefault();
        }

        // method to set the text box values from the Properties.Settings
        private void GetPropertiesSettingsDefault()
        {
            COIDB_ConnectionStringTextBox.Text = Properties.Settings.Default.IDBConnectionString;
            Vault_ConnectionStringTextBox.Text = Properties.Settings.Default.VaultConnectionString;
            KVM_DatabaseNameTextBox.Text = Properties.Settings.Default.KvmSqlDatabaseName;
            txtExportDirectory.Text = Properties.Settings.Default.ExportDirectory;
        }

        private void TextBox_LostFocus(object sender, System.EventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.IDBConnectionString = COIDB_ConnectionStringTextBox.Text;
            Properties.Settings.Default.VaultConnectionString = Vault_ConnectionStringTextBox.Text;
            Properties.Settings.Default.KvmSqlDatabaseName = KVM_DatabaseNameTextBox.Text;
            Properties.Settings.Default.ExportDirectory = txtExportDirectory.Text;

            Properties.Settings.Default.Save();
        }


        #region Helpers

        private void InitializeLogging()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var fi = new FileInfo(thisAssembly.Location + ".log4net");
            log4net.Config.XmlConfigurator.Configure(fi);
        }

        // method for the external timer class to call
        public void updateProgressStopWatchDisplay()
        {
            // update the label showing the stopwatch progress
            if (InvokeRequired && (mProcessingStopwatch != null))
            {
                Invoke(new ThreadStart(updateProgressStopWatchDisplay));
            }
            else if (mProcessingStopwatch != null)
            {
                mProcessStopWatchTextBox.Text = mProcessingStopwatch.ElapsedTimeString("{0:00}:{1:00}:{2:00}");
            }
        }


        protected void appendRichTextBox(string text)
        {
            if (InvokeRequired)
            {
                // create a delegate to handle the text updae
                UpdateTextDelegate mUpdateTextDelegate = new UpdateTextDelegate(appendRichTextBox);
                BeginInvoke(mUpdateTextDelegate, new object[] { text });
            }
            else
            {
                m_StatusRichTextBox.AppendText(text);

                // scroll to the end
                m_StatusRichTextBox.SelectionStart = m_StatusRichTextBox.Text.Length;
                m_StatusRichTextBox.ScrollToCaret();
            }
        }

        #endregion Helpers


        #region IDB processing

        private void LoadIDBButton_Click(object sender, EventArgs e)
        {
            Logger.Debug(string.Format("LoadCOIntDbSqlCache Button"));

            mProcessingStopwatch = new StopwatchPlus();

            // create the delegate
            mUpdateTextDelegate = new UpdateTextDelegate(appendRichTextBox);
            Logger.SetUpdateTextDelegate(mUpdateTextDelegate);

            // setup a worker thread to process the content
            ThreadStart starter = LoadCOIntDbSqlCache;
            starter += () =>
            {
                // Do what you want in the callback
                Cursor.Current = Cursors.Default;

                Logger.Debug(string.Format("LoadCOIntDbSqlCache Total Time {0}", mProcessingStopwatch.ElapsedTimeString()));

                mProcessingStopwatch = null;

                // display a completed dialog
                MessageBox.Show("LoadCOIntDbSqlCache Done");
            };

            mProcessWorkerThread = new Thread(starter) { IsBackground = true };
            mProcessWorkerThread.Start();
        }

        // method to load the COIntDbSqlCache
        private void LoadCOIntDbSqlCache()
        {
            mCOIntDbSqlCache = SetupSQLCacheForVaultBCPLoad();
        }

        // method to acquire the information on the files of interest
        public MCDD.CO_IntermediateDbSqlCache SetupSQLCacheForVaultBCPLoad()
        {
            StopwatchPlus swp = new StopwatchPlus();

            // log total time taken
            Logger.Debug("SetupSQLCacheForVaultBCPLoad(): start");


            // get the source information
            string sSrcVaultConnString = COIDB_ConnectionStringTextBox.Text;

            // get the source cache
            MCDD.CO_IntermediateDbSqlCache sqlCache = new MCDD.CO_IntermediateDbSqlCache(sSrcVaultConnString);

            sqlCache.LoadInitialAllFilesAndFinalizeCache();

            Logger.Debug(string.Format("SetupSQLCacheForVaultBCPLoad() Total Time {0}", swp.ElapsedTimeString()));

            return (sqlCache);
        }

        // method to process the local file for checksum, size, date, etc
        private void ProcessLocalFileButton_Click(object sender, EventArgs e)
        {
            // verify that the user wants to process checksum values
            if (!UpdateLocalChecksumsCheckBox.Checked || (MessageBox.Show("This will process file cheksums and overwrite content in the IDB: Continue?", "Checksum Processing", MessageBoxButtons.YesNo) == DialogResult.Yes))
            {
                Logger.Debug(string.Format("ProcessLocalFileButton Button"));

                mProcessingStopwatch = new StopwatchPlus();

                // create the delegate
                mUpdateTextDelegate = new UpdateTextDelegate(appendRichTextBox);
                Logger.SetUpdateTextDelegate(mUpdateTextDelegate);

                // setup a worker thread to process the content
                ThreadStart starter = ProcessLocalFileChecksums;
                starter += () =>
                {
                    // Do what you want in the callback
                    Cursor.Current = Cursors.Default;

                    Logger.Debug(string.Format("ProcessLocalFileButton Total Time {0}", mProcessingStopwatch.ElapsedTimeString()));

                    mProcessingStopwatch = null;

                    // display a completed dialog
                    MessageBox.Show("ProcessLocalFileButton Done");
                };

                mProcessWorkerThread = new Thread(starter) { IsBackground = true };
                mProcessWorkerThread.Start();
            }
        }

        // method to process the local file for checksum, size, date, etc
        public void ProcessLocalFileChecksums()
        {
            StopwatchPlus swp = new StopwatchPlus();

            // log total time taken
            Logger.Debug("ProcessLocalFileChecksums(): start");

            // process the checksum information
            mCOIntDbSqlCache.ProcessLocalFileInformation(UpdateLocalChecksumsCheckBox.Checked);

            // generate report on files missing
            List<string> filesMissing = mCOIntDbSqlCache.m_FilesDict.Values.Where(fi => !fi.LocalFileExists).Select(fi => fi.LocalFullFileName).ToList();
            if (filesMissing.Count() > 0)
            {
                string outputString = string.Format("Files Missing {0:n0}", filesMissing.Count());
                outputString += Environment.NewLine + string.Join(Environment.NewLine, filesMissing);
                Logger.Debug(outputString);
            }


            Logger.Debug(string.Format("ProcessLocalFileChecksums() Total Time {0}", swp.ElapsedTimeString()));
        }

        // files where originally XCOPYd into their wrong path target path
        private void MoveMissingFilesButton_Click(object sender, EventArgs e)
        {
            // generate report on files missing
            List<string> filesMissing = mCOIntDbSqlCache.m_FilesDict.Values.Where(fi => !fi.LocalFileExists).Select(fi => fi.LocalFullFileName).ToList();
            if (filesMissing.Count() > 0)
            {
                // discover where these files may be
                List<string> foundFiles = Directory.GetFiles(@"C:\VaultBCP\Marvin\rd_data", "*.*", SearchOption.AllDirectories).ToList();

                // convert that list into a dictionary
                Dictionary<string, string> foundFilesDictionary = foundFiles.ToDictionary(st => Path.GetFileName(st), st => st, StringComparer.InvariantCultureIgnoreCase);

                // iterate through each missing file and move them
                foreach (string missingFile in filesMissing)
                {
                    string fname = Path.GetFileName(missingFile);
                    string currentLocation = foundFilesDictionary[fname];

                    // make the target directory
                    string targetDirectory = Path.GetDirectoryName(missingFile);
                    System.IO.Directory.CreateDirectory(targetDirectory);

                    // move the file
                    System.IO.File.Move(currentLocation, missingFile);
                }

            }

        }


        // method to to verify columns exist in the database for processing the local file for checksum, size, date, etc
        private void SetupColumnsButton_Click(object sender, EventArgs e)
        {
            // process the checksum information
            mCOIntDbSqlCache.SetupDatabaseColumnsForLocalFileProcessing();
        }

        // update the database local file checksum information
        private void UpdateChecksumInDBButton_Click(object sender, EventArgs e)
        {
            // update the database with the checksum information
            mCOIntDbSqlCache.UpdateDBLocalFileChecksumStatistics();
        }

        #endregion IDB processing


        #region Vault files processing

        private void LoadVaultFilesButton_Click(object sender, EventArgs e)
        {
            Logger.Debug(string.Format("LoadVaultFiles Button"));

            mProcessingStopwatch = new StopwatchPlus();

            // create the delegate
            mUpdateTextDelegate = new UpdateTextDelegate(appendRichTextBox);
            Logger.SetUpdateTextDelegate(mUpdateTextDelegate);

            // setup a worker thread to process the content
            ThreadStart starter = LoadVaultFiles;
            starter += () =>
            {
                // Do what you want in the callback
                Cursor.Current = Cursors.Default;

                Logger.Debug(string.Format("LoadVaultFiles Total Time {0}", mProcessingStopwatch.ElapsedTimeString()));

                mProcessingStopwatch = null;

                // display a completed dialog
                MessageBox.Show("LoadVaultFiles Done");
            };

            mProcessWorkerThread = new Thread(starter) { IsBackground = true };
            mProcessWorkerThread.Start();
        }

        // method to load the VaultCache
        private void LoadVaultFiles()
        {
            // get a list of Vault files from the IDB
            // List<string> idbFilesList = mCOIntDbSqlCache.m_CO_FileMasterList.Select(fm => fm.mFullVaultFilePath).ToList();

            // ask the VaultCache to load the specific files that are also in the IDB

            // get the source information
            string sSrcVaultConnString = Vault_ConnectionStringTextBox.Text;

            // get the source cache
            mVaultCache = new MVDD.VaultCache(sSrcVaultConnString);

            // set the KVM
            mVaultCache.SetKVMDatabaseConnection(KVM_DatabaseNameTextBox.Text);

            // get the VaultCache
            MVDD.VaultCache.LoadAllFoldersFilesItemsAndFinalizeCache(mVaultCache);
        }


        #endregion Vault files processing

        #region AnalyzeIDBAndVault

        // generate a report on file comparisons for IDB Delta and the target Vault
        private void AnalyzeIDBAndVaultButton_Click(object sender, EventArgs e)
        {
            if (!UpdateDBAndIDBVaultCheckBox.Checked || (MessageBox.Show("This will Update Files table in IDB: Continue?", "Analyze IDB And Vault Process", MessageBoxButtons.YesNo) == DialogResult.Yes))
            {

                Logger.Debug(string.Format("AnalyzeIDBAndVault Button"));

                mProcessingStopwatch = new StopwatchPlus();

                // create the delegate
                mUpdateTextDelegate = new UpdateTextDelegate(appendRichTextBox);
                Logger.SetUpdateTextDelegate(mUpdateTextDelegate);

                // setup a worker thread to process the content
                ThreadStart starter = AnalyzeIDBAndVault;
                starter += () =>
                {
                // Do what you want in the callback
                Cursor.Current = Cursors.Default;

                    Logger.Debug(string.Format("AnalyzeIDBAndVault Total Time {0}", mProcessingStopwatch.ElapsedTimeString()));

                    mProcessingStopwatch = null;

                // display a completed dialog
                MessageBox.Show("AnalyzeIDBAndVault Done");
                };

                mProcessWorkerThread = new Thread(starter) { IsBackground = true };
                mProcessWorkerThread.Start();
            }
        }


        // generate a report on file comparisons for IDB Delta and the target Vault
        private void AnalyzeIDBAndVault()
        {
            // Ask CO_IDB_VaultFileMaster to process the MCDD.Hierarchy.CO_FileMaster and MVDD.VaultFileMaster objects
            CO_IDB_VaultFileMaster.Combine_CO_IDB_And_VaultCache(mCOIntDbSqlCache, mVaultCache);

            // copy the information to the correct object elements
            CO_IDB_VaultFileMaster.CopyVaultFMElemInfoToIDBFile();


            // update the DB if checked
            if (UpdateDBAndIDBVaultCheckBox.Checked)
            {
                // update the database
                mCOIntDbSqlCache.UpdateDBVaultFileChecksumStatistics();
            }

            // get the files that are in the Vault by full vault file path
            List<CO_IDB_VaultFileMaster> filesMatchingFullPath = CO_IDB_VaultFileMaster.mCO_IDB_VaultFileMaster_List.Where(coivfm => (coivfm.mVaultFileMaster != null)).ToList();
            List<CO_IDB_VaultFileMaster> filesWithDiffLocalVaultChecksum = CO_IDB_VaultFileMaster.mCO_IDB_VaultFileMaster_List.Where(coivfm => ((coivfm.mVaultFileMaster != null) && !coivfm.mChecksumsMatch)).ToList();


            // get the files that are in the Vault by name, and by full path matching
            List<CO_IDB_VaultFileMaster> filesMatchingFileName = CO_IDB_VaultFileMaster.mCO_IDB_VaultFileMaster_List.Where(coivfm => ((coivfm.mVaultFileMaster != null) || coivfm.mAlternateVaultFileMasterList.Any())).ToList();

            // get the files that are in the Vault by name, but not by full path matching
            List<CO_IDB_VaultFileMaster> filesOnlyMatchingFileName = CO_IDB_VaultFileMaster.mCO_IDB_VaultFileMaster_List.Where(coivfm => ((coivfm.mVaultFileMaster == null) && coivfm.mAlternateVaultFileMasterList.Any())).ToList();

            // get the list of files NOT CURRENTLY IN THE VAULT
            List<CO_IDB_VaultFileMaster> filesNotInVaultAtAll = CO_IDB_VaultFileMaster.mCO_IDB_VaultFileMaster_List.Where(coivfm => ((coivfm.mVaultFileMaster == null) && !coivfm.mAlternateVaultFileMasterList.Any())).ToList();

            // generate a report

            // report on what was found, what was not
            Logger.Debug(string.Format("Total Delta Files To Load:\t\t\t\t{0:n0}", mCOIntDbSqlCache.m_CO_FileMasterList.Count()));
            Logger.Debug(string.Format("Total File Masters In Vault:\t\t\t\t{0:n0}", mVaultCache.m_VaultFileMasterDict.Count()));
            Logger.Debug(string.Format("Total File Iterations In Vault:\t\t\t\t{0:n0}", mVaultCache.m_VaultFileIterationDict.Count()));

            Logger.Debug(string.Format("Delta Files Already In Vault Matching File Path:\t\t{0:n0}", filesMatchingFullPath.Count()));
            Logger.Debug(string.Format("Delta Files With Different Local/Vault Checksum:\t\t{0:n0}", filesWithDiffLocalVaultChecksum.Count()));
            Logger.Debug(string.Format("Delta Files NOT Matching File Path:\t\t\t{0:n0}", CO_IDB_VaultFileMaster.mCO_IDB_VaultFileMaster_List.Count() - filesMatchingFullPath.Count()));

            Logger.Debug(string.Format("Delta Files Already In Vault Matching File Name:\t\t{0:n0}", filesMatchingFileName.Count()));
            Logger.Debug(string.Format("Delta Files In Vault Matching File Name/Not Path:\t\t{0:n0}", filesOnlyMatchingFileName.Count()));
            Logger.Debug(string.Format("Delta Files NOT Matching ANY File Names In Vault:\t{0:n0}", filesNotInVaultAtAll.Count()));

            string matchingFilenameOnlyReportFile = @"C:\Temp\IDB.Discover.Vault.DeltaMatchingNameOnly.txt";
            string filenameNotInVaultReportFile = @"C:\Temp\IDB.Discover.Vault.DeltaFilenameNotInVault.txt";
            string deltaFilesInIDBWithDiffLocalVaultChecksum = @"C:\Temp\IDB.Discover.Vault.DeltaFileDiffChecksums.txt";
            Logger.Debug(string.Format("Review report files \"{0}\", \"{1}\", and \"{2}\"", matchingFilenameOnlyReportFile, filenameNotInVaultReportFile, deltaFilesInIDBWithDiffLocalVaultChecksum));

            // generate report for files found in Vault but not in same folder
            List<string> outputList = new List<string>();
            outputList.Add("FileName\tExten\tExpVaultPath\tFoundVaultPath");
            outputList.AddRange(filesOnlyMatchingFileName.Select(coivfm => coivfm.mCO_FileMaster.mName + "\t" +
                                                                            coivfm.mCO_FileMaster.mExtension + "\t" + 
                                                                            coivfm.mCO_FileMaster.mFullVaultFilePath + 
                                                                            coivfm.mAlternateVaultFileMasterList.First().GetVaultFileMasterFullPathName()));

            // write the report file 
            File.WriteAllLines(matchingFilenameOnlyReportFile, outputList);


            // generate report for files NOT found in Vault
            outputList = new List<string>();
            outputList.Add("FileName\tExten\tExpVaultPath");
            outputList.AddRange(filesNotInVaultAtAll.Select(coivfm => coivfm.mCO_FileMaster.mName + "\t" +
                                                                            coivfm.mCO_FileMaster.mExtension + "\t" +
                                                                            coivfm.mCO_FileMaster.mFullVaultFilePath));

            // write the report file 
            File.WriteAllLines(filenameNotInVaultReportFile, outputList);


            // generate report for files with different checksums
            // these are the true delta files
            outputList = new List<string>();
            outputList.Add("FileName\tExten\tExpVaultPath");
            outputList.AddRange(filesWithDiffLocalVaultChecksum.Select(coivfm => coivfm.mCO_FileMaster.mName + "\t" +
                                                                            coivfm.mCO_FileMaster.mExtension + "\t" +
                                                                            coivfm.mCO_FileMaster.mFullVaultFilePath));

            // write the report file 
            File.WriteAllLines(deltaFilesInIDBWithDiffLocalVaultChecksum, outputList);


        }


        #endregion AnalyzeIDBAndVault

        #region IDB Folders fix methods

        private void FixIDBFoldersButton_Click(object sender, EventArgs e)
        {
            if (!UpdateDBFixFoldersCheckBox.Checked || (MessageBox.Show("This will Update Folders table in IDB: Continue?", "Vault Folders Integrity Process", MessageBoxButtons.YesNo) == DialogResult.Yes))
            {
                Logger.Debug(string.Format("FixIDBFolders Button"));

                mProcessingStopwatch = new StopwatchPlus();

                // create the delegate
                mUpdateTextDelegate = new UpdateTextDelegate(appendRichTextBox);
                Logger.SetUpdateTextDelegate(mUpdateTextDelegate);

                // setup a worker thread to process the content
                ThreadStart starter = FixIDBFolders;
                starter += () =>
                {
                // Do what you want in the callback
                Cursor.Current = Cursors.Default;

                    Logger.Debug(string.Format("FixIDBFolders Total Time {0}", mProcessingStopwatch.ElapsedTimeString()));

                    mProcessingStopwatch = null;

                // display a completed dialog
                MessageBox.Show("FixIDBFolders Done");
                };

                mProcessWorkerThread = new Thread(starter) { IsBackground = true };
                mProcessWorkerThread.Start();
            }
        }

        private void FixIDBFolders()
        {
            // get the source information
            string sSrcVaultConnString = COIDB_ConnectionStringTextBox.Text;

            // get the source cache
            MCDD.CO_IntermediateDbSqlCache sqlCache = new MCDD.CO_IntermediateDbSqlCache(sSrcVaultConnString);

            sqlCache.LoadInitialForFoldersRepair();

            // process the folders and verify they have no issues
            sqlCache.RepairIDBFolders(UpdateDBFixFoldersCheckBox.Checked);
        }

        #endregion IDB Folders fix methods

        #region modify "Vault.xml" file and adjust the FileIterations as FileIterationRefs


        // generate a report on file comparisons for IDB Delta and the target Vault
        private void UpdateVaultXmlDeltaButton_Click(object sender, EventArgs e)
        {
            if ((MessageBox.Show("This will Update The Vault.xml File: Continue?", "Prep Vault.xml for Delta Loading", MessageBoxButtons.YesNo) == DialogResult.Yes))
            {

                Logger.Debug(string.Format("UpdateVaultXmlDelta Button"));

                mProcessingStopwatch = new StopwatchPlus();

                // create the delegate
                mUpdateTextDelegate = new UpdateTextDelegate(appendRichTextBox);
                Logger.SetUpdateTextDelegate(mUpdateTextDelegate);

                // setup a worker thread to process the content
                ThreadStart starter = UpdateVaultXmlDelta;
                starter += () =>
                {
                    // Do what you want in the callback
                    Cursor.Current = Cursors.Default;

                    Logger.Debug(string.Format("UpdateVaultXmlDelta Total Time {0}", mProcessingStopwatch.ElapsedTimeString()));

                    mProcessingStopwatch = null;

                    // display a completed dialog
                    MessageBox.Show("UpdateVaultXmlDelta Done");
                };

                mProcessWorkerThread = new Thread(starter) { IsBackground = true };
                mProcessWorkerThread.Start();
            }
        }


        private void UpdateVaultXmlDelta()
        {
            string sVaultXmlFile = Path.Combine(txtExportDirectory.Text, "Vault.xml");
            VaultXmlManipulator.ModifyVaultXmlFile(sVaultXmlFile, CO_IDB_VaultFileMaster.mCO_IDB_VaultFileMaster_List);
        }

        #endregion modify "Vault.xml" file and adjust the FileIterations as FileIterationRefs

    }
}
