using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using Newtonsoft.Json;

namespace ElectricityDLL {
    /// <summary>
    /// 导线类
    /// </summary>
    public class Wire : EleComponent {
        [Category(Consts.PGC_cat3), DisplayName("名称")]
        public string WireName {
            get {
                return "导线" + SymbolName;
            }
        }

        [Category(Consts.PGC_cat4), DisplayName("起点连接")]
        public string ConnectionToStr1 {
            get {
                if (Start.T != null) {
                    return Fun.GetChineseName(Start.T.Owner.Type) + Start.T.Owner.SymbolName;
                }
                else {
                    return Consts.PGC_unknow;
                }
            }
        }

        [Category(Consts.PGC_cat4), DisplayName("终点连接")]
        public string ConnectionToStr2 {
            get {
                if (End.T != null) {
                    return Fun.GetChineseName(End.T.Owner.Type) + End.T.Owner.SymbolName;
                }
                else {
                    return Consts.PGC_unknow;
                }
            }
        }

        [Category(Consts.PGC_cat2), DisplayName("电流"), Description("流经导线的电流，单位安培(A)")]
        public string CurrentStr {
            get {
                return FormatNumeric(Math.Abs(Current), "A");
            }
        }

        [Browsable(false)]
        public RectangleF RectF {
            get {
                float x1 = float.MaxValue, x2 = 0, y1 = float.MaxValue, y2 = 0;
                foreach (PointF point in PointFs) {
                    x1 = Math.Min(x1, point.X);
                    x2 = Math.Max(x2, point.X);
                    y1 = Math.Min(y1, point.Y);
                    y2 = Math.Max(y2, point.Y);
                }
                return new RectangleF(x1, y1, x2 - x1, y2 - y1);
            }
        }

        /// <summary>
        /// 电流强度
        /// </summary>
        [Browsable(false)]
        public float CurrentStrength { get; set; } = 0f;

        [Browsable(false)]
        [Xuliehua(Consts.Json_Junctions)]
        public List<Junction> Junctions { get; private set; } = new List<Junction>();

        [Browsable(false)]
        [Xuliehua(Consts.Json_Points)]
        internal List<PointF> PointFs { get; } = new List<PointF>();

        [Browsable(false)]
        private float Times { get; } = 20f;


        /// <summary>
        /// 选中导线时的各拖拽点的半径
        /// </summary>
        [Browsable(false)]
        int Radius { get; } = 5;

        /// <summary>
        /// 拖拽圆点的填充色
        /// </summary>
        [Browsable(false)]
        Color CircleFillColor { get; } = Color.White;

        /// <summary>
        /// 拖拽原点的边框色
        /// </summary>
        [Browsable(false)]
        Color CircleBorderColor { get; set; } = Color.DarkGray;

        /// <summary>
        /// 控制线颜色
        /// </summary>
        [Browsable(false)]
        Color ControlLineColor { get; set; } = Color.LightCyan;

        /// <summary>
        /// 无电颜色
        /// </summary>
        [Browsable(false)]
        Color NormalLineColor { get; set; } = Color.DarkRed;

        /// <summary>
        /// 通电颜色
        /// </summary>
        [Browsable(false)]
        Color WorkLineColor { get; set; } = Color.Blue;

        /// <summary>
        /// 正常线宽
        /// </summary>
        [Browsable(false)]
        float LineWidth { get; } = 2f;

        /// <summary>
        /// 选中时的线宽
        /// </summary>
        [Browsable(false)]
        float SelectedLineWidth { get; } = 4;

        /// <summary>
        /// 是否是不合格的电路元素
        /// </summary>
        [Browsable(false)]
        [JsonIgnore]
        public bool IsBadPath {
            get {
                int n = 0;
                if (Junctions[0].T != null) {
                    n++;
                }
                if (Junctions[3] != null) {
                    n++;
                }
                return n != 2;
            }
        }

        [Browsable(false)]
        public override string CnName => base.CnName;

        public override bool Contains(PointF point) {
            bool b = false;
            if (PointFs.Count > 0) {
                for (int i = 0; i < PointFs.Count - 1; i++) {
                    b = Fun.IsInLine(point, PointFs[i], PointFs[i + 1], SelectedLineWidth);
                    if (b) break;
                }
            }
            else {
                b = Fun.IsInLine(point, Junctions[0].WorldPoint, Junctions[3].WorldPoint, SelectedLineWidth);
            }
            return b;
        }

