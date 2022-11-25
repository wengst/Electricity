using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace ElectricityDLL
{

    /// <summary>
    /// 电阻器
    /// </summary>
    [Serializable]
    public class Resistor : Element
    {
        //[Browsable(false)]
        public ResistorCircuitImageInfo CircuitImageInfo { get; private set; }

        [JsonPropertyName("r")]
        /// <summary>
        /// 电阻值
        /// </summary>
        [Browsable(false)]
        [Xuliehua("Resistance")]
        public float Resistance { get; set; } = 5;

        [DisplayName("电阻"), Description("电阻器的电阻值。可更改该参数"), Category(Consts.PGC_cat1)]
        public string StrResistance
        {
            get
            {
                return FormatNumeric(Resistance, "Ω");
            }
            set { Resistance = StrToFloat(value, "Ω", Resistance); }
        }

        [Browsable(false)]
        public float Power
        {
            get
            {
                return Current * Current * Resistance;
            }
        }

        [DisplayName("功率"), Description("实际功率，单位瓦特(W)，由P=I^2*R计算获得"), Category(Consts.PGC_cat2)]
        public string PowerStr
        {
            get
            {
                return Power.ToString("f2") + "W";
            }
        }

        [Browsable(false)]
        public float Voltage
        {
            get
            {
                return Math.Abs(Current) * Resistance;
            }
        }

        [DisplayName("电流"), Description("流过电阻器的电流强度，单位安培(A)"), Category(Consts.PGC_cat2)]
        public string CurrentStr
        {
            get { return Math.Abs(Current).ToString("f2") + "A"; }
        }

        [DisplayName("电压"), Description("加载到电阻器两端的电压，单位伏特(V)，由U=IR计算获得"), Category(Consts.PGC_cat2)]
        public string VoltageStr
        {
            get
            {
                return Voltage.ToString("f2") + "V";
            }
        }

        public void ChangeStat()
        {
            foreach (Spec spec in Specs)
            {
                if (spec.Stat == Stat && spec.Scale == Scale)
                {
                    OutputImage = spec.BackImage;
                }
            }
        }

        public Resistor()
        {
            Type = ComponentType.Resistor;
            Symbol = "R";
            OutputImage = Properties.Resources.rsistanceNormal;
            Width = OutputImage.Width;
            Height = OutputImage.Height;

            Spec X1 = new Spec(Properties.Resources.rsistanceNormal, 1, WorkStat.StopOrOpen);
            X1.TerminalAreas.Add(new TerminalArea(TerminalKey.Left, 2, 12, 8, 8, 6, 19));
            X1.TerminalAreas.Add(new TerminalArea(TerminalKey.Right, 69, 12, 8, 8, 74, 19));
            Specs.Add(X1);

            Spec X2 = new Spec(Properties.Resources.rsistanceHot, 1, WorkStat.Working);
            X2.TerminalAreas.Add(new TerminalArea(TerminalKey.Left, 2, 12, 8, 8, 6, 19));
            X2.TerminalAreas.Add(new TerminalArea(TerminalKey.Right, 69, 12, 8, 8, 74, 19));
            Specs.Add(X2);

            Terminal Left = new Terminal() { Owner = this, Key = TerminalKey.Left };
            Terminal Right = new Terminal() { Owner = this, Key = TerminalKey.Right };
            Terminals.Add(Left);
            Terminals.Add(Right);

            TerminalPairs.Add(new TerminalPair(Left, Right));
            SetCurrentTerminals();

            CircuitPath.AddLine(0, 6, 24, 6);
            CircuitPath.AddRectangle(new RectangleF(24, 0, 24, 12));
            CircuitPath.AddLine(72, 6, 96, 6);

            CircuitImageInfo = new ResistorCircuitImageInfo(this);
        }

        public override void InitCircuitPath()
        {
            CircuitPath.AddLine(0, 6, 24, 6);
            CircuitPath.AddRectangle(new RectangleF(24, 0, 24, 12));
            CircuitPath.AddLine(72, 6, 96, 6);
        }

        public override float GetResistance(Terminal t1, Terminal t2)
        {
            if (IsMyTerminal(t1) && IsMyTerminal(t2))
            {
                switch (Fault)
                {
                    case FaultType.无:
                        return Resistance;
                    case FaultType.断路:
                        return float.NaN;
                    case FaultType.短路:
                        return 0f;
                }
            }
            return float.NaN;
        }

        public override string ToJson()
        {
            string s = base.ToJson();
            string ss = ",Resistance:" + Resistance;
            s = s.Replace(Consts.ZWF, ss);
            return s;
        }
    }
}
