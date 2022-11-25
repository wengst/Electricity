using System.Collections.Generic;
using System.Drawing;

namespace ElectricityDLL {
    /// <summary>
    /// 元器件规格
    /// </summary>
    public class Spec {
        /// <summary>
        /// 背景图
        /// </summary>
        public Image BackImage { get; set; }
        /// <summary>
        /// 放大倍数
        /// </summary>
        public int Scale { get; set; }
        /// <summary>
        /// 工作状态
        /// </summary>
        public WorkStat Stat { get; set; }
        /// <summary>
        /// 接线柱区域集合
        /// </summary>
        public List<TerminalArea> TerminalAreas { get; } = new List<TerminalArea>();

        public Spec(Image img = null, int scale = 1, WorkStat stat = WorkStat.StopOrOpen | WorkStat.Working) {
            BackImage = img;
            Scale = scale;
            Stat = stat;
        }
    }
}
