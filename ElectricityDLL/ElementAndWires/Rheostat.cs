using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.Json.Serialization;

namespace ElectricityDLL
{
    /// <summary>
    /// 变阻器
    /// </summary>
    [Serializable]
    public class Rheostat : Element
    {
        //[Browsable(false)]
        public RheostatCircuitImageInfo CircuitImageInfo { get; set; }

        [JsonPropertyName("max")]
        [Browsable(false)]
        [Xuliehua("MaxResistance")]
        public float MaxResistance { get; set; } = 20f;

        [DisplayName("最大阻值"), Category(Consts.PGC_cat1), Description("滑动变阻器的最大阻值，此参数可更改")]
        public string StrMaxResistance
        {
            get { return FormatNumeric(MaxResistance, "Ω"); }
            set { MaxResistance = StrToFloat(value, "Ω", MaxResistance); }
        }

        [Category(Consts.PGC_cat4), DisplayName("左侧电阻"), Description("从上接线柱到左下接线柱的电阻")]
        public string Hp2LD
        {
            get
            {
                float dz = GetResistance(MiddleUp, LeftDown);
                if (dz != float.NaN)
                {
                    float a = GetResistance(LeftUp, LeftDown);
                    return FormatNumeric(a, "Ω");
                }
                else
                {
                    return "陷入沉思";
                }
            }
        }

        [Category(Consts.PGC_cat4), DisplayName("右侧电阻"), Description("从上接线柱到右下接线柱的电阻")]
        public string Hp2RD
        {
            get
            {
                float dz = GetResistance(MiddleUp, LeftDown);
                if (dz != float.NaN)
                {
                    float a = GetResistance(LeftUp, RightDown);
                    return FormatNumeric(a, "Ω");
                }
                else
                {
                    return "陷入沉思";
                }
            }
        }

        [Category(Consts.PGC_cat2), DisplayName("左侧电压"), Description("上接线柱与左侧接线柱间的电压")]
        public string LeftAuctalVoltage
        {
            get
            {
                float u1 = float.NaN, u2 = float.NaN;
                if (LeftUp.JoinedNode != null || RightUp.JoinedNode != null)
                {
                    u1 = LeftUp.JoinedNode == null ? RightUp.JoinedNode.Potential : LeftUp.JoinedNode.Potential;
                }
                else
                {
                    u1 = float.NaN;
                }

                if (LeftDown.JoinedNode != null)
                {
                    u2 = LeftDown.JoinedNode.Potential;
                }
                else
                {
                    u2 = float.NaN;
                }
                float u = Math.Abs(u1 - u2);
                if (float.IsNaN(u))
                {
                    return "陷入沉思";
                }
                else
                {
                    return FormatNumeric(u, "V");
                }
            }
        }

        [Category(Consts.PGC_cat2), DisplayName("右侧电压"), Description("上接线柱与左侧接线柱间的电压")]
        public string RightAuctalVoltage
        {
            get
            {
                float u1 = float.NaN, u2 = float.NaN;
                if (LeftUp.JoinedNode != null || RightUp.JoinedNode != null)
                {
                    u1 = LeftUp.JoinedNode == null ? RightUp.JoinedNode.Potential : LeftUp.JoinedNode.Potential;
                }
                if (RightDown.JoinedNode != null)
                {
                    u2 = RightDown.JoinedNode.Potential;
                }
                if (!float.IsNaN(u1) && !float.IsNaN(u2))
                {
                    float u = Math.Abs(u1 - u2);
                    if (float.IsNaN(u))
                    {
                        return "陷入沉思";
                    }
                    else
                    {
                        return FormatNumeric(u, "V");
                    }
                }
                else
                {
                    return "陷入沉思";
                }
            }
        }

        [Category(Consts.PGC_cat2), DisplayName("左侧电流"), Description("从上接线柱到左下接线柱的电流")]
        public string LeftCurrent
        {
            get
            {
                TerminalPair tp = GetValidTerminal();
                if (tp != null && (tp.Keys & 32) == 32 && (tp.Keys & 128) == 0)
                {
                    if (!Consts.IsZero(tp.Current) && !float.IsNaN(tp.Current) && !float.IsInfinity(tp.Current))
                    {
                        return FormatNumeric(tp.Current, "A");
                    }
                }
                return Consts.PGC_unknow;
            }
        }

        [Category(Consts.PGC_cat2), DisplayName("右侧电流"), Description("从上接线柱到右下接线柱的电流")]
        public string RightCurrent
        {
            get
            {
                TerminalPair tp = GetValidTerminal();
                if (tp != null && (tp.Keys & 32) == 0 && (tp.Keys & 128) == 128)
                {
                    if (!Consts.IsZero(tp.Current) && !float.IsNaN(tp.Current) && !float.IsInfinity(tp.Current))
                    {
                        return FormatNumeric(tp.Current, "A");
                    }
                }
                return Consts.PGC_unknow;
            }
        }

        [Browsable(false)]
        [Xuliehua("Vane")]
        public Vane TheVane { get; private set; }
        [Browsable(false)]
        private float LeftX = 14;
        [Browsable(false)]
        private float RightX = 95;
        [Browsable(false)]
        private float Top = 0;
        [Browsable(false)]
        private float StartX = 51;

