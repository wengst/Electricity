using ElectricityDLL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace ElectricityApp
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }
        public Main(string fileName)
        {
            InitializeComponent();
            openfileName = fileName;
        }
        #region 私有字段

        private string openfileName { get; set; } = "";

        private string configFile = @"config.json";
        private string circuitExt = "circuit";

        AppConfig appConfig = new AppConfig() { MyTheme = Theme.浅色 };

        /// <summary>
        /// 所有停放在桌面上的元器件的集合
        /// </summary>
        List<EleComponent> EleComponents { get; } = new List<EleComponent>();
        #endregion

        #region 小方法
        /// <summary>
        /// 读取配置文件
        /// </summary>
        private void ReadAppConfig()
        {
            if (File.Exists(configFile))
            {
                string str = File.ReadAllText(configFile);
                JsonSerializerOptions options = new JsonSerializerOptions();
                appConfig = (AppConfig)JsonSerializer.Deserialize(str, typeof(AppConfig), options);
            }
        }

        /// <summary>
        /// 初始化主题工具
        /// </summary>
        private void InitThemeTools()
        {
            string[] themeNames = Enum.GetNames(typeof(Theme));
            for (int i = 0; i < themeNames.Length; i++)
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = themeNames[i];
                item.Click += ThemeItem_Click;
                if (item.Text == appConfig.MyTheme.ToString())
                {
                    item.Checked = true;
                    ChangeTheme(appConfig.MyTheme);
                }
                toolThemes.DropDownItems.Add(item);
            }
        }

        /// <summary>
        /// 改变主题
        /// </summary>
        /// <param name="theme"></param>
        private void ChangeTheme(Theme theme)
        {
            switch (theme)
            {
                case Theme.浅色:

                    toolStrip1.BackColor = Control.DefaultBackColor;
                    toolStrip1.ForeColor = Control.DefaultForeColor;

                    pb.BackColor = Color.White;
                    pb.ForeColor = Control.DefaultForeColor;

                    splitContainer1.Panel1.BackColor = Control.DefaultBackColor;
                    splitContainer1.Panel1.ForeColor = Control.DefaultForeColor;

                    this.BackColor = Control.DefaultBackColor;
                    break;
                case Theme.深色:
                    toolStrip1.BackColor = Color.DarkGray;
                    toolStrip1.ForeColor = Color.White;

                    pb.BackColor = Color.Black;
                    pb.ForeColor = Color.White;

                    this.BackColor = Color.DarkGray;
                    splitContainer1.Panel1.BackColor = Color.Black;
                    splitContainer1.Panel1.ForeColor = Color.White;
                    break;
                case Theme.粉色:
                    toolStrip1.BackColor = Color.DeepPink;
                    toolStrip1.ForeColor = Color.Yellow;

                    pb.BackColor = Color.LightPink;
                    pb.ForeColor = Color.DeepPink;

                    this.BackColor = Color.Pink;

                    splitContainer1.Panel1.BackColor = Color.LightPink;
                    splitContainer1.Panel1.ForeColor = Color.DeepPink;
                    break;
                case Theme.灰色:
                    toolStrip1.BackColor = Color.Gray;
                    toolStrip1.ForeColor = Color.Black;

                    pb.BackColor = Color.Gray;
                    pb.ForeColor = Color.Black;
                    BackColor = Color.Gray;

                    splitContainer1.Panel1.BackColor = Color.Gray;
                    splitContainer1.Panel1.ForeColor = Color.Black;
                    break;
                case Theme.蓝色:
                    toolStrip1.BackColor = Color.SkyBlue;
                    toolStrip1.ForeColor = Color.DarkBlue;

                    pb.BackColor = Color.SkyBlue;
                    pb.ForeColor = Color.DarkBlue;
                    BackColor = Color.DarkBlue;

                    splitContainer1.Panel1.BackColor = Color.SkyBlue;
                    splitContainer1.Panel1.ForeColor = Color.DarkBlue;
                    break;
            }
        }

        private void OpenFile(string fileName)
        {
            string str = File.ReadAllText(fileName);
            JsonWorkbench.LoadFromJson(str, pb);
            ReferTools();
        }
        #endregion

        #region 界面处理程序
        /// <summary>
        /// 添加工具按钮
        /// </summary>
        private void InitToolBox()
        {
            splitContainer1.Panel1.Controls.Clear();
            for (int i = ElectricityConfig.ElementConfigs.Count - 1; i >= 0; i--)
            {
                ElementConfigItem item = ElectricityConfig.ElementConfigs[i];
                Button btn = new Button();
                btn.Text = Fun.GetChineseName(item.Type) + "(" + (item.MaxAmount - pb.GetElementCountByType(item.Type)) + ")";
                btn.Image = item.ToolImage;
                btn.Dock = DockStyle.Top;
                btn.Height = 60;
                btn.TextAlign = ContentAlignment.BottomCenter;
                btn.TextImageRelation = TextImageRelation.ImageAboveText;
                btn.Tag = item.Type;
                btn.Font = new Font("黑体", 10f);
                btn.Click += EleButtonClick;
                splitContainer1.Panel1.Controls.Add(btn);
            }
        }

        private void ReferTools()
        {
            for (int i = 0; i < splitContainer1.Panel1.Controls.Count; i++)
            {
                if (splitContainer1.Panel1.Controls[i].GetType() == typeof(Button))
                {
                    Button btn = (Button)splitContainer1.Panel1.Controls[i];
                    ComponentType tp = (ComponentType)btn.Tag;
                    int max = ElectricityConfig.ElementMaxAmount(tp);
                    int cur = pb.GetElementCountByType(tp);
                    btn.Enabled = cur < max;
                    btn.Text = Fun.GetChineseName(tp) + "(" + (max - cur) + ")";
                }
            }
        }
        #endregion

        #region 控件代理方法集合
        /// <summary>
        /// 主题按钮单击事件代理方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThemeItem_Click(object sender, EventArgs e)
        {

            if (sender.GetType() == typeof(ToolStripMenuItem))
            {
                ToolStripMenuItem item = (ToolStripMenuItem)sender;
                ToolStripItem p = item.OwnerItem;
                //Console.WriteLine(p.GetType());
                if (p.GetType() == typeof(ToolStripDropDownButton))
                {
                    ToolStripDropDownButton tsdd = (ToolStripDropDownButton)p;
                    for (int i = 0; i < tsdd.DropDownItems.Count; i++)
                    {
                        ToolStripItem t = tsdd.DropDownItems[i];
                        if (t.GetType() == typeof(ToolStripMenuItem))
                        {
                            ((ToolStripMenuItem)t).Checked = false;
                        }
                    }
                }
                string name = item.Text;
                Theme theme = (Theme)Enum.Parse(typeof(Theme), name);
                ChangeTheme(theme);
                if (theme != appConfig.MyTheme)
                {
                    appConfig.MyTheme = theme;
                    string str = JsonSerializer.Serialize(appConfig, typeof(AppConfig), new JsonSerializerOptions());
                    File.WriteAllText(configFile, str);
                }
                item.Checked = true;
                pb.Draw();
            }
        }

        /// <summary>
        /// 元件按钮单击事件方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EleButtonClick(object sender, EventArgs e)
        {
            if (sender.GetType().Name == "Button")
            {
                Button btn = (Button)sender;
                ComponentType type = (ComponentType)btn.Tag;
                pb.AddElement(type);
                ReferTools();
            }
        }
        public void ShowStatus(object sender, EventArgs e)
        {
            this.statusLabel.BackColor = Control.DefaultBackColor;
            this.statusLabel.ForeColor = Control.DefaultForeColor;
            this.statusLabel.Text = pb.DDI != null ? pb.DDI.ToString() : "";
        }

        private void ShowElementProperties(object sender, OnSelectedElementEventArgs e)
        {
            if (e.SelectedObject != null)
            {
                propertyGrid1.Dock = DockStyle.Fill;
                propertyGrid1.Visible = true;
                label1.Visible = false;
                propertyGrid1.SelectedObject = e.SelectedObject;
            }
            else
            {
                propertyGrid1.Visible = false;
                label1.Visible = true;
                label1.Dock = DockStyle.Fill;
                label1.BackColor = Color.White;
                label1.ForeColor = Color.Black;
                label1.Padding = new Padding(8);
                label1.Text = pb.CircuitComment;
                propertyGrid1.SelectedObject = null;
            }
        }

        private void ShowDragDropInfo(object sender, DragDropElementEventArgs e)
        {
            this.statusLabel.BackColor = Control.DefaultBackColor;
            this.statusLabel.ForeColor = Control.DefaultForeColor;
            if (e != null && e.DDI != null)
            {
                if (e.DDI.Button == MouseButtons.Left)
                {
                    if (e.DDI.Target != null)
                    {
                        EleComponent o = e.DDI.Target;
                        switch (e.DDI.Type)
                        {
                            case ComponentType.Ammeter:
                                statusLabel.Text = "拖动电流表" + o.SymbolName;
                                break;
                            case ComponentType.Voltmeter:
                                statusLabel.Text = "拖动电压表" + o.SymbolName;
                                break;
                            case ComponentType.Terminal:
                                statusLabel.Text = "移动鼠标可拉出一条导线，当导线连接到电器元件的时候，导线的颜色会改变";
                                break;
                            case ComponentType.BatteryCase:
                                BatteryCase bc = (BatteryCase)o;
                                statusLabel.Text = "拖动电源" + o.SymbolName + "(" + bc.Voltage + "V)";
                                break;
                            case ComponentType.Lampstand:
                                Lampstand lp = (Lampstand)o;
                                statusLabel.Text = "拖动小灯泡" + o.SymbolName + "(额定电压=" + lp.RatedVoltage + "V,额定功率=" + lp.RatedPower + "W)";
                                break;
                            case ComponentType.Resistor:
                                Resistor rt = (Resistor)e.DDI.Target;
                                statusLabel.Text = "拖动电阻" + o.SymbolName + "(阻值=" + rt.Resistance + "Ω)";
                                break;
                            case ComponentType.Rheostat:
                                Rheostat rh = (Rheostat)o;
                                statusLabel.Text = "拖动滑动变阻器" + o.SymbolName + "(最大阻值=" + rh.MaxResistance + "Ω)";
                                break;
                            case ComponentType.Switch:
                                statusLabel.Text = "拖动开关" + o.SymbolName;
                                break;
                            case ComponentType.Vane:
                                statusLabel.Text = "可左右移动滑片，改变连入电路的阻值";
                                break;
                            case ComponentType.Knify:
                                statusLabel.Text = "单击可改变开关的闭合和断开状态";
                                break;
                            case ComponentType.WireControlPoint:
                                statusLabel.Text = "拖动这个控制点，可使导线弯曲或改变弯曲的程度";
                                break;
                            case ComponentType.WireJunction:
                                statusLabel.Text = "拖动导线端点，可使其脱离或连接到电器元件";
                                break;
                            case ComponentType.Wire:
                                statusLabel.Text = "正在拖动导线";
                                break;
                        }
                    }
                }
                else
                {
                    switch (e.DDI.Type)
                    {
                        case ComponentType.Terminal:
                            statusLabel.Text = "可按下鼠标左键，以拉出一条导线";
                            break;
                        case ComponentType.Vane:
                            statusLabel.Text = "可按下鼠标左键，以移动滑片";
                            break;
                        case ComponentType.Knify:
                            statusLabel.Text = "可按下鼠标左键，以改变断开或闭合状态";
                            break;
                        case ComponentType.Ammeter:
                        case ComponentType.BatteryCase:
                        case ComponentType.Fan:
                        case ComponentType.Lampstand:
                        case ComponentType.Resistor:
                        case ComponentType.Rheostat:
                        case ComponentType.Switch:
                        case ComponentType.Voltmeter:
                        case ComponentType.Wire:
                            statusLabel.Text = "可按下鼠标左键选择元件";
                            break;
                    }
                }
            }
            else
            {
                //statusLabel.Text = "作者：翁树棠 ；微信(电话)：13858145960 ； 淘宝店铺：仙舟杂货铺（铺内有不会闪烁和无这条提示的版本出售）";
            }
        }
        /// <summary>
        /// 删除元件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteElement_Click(object sender, EventArgs e)
        {
            pb.DeleteComponent();
            ReferTools();
        }

        /// <summary>
        /// 清空工作区上的所有元件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearWorkArea_Click(object sender, EventArgs e)
        {
            pb.ClearComponents();
            ReferTools();
        }

        #endregion

        #region 主界面事件方法
        /// <summary>
        /// 当工作区的大小发生改变时，Do重绘工作区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pb_Resize(object sender, EventArgs e)
        {
            pb.Draw();
        }

        /// <summary>
        /// 当需要重绘工作区时，怎么做。How Do
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pb_Paint(object sender, PaintEventArgs e)
        {
            //TheWorkbench.DrawImage(sender, e);
        }

        #endregion

        private void Main_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = false;
            //读取应用程序配置
            ReadAppConfig();
            //按主题设置颜色
            InitThemeTools();
            //初始化工具按钮
            InitToolBox();
            pb.OnDragDropElement += ShowDragDropInfo;
            pb.OnSelectedElement += ShowElementProperties;
            pb.Resize += Pb_Resize;
            pb.Paint += Pb_Paint;
            toolAutoAnalyaze.Visible = !pb.IsAutoAnalyze;
            if (!string.IsNullOrEmpty(openfileName))
            {
                OpenFile(openfileName);
            }
            //var l = 1;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            pb.DoTurnOnCircuit();
        }

        private void Main_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                pb.DeleteComponent();
                ReferTools();
            }
        }

        private void toolOptions_Click(object sender, EventArgs e)
        {
            frmOptions fopt = new frmOptions();
            fopt.Bench = pb;
            fopt.Owner = this;
            fopt.StartPosition = FormStartPosition.CenterParent;
            fopt.ShowDialog();
        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            frmAbout about = new frmAbout();
            about.Owner = this;
            about.StartPosition = FormStartPosition.CenterParent;
            about.ShowDialog();
        }

        private void toolAutoAnalyaze_Click(object sender, EventArgs e)
        {
            pb.DoTurnOnCircuit();
        }

        private void toolSaveRealPict_Click(object sender, EventArgs e)
        {
            if (pb.Items.Count > 0)
            {
                saveFileDialog1.Filter = "JPG|*.jpg;|PNG|*.png";
                saveFileDialog1.FileName = "";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(saveFileDialog1.FileName))
                {
                    frmSaveImage frmSave = new frmSaveImage();
                    string fileName = saveFileDialog1.FileName;
                    string ext = saveFileDialog1.FilterIndex == 1 ? "jpg" : "png";

                    frmSave.Owner = this;
                    frmSave.StartPosition = FormStartPosition.CenterParent;
                    if (frmSave.ShowDialog() == DialogResult.OK)
                    {
                        object o = frmSave.SelectedObject;
                        bool b = false;
                        if (o == null)
                        {
                            b = true;
                        }
                        else
                        {
                            if (o.ToString() == "整个工作台")
                            {
                                b = true;
                            }
                            else
                            {
                                b = false;
                            }
                        }
                        Image img = pb.GetImage(ext, b);
                        if (ext == "jpg")
                        {
                            img.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }
                        else
                        {
                            img.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("没有要保存的内容");
            }
        }

        private void toolSaveFile_Click(object sender, EventArgs e)
        {
            if (pb.Items.Count > 0)
            {
                saveFileDialog1.Filter = "电路文件|*." + circuitExt;
                saveFileDialog1.FileName = "";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(saveFileDialog1.FileName))
                {
                    string str = JsonWorkbench.GetJsonStr(pb);
                    File.WriteAllText(saveFileDialog1.FileName, str);
                }
            }
            else
            {
                MessageBox.Show("工作台上没有任何电器元件！");
            }
        }

        private void toolOpenCircuit_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "电路文件|*.circuit";
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(openFileDialog1.FileName))
            {
                OpenFile(openFileDialog1.FileName);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            pb.DrawCircuitImage();
        }
    }
}
