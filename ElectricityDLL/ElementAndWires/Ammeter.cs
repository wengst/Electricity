using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace ElectricityDLL {
    /// <summary>
    /// 电流表
    /// </summary>
    public class Ammeter : Element {
        [Browsable(false)]
        public AmmeterCircuitImageInfo CircuitImageInfo { get; private set; }

        [Image(), Browsable(false)]
        public Image InnerPicture { get; set; }

        [NumberUpDown("R0", IsReadOnly = true, IsMergeCol = false), Browsable(false)]
        public float r0 { get; set; } = 10f;

        [NumberUpDown("R1", 0.01f, 5.00f, 0.01f, 0.22f, IsReadOnly = true, IsMergeCol = false), Browsable(false)]
        public float r1 { get; set; } = 0.22f;

        [NumberUpDown("R2", 0.01f, 5.00f, 0.01f, 0.88f, IsReadOnly = true, IsMergeCol = false), Browsable(false)]
        public float r2 { get; set; } = 0.88f;

        [NumberUpDown("R3", 10f, 500f, 10f, 220f, IsReadOnly = true, IsMergeCol = false), Browsable(false)]
        public float r3 { get; set; } = 220f;

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

        /// <summary>
        /// 电压表的指针数据
        /// </summary>
        public List<Pointer> Pointers = new List<Pointer>();

        [Category(Consts.PGC_cat4), DisplayName("当前量程")]
        public string CurrentRange {
            get {
                if (Left.Junctions.Count > 0 && Right.Junctions.Count > 0) {
                    return "0～3A";
                }
                else if (Left.Junctions.Count > 0 && Middle.Junctions.Count > 0) {
                    return "0～0.6A";
                }
                else {
                    return "陷入沉思";
                }
            }
        }

        [Category(Consts.PGC_cat2), DisplayName("电流表示数"), Description("如果示数是负数，表明线路接反了")]
        public string CurrentStr {
            get {
                return FormatNumeric(Current, "A");
            }
        }

        private void Init() {
            Type = ComponentType.Ammeter;
            Symbol = "A";
            OutputImage = Properties.Resources.ammeter1XInit;
            Width = OutputImage.Width;
            Height = OutputImage.Height;

            Terminal Left = new Terminal(this, TerminalKey.Left) { NeedAD = AD.EndToStart };
            Terminal Right = new Terminal(this, TerminalKey.Right) { NeedAD = AD.StartToEnd };
            Terminal Middle = new Terminal(this, TerminalKey.Middle) { NeedAD = AD.StartToEnd };
            Terminals.Add(Left);
            Terminals.Add(Middle);
            Terminals.Add(Right);

            Spec X1 = new Spec(Properties.Resources.ammeter1X, 1);
            X1.TerminalAreas.Add(new TerminalArea(TerminalKey.Left, 4, 37, 8, 8, 7, 45));
            X1.TerminalAreas.Add(new TerminalArea(TerminalKey.Middle, 28, 37, 8, 8, 33, 44));
            X1.TerminalAreas.Add(new TerminalArea(TerminalKey.Right, 53, 37, 8, 8, 57, 44));
            Specs.Add(X1);

            Spec X2 = new Spec(Properties.Resources.ammeter2X, 2);
            X2.TerminalAreas.Add(new TerminalArea(TerminalKey.Left, 6, 72, 16, 18, 14, 87));
            X2.TerminalAreas.Add(new TerminalArea(TerminalKey.Middle, 56, 72, 16, 18, 65, 87));
            X2.TerminalAreas.Add(new TerminalArea(TerminalKey.Right, 106, 72, 16, 18, 105, 87));
            Specs.Add(X2);

            Spec X3 = new Spec(Properties.Resources.ammeter3X, 3);
            X3.TerminalAreas.Add(new TerminalArea(TerminalKey.Left, 8, 108, 23, 26, 20, 120));
            X3.TerminalAreas.Add(new TerminalArea(TerminalKey.Middle, 84, 108, 23, 26, 86, 120));
            X3.TerminalAreas.Add(new TerminalArea(TerminalKey.Right, 158, 108, 23, 26, 172, 120));
            Specs.Add(X3);

            Spec X4 = new Spec(Properties.Resources.ammeter4X, 4);
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
        }

        public Ammeter() {
            Init();
            SetCurrentTerminals();
            CircuitImageInfo = new AmmeterCircuitImageInfo(this);
        }

        public override void Draw(Graphics g, Color BC, Color FC, Font f) {
            base.Draw(g, BC, FC, f);
            Pointer p = CurrentPointer;
            TerminalPair t = GetValidTerminal();
            float angle = (float)Math.Atan((p.P.Y - p.O.Y) / (p.P.X - p.O.X));
            Console.WriteLine("TerminalPairs.Count=" + TerminalPairs.Count);
            bool tn = t == null;
            Console.WriteLine("t==null = " + tn.ToString());
            if (tn) {
                angle = 0;
            }
            else {
                float min = -44f;
                float max = 124f;
                bool clockWise = (t.T1.Polar == Polarity.Positive && t.T2.Polar == Polarity.Negative) ? true : false;
                angle = t.Keys == 6 ? t.Current * 4 / 0.1f : t.Current * 4 / 0.02f;
                //if (!clockWise) angle = -angle;
                if (angle > max) angle = max;
                if (angle < min) angle = min;
            }


            if (!float.IsNaN(angle)) {
                //PointF p1 = Fun.GetRotatePoint(p.O, p.P, angle);
                PointF wc = new PointF(X + p.O.X, Y + p.O.Y);
                //PointF wp = new PointF(X + p1.X, Y + p1.Y);
                g.TranslateTransform(X + p.O.X, Y + p.O.Y);
                g.RotateTransform(angle);
                g.DrawLine(new Pen(Color.Red), 0, 0, p.P.X - p.O.X, p.P.Y - p.O.Y);
                g.TranslateTransform(-(X + p.O.X), -(Y + p.O.Y));
                g.ResetTransform();
            }
        }

        public override float GetResistance(Terminal t1, Terminal t2) {
            if (t1 == null && t2 == null) return float.NaN;
            float r = 0f;
            int key = (int)t1.Key | (int)t2.Key;
            if (key == 10) {
                //Left And Middle——0~0.6A
                r = (r0 + r3) * (r1 + r2) / (r0 + r1 + r2 + r3);
            }
            else if (key == 6) {
                r = (r0 + r2 + r3) * r1 / (r0 + r1 + r2 + r3);
            }
            else if (key == 12) {
                r = (r0 + r1 + r3) * r2 / (r0 + r1 + r2 + r3);
            }
            else {
                r = float.NaN;
            }

            if (IsIdeal) {
                return 0;
            }
            return r;

        }
    }
}
