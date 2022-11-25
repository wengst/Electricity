using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace ElectricityDLL {
    /// <summary>
    /// 接线柱
    /// </summary>
    public class Terminal : EleComponent {
        [Category(Consts.PGC_cat3), DisplayName("位置"), Description("接线柱所属元器件和位置")]
        public override string CnName {
            get {
                string n = Owner.SymbolName + "的";
                switch (Key) {
                    case TerminalKey.Left:
                        n += "左边";
                        break;
                    case TerminalKey.Right:
                        n += "右边";
                        break;
                    case TerminalKey.LeftDown:
                        n += "左下";
                        break;
                    case TerminalKey.LeftUp:
                        n += "左上";
                        break;
                    case TerminalKey.Middle:
                        n += "中间";
                        break;
                    case TerminalKey.MiddleDown:
                        n += "中下";
                        break;
                    case TerminalKey.MiddleUp:
                        n += "中下";
                        break;
                    case TerminalKey.RightDown:
                        n += "右下";
                        break;
                    default:
                        n += "右上";
                        break;
                }
                n += "接线柱";
                return n;
            }
        }

        [Browsable(false)]
        [IgnoreXuliehua]
        public override string SymbolName { get => base.SymbolName; set => base.SymbolName = value; }

        [Category(Consts.PGC_cat2), DisplayName("节点电位"), Description("接线柱所在节点的电位(或叫电势)，两节点间的电位差也叫电压。单位伏特(V)")]
        public string JDDW {
            get {
                if (JoinedNode != null && !float.IsNaN(JoinedNode.Potential) && Owner.Type != ComponentType.BatteryCase) {
                    return JoinedNode.Potential.ToString("f2") + "V";
                }
                else if (Owner.Type == ComponentType.BatteryCase && Key == TerminalKey.Left) {
                    return "0V";
                }
                else if (Owner.Type == ComponentType.BatteryCase && Key == TerminalKey.Right) {
                    return ((BatteryCase)Owner).Voltage.ToString("f2") + "V";
                }
                return Consts.PGC_unknow;
            }
        }

        /// <summary>
        /// 接线柱位置的电位
        /// 每个接线柱节点只有唯一电位
        /// 电位可以是正数也可以是负数。
        /// 如果是负数，表示该点电位比参考电位低
        /// </summary>
        [Browsable(false)]
        public float Potential { get; set; } = float.NaN;

        /// <summary>
        /// 预设的电位。电源正极处的电位为0
        /// </summary>
        [Browsable(false)]
        public float AssumePotential { get; set; } = 0;

        /// <summary>
        /// 是否是虚拟接线柱
        /// </summary>
        [Browsable(false)]
        public bool IsVirtual { get; set; } = false;

        /// <summary>
        /// 推测的电流方向
        /// </summary>
        [Browsable(false)]
        public AD SpeculateAD { get; set; } = AD.UnKnow;

        /// <summary>
        /// 需要的电流方向
        /// </summary>
        [Browsable(false)]
        public AD NeedAD { get; set; } = AD.TwoWay;

        [Browsable(false)]
        public int IsValid { get; set; } = -1;

        [Browsable(false)]
        public int BothSideObjCount { get; set; } = 0;

        /// <summary>
        /// 已加入的节点或者称为等电位节点
        /// <para>既用无故障的导线、已闭合且无断路故障的开关、理想电路中的电流表等连接起来的接线柱的节点</para>
        /// </summary>
        [Browsable(false)]
        public Node JoinedNode { get; set; }

        /// <summary>
        /// 检测是否与指定接线柱等电位
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool IsEquPotential(Terminal t) {
            Terminal tmp;
            //本接线柱上的导线接头到另一端的导线接头之间有没有故障，没故障则判断等电位
            foreach (Junction junction in Junctions) {
                tmp = junction.AnotherSideTerminal();
                if (tmp != null && tmp == t && junction.Owner.Fault == FaultType.无) {
                    return true;
                }
            }
            //判断到元件的另一端接线之间的电阻，如为零，则等电位
            Element owner = (Element)Owner;
            foreach (Terminal terminal in owner.Terminals) {
                if (terminal.Id == t.Id && terminal.Id != Id && owner.GetResistance(this, terminal) == 0 && Owner.Type != ComponentType.BatteryCase) {
                    return true;
                }
            }
            return false;
        }

        [Browsable(false)]
        public List<Junction> Junctions { get; } = new List<Junction>();

        /// <summary>
        /// 该接线柱是否存在支路
        /// </summary>
        [Browsable(false)]
        public bool HasBranch {
            /**
             * 这个判断一定要准备，切合含义
             */
            get {
                int n = 0;
                Element e = (Element)Owner;
                List<Terminal> ts = new List<Terminal>();
                if (e.Type == ComponentType.Rheostat) {
                    Rheostat r = (Rheostat)e;
                    int j0 = 0, j1 = 0, j2 = 0;
                    j0 = r.LeftUp.Junctions.Count + r.RightUp.Junctions.Count;
                    j1 = r.LeftDown.Junctions.Count;
                    j2 = r.RightDown.Junctions.Count;

                    if (Key == TerminalKey.MiddleUp || Key == TerminalKey.LeftUp || Key == TerminalKey.RightUp) {
                        return j0 > 1 || (j1 > 0 && j2 > 0);
                    }
                    else if (Key == TerminalKey.LeftDown) {
                        return j1 > 1;
                    }
                    else if (Key == TerminalKey.Right) {
                        return j2 > 1;
                    }
                }
                else if (e.Type == ComponentType.BatteryCase) {
                    return Junctions.Count > 1;
                }
                else {
                    for (int i = 0; i < e.Terminals.Count; i++) {
                        if (e.Terminals[i].Id != Id && e.Terminals[i].Junctions.Count > 0) {
                            n++;
                        }
                    }
                    if (Junctions.Count > 1) {
                        n += (Junctions.Count - 1);
                    }
                    return n >= 2;
                }
                return false;
            }
        }

        public void ClearJunctions() {
            for (int i = Junctions.Count - 1; i >= 0; i--) {
                Junction j = Junctions[i];
                j.T = null;
                Junctions.RemoveAt(i);
            }
        }

        public void AddJunction(Junction j) {
            if (j == null || j.Area == WireArea.EndHandle || j.Area == WireArea.StartHandle) return;
            bool b = false;
            foreach (Junction item in Junctions) {
                if (item.Id == j.Id) {
                    b = true;
                }
            }
            if (!b) {
                Junctions.Add(j);
            }
        }

        public void RemoveJunction(Junction j) {
            if (j == null) return;
            for (int i = 0; i < Junctions.Count; i++) {
                if (Junctions[i].Id == j.Id) {
                    Junctions.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// 获取其他的实体接线柱
        /// </summary>
        [Browsable(false)]
        public List<Terminal> OtherTerminals {
            get {
                List<Terminal> list = new List<Terminal>();
                Element owner = (Element)Owner;
                for (int i = 0; i < owner.Terminals.Count; i++) {
                    Terminal t = owner.Terminals[i];
                    if (t.Id != Id && !t.IsVirtual) {
                        list.Add(t);
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// 获取除本接线柱及另外一个接线柱之外的其他接线柱
        /// </summary>
        /// <param name="it"></param>
        /// <returns></returns>
        public List<Terminal> OtherTerminalsNoIt(Terminal it) {
            List<Terminal> others = OtherTerminals;
            for (int i = 0; i < others.Count; i++) {
                if (others[i].Id == it.Id) {
                    others.RemoveAt(i);
                }
            }
            return others;
        }

        /// <summary>
        /// 获取通过导线连接到本接线柱的接线柱集合
        /// </summary>
        [Browsable(false)]
        public List<Terminal> ConnectedTerminals {
            get {
                List<Terminal> list = new List<Terminal>();
                foreach (Junction junction in Junctions) {
                    Terminal t = junction.AnotherSideTerminal();
                    if (t != null) {
                        list.Add(t);
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// 返回排除掉某接线柱的邻居
        /// </summary>
        /// <param name="exclude"></param>
        /// <returns></returns>
        public List<Terminal> Neighbors(Terminal exclude) {
            List<Terminal> list = ConnectedTerminals;
            for (int i = 0; i < list.Count; i++) {
                if (list[i].Id == exclude.Id) {
                    list.RemoveAt(i);
                    break;
                }
            }
            return list;
        }

        /// <summary>
        /// 当接线柱所在的元件是电流表，开关或短路时，就属于路径接线柱
        /// </summary>
        [Browsable(false)]
        public bool IsPath {
            get {
                bool b = false;
                if (Owner.Fault == FaultType.短路) {
                    b = true;
                }
                else {
                    if (Owner.Type == ComponentType.Ammeter || (Owner.Type == ComponentType.Switch && Owner.Stat == WorkStat.Working)) {
                        b = true;
                    }
                }
                return b;
            }
        }

        /// <summary>
        /// 获取所有到邻居接线柱的路径
        /// </summary>
        [Browsable(false)]
        public List<ElePath> AllNeighborPaths {
            get {
                List<ElePath> paths = new List<ElePath>();
                List<EleComponent> eles;
                foreach (Junction junction in Junctions) {
                    Terminal t = junction.AnotherSideTerminal();
                    if (t != null) {
                        eles = new List<EleComponent>();
                        eles.Add(this);
                        eles.Add(this.Owner);
                        eles.Add(t);
                        paths.Add(new ElePath(eles));
                    }
                }
                return paths;
            }
        }

        /// <summary>
        /// 元件路径
        /// </summary>
        /// <param name="pe"></param>
        /// <returns></returns>
        public List<PathElement> ElementPaths(PathElement pe = null) {
            List<PathElement> ls = new List<PathElement>();
            Element E = null;
            if (Owner.Type != ComponentType.BatteryCase && Owner.Type != ComponentType.Rheostat && Junctions.Count > 0) {
                E = (Element)Owner;
                for (int i = 0; i < E.Terminals.Count; i++) {
                    Terminal t = E.Terminals[i];
                    if (t.Id != Id && t.Junctions.Count > 0) {
                        ls.Add(new PathElement(this, Owner, t));
                    }
                }
            }
            if (Owner.Type == ComponentType.Rheostat && Junctions.Count > 0) {
                E = (Rheostat)Owner;
                Terminal Tlu = E.FindTerminal(TerminalKey.LeftUp);
                Terminal Tru = E.FindTerminal(TerminalKey.RightUp);
                Terminal Tmu = E.FindTerminal(TerminalKey.MiddleUp);
                Terminal Tld = E.FindTerminal(TerminalKey.LeftDown);
                Terminal Trd = E.FindTerminal(TerminalKey.RightDown);
                Tmu.Junctions.Clear();
                Tmu.Junctions.AddRange(Tlu.Junctions);
                Tmu.Junctions.AddRange(Tru.Junctions);
                if (Tmu.Junctions.Count > 0 && Tld.Junctions.Count > 0) {
                    ls.Add(new PathElement(Tmu, E, Tld));
                }
                if (Tmu.Junctions.Count > 0 && Trd.Junctions.Count > 0) {
                    ls.Add(new PathElement(Tmu, E, Trd));
                }
                if (Tld.Junctions.Count > 0 && Trd.Junctions.Count > 0) {
                    ls.Add(new PathElement(Tld, E, Trd));
                }
            }

            if (pe != null) {
                for (int i = 0; i < ls.Count; i++) {
                    PathElement pe0 = ls[i];
                    if (pe0.IsEqual(pe)) {
                        ls.RemoveAt(i);
                        break;
                    }
                }
            }
            return ls;

        }

        /// <summary>
        /// 导线路径
        /// </summary>
        /// <param name="pe"></param>
        /// <returns></returns>
        public List<PathElement> WirePaths(PathElement pe = null) {
            List<PathElement> ls = new List<PathElement>();
            foreach (Junction junction in Junctions) {
                Terminal t2 = junction.AnotherSideTerminal(false);
                if (t2 != null) {
                    ls.Add(new PathElement(this, junction.Owner, t2));
                }
            }
            if (pe != null) {
                for (int i = 0; i < ls.Count; i++) {
                    PathElement pe0 = ls[i];
                    if (pe0.IsEqual(pe)) {
                        ls.RemoveAt(i);
                        break;
                    }
                }
            }
            return ls;
        }

        /// <summary>
        /// 获取该接线柱的路径元素
        /// <para>如果传递了路径元素，就排除该路径元素</para>
        /// </summary>
        /// <param name="pe"></param>
        /// <returns></returns>
        public List<PathElement> GetAllPathElement(PathElement pe = null) {
            List<PathElement> ls = ElementPaths(pe);
            ls.AddRange(WirePaths(pe));
            return ls;
        }

        /// <summary>
        /// 获取同一元件上，到不同接线柱的路径
        /// </summary>
        /// <returns></returns>
        public List<ElePath> GetElementPath() {
            List<ElePath> ls = new List<ElePath>();
            Element element = (Element)Owner;
            for (int i = 0; i < element.Terminals.Count; i++) {
                Terminal t = element.Terminals[i];
                if (t.Id != Id && t.Junctions.Count > 0) {
                    List<EleComponent> path = new List<EleComponent>();
                    path.Add(this);
                    path.Add(Owner);
                    path.Add(t);
                    ls.Add(new ElePath(path));
                }
            }
            return ls;
        }

        [Browsable(false)]
        public List<ElePath> AllPaths {
            get {
                List<ElePath> paths = AllNeighborPaths;
                foreach (Terminal terminal in OtherTerminals) {
                    if (terminal.Id != Id && terminal.Junctions.Count > 0) {
                        List<EleComponent> path = new List<EleComponent>();
                        path.Add(this);
                        path.Add(terminal.Owner);
                        path.Add(terminal);
                        paths.Add(new ElePath(path));
                    }
                }
                return paths;
            }
        }

        [Browsable(false)]
        public List<Wire> Wires {
            get {
                List<Wire> wires = new List<Wire>();
                foreach (Junction item in Junctions) {
                    wires.Add((Wire)item.Owner);
                }
                return wires;
            }
        }

        /// <summary>
        /// 连接到的电源极性
        /// </summary>
        [Browsable(false)]
        public Polarity Polar { get; set; } = Polarity.Notset;
        /// <summary>
        /// 接线柱位置枚举
        /// </summary>
        [Browsable(false)]
        [Xuliehua("Key")]
        public TerminalKey Key { get; set; }

        [Browsable(false)]
        public PointF LocalLinkPoint { get; set; } = PointF.Empty;

        [Browsable(false)]
        public PointF WorldLinkPoint {
            get {
                return new PointF(Owner.X + LocalLinkPoint.X, Owner.Y + LocalLinkPoint.Y);
            }
        }

        [Browsable(false)]
        public override PointF WorldPoint {
            get {
                return new PointF(Owner.X + X, Owner.Y + Y);
            }
        }

        public bool IsClosesd(PointF p, float l) {
            return Fun.CalculateDistance(p, WorldLinkPoint) < l;
        }

        public Terminal(Element owner = null, TerminalKey key = TerminalKey.Left) {
            if (owner != null) {
                Owner = owner;
            }
            Key = key;
            Type = ComponentType.Terminal;
        }

        [Browsable(false)]
        public float WX {
            get {
                return Owner.X + X;
            }
        }

        [Browsable(false)]
        public float WY {
            get { return Owner.Y + Y; }
        }

        [Browsable(false)]
        public RectangleF WRF => new RectangleF(WX, WY, Width, Height);
        [Browsable(false)]
        public Rectangle WR => new Rectangle((int)WX, (int)WY, (int)Width, (int)Height);

        public override bool Contains(PointF point) {
            RectangleF rect = new RectangleF(WorldPoint, new SizeF(Width + 4, Height + 4));
            bool b = rect.Contains(point);
            return b;
        }

        public override string ToString() {
            return Owner.SymbolName + "." + Key.ToString();
        }

        public void Draw(Graphics g) {

        }

        public enum TerminalDirection {
            /// <summary>
            /// 向下
            /// </summary>
            Down,
            /// <summary>
            /// 向上
            /// </summary>
            Up,
            /// <summary>
            /// 向左
            /// </summary>
            Left,
            /// <summary>
            /// 向右
            /// </summary>
            Right
        }
    }
}