        private RectangleF GetWorldRectangleF(PointF p) {
            return new RectangleF(X + p.X - Radius, Y + p.Y - Radius, 2 * Radius + 1, 2 * Radius + 1);
        }

        /// <summary>
        /// 获取坐标点在导线上的位置
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public WireArea GetArea(PointF p) {
            if (IsSelected) {
                foreach (Junction junction in Junctions) {
                    if (junction.Contains(p)) {
                        return junction.Area;
                    }
                }
            }
            else {
                if (Fun.PointInBezier(p, Junctions[0].WorldPoint, Junctions[1].WorldPoint, Junctions[2].WorldPoint, Junctions[3].WorldPoint)) {
                    return WireArea.Body;
                }
            }
            return WireArea.No;
        }

        private void InitPoint() { }

        private void setJ1J2() {
            float lx = Junctions[3].X - Junctions[0].X;
            float ly = Junctions[3].Y - Junctions[0].Y;
            if (!Junctions[1].IsMoved) {
                Junctions[1].X = Junctions[0].X + lx / 3;
                Junctions[1].Y = Junctions[0].Y + ly / 3;
            }
            if (!Junctions[2].IsMoved) {
                Junctions[2].X = Junctions[0].X + 2 * lx / 3;
                Junctions[2].Y = Junctions[0].Y + 2 * ly / 3;
            }
        }

        [Browsable(false)]
        public Junction Start {
            get {
                if (Junctions.Count == 4) {
                    return Junctions[0];
                }
                return null;
            }
        }

        [Browsable(false)]
        public Junction End {
            get {
                if (Junctions.Count == 4) {
                    return Junctions[3];
                }
                return null;
            }
        }

        [Browsable(false)]
        public Junction Handler1 {
            get {
                if (Junctions.Count == 4) {
                    Junction j = Junctions[1];
                    if (!j.IsMoved) {

                    }
                }
                return null;
            }
        }

        [Browsable(false)]
        public Junction Handler2 {
            get {
                if (Junctions.Count == 4) {
                    return Junctions[2];
                }
                return null;
            }
        }

        public void SetJunction(Terminal t, WireArea a) {
            if (t != null && (a == WireArea.EndPoint || a == WireArea.StartPoint)) {
                foreach (Junction j in Junctions) {
                    if (j.Area == a) {
                        if (j.T != null && j.T.Id != t.Id) {
                            j.T.RemoveJunction(j);
                        }
                        j.T = t;
                        t.AddJunction(j);
                        j.X = t.WorldLinkPoint.X;
                        j.Y = t.WorldLinkPoint.Y;
                        IsDirty = true;
                    }
                }
                setJ1J2();
            }
            else {
                throw new Exception("接线柱参数为NULL或将接线柱分配给控制点");
            }
        }

        public void SetJunction(Terminal t) {
            foreach (Junction junction in Junctions) {
                if (junction.T != null && junction.T.Id == t.Id) {
                    junction.X = t.WorldLinkPoint.X;
                    junction.Y = t.WorldLinkPoint.Y;
                    setJ1J2();
                    IsDirty = true;
                }
            }
        }

        public Wire SetJunction(PointF p, WireArea a) {
            foreach (Junction junction in Junctions) {
                if (junction.Area == a) {
                    junction.X = p.X;
                    junction.Y = p.Y;
                    if (a == WireArea.EndHandle || a == WireArea.StartHandle) {
                        junction.IsMoved = true;
                    }
                    IsDirty = true;
                }
            }
            setJ1J2();
            return this;
        }

        public Wire SetJunction(Junction j, PointF p) {
            foreach (Junction junction in Junctions) {
                if (junction.Id == j.Id) {
                    j.X = p.X;
                    j.Y = p.Y;
                    if (!junction.IsMoved) { junction.IsMoved = true; }
                    IsDirty = true;
                }
            }
            setJ1J2();
            return this;
        }

