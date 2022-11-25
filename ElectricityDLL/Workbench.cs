using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows.Forms;


namespace ElectricityDLL {
    /// <summary>
    /// 工作台
    /// </summary>
    [Serializable]
    public partial class Workbench : Panel {

        #region Properties
        [JsonIgnore]
        public string CircuitComment {
            get {
                StringBuilder sb = new StringBuilder();
                int b = 0, k = 0, a = 0, v = 0, o = 0, w = 0;
                foreach (EleComponent e in Items) {
                    switch (e.Type) {
                        case ComponentType.Ammeter:
                            a++;
                            break;
                        case ComponentType.BatteryCase:
                            b++;
                            break;
                        case ComponentType.Fan:
                        case ComponentType.Lampstand:
                        case ComponentType.Resistor:
                        case ComponentType.Rheostat:
                            o++;
                            break;
                        case ComponentType.Switch:
                            k++;
                            break;
                        case ComponentType.Voltmeter:
                            v++;
                            break;
                        case ComponentType.Wire:
                            w++;
                            break;
                    }
                }
                if (Items.Count == 0) {
                    sb.Append("请在左侧的原件工具栏，单击要放到工作台上的原件按钮，然后拖放到合适的位置");
                }
                else {
                    if (b == 0) {
                        sb.Append("\n请把电源拖放到工作台");
                    }
                    else {
                        if (k + o == 0) {
                            sb.Append("请把其他元器件，如灯泡、开关等拖放到工作台");
                        }
                        else {
                            if (k == 0) {
                                sb.Append("\n至少需要一个开关");
                            }
                            else if (o == 0) {
                                sb.Append("\n至少需要一个用电器");
                            }
                            else {
                                sb.Append("\n现在工作台上有" + b + "个电源，" + k + "个开关，" + o + "个用电器。");
                                if (a != 0) {
                                    sb.Append(string.Format(@"还有{0}个电流表", a));
                                }
                                if (v != 0) {
                                    sb.Append(string.Format(@"{0}个电压表", v));
                                }
                                if (w == 0) {
                                    sb.Append("\n把元器件移动到相应位置后，请把鼠标移动到红色或黑色接线柱上，按下鼠标左键，移动鼠标，拖出一条导线，并继续移动到另一个接线柱上，实现连接。");
                                }
                                else {
                                    if (EquationCircuits.Count == 0) {
                                        sb.Append("\n电路连接好之后，将开关闭合或断开，再点击【分析计算】，程序会计算用电器的电流和实际电压，并以一定的形式显现出来。");
                                    }
                                    else {
                                        switch (GetCircuitState()) {
                                            case CT.AllOpen:
                                                sb.Append("\n目前电路没有接通");
                                                break;
                                            case CT.Base:
                                                sb.Append("\n它们构成了一个基本电路，即电源1、用电器1、开关1、电路1");
                                                break;
                                            case CT.Bridge:
                                                sb.Append("\n它们构成了一个桥电路");
                                                foreach (Branch branch in ValidBranchs) {
                                                    if (branch.CurrentDirection == AD.TwoWay) {
                                                        sb.Append("\n支路" + branch.ToString() + "为电路中的桥，如果桥两端的电位相等，该支路没有电流；如果两端的电位不相等，电流从电位高的一端流向电位低的一端");
                                                    }
                                                }
                                                break;
                                            case CT.HasFault:
                                                sb.Append("\n这个电路看上去好像不太正确");
                                                break;
                                            case CT.Mix:
                                                sb.Append("\n这是一个混合连接电路，既有串联也有并联");
                                                break;
                                            case CT.Parallel:
                                                sb.Append("\n它们连成了一个并联电路");
                                                break;
                                            case CT.PowerShort:
                                                sb.Append("\n这个电路有个严重问题，发生电源短路了");
                                                break;
                                            case CT.SectionOpen:
                                                break;
                                            case CT.Series:
                                                sb.Append("\n它们构成了一个串联电路");
                                                break;
                                        }
                                    }
                                }

                            }

                        }
                    }
                }
                return sb.ToString();
            }
        }

        [Category("行为"), DisplayName("显示元件名"), DefaultValue(true)]
        [JsonPropertyName("sen")]
        [Xuliehua(Consts.Json_ShowElementName)]
        public bool IsShowElementName { get; set; } = true;

