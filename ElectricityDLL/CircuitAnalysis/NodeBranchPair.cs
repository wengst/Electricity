namespace ElectricityDLL.CircuitAnalysis {
    /// <summary>
    /// 节点支路对
    /// <para>记录每条支路及其电流方向</para>
    /// </summary>
    public class NodeBranchPair {
        /// <summary>
        /// 电流方向
        /// </summary>
        public AD D { get; set; }
        /// <summary>
        /// 支路
        /// </summary>
        public Branch B { get; set; }
    }
}
