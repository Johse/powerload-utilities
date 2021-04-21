namespace IDB.Load.BCP
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
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
            this.SuspendLayout();
            // 
            // connectionTxtBox
            // 
            this.connectionTxtBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.connectionTxtBox.Location = new System.Drawing.Point(14, 100);
            this.connectionTxtBox.Multiline = true;
            this.connectionTxtBox.Name = "connectionTxtBox";
            this.connectionTxtBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.connectionTxtBox.Size = new System.Drawing.Size(572, 62);
            this.connectionTxtBox.TabIndex = 0;
            this.connectionTxtBox.TextChanged += new System.EventHandler(this.connectionTxtBox_TextChanged);
            // 
            // startBtn
            // 
            this.startBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.startBtn.BackColor = System.Drawing.SystemColors.Control;
            this.startBtn.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startBtn.Location = new System.Drawing.Point(481, 263);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(105, 40);
            this.startBtn.TabIndex = 11;
            this.startBtn.Text = "Start";
            this.startBtn.UseVisualStyleBackColor = true;
            this.startBtn.Click += new System.EventHandler(this.startBtn_Click);
            // 
            // dataPathTxtBox
            // 
            this.dataPathTxtBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataPathTxtBox.Location = new System.Drawing.Point(14, 38);
            this.dataPathTxtBox.Multiline = true;
            this.dataPathTxtBox.Name = "dataPathTxtBox";
            this.dataPathTxtBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataPathTxtBox.Size = new System.Drawing.Size(500, 28);
            this.dataPathTxtBox.TabIndex = 12;
            this.dataPathTxtBox.TextChanged += new System.EventHandler(this.dataPathTxtBox_TextChanged);
            // 
            // chooseBtn
            // 
            this.chooseBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chooseBtn.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chooseBtn.ImageAlign = System.Drawing.ContentAlignment.TopRight;
            this.chooseBtn.Location = new System.Drawing.Point(522, 38);
            this.chooseBtn.Name = "chooseBtn";
            this.chooseBtn.Size = new System.Drawing.Size(64, 29);
            this.chooseBtn.TabIndex = 13;
            this.chooseBtn.Text = "...";
            this.chooseBtn.UseVisualStyleBackColor = true;
            this.chooseBtn.Click += new System.EventHandler(this.chooseBtn_Click);
            // 
            // CancelBtn
            // 
            this.CancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelBtn.BackColor = System.Drawing.SystemColors.Control;
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Enabled = false;
            this.CancelBtn.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CancelBtn.Location = new System.Drawing.Point(370, 263);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(105, 40);
            this.CancelBtn.TabIndex = 16;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // workerFolders
            // 
            this.workerFolders.DoWork += new System.ComponentModel.DoWorkEventHandler(this.workerFolders_DoWork);
            this.workerFolders.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.WorkerFolders_ProgressChanged);
            this.workerFolders.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.WorkerFolders_RunWorkerCompleted);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(14, 194);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(572, 33);
            this.progressBar1.TabIndex = 17;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(10, 15);
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
            this.label2.Location = new System.Drawing.Point(10, 80);
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
            this.label3.Location = new System.Drawing.Point(10, 174);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 17);
            this.label3.TabIndex = 20;
            this.label3.Text = "Progress Total:";
            // 
            // ScanedFilesQantity
            // 
            this.ScanedFilesQantity.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ScanedFilesQantity.Location = new System.Drawing.Point(10, 230);
            this.ScanedFilesQantity.Name = "ScanedFilesQantity";
            this.ScanedFilesQantity.Size = new System.Drawing.Size(169, 29);
            this.ScanedFilesQantity.TabIndex = 21;
            this.ScanedFilesQantity.Text = "Scanned Files:";
            this.ScanedFilesQantity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // processPart
            // 
            this.processPart.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.processPart.Location = new System.Drawing.Point(10, 259);
            this.processPart.Name = "processPart";
            this.processPart.Size = new System.Drawing.Size(197, 27);
            this.processPart.TabIndex = 22;
            // 
            // workerFileFileRelations
            // 
            this.workerFileFileRelations.DoWork += new System.ComponentModel.DoWorkEventHandler(this.WorkerFileFileRelations_DoWork);
            this.workerFileFileRelations.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.WorkerFileFileRelations_ProgressChanged);
            this.workerFileFileRelations.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.WorkerFileFileRelations_RunWorkerCompleted);
            // 
            // workerItems
            // 
            this.workerItems.DoWork += new System.ComponentModel.DoWorkEventHandler(this.workerItems_DoWork);
            this.workerItems.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.workerItems_ProgressChanged);
            this.workerItems.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.workerItems_RunWorkerCompleted);
            // 
            // workerItemItemsRelation
            // 
            this.workerItemItemsRelation.DoWork += new System.ComponentModel.DoWorkEventHandler(this.workerItemItemsRelation_DoWork);
            this.workerItemItemsRelation.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.workerItemItemsRelation_ProgressChanged);
            this.workerItemItemsRelation.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.workerItemItemsRelation_RunWorkerCompleted);
            // 
            // DataScanner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 315);
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
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "DataScanner";
            this.Text = "IDB.Load.BCP";
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
    }
}

