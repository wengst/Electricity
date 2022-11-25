using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace ElectricityDLL {
    /// <summary>
    /// 开关的刀类型
    /// </summary>
    public class Knify : EleComponent {
        /// <summary>
        /// 非正放的四边形的四个顶点
        /// </summary>
        [Browsable(false)]
        public List<PointF> FourPoints { get; } = new List<PointF>();

        [Category(Consts.PGC_cat3), DisplayName("名称")]
        public override string CnName => "开关的闸刀";

        [Browsable(false)]
        public override PointF WorldPoint {
            get {
                if (Owner != null) {
                    return new PointF(Owner.X + X, Owner.Y + Y);
                }
                return PointF.Empty;
            }
        }

        public Knify(Element o, WorkStat s, PointF[] ps) {
            Stat = s;
            Owner = o;
            this.Type = ComponentType.Knify;
            FourPoints = new List<PointF>();
            for (int i = 0; i < ps.Length; i++) {
                FourPoints.Add(ps[i]);
            }
        }

        public override bool Contains(PointF point) {
            List<PointF> points = new List<PointF>();
            foreach (PointF pointF in FourPoints) {
                PointF p = new PointF(Owner.X + pointF.X, Owner.Y + pointF.Y);
                points.Add(p);
            }
            return Fun.IsInPolygon(point, points);
        }
    }
}
