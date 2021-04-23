namespace IDB.Translate.BCP
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtConnectionString = new System.Windows.Forms.TextBox();
            this.btnExport = new System.Windows.Forms.Button();
            this.txtExportDirectory = new System.Windows.Forms.TextBox();
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxVaultVersion = new System.Windows.Forms.ComboBox();
            this.progressBarTask = new System.Windows.Forms.ProgressBar();
            this.labelProgressTask = new System.Windows.Forms.Label();
            this.progressBarTotal = new System.Windows.Forms.ProgressBar();
            this.labelProgressTotal = new System.Windows.Forms.Label();
            this.DisableConfigurationExportCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(148, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "IDB SQL Connection String";
            // 
            // txtConnectionString
            // 
            this.txtConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConnectionString.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtConnectionString.Location = new System.Drawing.Point(12, 27);
            this.txtConnectionString.Name = "txtConnectionString";
            this.txtConnectionString.Size = new System.Drawing.Size(460, 23);
            this.txtConnectionString.TabIndex = 1;
            this.txtConnectionString.TextChanged += new System.EventHandler(this.OnTxtConnectionStringTextChanged);
            // 
            // btnExport
            // 
            this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExport.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExport.Location = new System.Drawing.Point(12, 186);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(460, 32);
            this.btnExport.TabIndex = 8;
            this.btnExport.Text = "Create BCP Package";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.OnBtnExportClick);
            // 
            // txtExportDirectory
            // 
            this.txtExportDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtExportDirectory.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtExportDirectory.Location = new System.Drawing.Point(12, 115);
            this.txtExportDirectory.Name = "txtExportDirectory";
            this.txtExportDirectory.Size = new System.Drawing.Size(429, 23);
            this.txtExportDirectory.TabIndex = 5;
            this.txtExportDirectory.TextChanged += new System.EventHandler(this.OnTxtExportDirectoryTextChanged);
            // 
            // btnSelect
            // 
            this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelect.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelect.Location = new System.Drawing.Point(447, 114);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(25, 25);
            this.btnSelect.TabIndex = 6;
            this.btnSelect.Text = "...";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.OnBtnSelectClick);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(322, 337);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(150, 32);
            this.btnClose.TabIndex = 12;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.OnBtnCloseClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 97);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "BCP Export Directory";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(12, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = "Vault Version";
            // 
            // comboBoxVaultVersion
            // 
            this.comboBoxVaultVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxVaultVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxVaultVersion.FormattingEnabled = true;
            this.comboBoxVaultVersion.Items.AddRange(new object[] {
            "2020",
            "2019",
            "2018",
            "2017"});
            this.comboBoxVaultVersion.Location = new System.Drawing.Point(12, 71);
            this.comboBoxVaultVersion.Name = "comboBoxVaultVersion";
            this.comboBoxVaultVersion.Size = new System.Drawing.Size(134, 23);
            this.comboBoxVaultVersion.TabIndex = 3;
            this.comboBoxVaultVersion.TextChanged += new System.EventHandler(this.OnComboBoxVaultVersionTextChanged);
            // 
            // progressBarTask
            // 
            this.progressBarTask.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarTask.Location = new System.Drawing.Point(12, 288);
            this.progressBarTask.Name = "progressBarTask";
            this.progressBarTask.Size = new System.Drawing.Size(460, 23);
            this.progressBarTask.Step = 1;
            this.progressBarTask.TabIndex = 11;
            // 
            // labelProgressTask
            // 
            this.labelProgressTask.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelProgressTask.AutoSize = true;
            this.labelProgressTask.Location = new System.Drawing.Point(12, 288);
            this.labelProgressTask.Name = "labelProgressTask";
            this.labelProgressTask.Size = new System.Drawing.Size(0, 13);
            this.labelProgressTask.TabIndex = 11;
            // 
            // progressBarTotal
            // 
            this.progressBarTotal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarTotal.Location = new System.Drawing.Point(12, 259);
            this.progressBarTotal.Name = "progressBarTotal";
            this.progressBarTotal.Size = new System.Drawing.Size(460, 23);
            this.progressBarTotal.Step = 1;
            this.progressBarTotal.TabIndex = 10;
            // 
            // labelProgressTotal
            // 
            this.labelProgressTotal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelProgressTotal.AutoSize = true;
            this.labelProgressTotal.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelProgressTotal.Location = new System.Drawing.Point(12, 241);
            this.labelProgressTotal.Name = "labelProgressTotal";
            this.labelProgressTotal.Size = new System.Drawing.Size(83, 15);
            this.labelProgressTotal.TabIndex = 9;
            this.labelProgressTotal.Text = "Progress Total:";
            // 
            // DisableConfigurationExportCheckBox
            // 
            this.DisableConfigurationExportCheckBox.AutoSize = true;
            this.DisableConfigurationExportCheckBox.Checked = true;
            this.DisableConfigurationExportCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DisableConfigurationExportCheckBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.DisableConfigurationExportCheckBox.Location = new System.Drawing.Point(12, 162);
            this.DisableConfigurationExportCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.DisableConfigurationExportCheckBox.Name = "DisableConfigurationExportCheckBox";
            this.DisableConfigurationExportCheckBox.Size = new System.Drawing.Size(178, 19);
            this.DisableConfigurationExportCheckBox.TabIndex = 7;
            this.DisableConfigurationExportCheckBox.Text = "Disable Configuration Export";
            this.DisableConfigurationExportCheckBox.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(484, 381);
            this.Controls.Add(this.DisableConfigurationExportCheckBox);
            this.Controls.Add(this.labelProgressTotal);
            this.Controls.Add(this.progressBarTotal);
            this.Controls.Add(this.labelProgressTask);
            this.Controls.Add(this.progressBarTask);
            this.Controls.Add(this.comboBoxVaultVersion);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.txtExportDirectory);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.txtConnectionString);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(500, 420);
            this.Name = "MainForm";
            this.Text = "IDB.Translate.BCP";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtConnectionString;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.TextBox txtExportDirectory;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxVaultVersion;
        private System.Windows.Forms.ProgressBar progressBarTask;
        private System.Windows.Forms.Label labelProgressTask;
        private System.Windows.Forms.ProgressBar progressBarTotal;
        private System.Windows.Forms.Label labelProgressTotal;
        private System.Windows.Forms.CheckBox DisableConfigurationExportCheckBox;
    }
}

