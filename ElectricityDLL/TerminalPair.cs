namespace ElectricityDLL {
    /// <summary>
    /// 接线柱对
    /// <para>描述同一元件上两个接线柱之间阻值的数据类型</para>
    /// </summary>
    public class TerminalPair {
        /// <summary>
        /// 是否是有效连接的接线柱对
        /// </summary>
        public bool IsValid {
            get {
                if (!Consts.IsZero(Current) && !float.IsNaN(Current)) {
                    return true;
                }
                if (!float.IsNaN(Voltage)) {
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// 接线柱1
        /// </summary>
        public Terminal T1 { get; }
        /// <summary>
        /// 接线柱2
        /// </summary>
        public Terminal T2 { get; }

        public int Keys {
            get {
                int k = 0;
                if (T1 != null) k |= (int)T1.Key;
                if (T2 != null) k |= (int)T2.Key;
                return k;
            }
        }
        /// <summary>
        /// 阻值
        /// </summary>
        public float Resistance { get; set; }

        /// <summary>
        /// 接线柱两端的电压
        /// </summary>
        public float Voltage {
            get {
                if (T1 != null && T2 != null && T1.JoinedNode != null && T2.JoinedNode != null) {
                    return T1.JoinedNode.Potential - T2.JoinedNode.Potential;
                }
                return float.NaN;
            }

        }
        /// <summary>
        /// 流过接线柱对的电流大小
        /// </summary>
        public float Current { get; set; } = float.NaN;

        public TerminalPair(Terminal t1, Terminal t2, float r = float.NaN) {
            T1 = t1; T2 = t2; Resistance = r;
        }
    }
}
