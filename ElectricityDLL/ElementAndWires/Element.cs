using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text.Json.Serialization;
using System.Drawing.Drawing2D;

namespace ElectricityDLL {
    /// <summary>
    /// 带接线柱的电器元件基类
    /// </summary>
    public class Element : EleComponent {

        /// <summary>
        /// 元件符号
        /// </summary>
        public GraphicsPath CircuitPath { get; } = new GraphicsPath();

        public virtual void InitCircuitPath() { }

        /// <summary>
        /// 返回电阻
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public virtual float GetResistance(Terminal t1, Terminal t2) {
            return 0;
        }

        public Terminal GetTerminal(TerminalKey key) {
            foreach (Terminal termin in Terminals) {
                if (termin.Key == key) { return termin; }
            }
            return null;
        }

        public List<PathElement> GetPathElements() {
            List<PathElement> ls = new List<PathElement>();
            List<Terminal> ts = new List<Terminal>();
            if (Type == ComponentType.Rheostat) {
                Terminal m = FindTerminal(TerminalKey.MiddleUp);
                m.Junctions.AddRange(FindTerminal(TerminalKey.LeftUp).Junctions);
                m.Junctions.AddRange(FindTerminal(TerminalKey.RightUp).Junctions);
                ts.Add(m);
                ts.Add(FindTerminal(TerminalKey.LeftDown));
                ts.Add(FindTerminal(TerminalKey.RightDown));
            }
            else {
                ts.AddRange(Terminals);
            }
            for (int i = 0; i < ts.Count; i++) {
                Terminal t1 = ts[i];
                for (int j = i + 1; j < ts.Count; j++) {
                    Terminal t2 = ts[j];
                    if (t1.Junctions.Count > 0 && t2.Junctions.Count > 0) {
                        ls.Add(new PathElement(t1, this, t2));
                    }
                }
            }
            return ls;
        }

        /// <summary>
        /// 元件规格集合
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public List<Spec> Specs { get; } = new List<Spec>();

        /// <summary>
        /// 接线柱集合。需要在子类的构造函数中初始化
        /// </summary>
        [JsonPropertyName("ts")]
        [Browsable(false)]
        public List<Terminal> Terminals { get; } = new List<Terminal>();

        public Terminal FindTerminal(TerminalKey k) {
            foreach (Terminal terminal in Terminals) {
                if (terminal.Key == k) {
                    return terminal;
                }
            }
            return null;
        }

        [JsonIgnore, Browsable(false)]
        public Terminal Left { get { return FindTerminal(TerminalKey.Left); } }

        [JsonIgnore, Browsable(false)]
        public Terminal Right { get { return FindTerminal(TerminalKey.Right); } }

        [JsonIgnore, Browsable(false)]
        public Terminal LeftUp { get { return FindTerminal(TerminalKey.LeftUp); } }

        [JsonIgnore, Browsable(false)]
        public Terminal LeftDown { get { return FindTerminal(TerminalKey.LeftDown); } }

        [JsonIgnore, Browsable(false)]
        public Terminal RightUp { get { return FindTerminal(TerminalKey.RightUp); } }

        [JsonIgnore, Browsable(false)]
        public Terminal RightDown { get { return FindTerminal(TerminalKey.RightDown); } }

        [JsonIgnore, Browsable(false)]
        public Terminal Middle { get { return FindTerminal(TerminalKey.Middle); } }

        /// <summary>
        /// 将滑动变阻器左上和右上的导线接头合并后的虚拟接线柱，用于查询电路用。不用于绘图，不对外展示。
        /// </summary>
        [JsonIgnore, Browsable(false)]
        public Terminal MiddleUp {
            get {
                Terminal mt = FindTerminal(TerminalKey.MiddleUp);
                mt.Junctions.Clear();
                mt.Junctions.AddRange(LeftUp.Junctions);
                mt.Junctions.AddRange(RightUp.Junctions);
                if (LeftUp.JoinedNode != null || RightUp.JoinedNode != null) {
                    mt.JoinedNode = LeftUp.JoinedNode != null ? LeftUp.JoinedNode : RightUp.JoinedNode;
                }
                return mt;
            }
        }