        [Category("行为"), DisplayName("显示导线名"), DefaultValue(false)]
        [JsonPropertyName("swn")]
        [Xuliehua(Consts.Json_ShowWireName)]
        public bool IsShowWireName { get; set; } = false;

        [Category("行为"), DisplayName("是否自动分析"), DefaultValue(true)]
        [JsonPropertyName("aa")]
        [Xuliehua(Consts.Json_AutoAnalyze)]
        public bool IsAutoAnalyze { get; set; } = true;

        /// <summary>
        /// 是否锁定编辑
        /// </summary>
        [JsonPropertyName("locked")]
        [Xuliehua(Consts.Json_Locked)]
        public bool IsLocked { get; set; } = false;

        [Category("行为"), DisplayName("理想电路"), Description("是否理想电路"), DefaultValue(true)]
        [JsonPropertyName(Consts.Json_Ideal)]
        [Xuliehua(Consts.Json_Ideal)]
        public bool IsIdeal { get; set; } = true;

        /// <summary>
        /// 未配置连接JoinedNode属性的接线柱
        /// </summary>
        private List<Terminal> GetUnJoinTerminals() {
            List<Terminal> ls = new List<Terminal>();
            foreach (Terminal terminal in Terminals) {
                if (terminal.JoinedNode == null) {
                    ls.Add(terminal);
                }
            }
            return ls;
        }

        /// <summary>
        /// 已连接的接线柱
        /// </summary>
        private List<Terminal> GetJoinedTerminals() {
            List<Terminal> ls = new List<Terminal>();
            foreach (Terminal terminal in Terminals) {
                if (terminal.JoinedNode != null) {
                    ls.Add(terminal);
                }
            }
            return ls;
        }

        [JsonIgnore, Browsable(false)]
        private List<Vane> Vanes {
            get {
                List<Vane> _vanes = new List<Vane>();
                foreach (EleComponent ele in Items) {
                    if (ele.Type == ComponentType.Rheostat) {
                        _vanes.Add(((Rheostat)ele).TheVane);
                    }
                }
                return _vanes;
            }
        }

