using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace ElectricityDLL {
    /// <summary>
    /// 滑动变阻器上的滑片
    /// </summary>
    public class Vane : EleComponent {

        [Category(Consts.PGC_cat3), DisplayName("名称")]
        public string VaneName {
            get {
                return "滑动变阻器" + Owner.SymbolName + "的滑片P";
            }
        }

        /// <summary>
        /// 顶点坐标
        /// </summary>
        [Browsable(false)]
        public List<PointF> Vertexs { get; private set; } = new List<PointF>();

        public Vane(Rheostat rheostat) {
            Type = ComponentType.Vane;
            Owner = rheostat;
            OutputImage = Properties.Resources.rheostatVane;
        }

        public override PointF WorldPoint {
            get {
                return new PointF(Owner.X + X, Owner.Y + Y);
            }
        }

        public override bool Contains(PointF point) {
            List<PointF> points = new List<PointF>();
            foreach (PointF pointF in Vertexs) {
                PointF p = new PointF(Owner.X + pointF.X, Owner.Y + pointF.Y);
                points.Add(p);
            }
            return Fun.IsInPolygon(point, points);
        }
    }
}