        [JsonPropertyName("ws"), Browsable(false)]
        public List<Wire> Wires {
            get {
                List<Wire> wires = new List<Wire>();
                for (int i = 0; i < Terminals.Count; i++) {
                    wires.AddRange(Terminals[i].Wires);
                }
                return wires;
            }
        }

        /// <summary>
        /// 向元件的某个接线柱添加导线接头
        /// </summary>
        /// <param name="junction"></param>
        /// <param name="t"></param>
        public void AddJunction(Junction junction, Terminal t) {
            if (Terminals.Contains(t)) {
                t.AddJunction(junction);
            }
        }

        public void ClearJuntions() {
            for (int i = 0; i < Terminals.Count; i++) {
                Terminals[i].ClearJunctions();
            }
        }

        /// <summary>
        /// 将导线接头从接线柱移除
        /// </summary>
        /// <param name="junction"></param>
        public void RemoveJunction(Junction junction) {
            foreach (Terminal terminal in Terminals) {
                terminal.RemoveJunction(junction);
            }
        }

        /// <summary>
        /// 将导线从元件移除
        /// </summary>
        /// <param name="w"></param>
        /// <param name="t"></param>
        public void RemoveWire(Wire w, Terminal t) {
            if (Terminals.Contains(t)) {
                foreach (Terminal terminal in Terminals) {
                    for (int i = terminal.Wires.Count - 1; i >= 0; i--) {
                        if (terminal.Wires[i].Id == w.Id) {
                            terminal.Wires.RemoveAt(i);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 元件上的接线柱对。需要在子类的构造函数中进行初始化
        /// </summary>
        [JsonIgnore, Browsable(false)]
        public List<TerminalPair> TerminalPairs { get; } = new List<TerminalPair>();

        /// <summary>
        /// 设置流过接线柱的电流
        /// </summary>
        /// <param name="path"></param>
        /// <param name="a"></param>
        public void SetTerminalPairCurrent(PathElement path, float a) {
            foreach (TerminalPair terminalPair in TerminalPairs) {
                if (path.ElementOrWire.Id == Id && path.Keys == terminalPair.Keys) {
                    bool valid = (!float.IsNaN(a) && !Consts.IsZero(a) && !float.IsInfinity(a));
                    Console.WriteLine("Valid=" + valid);
                    terminalPair.Current = a;
                }
            }
        }

        public void ResetTerminalPair() {
            foreach (TerminalPair terminal in TerminalPairs) {
                terminal.Current = float.NaN;
            }
        }

        /// <summary>
        /// 设置两接线柱间的电压
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        public void SetTerminalPairVoltage(Node n1, Node n2) {
            foreach (TerminalPair t in TerminalPairs) {
                if (t.T1 != null && t.T2 != null && ((t.T1.JoinedNode == n1 && t.T2.JoinedNode == n2) || (t.T1.JoinedNode == n2 && t.T2.JoinedNode == n1))) {

                }
            }
        }
        /// <summary>
        /// 获取有效接线柱对
        /// </summary>
        public TerminalPair GetValidTerminal() {
            TerminalPair p = null;
            for (int i = 0; i < TerminalPairs.Count; i++) {
                if (TerminalPairs[i].IsValid) {
                    p = TerminalPairs[i];
                    break;
                }
            }
            return p;
        }

        [JsonIgnore, Browsable(false)]
        public Terminal MouseUpTerminal { get; set; }


        /// <summary>
        /// 对于有不同规格的元件，需要在改变规格时，设置新规格下的绘图参数
        /// <para>比如放大比例变化的时候，就需要设置新的绘图参数</para>
        /// <para>不同比例或工作模式下的绘图数据存放在Specs集合中，本方法根据Stat和Scale获取新绘图数据</para>
        /// </summary>
        protected void SetCurrentTerminals() {
            Spec sp = GetCurrentSpec();
            if (sp != null) {
                foreach (Terminal terminal in Terminals) {
                    foreach (TerminalArea terminalArea in sp.TerminalAreas) {
                        if (terminalArea.Key == terminal.Key) {
                            terminal.X = X + terminalArea.X;
                            terminal.Y = Y + terminalArea.Y;
                            terminal.Width = terminalArea.W;
                            terminal.Height = terminalArea.H;
                            terminal.LocalLinkPoint = new PointF(terminalArea.CX, terminalArea.CY);
                            foreach (Junction junction in terminal.Junctions) {
                                junction.X = X + terminal.LocalLinkPoint.X;
                                junction.Y = Y + terminal.LocalLinkPoint.Y;
                            }
                        }
                    }
                }
            }
        }

        private int _scale = 1;

        [JsonPropertyName("scale"), Browsable(false)]
        public override int Scale {
            get { return _scale; }
            set {
                if (_scale != value && value >= 1 && value <= 4) {
                    _scale = value;
                    SetCurrentTerminals();
                }
            }
        }

        private WorkStat _stat = WorkStat.StopOrOpen;

        [JsonPropertyName("stat"), Browsable(false)]
        public override WorkStat Stat {
            get { return _stat; }
            set {
                if (_stat != value) {
                    _stat = value;
                    SetCurrentTerminals();
                }
            }
        }

        [Browsable(false)]
        Terminal LastMouseUp { get; set; }

        /// <summary>
        /// 获取最近的接线柱
        /// </summary>
        /// <param name="p">世界坐标点</param>
        /// <param name="l">距离</param>
        /// <returns></returns>
        public Terminal GetClosestTerminal(PointF p, float l) {
            Spec spec = GetCurrentSpec();
            if (spec != null && spec.TerminalAreas.Count > 0) {
                foreach (TerminalArea terminalArea in spec.TerminalAreas) {
                    RectangleF wr = L2W_RectangleF(terminalArea.X, terminalArea.Y, terminalArea.W, terminalArea.H);
                    PointF zx = new PointF((wr.X + wr.Width / 2), (wr.Y + wr.Height) / 2);
                    if (Fun.CalculateDistance(p, zx) <= l) {
                        for (int i = 0; i < Terminals.Count; i++) {
                            if (Terminals[i].Key == terminalArea.Key) {
                                return Terminals[i];
                            }
                        }
                    }
                }
            }
            return null;
        }

        public void MouseDown(PointF worldPoint) {
            IsSelected = EleRectangleF.Contains(worldPoint);
        }

        public void MouseMove(PointF worldPoint) {
            if (EleRectangleF.Contains(worldPoint)) {
                Spec spec = GetCurrentSpec();
                if (!IsMouseUp) { IsDirty = true; }
                IsMouseUp = true;
                foreach (Terminal terminal in Terminals) {
                    TerminalArea ta = GetTerminalArea(spec, terminal.Key);
                    RectangleF rf = new RectangleF(ta.X + X, ta.Y + Y, ta.W, ta.H);
                    if (rf.Contains(worldPoint)) {
                        if (MouseUpTerminal == null || MouseUpTerminal != terminal) { IsDirty = true; }
                        MouseUpTerminal = terminal;
                    }
                    else {
                        if (MouseUpTerminal != null) { IsDirty = true; }
                        MouseUpTerminal = null;
                    }
                }
            }
            else {
                if (IsMouseUp) { IsDirty = true; }
                IsMouseUp = false;
            }
        }

        public override void Move(Offset offset) {
            base.Move(offset);
            for (int i = 0; i < Terminals.Count; i++) {
                Terminal t = Terminals[i];
                if (t.Wires.Count > 0) {
                    foreach (Wire wire in t.Wires) {
                        wire.SetJunction(t);
                    }
                }
            }
        }

        private Spec GetCurrentSpec() {
            Spec spec = null;
            for (int i = 0; i < Specs.Count; i++) {
                spec = Specs[i];
                if (spec.Scale == Scale && (spec.Stat & Stat) == Stat) {
                    return spec;
                }
            }
            return null;
        }

        private TerminalArea GetTerminalArea(Spec spec, TerminalKey key) {
            if (spec != null) {
                foreach (TerminalArea terminalArea in spec.TerminalAreas) {
                    if (terminalArea.Key == key) {
                        return terminalArea;
                    }
                }
            }
            return default;
        }

        public virtual void Reset() { }

        private Terminal GetTerminalByKey(TerminalKey k) {
            foreach (Terminal terminal in Terminals) {
                if (terminal.Key == k) {
                    return terminal;
                }
            }
            return null;
        }

        public float GetRsistance(Terminal t1 = null, Terminal t2 = null, bool IsIdeal = true) {
            float r = float.NaN;
            if (t1 == null && t2 == null) {
                switch (Type) {
                    case ComponentType.Fan:
                    case ComponentType.BatteryCase:
                    case ComponentType.Lampstand:
                    case ComponentType.Resistor:
                    case ComponentType.Ammeter:
                    case ComponentType.Voltmeter:
                        t1 = GetTerminalByKey(TerminalKey.Left);
                        t2 = GetTerminalByKey(TerminalKey.Right);
                        break;
                    case ComponentType.Rheostat:
                        t1 = GetTerminalByKey(TerminalKey.LeftUp);
                        t2 = GetTerminalByKey(TerminalKey.RightDown);
                        break;
                }
            }
            if (t1 == null || t2 == null) {
                throw new System.Exception("请传递正确的接线柱");
            }
            if (Fault == FaultType.断路) {
                r = float.NaN;
            }
            else if (Fault == FaultType.短路) {
                r = 0;
            }
            else {
                if (Type == ComponentType.Ammeter) {
                    if (IsIdeal) {
                        r = 0;
                    }
                    else {
                        r = ((Ammeter)this).GetResistance(t1, t2);
                    }
                }
                else if (Type == ComponentType.Switch && Stat == WorkStat.Working) {
                    r = 0;
                }
                else if (Type == ComponentType.Switch && Stat == WorkStat.StopOrOpen) {
                    r = float.NaN;
                }
                else if (Type == ComponentType.Rheostat) {
                    if (((int)t1.Key + (int)t2.Key == 6)) {
                        r = 0;
                    }
                    else if (((int)t1.Key + (int)t2.Key) == 24) {
                        r = ((Rheostat)this).MaxResistance;
                    }
                    else {
                        r = ((Rheostat)this).GetResistance(t1, t2);
                    }
                }
                else {
                    switch (Type) {
                        case ComponentType.Fan:
                            r = ((Fan)this).GetResistance(t1, t2);
                            break;
                        case ComponentType.Resistor:
                            r = ((Resistor)this).GetResistance(t1, t2);
                            break;
                        case ComponentType.Voltmeter:
                            if (IsIdeal) {
                                r = float.NaN;
                            }
                            else {
                                r = ((Voltmeter)this).GetResistance(t1, t2);
                            }
                            break;
                        case ComponentType.Lampstand:
                            r = ((Lampstand)this).GetResistance(t1, t2);
                            break;
                        case ComponentType.BatteryCase:
                            if (IsIdeal) {
                                r = 0;
                            }
                            else {
                                r = ((BatteryCase)this).GetResistance(t1, t2);
                            }
                            break;
                    }
                }
            }
            return r;
        }

        public void LinkWire(Wire w, WireArea wa) {

        }

        /// <summary>
        /// 不合格的电路元素
        /// </summary>
        [Browsable(false)]
        public bool IsBadPath {
            get {
                int n = 0;
                for (int i = 0; i < Terminals.Count; i++) {
                    Terminal t = Terminals[i];
                    if (t.Junctions.Count > 0) {
                        n++;
                    }
                }
                return n < 2;
            }
        }

        public bool IsMyTerminal(Terminal t) {
            if (t == null) return false;
            foreach (Terminal terminal in Terminals) {
                if (terminal.Id == t.Id) {
                    return true;
                }
            }
            return false;
        }

        private void DrawSymbolName(Graphics g, Brush b) {
            Font f = new Font("Times New Roman", 10, FontStyle.Italic | FontStyle.Bold);
            string zm = SymbolName.Substring(0, 1);
            string sz = SymbolName.Substring(1, SymbolName.Length - 1);
            float fs = f.Size * 0.7f;
            SizeF zmsf = g.MeasureString(zm, f);
            Font f_sz = new Font(f.FontFamily, fs, FontStyle.Italic | FontStyle.Bold);
            SizeF szsf = g.MeasureString(sz, f_sz);
            SizeF sf = new SizeF(zmsf.Width + szsf.Width, zmsf.Height + szsf.Height * 0.3f);
            float x = X + (Width - sf.Width) / 2;
            float y = Y + Height + 1;
            g.DrawString(zm, f, b, x, y);
            x += zmsf.Width / 2 + 3;
            y += szsf.Height * 0.4f;
            g.DrawString(sz, f_sz, b, x, y);
        }

        public override void Draw(Graphics g, Color BC, Color FC, Font f) {
            Spec spec = GetCurrentSpec();

            if (spec != null) {
                Width = spec.BackImage.Width;
                Height = spec.BackImage.Height;
                g.DrawImage(spec.BackImage, X, Y, Width, Height);
                if (IsSelected || IsMouseUp) {
                    Pen p = new Pen(FC);
                    if (MouseUpTerminal == null) {
                        if (IsMouseUp && !IsSelected) {
                            p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        }
                        g.DrawRectangle(p, SelectedRectangle);
                    }
                    else {
                        TerminalArea ta = GetTerminalArea(spec, MouseUpTerminal.Key);
                        Rectangle rect = new Rectangle((int)(X + ta.X), (int)(Y + ta.Y), (int)ta.W, (int)ta.H);
                        p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        g.DrawRectangle(p, rect);
                    }
                }
                if (Bench != null && Bench.IsShowElementName) {
                    string sn = SymbolName;
                    if (IsShowMainProperties) {
                        switch (Type) {
                            case ComponentType.BatteryCase:
                                sn += "=" + ((BatteryCase)this).Voltage + "V";
                                break;
                            case ComponentType.Resistor:
                                sn += "=" + ((Resistor)this).Resistance + "Ω";
                                break;
                            case ComponentType.Lampstand:
                                Lampstand l = ((Lampstand)this);
                                sn += " : P额=" + l.RatedPower.ToString("f2") + "W,V额=" + l.RatedVoltage + "V";
                                break;
                            case ComponentType.Rheostat:
                                sn += " :最大阻值" + ((Rheostat)this).MaxResistance + "Ω";
                                break;
                        }
                        if (!Consts.IsZero(Current) && !float.IsNaN(Current) && Type != ComponentType.BatteryCase) {
                            sn += " I=" + Current.ToString("f2") + "A";
                        }
                    }
                    //SizeF ss = g.MeasureString(sn, f);
                    //float x = X + (Width - ss.Width) / 2;
                    //float y = Y + Height + 1;
                    //TextRegion = new RectangleF(x, y, ss.Width, ss.Height);
                    //g.DrawString(sn, f, new SolidBrush(FC), x, y);
                    DrawSymbolName(g, new SolidBrush(FC));
                }
            }
            IsDirty = false;
        }

        public virtual string ToJson() {
            string s = "{Name:" + SymbolName + ",Type:" + (int)Type + ",Stat:" + (int)Stat + ",Scale:" + Scale + ",X:" + X + "," + "Y:" + Y + Consts.ZWF + "}";
            return s;
        }

        public static Element BuildElement(string json) {
            json = Consts.DeleteBothEndChar(json, "[", "]");
            json = Consts.DeleteBothEndChar(json, "{", "}");
            string[] keyvalues = json.Split(',');
            if (keyvalues != null) {
                foreach (string nv in keyvalues) {
                    string[] p = nv.Split(':');
                    switch (p[0]) {
                        case "":
                            break;
                    }
                }
            }
            return null;
        }
    }
}
