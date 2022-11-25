namespace ElectricityDLL {
    public struct TerminalArea {
        public TerminalKey Key { get; set; }
        /// <summary>
        /// 接线柱坐标X
        /// </summary>
        public float X;
        /// <summary>
        /// 接线柱坐标Y
        /// </summary>
        public float Y;
        /// <summary>
        /// 接线柱宽
        /// </summary>
        public float W;
        /// <summary>
        /// 接线柱高
        /// </summary>
        public float H;
        /// <summary>
        /// 接线柱接线点坐标X
        /// </summary>
        public float CX;
        /// <summary>
        /// 接线柱接线点坐标Y
        /// </summary>
        public float CY;

        public TerminalArea(TerminalKey key, float x, float y, float w, float h, float cx, float cy) {
            Key = key; X = x; Y = y; W = w; H = h; CX = cx; CY = cy;
        }
    }
}