        private void InitRheostat()
        {
            Spec P = new Spec(Properties.Resources.rheostatBase);
            P.TerminalAreas.Add(new TerminalArea(TerminalKey.LeftUp, 0, 2, 8, 8, 8, 5));
            P.TerminalAreas.Add(new TerminalArea(TerminalKey.LeftDown, 0, 24, 8, 8, 8, 29));
            P.TerminalAreas.Add(new TerminalArea(TerminalKey.RightUp, 114, 2, 8, 8, 114, 5));
            P.TerminalAreas.Add(new TerminalArea(TerminalKey.RightDown, 114, 24, 8, 8, 114, 29));
            Specs.Add(P);

            Terminal LeftUp = new Terminal(this, TerminalKey.LeftUp);
            Terminal LeftDown = new Terminal(this, TerminalKey.LeftDown);
            Terminal RightUp = new Terminal(this, TerminalKey.RightUp);
            Terminal RightDown = new Terminal(this, TerminalKey.RightDown);
            Terminal MiddleUp = new Terminal(this, TerminalKey.MiddleUp) { IsVirtual = true };
            Terminals.AddRange(new[] { LeftUp, LeftDown, RightUp, RightDown, MiddleUp });

            TerminalPair LeftUpRightUp = new TerminalPair(LeftUp, RightUp);
            TerminalPair LeftUpRightDown = new TerminalPair(LeftUp, RightDown);
            TerminalPair LeftDownRightUp = new TerminalPair(LeftDown, RightUp);
            TerminalPair LeftDownRightDown = new TerminalPair(LeftDown, RightDown);
            TerminalPairs.AddRange(new[] { LeftUpRightUp, LeftUpRightDown, LeftDownRightUp, LeftDownRightDown });
            SetCurrentTerminals();
        }

        private void InitVane()
        {
            TheVane = new Vane(this);
            TheVane.X = StartX;
            TheVane.Y = Top;
            TheVane.Width = TheVane.OutputImage.Width - 6;
            TheVane.Height = TheVane.OutputImage.Height - 6;
            TheVane.Vertexs.Add(new PointF(StartX, Top));
            TheVane.Vertexs.Add(new PointF(StartX + 20, Top));
            TheVane.Vertexs.Add(new PointF(StartX, 12));
            TheVane.Vertexs.Add(new PointF(StartX + 20, 12));
        }

        public void MoveVane(Offset offset)
        {
            float x = TheVane.X + offset.X;
            if (x < LeftX) x = 14;
            if (x > RightX) x = RightX;
            float py = x - TheVane.X;
            TheVane.X = x;
            for (int i = 0; i < TheVane.Vertexs.Count; i++)
            {
                PointF p = TheVane.Vertexs[i];
                TheVane.Vertexs[i] = new PointF(p.X + py, p.Y);
            }
            IsDirty = true;
        }

        public override float GetResistance(Terminal t1, Terminal t2)
        {
            int k = (int)t1.Key + (int)t2.Key;
            switch (k)
            {
                case 80:
                    return 0;
                case 160:
                    return MaxResistance;
                case 48:
                case 96:
                case 288:
                    //上接左下
                    return MaxResistance * ((TheVane.X - LeftX) / (RightX - LeftX));
                case 144:
                case 192:
                case 384:
                    //上接右下
                    return MaxResistance * ((RightX - TheVane.X) / (RightX - LeftX));
            }
            return float.NaN;
        }

        public override void InitCircuitPath()
        {
            PointF[] pfs = new PointF[5];
            pfs[0] = new PointF(0, 18.5f);
            pfs[1] = new PointF(20, 18.5f);
            pfs[2] = new PointF(20, 0);
            pfs[3] = new PointF(55.5f, 0);
            pfs[4] = new PointF(55.5f, 12.25f);
            CircuitPath.AddLines(pfs);
            CircuitPath.AddRectangle(new RectangleF(30f,12.25f,40f,12f));
            CircuitPath.AddLine(78.05f, 18.25f, 98.05f, 18.25f);
        }

        public Rheostat()
        {
            Type = ComponentType.Rheostat;
            Symbol = "R";
            TheVane = new Vane(this);

            OutputImage = Properties.Resources.rheostatBase;
            Width = OutputImage.Width;
            Height = OutputImage.Height;

            InitRheostat();
            InitVane();
            CircuitImageInfo = new RheostatCircuitImageInfo(this);
        }

        public override void Draw(Graphics g, Color BC, Color FC, Font f)
        {
            base.Draw(g, BC, FC, f);
            g.DrawImage(TheVane.OutputImage, TheVane.WorldPoint.X, TheVane.WorldPoint.Y, TheVane.Width, TheVane.Height);
        }

        public override string ToJson()
        {
            string s = base.ToJson();
            string ss = ",MaxResistance:" + MaxResistance + ",VX:" + TheVane.X;
            s = s.Replace(Consts.ZWF, ss);
            return s;
        }
    }
}
