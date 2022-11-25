using System.ComponentModel;
using System.Drawing;

namespace ElectricityDLL {
    /// <summary>
    /// 风扇叶
    /// </summary>
    public class FanLeaf : EleComponent {
        [Category(Consts.PGC_cat3), DisplayName("名称")]
        public string LeafName {
            get {
                return "电动机的扇叶";
            }
        }
        /// <summary>
        /// 扇叶的轴心
        /// </summary>
        [Browsable(false)]
        public PointF COP { get; set; }
        private void Init() {
            OutputImage = Properties.Resources.fanLeaf;
            Width = OutputImage.Width;
            Height = OutputImage.Height;
            COP = new PointF(14, 14);
        }

        public FanLeaf() {
            Init();
        }
    }
}
