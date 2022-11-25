using System.ComponentModel;
using System.Drawing;

namespace ElectricityDLL {
    public class Switch : Element {

        public SwitchCircuitImageInfo CircuitImageInfo { get; private set; }
        /// <summary>
        /// 本开关的开关刀
        /// </summary>
        [Browsable(false)]
        public Knify K { get; private set; }

        [Category(Consts.PGC_cat4), DisplayName("开关状态")]
        public string KnifyStat {
            get {
                return Stat == WorkStat.StopOrOpen ? "断开" : "闭合";
            }
        }

        /// <summary>
        /// 改变状态
        /// </summary>
        public void ChangeStat() {
            if (Stat == WorkStat.StopOrOpen) {
                Stat = WorkStat.Working;
                OutputImage = Properties.Resources.switchClosed;
                X = X + (Width - OutputImage.Width);
                Y = Y + (Height - OutputImage.Height);
                Width = OutputImage.Width;
                Height = OutputImage.Height;
                K = new Knify(this, Stat, new PointF[] { new PointF(18, 3), new PointF(65, 3), new PointF(65, 7), new PointF(18, 7) });
            }
            else {
                Stat = WorkStat.StopOrOpen;
                OutputImage = Properties.Resources.switchOpen;
                X = X + (Width - OutputImage.Width);
                Y = Y + (Height - OutputImage.Height);
                Width = OutputImage.Width;
                Height = OutputImage.Height;
                K = new Knify(this, Stat, new PointF[] { new PointF(18, 18), new PointF(59, 1), new PointF(62, 6), new PointF(19, 22) });
            }
            SetCurrentTerminals();
            IsDirty = true;
        }

        public Switch() {
            Type = ComponentType.Switch;
            Symbol = "S";
            OutputImage = Properties.Resources.switchOpen;
            Width = OutputImage.Width;
            Height = OutputImage.Height;

            Spec P1 = new Spec(Properties.Resources.switchOpen, 1, WorkStat.StopOrOpen);
            P1.TerminalAreas.Add(new TerminalArea(TerminalKey.Left, 3, 18, 8, 8, 7, 23.5f));
            P1.TerminalAreas.Add(new TerminalArea(TerminalKey.Right, 67, 18, 8, 8, 71, 23.5f));
            Specs.Add(P1);

            Spec P2 = new Spec(Properties.Resources.switchClosed, 1, WorkStat.Working);
            P2.TerminalAreas.Add(new TerminalArea(TerminalKey.Left, 3, 4, 8, 8, 7, 10));
            P2.TerminalAreas.Add(new TerminalArea(TerminalKey.Right, 67, 4, 8, 8, 70.5f, 10));
            Specs.Add(P2);

            Terminal Left = new Terminal(this, TerminalKey.Left);
            Terminal Right = new Terminal(this, TerminalKey.Right);
            Terminals.AddRange(new[] { Left, Right });

            TerminalPairs.Add(new TerminalPair(Left, Right));

            SetCurrentTerminals();

            K = new Knify(this, Stat, new PointF[] { new PointF(18, 18), new PointF(59, 1), new PointF(62, 6), new PointF(19, 22) });

            CircuitImageInfo = new SwitchCircuitImageInfo(this);
        }

        public override void InitCircuitPath()
        {
            
            
        }

        public override float GetResistance(Terminal t1, Terminal t2) {
            if (IsMyTerminal(t1) && IsMyTerminal(t2)) {
                switch (Fault) {
                    case FaultType.无:
                        return Stat == WorkStat.StopOrOpen ? float.NaN : 0f;
                    case FaultType.断路:
                        return float.NaN;
                    case FaultType.短路:
                        return 0f;
                }
            }
            return float.NaN;
        }
    }
}
