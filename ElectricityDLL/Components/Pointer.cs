using System.Drawing;

namespace ElectricityDLL {
    /// <summary>
    /// 仪表指针
    /// </summary>
    public struct Pointer {
        /// <summary>
        /// 圆心
        /// </summary>
        public PointF O { get; }

        /// <summary>
        /// 指针尖端位置坐标
        /// </summary>
        public PointF P { get; }

        /// <summary>
        /// 缩放比例
        /// </summary>
        public int Scale { get; }

        /// <summary>
        /// 所有者
        /// </summary>
        public Element Owner { get; }

        public Pointer(Element o, PointF c, PointF p, int s = 1) {
            Owner = o;
            O = c;
            P = p;
            Scale = s;
        }
    }
}
