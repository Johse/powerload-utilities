namespace IDB.Load.BCP
{
    partial class DataScanner
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataScanner));
            this.connectionTxtBox = new System.Windows.Forms.TextBox();
            this.startBtn = new System.Windows.Forms.Button();
            this.dataPathTxtBox = new System.Windows.Forms.TextBox();
            this.chooseBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            this.workerFolders = new System.ComponentModel.BackgroundWorker();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ScanedFilesQantity = new System.Windows.Forms.Label();
            this.processPart = new System.Windows.Forms.Label();
            this.workerFileFileRelations = new System.ComponentModel.BackgroundWorker();
            this.workerItems = new System.ComponentModel.BackgroundWorker();
            this.workerItemItemsRelation = new System.ComponentModel.BackgroundWorker();
            this.checkBoxInsertItem = new System.Windows.Forms.CheckBox();
            this.buttonOpenLog = new System.Windows.Forms.Button();
            this.InsertItemItemRelations = new System.Windows.Forms.CheckBox();
            this.ValidateBtn = new System.Windows.Forms.Button();
            this.ItemsInsert = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // connectionTxtBox
            // 
            this.connectionTxtBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.connectionTxtBox.Location = new System.Drawing.Point(12, 95);
            this.connectionTxtBox.Multiline = true;
            this.connectionTxtBox.Name = "connectionTxtBox";
            this.connectionTxtBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.connectionTxtBox.Size = new System.Drawing.Size(588, 54);
            this.connectionTxtBox.TabIndex = 0;
            // 
            // startBtn
            // 
            this.startBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.startBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.startBtn.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startBtn.Location = new System.Drawing.Point(598, 283);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(91, 42);
            this.startBtn.TabIndex = 11;
            this.startBtn.Text = "Start";
            this.startBtn.UseVisualStyleBackColor = true;
            this.startBtn.Click += new System.EventHandler(this.startBtn_Click);
            // 
            // dataPathTxtBox
            // 
            this.dataPathTxtBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataPathTxtBox.Location = new System.Drawing.Point(12, 33);
            this.dataPathTxtBox.Multiline = true;
            this.dataPathTxtBox.Name = "dataPathTxtBox";
            this.dataPathTxtBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataPathTxtBox.Size = new System.Drawing.Size(588, 25);
            this.dataPathTxtBox.TabIndex = 12;
            // 
            // chooseBtn
            // 
            this.chooseBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chooseBtn.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chooseBtn.ImageAlign = System.Drawing.ContentAlignment.TopRight;
            this.chooseBtn.Location = new System.Drawing.Point(606, 33);
            this.chooseBtn.Name = "chooseBtn";
            this.chooseBtn.Size = new System.Drawing.Size(55, 25);
            this.chooseBtn.TabIndex = 13;
            this.chooseBtn.Text = "...";
            this.chooseBtn.UseVisualStyleBackColor = true;
            this.chooseBtn.Click += new System.EventHandler(this.chooseBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CancelBtn.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CancelBtn.Location = new System.Drawing.Point(501, 283);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(91, 42);
            this.CancelBtn.TabIndex = 16;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // backgroundWorker1
            // 
            this.workerFolders.DoWork += new System.ComponentModel.DoWorkEventHandler(this.workerFolders_DoWork);
            this.workerFolders.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.WorkerFolders_ProgressChanged);
            this.workerFolders.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.WorkerFolders_RunWorkerCompleted);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 205);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(677, 29);
            this.progressBar1.TabIndex = 17;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(173, 17);
            this.label1.TabIndex = 18;
            this.label1.Text = "Path to BCP export package:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(9, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(197, 17);
            this.label2.TabIndex = 19;
            this.label2.Text = "SQL Database Connection String";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(12, 180);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 17);
            this.label3.TabIndex = 20;
            this.label3.Text = "Progress Total:";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // ScanedFilesQantity
            // 
            this.ScanedFilesQantity.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ScanedFilesQantity.Location = new System.Drawing.Point(9, 258);
            this.ScanedFilesQantity.Name = "ScanedFilesQantity";
            this.ScanedFilesQantity.Size = new System.Drawing.Size(145, 25);
            this.ScanedFilesQantity.TabIndex = 21;
            this.ScanedFilesQantity.Text = "Scanned Files:";
            // 
            // processPart
            // 
            this.processPart.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.processPart.Location = new System.Drawing.Point(12, 283);
            this.processPart.Name = "processPart";
            this.processPart.Size = new System.Drawing.Size(254, 23);
            this.processPart.TabIndex = 22;
            // 
            // backgroundWorker2
            // 
            this.workerFileFileRelations.DoWork += new System.ComponentModel.DoWorkEventHandler(this.WorkerFileFileRelations_DoWork);
            this.workerFileFileRelations.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.WorkerFileFileRelations_ProgressChanged);
            this.workerFileFileRelations.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.WorkerFileFileRelations_RunWorkerCompleted);
            // 
            // backgroundWorker3
            // 
            this.workerItems.DoWork += new System.ComponentModel.DoWorkEventHandler(this.workerItems_DoWork);
            this.workerItems.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.workerItems_ProgressChanged);
            this.workerItems.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.workerItems_RunWorkerCompleted);
            // 
            // backgroundWorker4
            // 
            this.workerItemItemsRelation.DoWork += new System.ComponentModel.DoWorkEventHandler(this.workerItemItemsRelation_DoWork);
            this.workerItemItemsRelation.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.workerItemItemsRelation_ProgressChanged);
            this.workerItemItemsRelation.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.workerItemItemsRelation_RunWorkerCompleted);
            // 
            // checkBoxInsertItem
            // 
            this.checkBoxInsertItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxInsertItem.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxInsertItem.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxInsertItem.Location = new System.Drawing.Point(576, 155);
            this.checkBoxInsertItem.Name = "checkBoxInsertItem";
            this.checkBoxInsertItem.Size = new System.Drawing.Size(98, 24);
            this.checkBoxInsertItem.TabIndex = 24;
            this.checkBoxInsertItem.Text = "Insert Item";
            this.checkBoxInsertItem.UseVisualStyleBackColor = true;
            // 
            // buttonOpenLog
            // 
            this.buttonOpenLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpenLog.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonOpenLog.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonOpenLog.Location = new System.Drawing.Point(394, 283);
            this.buttonOpenLog.Name = "buttonOpenLog";
            this.buttonOpenLog.Size = new System.Drawing.Size(101, 42);
            this.buttonOpenLog.TabIndex = 25;
            this.buttonOpenLog.Text = "Open log file";
            this.buttonOpenLog.UseVisualStyleBackColor = true;
            this.buttonOpenLog.Click += new System.EventHandler(this.buttonOpenLog_Click);
            // 
            // InsertItemItemRelations
            // 
            this.InsertItemItemRelations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.InsertItemItemRelations.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.InsertItemItemRelations.Location = new System.Drawing.Point(576, 182);
            this.InsertItemItemRelations.Name = "InsertItemItemRelations";
            this.InsertItemItemRelations.Size = new System.Drawing.Size(139, 17);
            this.InsertItemItemRelations.TabIndex = 27;
            this.InsertItemItemRelations.Text = "Insert ItemItemRelations";
            this.InsertItemItemRelations.UseVisualStyleBackColor = true;
            // 
            // ValidateBtn
            // 
            this.ValidateBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ValidateBtn.Enabled = false;
            this.ValidateBtn.Location = new System.Drawing.Point(283, 283);
            this.ValidateBtn.Name = "ValidateBtn";
            this.ValidateBtn.Size = new System.Drawing.Size(107, 42);
            this.ValidateBtn.TabIndex = 28;
            this.ValidateBtn.Text = "Validate";
            this.ValidateBtn.UseVisualStyleBackColor = true;
            this.ValidateBtn.Visible = false;
            this.ValidateBtn.Click += new System.EventHandler(this.ValidateBtn_Click);
            // 
            // ItemsInsert
            // 
            this.ItemsInsert.Location = new System.Drawing.Point(283, 346);
            this.ItemsInsert.Name = "ItemsInsert";
            this.ItemsInsert.Size = new System.Drawing.Size(75, 23);
            this.ItemsInsert.TabIndex = 29;
            this.ItemsInsert.Text = "Insert Items and Relations";
            this.ItemsInsert.UseVisualStyleBackColor = true;
            this.ItemsInsert.Visible = false;
            this.ItemsInsert.Click += new System.EventHandler(this.ItemsInsert_Click);
            // 
            // DataScanner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(721, 336);
            this.Controls.Add(this.ItemsInsert);
            this.Controls.Add(this.ValidateBtn);
            this.Controls.Add(this.InsertItemItemRelations);
            this.Controls.Add(this.buttonOpenLog);
            this.Controls.Add(this.checkBoxInsertItem);
            this.Controls.Add(this.processPart);
            this.Controls.Add(this.ScanedFilesQantity);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.chooseBtn);
            this.Controls.Add(this.dataPathTxtBox);
            this.Controls.Add(this.startBtn);
            this.Controls.Add(this.connectionTxtBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "DataScanner";
            this.Text = "IDB.Load.BCP";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox connectionTxtBox;
        private System.Windows.Forms.Button startBtn;
        private System.Windows.Forms.TextBox dataPathTxtBox;
        private System.Windows.Forms.Button chooseBtn;
        private System.Windows.Forms.Button CancelBtn;
        private System.ComponentModel.BackgroundWorker workerFolders;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label ScanedFilesQantity;
        private System.Windows.Forms.Label processPart;
        private System.ComponentModel.BackgroundWorker workerFileFileRelations;
        private System.ComponentModel.BackgroundWorker workerItems;
        private System.ComponentModel.BackgroundWorker workerItemItemsRelation;
        private System.Windows.Forms.CheckBox checkBoxInsertItem;
        private System.Windows.Forms.Button buttonOpenLog;
        private System.Windows.Forms.CheckBox InsertItemItemRelations;
        private System.Windows.Forms.Button ValidateBtn;
        private System.Windows.Forms.Button ItemsInsert;
    }
}

