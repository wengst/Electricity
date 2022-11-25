namespace ElectricityDLL {
    /// <summary>
    /// 选项
    /// </summary>
    public class Option {
        /// <summary>
        /// 自动分析电路
        /// </summary>
        public bool IsAutoAnalysis { get; set; } = false;
        /// <summary>
        /// 理想电路
        /// </summary>
        public bool IsIdeal { get; set; } = true;
    }
}