        /// <summary>
        /// 所有的导线端
        /// </summary>
        [JsonIgnore]
        private List<Junction> Junctions {
            get {
                List<Junction> list = new List<Junction>();
                foreach (EleComponent item in Items) {
                    if (item.Type == ComponentType.Wire && item.IsSelected) {
                        list.AddRange(((Wire)item).Junctions);
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// 全部的导线端子
        /// </summary>
        [JsonIgnore]
        private List<Junction> WireEndPoints {
            get {
                List<Junction> list = new List<Junction>();
                foreach (EleComponent item in Items) {
                    if (item.Type == ComponentType.Wire) {
                        Wire w = (Wire)item;
                        foreach (Junction j in w.Junctions) {
                            if (j.Area == WireArea.EndPoint || j.Area == WireArea.StartPoint) {
                                list.Add(j);
                            }
                        }
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// 所有的开关的闸刀
        /// </summary>
        [JsonIgnore]
        private List<Knify> Knifies {
            get {
                List<Knify> list = new List<Knify>();
                foreach (EleComponent item in Items) {
                    if (item.Type == ComponentType.Switch) {
                        list.Add(((Switch)item).K);
                    }
                }
                return list;
            }
        }

        [JsonIgnore]
        int LastMouseUpIndex = -1;

        /// <summary>
        /// 工作台上需要擦除的多边形区域。
        /// </summary>
        [JsonIgnore]
        private GraphicsPath DirtiedPath { get; set; } = new GraphicsPath();

        /// <summary>
        /// 将图形绘制到这个图片上，然后一起输出到屏幕
        /// </summary>
        [JsonIgnore]
        private Image TargetImg { get; set; }

        [JsonIgnore]
        private List<EleComponent> AllComponents {
            get {
                List<EleComponent> list = new List<EleComponent>();
                list.AddRange(Elements);
                list.AddRange(Terminals);
                list.AddRange(Vanes);
                list.AddRange(Wires);
                list.AddRange(Junctions);
                list.AddRange(Knifies);
                return list;
            }
        }

        [JsonIgnore, Description("帧率"), Category("数据"), DefaultValue(15)]
        public int FPS { get; set; } = 24;

        [JsonIgnore]
        private bool _showMainProperties = false;
        /// <summary>
        /// 在元件旁显示主要属性
        /// </summary>
        [JsonIgnore]
        public bool IsShowMainProperties {
            get {
                return _showMainProperties;
            }
            set {
                bool z = _showMainProperties != value;
                _showMainProperties = value;
                if (z) {
                    foreach (EleComponent item in Items) {
                        if (item.GetType().BaseType == typeof(Element) || item.Type == ComponentType.Wire) {
                            item.IsShowMainProperties = value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 选项
        /// </summary>
        [JsonIgnore]
        [JsonPropertyName("opt"), Category("数据"), Description("选项")]
        public Option Opt { get; set; } = new Option();

        /// <summary>
        /// 工作台绘图工具
        /// </summary>
        [JsonIgnore]
        private Graphics G { get { return CreateGraphics(); } }

        /// <summary>
        /// 工作台绘图区
        /// </summary>
        [JsonIgnore]
        private RectangleF R { get { return DisplayRectangle; } }

        [JsonIgnore, Browsable(false)]
        bool IsDirty {
            get {
                for (int i = 0; i < AllComponents.Count; i++) {
                    if (AllComponents[i].IsDirty) { return true; }
                }
                return false;
            }
        }

        /// <summary>
        /// 工作台上所有器材的数量
        /// </summary>
        [JsonIgnore, Browsable(false)]
        public int Count { get { return Items.Count; } }

        /// <summary>
        /// 工作台上全部非导线器材的总数
        /// </summary>
        [JsonIgnore, Browsable(false)]
        public int ElementCount {
            get {
                int m = 0;
                foreach (EleComponent ec in Items) {
                    if (ec.Type != ComponentType.Wire) m++;
                }
                return m;
            }
        }

        /// <summary>
        /// 开始多选的最小距离
        /// </summary>
        [Category("数据"), DefaultValue(50f), Description("圈选屏幕元件的最小距离设置值"), JsonIgnore]
        public float MinMultipleChoiceDistance { get; set; } = 50f;
        /// <summary>
        /// 短线端点磁贴最大距离
        /// </summary>
        [Category("数据"), DefaultValue(15f), Description("导线端点靠近接线柱的磁性特性距离"), JsonIgnore]
        public float MaxMagneticDistance { get; set; } = 15f;
        /// <summary>
        /// 元件最小移动距离
        /// </summary>
        [Category("数据"), DefaultValue(15f), Description("当鼠标移动超过设置值时开始移动元件"), JsonIgnore]
        public float MinElementMoveDistance { get; set; } = 15f;

        /// <summary>
        /// 导线最短长度
        /// </summary>
        [Category("数据"), DefaultValue(30f), Description("导线最小长度"), JsonIgnore]
        public float MinWireLength { get; set; } = 30f;
        /// <summary>
        /// 导线贝塞尔控制点最小移动距离
        /// </summary>
        [Category("数据"), DefaultValue(15f), Description("导线贝塞尔控制点最小移动距离"), JsonIgnore]
        public float MinWireHandleDistance { get; set; } = 15f;

        /// <summary>
        /// 滑动变阻器滑片每次移动的最小距离
        /// </summary>
        [Category("数据"), DefaultValue(4f), Description("滑动变阻器滑片每次移动的最小距离"), JsonIgnore]
        public float MinVaneMoveDistance { get; set; } = 4f;

        /// <summary>
        /// 工作台上所有元器件的集合
        /// </summary>
        [JsonPropertyName("eles"), Browsable(false)]
        public List<EleComponent> Items { get; } = new List<EleComponent>();

        [JsonIgnore]
        private Timer timer;

        [Browsable(false)]
        [JsonIgnore]
        public bool IsAnimation {
            get {
                foreach (EleComponent ele in AllComponents) {
                    if (ele.IsAnimation) {
                        return true;
                    }
                }
                return false;
            }
        }

        [JsonIgnore, Browsable(false)]
        public List<BatteryCase> BCs {
            get {
                List<BatteryCase> bcs = new List<BatteryCase>();
                foreach (EleComponent item in Items) {
                    if (item.Type == ComponentType.BatteryCase) {
                        bcs.Add((BatteryCase)item);
                    }
                }
                return bcs;
            }
        }

        /// <summary>
        /// 全部导线
        /// </summary>
        [JsonIgnore, Browsable(false)]
        [Xuliehua("Wires")]
        public List<Wire> Wires {
            get {
                List<Wire> wires = new List<Wire>();
                foreach (EleComponent item in Items) {
                    if (item.Type == ComponentType.Wire) {
                        wires.Add((Wire)item);
                    }
                }
                return wires;
            }
        }

        /// <summary>
        /// 全部元件
        /// </summary>
        [JsonIgnore, Browsable(false)]
        [Xuliehua("Elements")]
        public List<Element> Elements {
            get {
                List<Element> eles = new List<Element>();
                foreach (var item in Items) {
                    if (item.GetType().BaseType == typeof(Element)) {
                        eles.Add((Element)item);
                    }
                }
                return eles;
            }
        }

        /// <summary>
        /// 全部电源
        /// </summary>
        [JsonIgnore]
        private List<BatteryCase> BatteryCases {
            get {
                List<BatteryCase> bs = new List<BatteryCase>();
                foreach (Element element in Elements) {
                    if (element.Type == ComponentType.BatteryCase) {
                        bs.Add((BatteryCase)element);
                    }
                }
                return bs;
            }
        }

        /// <summary>
        /// 不含电源的元器件
        /// </summary>
        [JsonIgnore]
        private List<Element> NoBCElements {
            get {
                List<Element> ls = new List<Element>();
                foreach (EleComponent item in AllComponents) {
                    if (item.GetType().BaseType == typeof(Element) && item.Type != ComponentType.BatteryCase) {
                        ls.Add((Element)item);
                    }
                }
                return ls;
            }
        }

        /// <summary>
        /// 全部接线柱，如果有电源，则电源接线柱排在前面
        /// </summary>
        [JsonIgnore, Browsable(false)]
        private List<Terminal> Terminals { get; } = new List<Terminal>();
        #endregion

        [JsonIgnore]
        private int FrameIndex = 0;
        private void StartTimer() {
            if (timer != null) {
                timer.Stop();
                timer.Dispose();
            }
            FrameIndex = 0;
            timer = new Timer();
            timer.Interval = (int)(1000f * (1 / (float)FPS));
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void StopTimer() {
            if (timer != null) {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
        }

        private void Timer_Tick(object sender, EventArgs e) {
            FrameIndex++;
            Draw_Paint(null, null);
        }

        /// <summary>
        /// 全部元件、部件（例如电流表以及电流表上的接线柱、导线、滑动变阻器及其滑片）
        /// </summary>
        [JsonIgnore]
        private List<EleComponent> AllEleComponents {
            get {
                List<EleComponent> items = new List<EleComponent>();
                for (int i = 0; i < Items.Count; i++) {
                    items.Add(Items[i]);
                    if (Items[i].GetType().BaseType == typeof(Element)) {
                        items.AddRange(((Element)Items[i]).Terminals);
                        if (Items[i].Type == ComponentType.Rheostat) {
                            items.Add(((Rheostat)Items[i]).TheVane);
                        }
                        if (Items[i].GetType() == typeof(Switch)) {
                            items.Add(((Switch)Items[i]).K);
                        }
                    }
                }
                return items;
            }
        }

        /// <summary>
        /// 获取所有需要重绘的区域
        /// </summary>
        /// <returns></returns>
        private List<RectangleF> GetDirtyRectangleFs() {
            List<RectangleF> rects = new List<RectangleF>();
            foreach (EleComponent item in AllEleComponents) {
                if (item.IsDirty) {
                    rects.Add(item.Region);
                }
            }
            return rects;
        }

        private EleComponent GetComponentByPoint(PointF point) {
            List<EleComponent> items = AllComponents;
            for (int i = items.Count - 1; i >= 0; i--) {
                EleComponent ele = items[i];
                switch (ele.Type) {
                    case ComponentType.Vane:
                        Vane v = (Vane)ele;
                        if (v.Contains(point)) return v;
                        break;
                    case ComponentType.Ammeter:
                    case ComponentType.BatteryCase:
                    case ComponentType.Fan:
                    case ComponentType.Lampstand:
                    case ComponentType.Meter:
                    case ComponentType.Ohmmeter:
                    case ComponentType.Other:
                    case ComponentType.Resistor:
                    case ComponentType.Rheostat:
                    case ComponentType.Switch:
                    case ComponentType.Voltmeter:
                        if (ele.Contains(point)) { return ele; }
                        break;
                    case ComponentType.WireJunction:
                        Junction j = (Junction)ele;
                        if (j.Contains(point)) {
                            return j;
                        }
                        break;
                    case ComponentType.Wire:
                        Wire w = (Wire)ele;
                        if (w.Contains(point)) { return w; }
                        break;
                    case ComponentType.Terminal:
                        Terminal t = (Terminal)ele;
                        if (t.Contains(point)) { return t; }
                        break;
                    case ComponentType.Knify:
                        Knify k = (Knify)ele;
                        if (k.Contains(point)) return k;
                        break;
                }
            }
            return null;
        }

        private int GetComponentIndex(EleComponent e) {
            for (int i = 0; i < Items.Count; i++) {
                if (Items[i].Id == e.Id) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 删除元件
        /// </summary>
        public bool DeleteComponent() {
            bool b = false;
            for (int i = 0; i < Items.Count; i++) {
                if (Items[i].IsSelected) {
                    if (Items[i].GetType().BaseType == typeof(Element)) {
                        ((Element)Items[i]).ClearJuntions();
                        for (int j = Terminals.Count - 1; j >= 0; j--) {
                            if (Terminals[j].Owner.Id == Items[i].Id) {
                                Terminals.RemoveAt(j);
                            }
                        }
                        Items.RemoveAt(i);
                        b = true; break;
                    }
                    else if (Items[i].Type == ComponentType.Wire) {
                        Wire wire = (Wire)Items[i];
                        wire.ClearJunctions();
                        Items.RemoveAt(i);
                        b = true;
                        break;
                    }
                }
            }
            if (Items.Count == 0) {
                //如果元件删除完了，那么元件的Id编号重新从零开始
                EleComponent.Identity = 100;
            }
            if (b) { Draw(); ; }
            return b;
        }

        /// <summary>
        /// 清除工作台上的全部元件
        /// </summary>
        /// <returns></returns>
        public bool ClearComponents() {

            Items.Clear();
            PathElements.Clear();
            Terminals.Clear();
            Nodes.Clear();
            Branchs.Clear();
            Circuits.Clear();

            G.FillRectangle(new SolidBrush(BackColor), R);
            EleComponent.Identity = 100;
            return true;
        }

        private string GetSymbolName(string symbol) {
            int n = GetElementCountByName(symbol);
            string symbolName = symbol + (n + 1).ToString();
            return symbolName;
        }


        /// <summary>
        /// 实例化除导线外的元器件到桌面
        /// </summary>
        /// <param name="type"></param>
        public void AddElement(ComponentType type) {
            string symbol = PublicFunction.GetSymbol(type);
            string symbolName = GetSymbolName(symbol);

            Element a = null;

            switch (type) {
                case ComponentType.Ammeter:
                    a = new Ammeter();
                    break;
                case ComponentType.BatteryCase:
                    a = new BatteryCase();
                    break;
                case ComponentType.Fan:
                    a = new Fan();
                    a.FPS = FPS;

                    break;
                case ComponentType.Lampstand:
                    a = new Lampstand();
                    break;
                case ComponentType.Resistor:
                    a = new Resistor();
                    break;
                case ComponentType.Rheostat:
                    a = new Rheostat();
                    break;
                case ComponentType.Switch:
                    a = new Switch();
                    break;
                case ComponentType.Voltmeter:
                    a = new Voltmeter();
                    break;
            }
            if (a != null) {
                a.Bench = this;
                a.SymbolName = symbolName;
                a.Width = a.OutputImage.Width;
                a.Height = a.OutputImage.Height;
                int w = (int)((R.Width - 200) / 120);
                int h = (int)((R.Height - 200) / 80);
                //a.X = (R.Width - a.Width) / 2;
                a.X = 100 + (Elements.Count % w) * 120;
                //a.Y = (R.Height - a.Height) / 2;
                a.Y = 100 + Elements.Count / w * 80;
                a.IsDirty = true;
                a.IsIdeal = IsIdeal;
                //a.IsShowMainProperties = IsShowMainProperties;
                Items.Add(a);
                if (a.Type == ComponentType.BatteryCase) {
                    Terminals.InsertRange(0, a.Terminals);
                }
                else {
                    foreach (Terminal terminal in a.Terminals) {
                        if (!terminal.IsVirtual) {
                            Terminals.Add(terminal);
                        }
                    }

                }
            }
            Draw();
        }

        public void AddElement(Element e) {
            e.Bench = this;
            e.IsDirty = true;
            e.Width = e.OutputImage.Width;
            e.Height = e.OutputImage.Height;
            Items.Add(e);
            if (e.Type == ComponentType.BatteryCase) {
                Terminals.InsertRange(0, e.Terminals);
            }
            else {
                foreach (Terminal terminal in e.Terminals) {
                    if (!terminal.IsVirtual) {
                        Terminals.Add(terminal);
                    }
                }
            }
        }

        private Terminal GetClosedTerminal(PointF p) {
            foreach (Terminal terminal in Terminals) {
                if (terminal.IsClosesd(p, MaxMagneticDistance)) {
                    return terminal;
                }
            }
            return null;
        }

        private void WriteLine(string s) {
            Console.WriteLine(Fun.NSpace(deepIndex) + s);
        }

        public Workbench() {
            this.MouseDown += Mouse_Down;
            this.MouseClick += Mouse_Click;
            this.MouseMove += Mouse_Move;
            this.MouseUp += Mouse_Up;
            this.Paint += Draw_Paint;
            this.MouseWheel += Mouse_WHEEL;

        }

        private void ResetElememt() {
            foreach (Element element in Elements) {
                element.ResetTerminalPair();
            }
        }

        /// <summary>
        /// 合上开关，进行电路分析
        /// </summary>
        public void DoTurnOnCircuit() {
            /**
             * 电路分析过程
             * 检查是否含有电源，如有则继续
             * 首先进行环境清理：清除Nodes、PathElement、Branchs、Circuits、EquationBranchs、EquationCircuits...等集合
             * 
             * 其次找出所有节点（节点：互相之间零电阻的接线柱集合）
             * 构建所有的最小电路单元
             * 查找所有的支路Branch（没有分支的电路）
             * 查找所有的回路Circuit（支路构成回路）
             * 剔除所有不含电源的回路Circuit，构建有效回路集合
             * 根据有效回路剔除所有不在回路中的支路
             * 计算有效回路的电阻，构建用于KVL计算的方程回路集合EquationCircuits
             * 通过用于方程的回路集合构建方程的支路集合EquationBranchs
             * 依据EquationCircuits和EquationBranchs构建方程组（KVL和KCL）
             * 求解方程组，计算支路电流和节点电压。
             * 显示计算的结果（如：UI呈现）
             * 
             */
            int i;
            ResetElememt();
            //reset();
            if (BCs.Count > 0) {
                //WorkStat s1 = WorkStat.StopOrOpen | WorkStat.Working;
                //WorkStat s2 = WorkStat.Working;
                //Console.WriteLine("s1=" + s1.ToString() + " ; s2=" + s2.ToString() + " ; s1 & s2 = " + (s1 & s2)+" ; (s1 & s2) == s2 Result "+((s1 & s2)==s2));
                //清除环境
                ClearNodes();
                Branchs.Clear();
                Circuits.Clear();
                EquationCircuits.Clear();
                //Equations.Clear();

                foreach (Element ele in Elements) {
                    ele.Current = 0f;
                    if (ele.Type != ComponentType.BatteryCase && ele.Type != ComponentType.Switch) {
                        ele.Stat = WorkStat.StopOrOpen;
                    }
                }
                foreach (Wire wire in Wires) {
                    wire.Current = 0f;
                    wire.Stat = WorkStat.StopOrOpen;
                }

                //将所有等电位的接线柱合并，设置为同一节点
                MergeEquPotentialTerminals();
                ComputeTerminalPaths();
                MoveCircuitToMaxPaths();

                IsCircuitChanged = true;

                //构建最小电路单位PathElement。
                BuildPathElements();

                //查找所有支路
                BeginFindBranch();
                //Console.WriteLine("BeginFindBranch()结果找到" + Branchs.Count + "条支路");

                //找完整电路
                FindCircuits();

                //找有效的电路、支路和节点
                FindValidCircuits();
                FindValidNodes();
                FindValidBranchs();

                Console.WriteLine("ClearUpBranchs()后有" + Branchs.Count + "条支路");
                //整理排列支路
                ArrangeBranchs();
                Console.WriteLine("ArrangeBranchs()后有" + Branchs.Count + "条支路");
                //*定义支路数最多节点的电位为零*/
                DelimitZeroPotential();

                FindEquationCircuits();

                Console.WriteLine("BuilderEquationCircuits()后有" + EquationCircuits.Count + "条方程回路");

                FindEquationBranchs();

                Console.WriteLine("FindEquationBranchs()后有" + EquationBranchs.Count + "条方程支路");

                Console.WriteLine("BuildConnections()后有" + EquationCircuits.Count + "条方程回路和" + EquationBranchs.Count + "条方程支路");
                //构建支路的连接关系
                BuildConnections();
                if (CircuitType != CT.AllOpen && CircuitType != CT.PowerShort) {
                    double[] result;

                    if (EquationBranchs.Count > 0) {
                        GetMatrix();
                        //EquationCollection ec = new EquationCollection();
                        //ec.Equations.AddRange(Equations);
                        //List<double> resu = ec.Solve();
                        List<Equation> eqs = new List<Equation>();
                        for (int k = 0; k < Equations.Count; k++) {
                            Equation equation = Equations[k];
                            equation.OrderName = "L" + (k + 1).ToString();
                            eqs.Add(equation.Clone());
                        }
                        result = Fun.ComputeEquations(eqs);
                        Vector<double> result2 = Fun.SolveEquations(Equations);
                        result = result2.ToArray<double>();
                        //result = Fun.ComputeEquations(Equations);
                        bool IsValid = ComputedIsValid(result.ToList());
                        if (IsValid) {
                            for (i = 0; i < EquationBranchs.Count; i++) {

                                EquationBranchs[i].A = (float)result[i];

                            }
                            ComputeCurrent();
                            ///计算节点电位
                            ComputeNodeVoltage();
                            ///设置元件接线柱间的电压
                            SetTerminalPairVoltage();
                            //ShowNodes();
                        }
                    }
                }
                else if (CircuitType == CT.PowerShort) {
                    if (OnStatusChanged != null) {
                        OnStatusChanged(this, new Mouse.StatusChangedEventArgs() { Status = "电源短路!!!" });
                    }
                    else {
                        MessageBox.Show("电源短路!!!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else if (CircuitType == CT.AllOpen) {
                    ComputeNodeVoltage();
                    SetTerminalPairVoltage();
                }
                Draw();
            }
            else {
                if (OnStatusChanged != null) {
                    OnStatusChanged(this, new Mouse.StatusChangedEventArgs() { Status = "没有电源，无法分析" });
                };
            }
        }

        public string GetJson() {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("ShowElementName:" + (IsShowElementName ? "1" : "0") + ",ShowWireName:" + (IsShowWireName ? "1" : "0") + ",Locked:" + (IsLocked ? "1" : "0") + ",");
            sb.Append("Items:[");
            for (int i = 0; i < Elements.Count; i++) {
                if (i > 0) {
                    sb.Append(",");
                }
                Element e = Elements[i];
                switch (e.Type) {
                    case ComponentType.Ammeter:
                    case ComponentType.Voltmeter:
                    case ComponentType.Switch:
                        sb.Append(e.ToJson().Replace(Consts.ZWF, ""));
                        break;
                    case ComponentType.BatteryCase:
                        sb.Append(((BatteryCase)e).ToJson());
                        break;
                    case ComponentType.Fan:
                        sb.Append(((Fan)e).ToJson());
                        break;
                    case ComponentType.Lampstand:
                        sb.Append(((Fan)e).ToJson());
                        break;
                    case ComponentType.Resistor:
                        sb.Append(((Resistor)e).ToJson());
                        break;
                    case ComponentType.Rheostat:
                        sb.Append(((Rheostat)e).ToJson());
                        break;
                }
            }
            sb.Append("]");
            if (Wires.Count > 0) {
                sb.Append(",Wires:[");
                for (int w = 0; w < Wires.Count; w++) {
                    Wire wire = Wires[w];
                    if (w == 0) {
                        sb.Append(wire.ToJson());
                    }
                    else {
                        sb.Append("," + wire.ToJson());
                    }
                }
                sb.Append("]");
            }
            sb.Append("}");

            return sb.ToString();
        }

        private string DeleteFirstChar(string source, string c) {
            int cl = c.Length;
            int sl = source.Length;
            if (source.Substring(0, cl) == c) {
                source = source.Substring(cl, sl - cl);
            }
            return source;
        }

        private string DeleteLastChar(string source, string c) {
            int cl = c.Length;
            int sl = source.Length;
            if (source.Substring(sl - cl, cl) == c) {
                source = source.Substring(0, sl - cl);
            }
            return source;
        }

        private EleComponent NewComponent(ComponentType t) {
            switch (t) {
                case ComponentType.Ammeter:
                    return new Ammeter();
                case ComponentType.BatteryCase:
                    return new BatteryCase();
                case ComponentType.Fan:
                    return new Fan();
                case ComponentType.Lampstand:
                    return new Lampstand();
                case ComponentType.Resistor:
                    return new Resistor();
                case ComponentType.Rheostat:
                    return new Rheostat();
                case ComponentType.Switch:
                    return new Switch();
                case ComponentType.Voltmeter:
                    return new Voltmeter();
                case ComponentType.Wire:
                    return new Wire();
            }
            return null;
        }

        private void AddElementFromJson(string json) {
            json = DeleteFirstChar(json, "{");
            json = DeleteLastChar(json, "}");
            string[] properties = json.Split(',');
            foreach (string pv in properties) {
                string[] p = pv.Split(':');
                object obj = null;
                int _scale = 1;
                int _stat = 0;
                float x = 0f, y = 0f, _r = float.NaN, _ir = float.NaN, _rv = float.NaN, _v = float.NaN, _rp = float.NaN, _maxr = float.NaN;
                string _name = "";

                if (p[0] == "Name") {
                    _name = p[1];
                }
                if (p[0] == "Type") {
                    obj = NewComponent(Consts.StrToType(p[1]));
                }
                if (p[0] == "Scale") {
                    _scale = Consts.StrToInt(p[1]);
                }
                if (p[0] == "Stat") {
                    _stat = Consts.StrToInt(p[1]);
                }
                if (p[0] == "X") {
                    x = Consts.StrToFloat(p[1]);
                }
                if (p[0] == "Y") {
                    y = Consts.StrToFloat(p[1]);
                }
                if (p[0] == "Resistance") {
                    _r = Consts.StrToFloat(p[1]);
                }
                if (p[0] == "Voltage") { _v = Consts.StrToFloat(p[1]); }
                if (p[0] == "InductiveReactance") { _ir = Consts.StrToFloat(p[1]); }
                if (p[0] == "RatedVoltage") { _rv = Consts.StrToFloat(p[1]); }
                if (p[0] == "RatedPower") { _rp = Consts.StrToFloat(p[1]); }
                if (p[0] == "MaxResistance") { _maxr = Consts.StrToFloat(p[1]); }
            }
        }

        private void AddWireFromJson(string json) {

        }

        public void LoadFromJson(string json) {
            json = DeleteFirstChar(json, "{");
            json = DeleteLastChar(json, "}");
            string[] properties = json.Split(',');
            foreach (string pv in properties) {
                string[] p = pv.Split(':');
                if (p[0] == "ShowElementName") { IsShowElementName = Consts.StrToBool(p[1]); }
                if (p[0] == "ShowWireName") { IsShowWireName = Consts.StrToBool(p[1]); }
                if (p[0] == "Locked") { IsLocked = Consts.StrToBool(p[1]); }
                if (p[0] == "Items") {
                    p[1] = DeleteFirstChar(p[1], "[");
                    p[1] = DeleteLastChar(p[1], "]");
                    string[] itemJsons = p[1].Split(',');
                    foreach (string itemStr in itemJsons) {
                        AddElementFromJson(itemStr);
                    }
                }
                if (p[0] == "Wires") {
                    p[1] = DeleteFirstChar(p[1], "[");
                    p[1] = DeleteLastChar(p[1], "]");
                    string[] itemJsons = p[1].Split(',');
                    foreach (string itemStr in itemJsons) {
                        AddWireFromJson(itemStr);
                    }
                }
            }
        }
    }
}
