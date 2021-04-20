namespace IDB.Load.Files
{
    partial class SQLError
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SQLError));
			this.SQLErrorlbl = new System.Windows.Forms.Label();
			this.OkBtn = new System.Windows.Forms.Button();
			this.detailsBtn = new System.Windows.Forms.Button();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// SQLErrorlbl
			// 
			this.SQLErrorlbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SQLErrorlbl.Location = new System.Drawing.Point(82, 28);
			this.SQLErrorlbl.Name = "SQLErrorlbl";
			this.SQLErrorlbl.Size = new System.Drawing.Size(214, 48);
			this.SQLErrorlbl.TabIndex = 0;
			this.SQLErrorlbl.Click += new System.EventHandler(this.SQLErrorlbl_Click);
			// 
			// OkBtn
			// 
			this.OkBtn.Location = new System.Drawing.Point(210, 112);
			this.OkBtn.Name = "OkBtn";
			this.OkBtn.Size = new System.Drawing.Size(87, 23);
			this.OkBtn.TabIndex = 2;
			this.OkBtn.Text = "OK";
			this.OkBtn.UseVisualStyleBackColor = true;
			this.OkBtn.Click += new System.EventHandler(this.OkBtn_Click);
			// 
			// detailsBtn
			// 
			this.detailsBtn.Location = new System.Drawing.Point(12, 112);
			this.detailsBtn.Name = "detailsBtn";
			this.detailsBtn.Size = new System.Drawing.Size(85, 23);
			this.detailsBtn.TabIndex = 4;
			this.detailsBtn.Text = "Details";
			this.detailsBtn.UseVisualStyleBackColor = true;
			this.detailsBtn.Click += new System.EventHandler(this.detailsBtn_Click);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(12, 12);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(64, 64);
			this.pictureBox1.TabIndex = 6;
			this.pictureBox1.TabStop = false;
			// 
			// SQLError
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(309, 147);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.detailsBtn);
			this.Controls.Add(this.OkBtn);
			this.Controls.Add(this.SQLErrorlbl);
			this.MaximizeBox = false;
			this.Name = "SQLError";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Error";
			this.TopMost = true;
			this.Load += new System.EventHandler(this.SQLError_Load);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label SQLErrorlbl;
        private System.Windows.Forms.Button OkBtn;
        private System.Windows.Forms.Button detailsBtn;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}