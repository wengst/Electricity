
namespace ElectricityApp
{
    partial class Main
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            ElectricityDLL.Option option1 = new ElectricityDLL.Option();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            this.toolSaveRealPict = new System.Windows.Forms.ToolStripMenuItem();
            this.toolSaveFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolOpenCircuit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolThemes = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolDelete = new System.Windows.Forms.ToolStripButton();
            this.toolClear = new System.Windows.Forms.ToolStripButton();
            this.toolOptions = new System.Windows.Forms.ToolStripButton();
            this.toolAutoAnalyaze = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.pb = new ElectricityDLL.Workbench();
            this.label1 = new System.Windows.Forms.Label();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.status = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.status.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Font = new System.Drawing.Font("方正粗黑宋简体", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStrip1.GripMargin = new System.Windows.Forms.Padding(0);
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSplitButton1,
            this.toolThemes,
            this.toolDelete,
            this.toolClear,
            this.toolOptions,
            this.toolAutoAnalyaze,
            this.toolStripButton1,
            this.toolStripButton2});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(900, 27);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripSplitButton1
            // 
            this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolSaveRealPict,
            this.toolSaveFile,
            this.toolOpenCircuit});
            this.toolStripSplitButton1.Image = global::ElectricityApp.Properties.Resources.file;
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.Size = new System.Drawing.Size(39, 24);
            this.toolStripSplitButton1.Text = "保存";
            // 
            // toolSaveRealPict
            // 
            this.toolSaveRealPict.Font = new System.Drawing.Font("方正粗黑宋简体", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolSaveRealPict.Image = global::ElectricityApp.Properties.Resources.realimage;
            this.toolSaveRealPict.Name = "toolSaveRealPict";
            this.toolSaveRealPict.Size = new System.Drawing.Size(208, 28);
            this.toolSaveRealPict.Text = "导出实物图";
            this.toolSaveRealPict.Click += new System.EventHandler(this.toolSaveRealPict_Click);
            // 
            // toolSaveFile
            // 
            this.toolSaveFile.Font = new System.Drawing.Font("方正粗黑宋简体", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolSaveFile.Image = global::ElectricityApp.Properties.Resources.json;
            this.toolSaveFile.Name = "toolSaveFile";
            this.toolSaveFile.Size = new System.Drawing.Size(208, 28);
            this.toolSaveFile.Text = "导出电路文件";
            this.toolSaveFile.Click += new System.EventHandler(this.toolSaveFile_Click);
            // 
            // toolOpenCircuit
            // 
            this.toolOpenCircuit.Name = "toolOpenCircuit";
            this.toolOpenCircuit.Size = new System.Drawing.Size(208, 28);
            this.toolOpenCircuit.Text = "打开电路文件";
            this.toolOpenCircuit.Click += new System.EventHandler(this.toolOpenCircuit_Click);
            // 
            // toolThemes
            // 
            this.toolThemes.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolThemes.Font = new System.Drawing.Font("方正粗黑宋简体", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolThemes.Image = global::ElectricityApp.Properties.Resources.theme;
            this.toolThemes.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolThemes.Name = "toolThemes";
            this.toolThemes.Size = new System.Drawing.Size(34, 24);
            this.toolThemes.Text = "主题";
            // 
            // toolDelete
            // 
            this.toolDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolDelete.Image = global::ElectricityApp.Properties.Resources.delete;
            this.toolDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolDelete.Name = "toolDelete";
            this.toolDelete.Size = new System.Drawing.Size(29, 24);
            this.toolDelete.Text = "删除";
            this.toolDelete.Click += new System.EventHandler(this.DeleteElement_Click);
            // 
            // toolClear
            // 
            this.toolClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolClear.Image = global::ElectricityApp.Properties.Resources.clear;
            this.toolClear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolClear.Name = "toolClear";
            this.toolClear.Size = new System.Drawing.Size(29, 24);
            this.toolClear.Text = "清空";
            this.toolClear.Click += new System.EventHandler(this.ClearWorkArea_Click);
            // 
            // toolOptions
            // 
            this.toolOptions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolOptions.Image = global::ElectricityApp.Properties.Resources.option;
            this.toolOptions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolOptions.Name = "toolOptions";
            this.toolOptions.Size = new System.Drawing.Size(29, 24);
            this.toolOptions.Text = "选项";
            this.toolOptions.Click += new System.EventHandler(this.toolOptions_Click);
            // 
            // toolAutoAnalyaze
            // 
            this.toolAutoAnalyaze.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolAutoAnalyaze.Image = global::ElectricityApp.Properties.Resources.analyzle;
            this.toolAutoAnalyaze.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolAutoAnalyaze.Name = "toolAutoAnalyaze";
            this.toolAutoAnalyaze.Size = new System.Drawing.Size(29, 24);
            this.toolAutoAnalyaze.Text = "分析和计算";
            this.toolAutoAnalyaze.Click += new System.EventHandler(this.toolAutoAnalyaze_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::ElectricityApp.Properties.Resources.about;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(29, 24);
            this.toolStripButton1.Text = "关于";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click_1);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 27);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel2.Controls.Add(this.status);
            this.splitContainer1.Size = new System.Drawing.Size(900, 513);
            this.splitContainer1.SplitterDistance = 84;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.pb);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.label1);
            this.splitContainer2.Panel2.Controls.Add(this.propertyGrid1);
            this.splitContainer2.Size = new System.Drawing.Size(815, 491);
            this.splitContainer2.SplitterDistance = 554;
            this.splitContainer2.TabIndex = 2;
            // 
            // pb
            // 
            this.pb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pb.FPS = 12;
            this.pb.IsCircuitChanged = false;
            this.pb.IsLocked = false;
            this.pb.IsShowMainProperties = true;
            this.pb.Location = new System.Drawing.Point(0, 0);
            this.pb.Name = "pb";
            option1.IsAutoAnalysis = false;
            option1.IsIdeal = true;
            this.pb.Opt = option1;
            this.pb.Size = new System.Drawing.Size(552, 489);
            this.pb.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(255, 489);
            this.label1.TabIndex = 1;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Font = new System.Drawing.Font("Times New Roman", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.propertyGrid1.Size = new System.Drawing.Size(252, 327);
            this.propertyGrid1.TabIndex = 0;
            // 
            // status
            // 
            this.status.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.status.Location = new System.Drawing.Point(0, 491);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(815, 22);
            this.status.TabIndex = 0;
            this.status.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 16);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(29, 24);
            this.toolStripButton2.Text = "toolStripButton2";
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 540);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Font = new System.Drawing.Font("宋体", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Main";
            this.Text = "初中电学实验箱1.1.0.0";
            this.Load += new System.EventHandler(this.Main_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Main_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Main_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Main_KeyDown);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.status.ResumeLayout(false);
            this.status.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripDropDownButton toolThemes;
        private System.Windows.Forms.ToolStripButton toolDelete;
        private System.Windows.Forms.ToolStripButton toolClear;
        private ElectricityDLL.Workbench pb;
        private System.Windows.Forms.StatusStrip status;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripButton toolOptions;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        public System.Windows.Forms.ToolStripButton toolAutoAnalyaze;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton1;
        private System.Windows.Forms.ToolStripMenuItem toolSaveRealPict;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem toolSaveFile;
        private System.Windows.Forms.ToolStripMenuItem toolOpenCircuit;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
    }
}

