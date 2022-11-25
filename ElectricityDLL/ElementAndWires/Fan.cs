using System;
using System.ComponentModel;
using System.Drawing;

namespace ElectricityDLL {
    /// <summary>
    /// 小风扇/电动机
    /// </summary>
    public class Fan : Element {
        public FanCircuitImageInfo CircuitImageInfo { get; private set; }

        [Xuliehua("Resistance")]
        internal float _r = 0.5f;

        [DisplayName("线圈电阻"), Category(Consts.PGC_cat1), Description("由导线的粗细、长度、材料、温度决定的导体电阻。可更改该参数")]
        public string Resistance {
            get {
                return FormatNumeric(_r, "Ω");
            }
            set {
                _r = StrToFloat(value, "Ω", _r);
            }
        }

        [Xuliehua("InductiveReactance")]
        internal float _ir = 9.5f;

        [Xuliehua("RateVoltage")]
        internal float _rv = 3f;

        [DisplayName("感抗"), Category(Consts.PGC_cat1), Description("当电动机的线圈中流过变化的电流时，会产生变化的磁场，这个变化的磁场会阻碍电子的移动，起到类似电阻的效果，可更改该参数")]
        public string InductiveReactance {
            get {
                return FormatNumeric(_ir, "Ω");
            }
            set {
                _ir = StrToFloat(value, "Ω", _ir);
            }
        }

        [DisplayName("额定功率"), Category(Consts.PGC_cat1), Description("电动机在额定电压下的功率，根据使用的场合，在设计时确定")]
        public string RatePower {
            get {
                return FormatNumeric(_rv * _rv / (_r + _ir), "W");
            }
        }

        [DisplayName("额定电压"), Category(Consts.PGC_cat1), Description("电动机正常工作时的电压，根据用电环境(如汽车电瓶12V，我国家庭电路220V)，在设计时确定")]
        public string RateVoltage {
            get {
                return FormatNumeric(_rv, "V");
            }
            set {
                _rv = StrToFloat(value, "V", _rv);
            }
        }

        [DisplayName("实际功率"), Description("电动机的实际功率，单位瓦特(W)，由电流的平方×(线圈电阻+感抗)简单计算获得"), Category(Consts.PGC_cat2)]
        public string Power {
            get {
                float a = Current * Current * (_r + _ir);
                return FormatNumeric(a, "W");
            }
        }

        [DisplayName("机械功率"), Description("电动机的机械功率，单位瓦特(W)，由电流的平方×感抗简单计算获得"), Category(Consts.PGC_cat2)]
        public string UsefulPower {
            get {
                float a = Current * Current * _ir;
                return FormatNumeric(a, "W");
            }
        }

        [DisplayName("实际电压"), Description("电动机的实际电压，单位伏特(V)，由电流×(线圈电阻+感抗)简单计算获得"), Category(Consts.PGC_cat2)]
        public string Voltage {
            get {
                return FormatNumeric(Math.Abs(Current) * (_r + _ir), "V");
            }
        }

        [DisplayName("机械效率"), Description("电动机的机械效率，机械功率与总功率的比值，由机械功率➗实际功率计算获得"), Category(Consts.PGC_cat2)]
        public string Efficiency {
            get {
                if (Consts.IsZero(Current) && !float.IsNaN(Current)) {
                    return ((int)(100 * _ir / (_r + _ir))).ToString() + "%";
                }
                else {
                    return Consts.PGC_unknow;
                }
            }
        }

        [Browsable(false)]
        private PointF Zhou { get; } = new PointF(41.5f, 5.5f);

        [Browsable(false)]
        private FanLeaf Leaf { get; } = new FanLeaf();

        private void Init() {
            Type = ComponentType.Fan;
            Symbol = "M";
            OutputImage = Properties.Resources.fanBase;
            Width = OutputImage.Width;
            Height = OutputImage.Height;

            Spec X1 = new Spec(Properties.Resources.fanBase);
            X1.TerminalAreas.Add(new TerminalArea(TerminalKey.Left, 9, 12, 8, 8, 13, 19));
            X1.TerminalAreas.Add(new TerminalArea(TerminalKey.Right, 67, 12, 8, 8, 70.5f, 19));
            Specs.Add(X1);

            Terminal Left = new Terminal() { Owner = this, Key = TerminalKey.Left };
            Terminals.Add(Left);
            Terminal Right = new Terminal() { Owner = this, Key = TerminalKey.Right };
            Terminals.Add(Right);

            TerminalPairs.Add(new TerminalPair(Left, Right));
        }

        public Fan() {
            Init();
            SetCurrentTerminals();
            CircuitImageInfo = new FanCircuitImageInfo(this);
        }

        public override float GetResistance(Terminal t1, Terminal t2) {
            return _r + _ir;
        }

        public override bool IsAnimation {
            get {
                if (!Consts.IsZero(Current) && !float.IsNaN(Current)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        public override void Draw(Graphics g, Color BC, Color FC, Font f) {
            //IsAnimation = true;
            base.Draw(g, BC, FC, f);

            float angle = 360f * FrameIndex / FPS;

            Bitmap bitmap = (Bitmap)Properties.Resources.fanLeaf.Clone();
            float dx = X + Zhou.X;
            float dy = Y + Zhou.Y;
            RectangleF tr = new RectangleF(dx, dy, bitmap.Width, bitmap.Height);

            g.TranslateTransform(dx, dy);
            g.RotateTransform(angle);
            g.DrawImage(bitmap, -Leaf.COP.X, -Leaf.COP.Y, bitmap.Width, bitmap.Height);
            g.TranslateTransform(-dx, -dy);
            g.ResetTransform();
        }

        public override string ToJson() {
            string s = base.ToJson();
            string ss = ",Resistance:" + _r + ",InductiveReactance:" + _ir + ",RatedVoltage:" + _rv;
            s = s.Replace(Consts.ZWF, ss);
            return s;
        }
    }
}