        public Wire SetJunction(Junction j, Terminal t) {
            foreach (Junction junction in Junctions) {
                if (junction.Id == j.Id) {
                    if (junction.T != null && junction.T.Id != t.Id) {
                        junction.T.RemoveJunction(junction);
                        junction.T = t;
                        t.AddJunction(junction);
                        junction.X = t.WorldLinkPoint.X;
                        junction.Y = t.WorldLinkPoint.Y;
                        IsDirty = true;
                    }
                    else if (junction.T == null) {
                        junction.T = t;
                        t.AddJunction(junction);
                        junction.X = t.WorldLinkPoint.X;
                        junction.Y = t.WorldLinkPoint.Y;
                        IsDirty = true;
                    }
                }
            }
            setJ1J2();
            return this;
        }

        /// <summary>
        /// 如果接线柱的所有者是滑动变阻器，就要稍作改变
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private Terminal ConvertTerminal(Terminal t) {
            if (t != null && t.Owner.Type == ComponentType.Rheostat && (t.Key == TerminalKey.LeftUp || t.Key == TerminalKey.RightUp)) {
                Element r = (Element)t.Owner;
                t = r.MiddleUp;
            }
            return t;
        }

        [Browsable(false)]
        public PathElement PathElement {
            get {
                if (Junctions[0] == null || Junctions[0].T == null) { return null; }
                if (Junctions[3] == null || Junctions[3].T == null) { return null; }
                Terminal t1 = ConvertTerminal(Junctions[0].T);
                Terminal t2 = ConvertTerminal(Junctions[3].T);

                PathElement p = new PathElement(t1, this, t2);
                if (t1.Owner.Type == ComponentType.BatteryCase && t1.Key == TerminalKey.Left) {
                    p.Left = t2;
                    p.Right = t1;
                }
                if (t2.Owner.Type == ComponentType.BatteryCase && t2.Key == TerminalKey.Right) {
                    p.Left = t2;
                    p.Right = t1;
                }
                return p;
            }
        }

        /// <summary>
        /// 移除导线指定区域的接线柱
        /// </summary>
        /// <param name="p"></param>
        /// <param name="a"></param>
        public Wire RemoveJunction(PointF p, WireArea a) {
            foreach (Junction junction in Junctions) {
                if (junction.Area == a) {
                    if (junction.T != null) {
                        junction.T.RemoveJunction(junction);
                    }
                    junction.T = null;
                    junction.X = p.X;
                    junction.Y = p.Y;
                }
            }
            setJ1J2();
            return this;
        }

        public Wire RemoveJunction(Junction j, PointF p) {
            foreach (Junction junction in Junctions) {
                if (junction.Id == j.Id) {
                    if (j.T != null) {
                        j.T.RemoveJunction(j);
                    }
                    j.T = null;
                    j.X = p.X;
                    j.Y = p.Y;
                }
            }
            return this;
        }

        /// <summary>
        /// 将本导线的所有接头从元件的接线柱上取下
        /// </summary>
        public void ClearJunctions() {
            foreach (Junction junction in Junctions) {
                if (junction.T != null) {
                    junction.T.RemoveJunction(junction);
                }
            }
        }

        /// <summary>
        /// 重置导线的电流方向
        /// </summary>
        public void ResetAssumeDirection() {
            Junctions[0].AssumeDirection = AD.UnKnow;
            Junctions[3].AssumeDirection = AD.UnKnow;
        }

        /// <summary>
        /// 整体移动导线
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public override void Move(Offset offset) {
            foreach (Junction j in Junctions) {
                j.X += offset.X;
                j.Y += offset.Y;
                if (j.T != null) {
                    j.T.RemoveJunction(j);
                    j.T = null;
                }
            }
        }

        public Wire(Terminal T) {
            Type = ComponentType.Wire;
            Symbol = "W";
            Junctions.Add(new Junction(this, WireArea.StartPoint, T) { Radius = Radius, IsMoved = true });
            Junctions.Add(new Junction(this, WireArea.StartHandle) { Radius = Radius });
            Junctions.Add(new Junction(this, WireArea.EndHandle) { Radius = Radius });
            Junctions.Add(new Junction(this, WireArea.EndPoint) { Radius = Radius, IsMoved = true });
        }
        public Wire() {
            Type = ComponentType.Wire;
            Symbol = "W";
        }
        private RectangleF GetCircleRect(PointF p) {
            return new RectangleF(p.X - Radius, p.Y - Radius, 2 * Radius + 1, 2 * Radius + 1);
        }
        private RectangleF GetCircleEndPoint(PointF p) {
            float bj = LineWidth * 2;
            return new RectangleF(p.X - bj, p.Y - bj, bj + 1, bj + 1);
        }

