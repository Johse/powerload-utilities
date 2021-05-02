namespace IDB.Discover.Vault
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
            this.labelProgressTask = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtExportDirectory = new System.Windows.Forms.TextBox();
            this.COIDB_ConnectionStringTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.mProcessStopWatchTextBox = new System.Windows.Forms.TextBox();
            this.m_StatusRichTextBox = new System.Windows.Forms.RichTextBox();
            this.LoadIDBButton = new System.Windows.Forms.Button();
            this.ProcessLocalFileButton = new System.Windows.Forms.Button();
            this.SetupColumnsButton = new System.Windows.Forms.Button();
            this.MoveMissingFilesButton = new System.Windows.Forms.Button();
            this.UpdateChecksumInDBButton = new System.Windows.Forms.Button();
            this.LoadVaultFilesButton = new System.Windows.Forms.Button();
            this.Vault_ConnectionStringTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.KVM_DatabaseNameTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.AnalyzeIDBAndVaultButton = new System.Windows.Forms.Button();
            this.FixIDBFoldersButton = new System.Windows.Forms.Button();
            this.UpdateDBFixFoldersCheckBox = new System.Windows.Forms.CheckBox();
            this.UpdateDBAndIDBVaultCheckBox = new System.Windows.Forms.CheckBox();
            this.UpdateLocalChecksumsCheckBox = new System.Windows.Forms.CheckBox();
            this.UpdateVaultXmlDeltaButton = new System.Windows.Forms.Button();
            this.CreateXCopyButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelProgressTask
            // 
            this.labelProgressTask.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelProgressTask.AutoSize = true;
            this.labelProgressTask.Location = new System.Drawing.Point(9, 304);
            this.labelProgressTask.Name = "labelProgressTask";
            this.labelProgressTask.Size = new System.Drawing.Size(0, 13);
            this.labelProgressTask.TabIndex = 25;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(9, 248);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "BCP Export Directory";
            // 
            // txtExportDirectory
            // 
            this.txtExportDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtExportDirectory.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtExportDirectory.Location = new System.Drawing.Point(11, 264);
            this.txtExportDirectory.Name = "txtExportDirectory";
            this.txtExportDirectory.Size = new System.Drawing.Size(397, 22);
            this.txtExportDirectory.TabIndex = 17;
            this.txtExportDirectory.Text = "C:\\TEMP\\BCP Package Directory";
            // 
            // COIDB_ConnectionStringTextBox
            // 
            this.COIDB_ConnectionStringTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.COIDB_ConnectionStringTextBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.COIDB_ConnectionStringTextBox.Location = new System.Drawing.Point(11, 22);
            this.COIDB_ConnectionStringTextBox.Name = "COIDB_ConnectionStringTextBox";
            this.COIDB_ConnectionStringTextBox.Size = new System.Drawing.Size(397, 22);
            this.COIDB_ConnectionStringTextBox.TabIndex = 15;
            this.COIDB_ConnectionStringTextBox.Text = "Server=(local)\\AUTODESKVAULT;Database=MDVaultHistoryDiff;Trusted_Connection=True;" +
    "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(214, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "CO IDB SQL Database Connection String";
            // 
            // mProcessStopWatchTextBox
            // 
            this.mProcessStopWatchTextBox.Location = new System.Drawing.Point(11, 313);
            this.mProcessStopWatchTextBox.Name = "mProcessStopWatchTextBox";
            this.mProcessStopWatchTextBox.ReadOnly = true;
            this.mProcessStopWatchTextBox.Size = new System.Drawing.Size(167, 20);
            this.mProcessStopWatchTextBox.TabIndex = 32;
            // 
            // m_StatusRichTextBox
            // 
            this.m_StatusRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_StatusRichTextBox.Location = new System.Drawing.Point(11, 339);
            this.m_StatusRichTextBox.Name = "m_StatusRichTextBox";
            this.m_StatusRichTextBox.ReadOnly = true;
            this.m_StatusRichTextBox.Size = new System.Drawing.Size(1060, 413);
            this.m_StatusRichTextBox.TabIndex = 33;
            this.m_StatusRichTextBox.Text = "";
            // 
            // LoadIDBButton
            // 
            this.LoadIDBButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadIDBButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoadIDBButton.Location = new System.Drawing.Point(552, 19);
            this.LoadIDBButton.Name = "LoadIDBButton";
            this.LoadIDBButton.Size = new System.Drawing.Size(188, 22);
            this.LoadIDBButton.TabIndex = 34;
            this.LoadIDBButton.Text = "Load IDB";
            this.LoadIDBButton.UseVisualStyleBackColor = true;
            this.LoadIDBButton.Click += new System.EventHandler(this.LoadIDBButton_Click);
            // 
            // ProcessLocalFileButton
            // 
            this.ProcessLocalFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ProcessLocalFileButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ProcessLocalFileButton.Location = new System.Drawing.Point(552, 69);
            this.ProcessLocalFileButton.Name = "ProcessLocalFileButton";
            this.ProcessLocalFileButton.Size = new System.Drawing.Size(188, 22);
            this.ProcessLocalFileButton.TabIndex = 35;
            this.ProcessLocalFileButton.Text = "Process Local File Checksums";
            this.ProcessLocalFileButton.UseVisualStyleBackColor = true;
            this.ProcessLocalFileButton.Click += new System.EventHandler(this.ProcessLocalFileButton_Click);
            // 
            // SetupColumnsButton
            // 
            this.SetupColumnsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SetupColumnsButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SetupColumnsButton.Location = new System.Drawing.Point(960, 6);
            this.SetupColumnsButton.Name = "SetupColumnsButton";
            this.SetupColumnsButton.Size = new System.Drawing.Size(111, 22);
            this.SetupColumnsButton.TabIndex = 36;
            this.SetupColumnsButton.Text = "Setup Columns";
            this.SetupColumnsButton.UseVisualStyleBackColor = true;
            this.SetupColumnsButton.Click += new System.EventHandler(this.SetupColumnsButton_Click);
            // 
            // MoveMissingFilesButton
            // 
            this.MoveMissingFilesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MoveMissingFilesButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MoveMissingFilesButton.Location = new System.Drawing.Point(960, 64);
            this.MoveMissingFilesButton.Name = "MoveMissingFilesButton";
            this.MoveMissingFilesButton.Size = new System.Drawing.Size(111, 22);
            this.MoveMissingFilesButton.TabIndex = 37;
            this.MoveMissingFilesButton.Text = "Move Missing Files";
            this.MoveMissingFilesButton.UseVisualStyleBackColor = true;
            this.MoveMissingFilesButton.Click += new System.EventHandler(this.MoveMissingFilesButton_Click);
            // 
            // UpdateChecksumInDBButton
            // 
            this.UpdateChecksumInDBButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.UpdateChecksumInDBButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UpdateChecksumInDBButton.Location = new System.Drawing.Point(960, 34);
            this.UpdateChecksumInDBButton.Name = "UpdateChecksumInDBButton";
            this.UpdateChecksumInDBButton.Size = new System.Drawing.Size(111, 22);
            this.UpdateChecksumInDBButton.TabIndex = 38;
            this.UpdateChecksumInDBButton.Text = "Update Checksum in DB";
            this.UpdateChecksumInDBButton.UseVisualStyleBackColor = true;
            this.UpdateChecksumInDBButton.Click += new System.EventHandler(this.UpdateChecksumInDBButton_Click);
            // 
            // LoadVaultFilesButton
            // 
            this.LoadVaultFilesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadVaultFilesButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoadVaultFilesButton.Location = new System.Drawing.Point(552, 136);
            this.LoadVaultFilesButton.Name = "LoadVaultFilesButton";
            this.LoadVaultFilesButton.Size = new System.Drawing.Size(188, 22);
            this.LoadVaultFilesButton.TabIndex = 39;
            this.LoadVaultFilesButton.Text = "Load Vault Files";
            this.LoadVaultFilesButton.UseVisualStyleBackColor = true;
            this.LoadVaultFilesButton.Click += new System.EventHandler(this.LoadVaultFilesButton_Click);
            // 
            // Vault_ConnectionStringTextBox
            // 
            this.Vault_ConnectionStringTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Vault_ConnectionStringTextBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Vault_ConnectionStringTextBox.Location = new System.Drawing.Point(11, 71);
            this.Vault_ConnectionStringTextBox.Name = "Vault_ConnectionStringTextBox";
            this.Vault_ConnectionStringTextBox.Size = new System.Drawing.Size(397, 22);
            this.Vault_ConnectionStringTextBox.TabIndex = 41;
            this.Vault_ConnectionStringTextBox.Text = "Server=(local)\\AUTODESKVAULT;Database=Marvin_Half_DEAN;Trusted_Connection=True;";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(9, 55);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(203, 13);
            this.label4.TabIndex = 40;
            this.label4.Text = "Vault SQL Database Connection String";
            // 
            // KVM_DatabaseNameTextBox
            // 
            this.KVM_DatabaseNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.KVM_DatabaseNameTextBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KVM_DatabaseNameTextBox.Location = new System.Drawing.Point(11, 125);
            this.KVM_DatabaseNameTextBox.Name = "KVM_DatabaseNameTextBox";
            this.KVM_DatabaseNameTextBox.Size = new System.Drawing.Size(397, 22);
            this.KVM_DatabaseNameTextBox.TabIndex = 43;
            this.KVM_DatabaseNameTextBox.Text = "Marvin_Half_Dean_KVM";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(9, 109);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(135, 13);
            this.label5.TabIndex = 42;
            this.label5.Text = "KVM SQL Database Name";
            // 
            // AnalyzeIDBAndVaultButton
            // 
            this.AnalyzeIDBAndVaultButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AnalyzeIDBAndVaultButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AnalyzeIDBAndVaultButton.Location = new System.Drawing.Point(552, 189);
            this.AnalyzeIDBAndVaultButton.Name = "AnalyzeIDBAndVaultButton";
            this.AnalyzeIDBAndVaultButton.Size = new System.Drawing.Size(188, 22);
            this.AnalyzeIDBAndVaultButton.TabIndex = 44;
            this.AnalyzeIDBAndVaultButton.Text = "Analyze IDB And Vault DB";
            this.AnalyzeIDBAndVaultButton.UseVisualStyleBackColor = true;
            this.AnalyzeIDBAndVaultButton.Click += new System.EventHandler(this.AnalyzeIDBAndVaultButton_Click);
            // 
            // FixIDBFoldersButton
            // 
            this.FixIDBFoldersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.FixIDBFoldersButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FixIDBFoldersButton.Location = new System.Drawing.Point(960, 122);
            this.FixIDBFoldersButton.Name = "FixIDBFoldersButton";
            this.FixIDBFoldersButton.Size = new System.Drawing.Size(111, 22);
            this.FixIDBFoldersButton.TabIndex = 45;
            this.FixIDBFoldersButton.Text = "Fix IDB Folders";
            this.FixIDBFoldersButton.UseVisualStyleBackColor = true;
            this.FixIDBFoldersButton.Click += new System.EventHandler(this.FixIDBFoldersButton_Click);
            // 
            // UpdateDBFixFoldersCheckBox
            // 
            this.UpdateDBFixFoldersCheckBox.AutoSize = true;
            this.UpdateDBFixFoldersCheckBox.Location = new System.Drawing.Point(960, 150);
            this.UpdateDBFixFoldersCheckBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.UpdateDBFixFoldersCheckBox.Name = "UpdateDBFixFoldersCheckBox";
            this.UpdateDBFixFoldersCheckBox.Size = new System.Drawing.Size(110, 17);
            this.UpdateDBFixFoldersCheckBox.TabIndex = 46;
            this.UpdateDBFixFoldersCheckBox.Text = "Update Database";
            this.UpdateDBFixFoldersCheckBox.UseVisualStyleBackColor = true;
            // 
            // UpdateDBAndIDBVaultCheckBox
            // 
            this.UpdateDBAndIDBVaultCheckBox.AutoSize = true;
            this.UpdateDBAndIDBVaultCheckBox.Location = new System.Drawing.Point(552, 216);
            this.UpdateDBAndIDBVaultCheckBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.UpdateDBAndIDBVaultCheckBox.Name = "UpdateDBAndIDBVaultCheckBox";
            this.UpdateDBAndIDBVaultCheckBox.Size = new System.Drawing.Size(110, 17);
            this.UpdateDBAndIDBVaultCheckBox.TabIndex = 47;
            this.UpdateDBAndIDBVaultCheckBox.Text = "Update Database";
            this.UpdateDBAndIDBVaultCheckBox.UseVisualStyleBackColor = true;
            // 
            // UpdateLocalChecksumsCheckBox
            // 
            this.UpdateLocalChecksumsCheckBox.AutoSize = true;
            this.UpdateLocalChecksumsCheckBox.Location = new System.Drawing.Point(552, 96);
            this.UpdateLocalChecksumsCheckBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.UpdateLocalChecksumsCheckBox.Name = "UpdateLocalChecksumsCheckBox";
            this.UpdateLocalChecksumsCheckBox.Size = new System.Drawing.Size(110, 17);
            this.UpdateLocalChecksumsCheckBox.TabIndex = 48;
            this.UpdateLocalChecksumsCheckBox.Text = "Update Database";
            this.UpdateLocalChecksumsCheckBox.UseVisualStyleBackColor = true;
            // 
            // UpdateVaultXmlDeltaButton
            // 
            this.UpdateVaultXmlDeltaButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.UpdateVaultXmlDeltaButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UpdateVaultXmlDeltaButton.Location = new System.Drawing.Point(552, 264);
            this.UpdateVaultXmlDeltaButton.Name = "UpdateVaultXmlDeltaButton";
            this.UpdateVaultXmlDeltaButton.Size = new System.Drawing.Size(188, 22);
            this.UpdateVaultXmlDeltaButton.TabIndex = 49;
            this.UpdateVaultXmlDeltaButton.Text = "Update Vault.xml Delta";
            this.UpdateVaultXmlDeltaButton.UseVisualStyleBackColor = true;
            this.UpdateVaultXmlDeltaButton.Click += new System.EventHandler(this.UpdateVaultXmlDeltaButton_Click);
            // 
            // CreateXCopyButton
            // 
            this.CreateXCopyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CreateXCopyButton.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CreateXCopyButton.Location = new System.Drawing.Point(960, 211);
            this.CreateXCopyButton.Name = "CreateXCopyButton";
            this.CreateXCopyButton.Size = new System.Drawing.Size(111, 22);
            this.CreateXCopyButton.TabIndex = 50;
            this.CreateXCopyButton.Text = "Create XCopy";
            this.CreateXCopyButton.UseVisualStyleBackColor = true;
            this.CreateXCopyButton.Click += new System.EventHandler(this.CreateXCopyButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1079, 764);
            this.Controls.Add(this.CreateXCopyButton);
            this.Controls.Add(this.UpdateVaultXmlDeltaButton);
            this.Controls.Add(this.UpdateLocalChecksumsCheckBox);
            this.Controls.Add(this.UpdateDBAndIDBVaultCheckBox);
            this.Controls.Add(this.UpdateDBFixFoldersCheckBox);
            this.Controls.Add(this.FixIDBFoldersButton);
            this.Controls.Add(this.AnalyzeIDBAndVaultButton);
            this.Controls.Add(this.KVM_DatabaseNameTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.Vault_ConnectionStringTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.LoadVaultFilesButton);
            this.Controls.Add(this.UpdateChecksumInDBButton);
            this.Controls.Add(this.MoveMissingFilesButton);
            this.Controls.Add(this.SetupColumnsButton);
            this.Controls.Add(this.ProcessLocalFileButton);
            this.Controls.Add(this.LoadIDBButton);
            this.Controls.Add(this.m_StatusRichTextBox);
            this.Controls.Add(this.mProcessStopWatchTextBox);
            this.Controls.Add(this.labelProgressTask);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtExportDirectory);
            this.Controls.Add(this.COIDB_ConnectionStringTextBox);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "MainForm";
            this.Text = "coolOrange Intermediate Database Vault Discover Utility";
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion
        private System.Windows.Forms.Label labelProgressTask;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtExportDirectory;
        private System.Windows.Forms.TextBox COIDB_ConnectionStringTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox mProcessStopWatchTextBox;
        private System.Windows.Forms.RichTextBox m_StatusRichTextBox;
        private System.Windows.Forms.Button LoadIDBButton;
        private System.Windows.Forms.Button ProcessLocalFileButton;
        private System.Windows.Forms.Button SetupColumnsButton;
        private System.Windows.Forms.Button MoveMissingFilesButton;
        private System.Windows.Forms.Button UpdateChecksumInDBButton;
        private System.Windows.Forms.Button LoadVaultFilesButton;
        private System.Windows.Forms.TextBox Vault_ConnectionStringTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox KVM_DatabaseNameTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button AnalyzeIDBAndVaultButton;
        private System.Windows.Forms.Button FixIDBFoldersButton;
        private System.Windows.Forms.CheckBox UpdateDBFixFoldersCheckBox;
        private System.Windows.Forms.CheckBox UpdateDBAndIDBVaultCheckBox;
        private System.Windows.Forms.CheckBox UpdateLocalChecksumsCheckBox;
        private System.Windows.Forms.Button UpdateVaultXmlDeltaButton;
        private System.Windows.Forms.Button CreateXCopyButton;
    }
}

