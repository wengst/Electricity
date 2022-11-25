using System.ComponentModel;

namespace ElectricityDLL {
    /// <summary>
    /// 电源
    /// </summary>
    public class BatteryCase : Element {
        //[Browsable(false)]
        public BatteryCircuitImageInfo CircuitImageInfo { get; private set; }

        /// <summary>
        /// 电源内阻
        /// </summary>
        [Browsable(false)]
        [Xuliehua("Resistance")]
        private float Resistance { get; set; } = 3f;

        /// <summary>
        /// 电压。初始是1.5V
        /// </summary>
        [Browsable(false)]
        [Xuliehua("Voltage")]
        public float Voltage { get; set; } = 1.5f;

        [DisplayName(Consts.PG_PowerVoltage), Description(Consts.PG_PowerVoltage_Description), Category(Consts.PGC_cat1)]
        public string StrVoltage {
            get {
                return FormatNumeric(Voltage, "V");
            }
            set { Voltage = StrToFloat(value, "V", Voltage); }
        }

        [Browsable(false)]
        public CircuitGroup SCG { get; set; }

        private void Init() {
            Type = ComponentType.BatteryCase;
            Symbol = "B";
            OutputImage = Properties.Resources.batteryCase1X;
            Width = OutputImage.Width;
            Height = OutputImage.Height;

            Spec X1 = new Spec(Properties.Resources.batteryCase1X, 1);
            X1.TerminalAreas.Add(new TerminalArea(TerminalKey.Left, 2, 15, 8, 8, 5, 22));
            X1.TerminalAreas.Add(new TerminalArea(TerminalKey.Right, 70, 15, 8, 8, 73.5f, 22));
            Specs.Add(X1);

            Terminal Left = new Terminal() { Owner = this, Key = TerminalKey.Left, SpeculateAD = AD.EndToStart };
            Left.Polar = Polarity.Negative;
            Terminals.Add(Left);
            Terminal Right = new Terminal() { Owner = this, Key = TerminalKey.Right, SpeculateAD = AD.StartToEnd };
            Right.Polar = Polarity.Positive;
            Terminals.Add(Right);

            TerminalPairs.Add(new TerminalPair(Left, Right));
        }

        public BatteryCase() {
            Init();
            SetCurrentTerminals();
            InitCircuitPath();
            CircuitImageInfo = new BatteryCircuitImageInfo(this);
        }

        public override float GetResistance(Terminal t1, Terminal t2) {
            if (IsMyTerminal(t1) && IsMyTerminal(t2)) {
                if (IsIdeal) {
                    return 0f;
                }
                else {
                    return Resistance;
                }
            }
            return float.NaN;
        }

        public override void InitCircuitPath()
        {
            CircuitPath.AddLine(0, 15, 28, 15);
            CircuitPath.AddLine(28, 0, 28, 30);
            CircuitPath.AddLine(36, 5.85f, 36, 23.85f);
            CircuitPath.AddLine(36, 15, 64, 15);
        }

        public override string ToJson() {
            string s = base.ToJson();
            s = s.Replace(Consts.ZWF, ",Voltage:" + Voltage);
            return s;
        }
    }
}
