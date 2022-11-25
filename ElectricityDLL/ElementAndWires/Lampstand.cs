using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ElectricityDLL {
    /// <summary>
    /// 小灯泡
    /// </summary>
    public class Lampstand : Element {
        public LampstandCircuitImageInfo CircuitImageInfo { get; private set; }

        /// <summary>
        /// 额定电压
        /// </summary>
        [Browsable(false)]
        [Xuliehua("RatedVoltage")]
        public float RatedVoltage { get; set; } = 2.5f;

        [Category(Consts.PGC_cat1), DisplayName("额定电压"), Description("小灯泡正常工作时的电压，单位伏特(V)。此参数可更改")]
        public string StrRateVoltage {
            get {
                return FormatNumeric(RatedVoltage, "V");
            }
            set {
                RatedVoltage = StrToFloat(value, "V", RatedVoltage);
            }
        }

        /// <summary>
        /// 额定功率
        /// </summary>
        [Browsable(false)]
        [Xuliehua("RatedPower")]
        public float RatedPower { get; set; } = 0.5f;

        [Category(Consts.PGC_cat1), DisplayName("额定功率"), Description("小灯泡在额定电压下工作时的功率，单位瓦特(W)，此参数可更改")]
        public string StrRatePower {
            get {
                return FormatNumeric(RatedPower, "W");
            }
            set {
                RatedPower = StrToFloat(value, "W", RatedPower);
            }
        }

        /// <summary>
        /// 最大电流
        /// </summary>
        [Browsable(false)]
        public float MaxCurrent { get; set; } = 100f;

        /// <summary>
        /// 最小电流
        /// </summary>
        [Browsable(false)]
        public float MinCurrent { get; set; } = 0.02f;

        /// <summary>
        /// 额定电流
        /// </summary>
        [Category(Consts.PGC_cat1), DisplayName("额定电流"), Description("小灯泡在额定电压下工作时，流经它的电流，单位安培(A)，此参数不允许更改")]
        public string RatedCurrent {
            get {
                float a = RatedPower / RatedVoltage;
                return FormatNumeric(a, "A");
            }
        }

        [DisplayName("实际功率"), Description("根据实际流过小灯泡的电流计算得到的功率，单位瓦特(W)，在这个计算中不考虑小灯泡电阻受温度影响"), Category(Consts.PGC_cat2)]
        public string ActualPower {
            get {
                float a = GetResistance(Left, Right) * Current * Current;
                return FormatNumeric(a, "W");
            }
        }

        [DisplayName("实际电压"), Description("根据实际流过小灯泡的电流计算得到的电压，单位伏特(V)，在这个计算中不考虑小灯泡电阻受温度影响"), Category(Consts.PGC_cat2)]
        public string ActualVoltage {
            get {
                float a = (GetResistance(Left, Right) * Math.Abs(Current));
                return FormatNumeric(a, "V");
            }
        }

        [DisplayName("实际电流"), Description("流经小灯泡的实际电流"), Category(Consts.PGC_cat2)]
        public string CurrentStr {
            get {
                float a = Math.Abs(Current);
                return FormatNumeric(a, "A");
            }
        }

        /// <summary>
        /// 伏安特性集合
        /// </summary>
        [Browsable(false)]
        public List<VACharacter> VAs { get; set; } = new List<VACharacter>();
        private void Init() {

            VAs.Add(VACharacter.VP(2.5f, 0.5f));

            Type = ComponentType.Lampstand;
            Symbol = "L";
            OutputImage = Properties.Resources.lampstandNormal;
            Width = OutputImage.Width;
            Height = OutputImage.Height;

            Spec X1 = new Spec(Properties.Resources.lampstandNormal, 1, WorkStat.StopOrOpen);
            X1.TerminalAreas.Add(new TerminalArea(TerminalKey.Left, 10, 22, 8, 8, 13.5f, 29.5f));
            X1.TerminalAreas.Add(new TerminalArea(TerminalKey.Right, 64, 22, 8, 8, 67.5f, 29.5f));
            Specs.Add(X1);

            Spec X2 = new Spec(Properties.Resources.lampstandLight, 1, WorkStat.Working);
            X2.TerminalAreas.Add(new TerminalArea(TerminalKey.Left, 10, 35, 8, 8, 13.5f, 41.5f));
            X2.TerminalAreas.Add(new TerminalArea(TerminalKey.Right, 65, 35, 8, 8, 67.5f, 41.5f));
            Specs.Add(X2);

            Terminal Left = new Terminal() { Owner = this, Key = TerminalKey.Left };
            Terminal Right = new Terminal() { Owner = this, Key = TerminalKey.Right };
            Terminals.Add(Left);
            Terminals.Add(Right);

            TerminalPairs.Add(new TerminalPair(Left, Right));
        }

        public Lampstand() {
            Init();
            SetCurrentTerminals();
            CircuitImageInfo = new LampstandCircuitImageInfo(this);
        }

        public override float GetResistance(Terminal t1, Terminal t2) {
            if (IsIdeal) {
                return RatedVoltage * RatedVoltage / RatedPower;
            }
            else {
                return RatedVoltage * RatedVoltage / RatedPower;
            }
        }

        public override string ToJson() {
            string s = base.ToJson();
            string ss = ",RatedVoltage:" + RatedVoltage + ",RatedPower:" + RatedPower;
            s = s.Replace(Consts.ZWF, ss);
            return s;
        }
    }
}