        private void BuildPoints() {
            float t = 1f / Times;
            PointFs.Clear();
            for (int i = 0; i <= Times; i++) {
                PointF p = Fun.GetBezierPointF(i * t, Junctions[0].WorldPoint, Junctions[1].WorldPoint, Junctions[2].WorldPoint, Junctions[3].WorldPoint);
                PointFs.Add(p);
            }
        }

        private Font fn = new Font("宋体", 9f);

        private void DrawBezier(Graphics g, Pen p) {
            BuildPoints();
            GraphicsPath path = new GraphicsPath();
            for (int i = 0; i < PointFs.Count - 1; i++) {
                path.AddLine(PointFs[i], PointFs[i + 1]);
                //g.DrawLine(p, PointFs[i], PointFs[i + 1]);
                //if (i == (int)(Times / 2)) {
                //    g.MeasureString(SymbolName, fn);
                //    PointF p1 = new PointF(PointFs[i].X + 4, PointFs[i].Y + 4);
                //    g.DrawString(SymbolName, fn, new SolidBrush(Color.Black), p1);
                //}
            }
            g.DrawPath(p, path);
        }

        public override void Draw(Graphics g, Color BC, Color FC, Font f) {
            //Console.WriteLine("DrawWire");
            Color penColor = (Consts.IsZero(Current) || float.IsNaN(Current)) ? NormalLineColor : WorkLineColor;
            penColor = (Junctions[0].T != null && Junctions[3].T != null) ? penColor : Color.Gray;

            float penWidth = IsSelected ? SelectedLineWidth : LineWidth;
            Pen pL = new Pen(penColor);
            Pen pC = new Pen(CircleBorderColor);
            Pen pB = new Pen(ControlLineColor);
            Brush cb, eb;
            pL.Width = penWidth;

            if (Junctions[1].IsMoved || Junctions[2].IsMoved) {
                DrawBezier(g, pL);
            }
            else {
                g.DrawLine(pL, Junctions[0].WorldPoint, Junctions[3].WorldPoint);
            }
            if (Bench != null && Bench.IsShowWireName) {
                float x = (Junctions[0].X + Junctions[3].X) / 2;
                float y = (Junctions[0].Y + Junctions[3].Y) / 2;
                PointF p = new PointF(x + 4, y + 4);
                g.MeasureString(SymbolName, fn);
                g.DrawString(SymbolName, fn, new SolidBrush(Color.Black), p);
            }

            if (IsSelected) {
                cb = new SolidBrush(CircleFillColor);
                eb = new SolidBrush(Color.Red);

                g.DrawLine(pB, Junctions[0].WorldPoint, Junctions[1].WorldPoint);
                g.DrawLine(pB, Junctions[3].WorldPoint, Junctions[2].WorldPoint);

                for (int i = 0; i < Junctions.Count; i++) {
                    Junction j = Junctions[i];
                    if (j.Area == WireArea.EndPoint || j.Area == WireArea.StartPoint) {
                        g.FillEllipse(eb, j.EleRectangleF);
                    }
                    else {
                        g.FillEllipse(cb, j.EleRectangleF);
                    }
                    g.DrawEllipse(pC, j.EleRectangleF);
                }
            }
            else {
                eb = new SolidBrush(Color.Yellow);
                if (Junctions[0].T != null) {
                    g.FillEllipse(eb, Junctions[0].SmallRect);
                }
                if (Junctions[3].T != null) {
                    g.FillEllipse(eb, Junctions[3].SmallRect);
                }
            }
        }

        public string ToJson() {
            string s = "{Name:" + SymbolName + ",Start:" + (Start.T == null ? "null" : Start.T.Owner.SymbolName + "." + (int)Start.T.Key) + ",End:" + (End.T == null ? "null" : End.T.Owner.SymbolName + "." + (int)End.T.Key) + ",Junctions:[";
            for (int i = 0; i < Junctions.Count; i++) {
                Junction j = Junctions[i];
                if (i == 0) {
                    s += j.ToJson();
                }
                else {
                    s += "," + j.ToJson();
                }
            }
            s += "]}";
            return s;
        }
    }
}
