namespace SecureBackupExplorer
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.comboBoxBackups = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.listBoxView = new System.Windows.Forms.ListBox();
            this.buttonRestore = new System.Windows.Forms.Button();
            this.textBoxLocation = new System.Windows.Forms.TextBox();
            this.buttonGo = new System.Windows.Forms.Button();
            textBoxLog = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxBackups
            // 
            this.comboBoxBackups.BackColor = System.Drawing.Color.Black;
            this.comboBoxBackups.ForeColor = System.Drawing.Color.White;
            this.comboBoxBackups.FormattingEnabled = true;
            this.comboBoxBackups.Location = new System.Drawing.Point(114, 3);
            this.comboBoxBackups.Name = "comboBoxBackups";
            this.comboBoxBackups.Size = new System.Drawing.Size(236, 21);
            this.comboBoxBackups.TabIndex = 0;
            this.comboBoxBackups.SelectedIndexChanged += new System.EventHandler(this.comboBoxBackups_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Sélection du backup";
            // 
            // listBoxView
            // 
            this.listBoxView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxView.BackColor = System.Drawing.Color.Black;
            this.listBoxView.ForeColor = System.Drawing.Color.White;
            this.listBoxView.FormattingEnabled = true;
            this.listBoxView.Location = new System.Drawing.Point(12, 58);
            this.listBoxView.Name = "listBoxView";
            this.listBoxView.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxView.Size = new System.Drawing.Size(902, 342);
            this.listBoxView.TabIndex = 2;
            this.listBoxView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBoxView_KeyDown);
            this.listBoxView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBoxView_MouseDoubleClick);
            // 
            // buttonRestore
            // 
            this.buttonRestore.AutoSize = true;
            this.buttonRestore.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonRestore.BackColor = System.Drawing.Color.DimGray;
            this.buttonRestore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonRestore.ForeColor = System.Drawing.Color.White;
            this.buttonRestore.Location = new System.Drawing.Point(12, 406);
            this.buttonRestore.Name = "buttonRestore";
            this.buttonRestore.Size = new System.Drawing.Size(251, 25);
            this.buttonRestore.TabIndex = 3;
            this.buttonRestore.Text = "Restaurer les objets sélectionnés dans un dossier";
            this.buttonRestore.UseVisualStyleBackColor = false;
            this.buttonRestore.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBoxLocation
            // 
            this.textBoxLocation.BackColor = System.Drawing.Color.Black;
            this.textBoxLocation.ForeColor = System.Drawing.Color.White;
            this.textBoxLocation.Location = new System.Drawing.Point(12, 32);
            this.textBoxLocation.Name = "textBoxLocation";
            this.textBoxLocation.Size = new System.Drawing.Size(750, 20);
            this.textBoxLocation.TabIndex = 4;
            // 
            // buttonGo
            // 
            this.buttonGo.AutoSize = true;
            this.buttonGo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonGo.BackColor = System.Drawing.Color.DimGray;
            this.buttonGo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonGo.ForeColor = System.Drawing.Color.White;
            this.buttonGo.Location = new System.Drawing.Point(768, 29);
            this.buttonGo.Name = "buttonGo";
            this.buttonGo.Size = new System.Drawing.Size(33, 25);
            this.buttonGo.TabIndex = 5;
            this.buttonGo.Text = "Go";
            this.buttonGo.UseVisualStyleBackColor = false;
            this.buttonGo.Click += new System.EventHandler(this.buttonGo_Click);
            // 
            // textBoxLog
            // 
            textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            textBoxLog.BackColor = System.Drawing.Color.Black;
            textBoxLog.ForeColor = System.Drawing.Color.White;
            textBoxLog.Location = new System.Drawing.Point(12, 435);
            textBoxLog.Multiline = true;
            textBoxLog.Name = "textBoxLog";
            textBoxLog.Size = new System.Drawing.Size(902, 120);
            textBoxLog.TabIndex = 6;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxBackups, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(908, 30);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(926, 580);
            this.Controls.Add(this.buttonGo);
            this.Controls.Add(this.textBoxLocation);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(textBoxLog);
            this.Controls.Add(this.buttonRestore);
            this.Controls.Add(this.listBoxView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "SecureBackupExplorer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxBackups;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox listBoxView;
        private System.Windows.Forms.Button buttonRestore;
        private System.Windows.Forms.TextBox textBoxLocation;
        private System.Windows.Forms.Button buttonGo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private static System.Windows.Forms.TextBox textBoxLog;
    }
}

