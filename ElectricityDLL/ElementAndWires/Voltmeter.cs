using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace ElectricityDLL {
    /// <summary>
    /// 电压表
    /// </summary>
    public class Voltmeter : Element {
        public VoltmeterCircuitImageInfo CircuitImageInfo { get; private set; }
        [Browsable(false)]
        public Image InnerPicture { get; set; } = Properties.Resources.voltmeter_biaotou;

        [NumberUpDown("R1", 100, 6000, 1000, 3000, IsReadOnly = true, IsMergeCol = false), Browsable(false)]
        public float r1 { get; set; } = 3000f;

        [NumberUpDown("R2", 400, 24000, 4000, 12000, IsReadOnly = true, IsMergeCol = false), Browsable(false)]
        public float r2 { get; set; } = 12000f;

        /// <summary>
        /// 电压表的指针绘图数据
        /// </summary>
        [Browsable(false)]
        public List<Pointer> Pointers { get; } = new List<Pointer>();

        [Browsable(false)]
        Pointer CurrentPointer {
            get {
                for (int i = 0; i < Pointers.Count; i++) {
                    Pointer _p = Pointers[i];
                    if (_p.Scale == Scale) { return _p; }
                }
                return default;
            }
        }

        [Category(Consts.PGC_cat4), DisplayName("当前量程")]
        public string CurrentRange {
            get {
                if (Left.Junctions.Count > 0 && Right.Junctions.Count > 0) {
                    return "0～15V";
                }
                else if (Left.Junctions.Count > 0 && Middle.Junctions.Count > 0) {
                    return "0～3V";
                }
                else {
                    return "陷入沉思";
                }
            }
        }

        [Category(Consts.PGC_cat2), DisplayName("电压表示数")]
        public string CurrentVoltage {
            get {
                if (Left.JoinedNode != null && Right.JoinedNode != null) {
                    float a = Right.JoinedNode.Potential - Left.JoinedNode.Potential;
                    return FormatNumeric(a, "V");
                }
                else if (Left.JoinedNode != null && Middle.JoinedNode != null) {
                    float a = Middle.JoinedNode.Potential - Left.JoinedNode.Potential;
                    return FormatNumeric(a, "V");
                }
                return "0 V";
            }
        }

        public Voltmeter() {
            Type = ComponentType.Voltmeter;
            Symbol = "V";
            OutputImage = Properties.Resources.voltmeter1XInit;

            Terminal Left = new Terminal(this, TerminalKey.Left) { NeedAD = AD.EndToStart };
            Terminal Right = new Terminal(this, TerminalKey.Right) { NeedAD = AD.StartToEnd };
            Terminal Middle = new Terminal(this, TerminalKey.Middle) { NeedAD = AD.StartToEnd };
            Terminals.Add(Left);
            Terminals.Add(Middle);
            Terminals.Add(Right);

            Spec X1 = new Spec(Properties.Resources.voltmeter1X, 1);
            X1.TerminalAreas.Add(new TerminalArea(TerminalKey.Left, 4, 37, 8, 8, 7, 45));
            X1.TerminalAreas.Add(new TerminalArea(TerminalKey.Middle, 28, 37, 8, 8, 33, 44));
            X1.TerminalAreas.Add(new TerminalArea(TerminalKey.Right, 53, 37, 8, 8, 57, 44));
            Specs.Add(X1);

            Spec X2 = new Spec(Properties.Resources.voltmeter2X, 2);
            X2.TerminalAreas.Add(new TerminalArea(TerminalKey.Left, 6, 72, 16, 18, 14, 87));
            X2.TerminalAreas.Add(new TerminalArea(TerminalKey.Middle, 56, 72, 16, 18, 65, 87));
            X2.TerminalAreas.Add(new TerminalArea(TerminalKey.Right, 106, 72, 16, 18, 105, 87));
            Specs.Add(X2);

            Spec X3 = new Spec(Properties.Resources.voltmeter3X, 3);
            X3.TerminalAreas.Add(new TerminalArea(TerminalKey.Left, 8, 108, 23, 26, 20, 120));
            X3.TerminalAreas.Add(new TerminalArea(TerminalKey.Middle, 84, 108, 23, 26, 86, 120));
            X3.TerminalAreas.Add(new TerminalArea(TerminalKey.Right, 158, 108, 23, 26, 172, 120));
            Specs.Add(X3);

            Spec X4 = new Spec(Properties.Resources.voltmeter4X, 4);
            X4.TerminalAreas.Add(new TerminalArea(TerminalKey.Left, 10, 145, 34, 32, 28, 173));
            X4.TerminalAreas.Add(new TerminalArea(TerminalKey.Middle, 112, 145, 34, 32, 128, 173));
            X4.TerminalAreas.Add(new TerminalArea(TerminalKey.Right, 212, 145, 34, 32, 228, 173));
            Specs.Add(X4);

            TerminalPair Pair1 = new TerminalPair(Right, Left);
            TerminalPair Pair2 = new TerminalPair(Middle, Left);
            TerminalPairs.AddRange(new[] { Pair1, Pair2 });

            Pointer p1 = new Pointer(this, new PointF(32.5f, 36.5f), new PointF(15.5f, 16.5f));
            Pointer p2 = new Pointer(this, new PointF(64.5f, 72.5f), new PointF(28.5f, 30.5f), 2);
            Pointer p3 = new Pointer(this, new PointF(96f, 107f), new PointF(41.5f, 43.5f), 3);
            Pointer p4 = new Pointer(this, new PointF(128f, 144f), new PointF(56f, 58.5f), 4);
            Pointers.AddRange(new[] { p1, p2, p3, p4 });

            SetCurrentTerminals();

            CircuitImageInfo = new VoltmeterCircuitImageInfo(this);
        }

        public override float GetResistance(Terminal t1, Terminal t2) {
            if (!IsIdeal) {
                if (t1 == null || t2 == null) return float.NaN;
                int k = (int)t1.Key + (int)t2.Key;
                if (k == 10) {
                    return r1;
                }
                else if (k == 6) {
                    return r1 + r2;
                }
                else if (k == 12) {
                    return r2;
                }
                else {
                    return float.NaN;
                }
            }
            else {
                return float.NaN;
            }
        }

        public override void Draw(Graphics g, Color BC, Color FC, Font f) {
            base.Draw(g, BC, FC, f);
            Pointer p = CurrentPointer;
            TerminalPair t = GetValidTerminal();
            Console.WriteLine("t.Volgate=" + (t == null ? "null" : t.Voltage.ToString()));
            float angle = (float)Math.Atan((p.P.Y - p.O.Y) / (p.P.X - p.O.X));
            if (t == null) {
                //angle = 0;
            }
            else {
                float min = -44f;
                float max = 124f;
                bool clockWise = (t.T1.Polar == Polarity.Positive && t.T2.Polar == Polarity.Negative) ? true : false;
                float a2 = t.Keys == 6 ? t.Voltage * 4 / 0.5f : t.Voltage * 4 / 0.1f;
                angle += a2;
                Console.WriteLine("初始角度" + angle.ToString("f2"));
                Console.WriteLine("偏转" + a2.ToString("f2"));
                //if (t.T1.Potential > t.T2.Potential) {
                //    angle = Math.Abs(angle);
                //}
                //else if (t.T1.Potential < t.T2.Potential) {
                //    angle = -Math.Abs(angle);
                //}
                //if (!clockWise) angle = -angle;
                if (angle > max) angle = max;
                if (angle < min) angle = min;
            }


            g.TranslateTransform(X + p.O.X, Y + p.O.Y);
            g.RotateTransform(angle);
            g.DrawLine(new Pen(Color.Red), 0, 0, p.P.X - p.O.X, p.P.Y - p.O.Y);
            g.TranslateTransform(-(X + p.O.X), -(Y + p.O.Y));
            g.ResetTransform();
        }
    }
}
