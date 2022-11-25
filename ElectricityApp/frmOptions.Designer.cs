
namespace ElectricityApp {
    partial class frmOptions {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.chkShowElementName = new System.Windows.Forms.CheckBox();
            this.chkShowWireName = new System.Windows.Forms.CheckBox();
            this.chkAutoAnalyze = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // chkShowElementName
            // 
            this.chkShowElementName.AutoSize = true;
            this.chkShowElementName.Checked = true;
            this.chkShowElementName.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowElementName.Location = new System.Drawing.Point(80, 41);
            this.chkShowElementName.Name = "chkShowElementName";
            this.chkShowElementName.Size = new System.Drawing.Size(104, 19);
            this.chkShowElementName.TabIndex = 0;
            this.chkShowElementName.Text = "显示元件名";
            this.chkShowElementName.UseVisualStyleBackColor = true;
            this.chkShowElementName.CheckedChanged += new System.EventHandler(this.chkShowElementName_CheckedChanged);
            // 
            // chkShowWireName
            // 
            this.chkShowWireName.AutoSize = true;
            this.chkShowWireName.Checked = true;
            this.chkShowWireName.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowWireName.Location = new System.Drawing.Point(80, 80);
            this.chkShowWireName.Name = "chkShowWireName";
            this.chkShowWireName.Size = new System.Drawing.Size(104, 19);
            this.chkShowWireName.TabIndex = 0;
            this.chkShowWireName.Text = "显示导线名";
            this.chkShowWireName.UseVisualStyleBackColor = true;
            this.chkShowWireName.CheckedChanged += new System.EventHandler(this.chkShowWireName_CheckedChanged);
            // 
            // chkAutoAnalyze
            // 
            this.chkAutoAnalyze.AutoSize = true;
            this.chkAutoAnalyze.Location = new System.Drawing.Point(80, 116);
            this.chkAutoAnalyze.Name = "chkAutoAnalyze";
            this.chkAutoAnalyze.Size = new System.Drawing.Size(134, 19);
            this.chkAutoAnalyze.TabIndex = 1;
            this.chkAutoAnalyze.Text = "自动分析和计算";
            this.chkAutoAnalyze.UseVisualStyleBackColor = true;
            this.chkAutoAnalyze.CheckedChanged += new System.EventHandler(this.chkAutoAnalyze_CheckedChanged);
            // 
            // frmOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(286, 196);
            this.Controls.Add(this.chkAutoAnalyze);
            this.Controls.Add(this.chkShowWireName);
            this.Controls.Add(this.chkShowElementName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmOptions";
            this.Text = "选项";
            this.Load += new System.EventHandler(this.frmOptions_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkShowElementName;
        private System.Windows.Forms.CheckBox chkShowWireName;
        private System.Windows.Forms.CheckBox chkAutoAnalyze;
    }
}