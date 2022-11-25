using System.ComponentModel;
using System.Drawing;

namespace ElectricityDLL {
    /// <summary>
    /// 导线连接点
    /// </summary>
    public class Junction : EleComponent {

        [Browsable(false)]
        public override string LinkedCircuit => base.LinkedCircuit;

        [Browsable(false)]
        [IgnoreXuliehua]
        public override string SymbolName { get => base.SymbolName; set => base.SymbolName = value; }

        [Browsable(false)]
        [IgnoreXuliehua]
        public override WorkStat Stat { get => base.Stat; set => base.Stat = value; }

        [Browsable(false)]
        [IgnoreXuliehua]
        public override ComponentType Type { get => base.Type; protected set => base.Type = value; }

        [Browsable(false)]
        [IgnoreXuliehua]
        public override FaultType Fault { get => base.Fault; set => base.Fault = value; }

        [Browsable(false)]
        [IgnoreXuliehua]
        public override int Scale { get => base.Scale; set => base.Scale = value; }

        public override string CnName {
            get {
                switch (Area) {
                    case WireArea.EndPoint:
                    case WireArea.StartPoint:
                        return "导线接头";
                    case WireArea.EndHandle:
                    case WireArea.StartHandle:
                        return "曲线控制点";
                }
                return "";
            }
        }

        [Browsable(false)]
        public AD AssumeDirection { get; set; } = AD.UnKnow;

        /// <summary>
        /// 坐标
        /// </summary>
        [Browsable(false)]
        public PointF P { get; set; } = PointF.Empty;

        /// <summary>
        /// 接线柱
        /// </summary>
        [Browsable(false)]
        [Xuliehua(Consts.Json_Terminal)]
        public Terminal T { get; set; } = null;

        [Browsable(false)]
        public Element Device { get; set; }

        /// <summary>
        /// 电位
        /// </summary>
        [Browsable(false)]
        public float Potential { get; set; } = 0f;

        /// <summary>
        /// 经过的电器数
        /// </summary>
        [Browsable(false)]
        public int Routers { get; set; } = 0;

        /// <summary>
        /// 接头处是否存在电压
        /// </summary>
        [Browsable(false)]
        public bool HasVoltage { get; set; } = false;

        [Browsable(false)]
        public bool HasBranch { get; set; } = false;

        [Browsable(false)]
        public bool IsMoved { get; set; } = false;

        [Browsable(false)]
        public float Radius { get; set; } = 3;

        public override RectangleF EleRectangleF {
            get {
                return new RectangleF(X - Radius, Y - Radius, Radius * 2 + 1, Radius * 2 + 1);
            }
        }

        [Browsable(false)]
        public RectangleF SmallRect {
            get {
                return new RectangleF(X - 2, Y - 2, 5, 5);
            }
        }

        public override RectangleF Region {
            get {
                return new RectangleF(X - Radius, Y - Radius, Radius * 2 + 1, Radius * 2 + 1);
            }
            set {
                X = value.X;
                Y = value.Y;
                Radius = (value.Width - 1) / 2;
            }
        }

        /// <summary>
        /// 位置
        /// </summary>
        [Browsable(false)]
        public WireArea Area { get; set; } = WireArea.No;

        public Junction(Wire w, WireArea a = WireArea.No, Terminal t = null) {
            Type = ComponentType.WireJunction;
            Owner = w;
            Area = a;
            if (t != null) {
                T = t;
                T.AddJunction(this);
                X = t.WorldLinkPoint.X;
                Y = t.WorldLinkPoint.Y;
            }
        }

        /// <summary>
        /// 获得导线另一端的接线柱
        /// </summary>
        /// <param name="applyWire"></param>
        /// <returns></returns>
        public Terminal AnotherSideTerminal(bool applyWire = true) {
            Terminal t = null;
            if (Owner != null) {
                Junction j = AnotherSideJunction;
                if (j != null) {
                    t = j.T;
                    if (t != null) {
                        if (!applyWire && t.Owner.GetType() == typeof(Rheostat) && (t.Key == TerminalKey.LeftUp || t.Key == TerminalKey.RightUp)) {
                            Terminal t1 = ((Rheostat)t.Owner).Terminals[4];
                            foreach (Terminal terminal in ((Element)t.Owner).Terminals) {
                                if (terminal.Key == TerminalKey.LeftUp || terminal.Key == TerminalKey.RightUp) {
                                    t1.Junctions.AddRange(terminal.Junctions);
                                }
                            }
                            t = t1;
                        }
                    }
                }
            }
            return t;
        }

        /// <summary>
        /// 获取另一端的导线接头
        /// </summary>
        [Browsable(false)]
        public Junction AnotherSideJunction {
            get {
                if (Owner != null) {
                    if (this.Area == WireArea.StartPoint) {
                        return ((Wire)Owner).Junctions[3];
                    }
                    else {
                        return ((Wire)Owner).Junctions[0];
                    }
                }
                return null;
            }
        }

        public string ToJson() {
            return "{X:" + X + ",Y:" + Y + "}";
        }
    }
}
