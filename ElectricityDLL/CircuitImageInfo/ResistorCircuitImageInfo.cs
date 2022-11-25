using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ElectricityDLL
{
    public class ResistorCircuitImageInfo : CircuitImageInfo
    {
        public ResistorCircuitImageInfo(Element owner = null)
        {
            Owner = owner;
            Size = new SizeF(96f, 12f);
            LinkEnds.Add(new Offset(0, 0.5f));
            LinkEnds.Add(new Offset(1, 0.5f));
        }

        public override void Draw(Graphics g)
        {
            if (Point.IsEmpty && Owner != null) { Point = Owner.WorldPoint; }

            float x0 = Point.X, y0 = Point.Y + Size.Height * 0.5f;
            float x1 = Point.X + ScaleSize.Width * 0.25f, y1 = y0;
            float x2 = Point.X + ScaleSize.Width * 0.25f, y2 = Point.Y;
            float w = ScaleSize.Width * 0.5f, h = ScaleSize.Height;
            RectangleF r = new RectangleF(x2, y2, w, h);
            float x3 = Point.X + ScaleSize.Width * 0.75f, y3 = Point.Y + ScaleSize.Height * 0.5f;
            float x4 = Point.X + ScaleSize.Width, y4 = y3;
            GraphicsPath gp = new GraphicsPath();
            gp.AddLine(x0, y0, x1, y1);
            gp.AddRectangle(r);
            gp.AddLine(x3, y3, x4, y4);
            g.DrawPath(new Pen(LineColor, LineWidth), gp);
            base.Draw(g);
        }
    }

    public class RheostatCircuitImageInfo : CircuitImageInfo
    {
        [Category(Consts.PGC_Sharp), DisplayName(Consts.PGDN_LeftToRight)]
        public bool IsLeftToRight { get; set; } = false;

        public RheostatCircuitImageInfo(Element owner = null)
        {
            Owner = owner;
            Size = new SizeF(96f, 24f);
            LinkEnds.Add(new Offset(0, 0.75f));
            LinkEnds.Add(new Offset(1, 0.75f));
        }

        public override void Draw(Graphics g)
        {
            if (Point.IsEmpty && Owner != null) { Point = Owner.WorldPoint; }

            GraphicsPath gp = new GraphicsPath();

            Offset[] l_ps = new Offset[] { new Offset(0, 0.75f), new Offset(0.167f, 0.75f), new Offset(0.167f, 0), new Offset(0.583f, 0), new Offset(0.583f, 0.5f), new Offset(0.833f, 0.75f), new Offset(1, 0.75f), new Offset(0.334f, 0.5f), new Offset(0.833f, 1) };
            Offset[] r_ps = new Offset[] { new Offset(0, 0.75f), new Offset(0.167f, 0.75f), new Offset(0.417f, 0.5f), new Offset(0.417f, 0f), new Offset(0.833f, 0f), new Offset(0.833f, 0.75f), new Offset(1f, 0.75f), new Offset(0.167f, 0.5f), new Offset(0.667f, 1) };
            PointF[] points = new PointF[9];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new PointF(Point.X + ScaleSize.Width * (IsLeftToRight ? l_ps[i].X : r_ps[i].X), Point.Y + ScaleSize.Height * (IsLeftToRight ? l_ps[i].Y : r_ps[i].Y));
            }
            RectangleF r = new RectangleF(points[7].X, points[7].Y, points[8].X - points[7].X, points[8].Y - points[7].Y);

            GraphicsPath sjx = new GraphicsPath();
            PointF[] sjx_points = new PointF[3];
            PointF sjxdd;
            Pen pen = new Pen(LineColor, LineWidth);
            SolidBrush sb = new SolidBrush(LineColor);

            if (IsLeftToRight)
            {
                g.DrawLine(pen, points[0], points[1]);
                g.DrawLine(pen, points[1], points[2]);
                g.DrawLine(pen, points[2], points[3]);
                g.DrawLine(pen, points[3], points[4]);
                g.DrawLine(pen, points[5], points[6]);
                sjxdd = points[4];
            }
            else
            {
                g.DrawLine(pen, points[0], points[1]);
                g.DrawLine(pen, points[2], points[3]);
                g.DrawLine(pen, points[3], points[4]);
                g.DrawLine(pen, points[4], points[5]);
                g.DrawLine(pen, points[5], points[6]);
                sjxdd = points[2];
            }
            g.DrawRectangle(pen, RectangleF2Rectangle(r));

            //g.DrawPath(new Pen(LineColor, LineWidth), gp);

            sjx_points[0] = sjxdd;
            sjx_points[1] = new PointF(sjxdd.X - Scale * LinkEndBoxSize.Width - 1, sjxdd.Y - Scale * LinkEndBoxSize.Height - 1);
            sjx_points[2] = new PointF(sjxdd.X + Scale * LinkEndBoxSize.Width + 1, sjxdd.Y - Scale * LinkEndBoxSize.Height - 1);
            sjx.AddPolygon(sjx_points);
            g.FillPath(new SolidBrush(LineColor), sjx);

            base.Draw(g);
        }
    }

    public class LampstandCircuitImageInfo : CircuitImageInfo
    {
        public LampstandCircuitImageInfo(Element owner = null)
        {
            Owner = owner;

            Size = new SizeF(72f, 16f);
            LinkEnds.Add(new Offset(0, 0.5f));
            LinkEnds.Add(new Offset(1, 0.5f));
        }

        public override void Draw(Graphics g)
        {
            if (Point.IsEmpty && Owner != null) { Point = Owner.WorldPoint; }
            base.DrawLikeAmmeterSymbol(g, "×");
            base.Draw(g);
        }
    }

    public class AmmeterCircuitImageInfo : CircuitImageInfo
    {
        private void init()
        {
            Size = new SizeF(96f, 16f);
            LinkEnds.Add(new Offset(0, 0.5f));
            LinkEnds.Add(new Offset(1, 0.5f));
        }
        public AmmeterCircuitImageInfo(Element owner)
        {
            Owner = owner;
            if (owner != null) Point = owner.WorldPoint;
            init();
        }
        public AmmeterCircuitImageInfo()
        {
            init();
        }

        public override void Draw(Graphics g)
        {
            if (Point.IsEmpty && Owner != null) { Point = Owner.WorldPoint; }
            base.DrawLikeAmmeterSymbol(g, "A");
            base.Draw(g);
        }
    }

    public class VoltmeterCircuitImageInfo : CircuitImageInfo
    {

        public VoltmeterCircuitImageInfo(Element owner)
        {
            Owner = owner;
            Size = new SizeF(96f, 16f);

            LinkEnds.Add(new Offset(0, 0.5f));
            LinkEnds.Add(new Offset(1, 0.5f));
        }

        public override void Draw(Graphics g)
        {
            if (Point.IsEmpty && Owner != null) { Point = Owner.WorldPoint; }
            base.DrawLikeAmmeterSymbol(g, "V");
            base.Draw(g);
        }
    }

    public class FanCircuitImageInfo : CircuitImageInfo
    {
        public FanCircuitImageInfo(Element owner)
        {
            Owner = owner;
            Size = new SizeF(96f, 16f);

            LinkEnds.Add(new Offset(0, 0.5f));
            LinkEnds.Add(new Offset(1, 0.5f));
        }

        public override void Draw(Graphics g)
        {
            if (Point.IsEmpty && Owner != null) { Point = Owner.WorldPoint; }
            base.DrawLikeAmmeterSymbol(g, "M");
            base.Draw(g);
        }
    }

    public class BatteryCircuitImageInfo : CircuitImageInfo
    {
        public BatteryCircuitImageInfo(Element owner)
        {
            Owner = owner;
            Size = new SizeF(60f, 20f);
            LinkEnds.Add(new Offset(0, 0.5f));
            LinkEnds.Add(new Offset(1, 0.5f));
        }

        public override void Draw(Graphics g)
        {
            if (Point.IsEmpty && Owner != null) { Point = Owner.WorldPoint; }

            Offset[] offsets = new Offset[] {
                new Offset(0,0.5f),
                new Offset(0.45f,0.5f),
                new Offset(0.45f,0),
                new Offset(0.45f,1),
                new Offset(0.55f,0.2f),
                new Offset(0.55f,0.8f),
                new Offset(0.55f,0.5f),
                new Offset(1,0.5f)
            };
            PointF[] points = new PointF[offsets.Length];
            for (int i = 0; i < offsets.Length; i++)
            {
                points[i] = new PointF(Point.X + ScaleSize.Width * offsets[i].X, Point.Y + ScaleSize.Height * offsets[i].Y);
            }
            Pen p = new Pen(LineColor, LineWidth);
            Pen p2 = new Pen(LineColor, LineWidth * 2);
            g.DrawLine(p, points[0], points[1]);
            g.DrawLine(p, points[2], points[3]);
            g.DrawLine(p2, points[4], points[5]);
            g.DrawLine(p, points[6], points[7]);
            p.Dispose(); p2.Dispose();
            base.Draw(g);
        }
    }

    public class SwitchCircuitImageInfo : CircuitImageInfo
    {
        /// <summary>
        /// 以高度重新计算Size大小
        /// </summary>
        /// <param name="h"></param>
        private void ResetSize(float h)
        {
            float w = (2f + (float)Math.Cos(30d * (Math.PI) / 180)) * h;
            Size = new SizeF(w, h);
        }
        public SwitchCircuitImageInfo(Element owner)
        {
            Owner = owner;
            ResetSize(20f);
            LinkEnds.Add(new Offset(0, 1));
            LinkEnds.Add(new Offset(1, 1));
        }

        public override void Draw(Graphics g)
        {
            if (Point.IsEmpty && Owner != null) { Point = Owner.WorldPoint; }
            Console.WriteLine("ScaleSize.Width=" + ScaleSize.Width + " ; ScaleSize.Height=" + ScaleSize.Height);
            Pen p = new Pen(LineColor, LineWidth);
            PointF p0 = new PointF(Point.X, Point.Y + ScaleSize.Height);
            PointF p1 = new PointF(Point.X + ScaleSize.Height, Point.Y + ScaleSize.Height);
            g.DrawLine(p, p0, p1);
            float dx = p1.X, dy = p1.Y;
            g.TranslateTransform(dx, dy);
            g.RotateTransform(-30);
            g.DrawLine(p, 0, 0, ScaleSize.Height, 0);
            g.TranslateTransform(-dx, -dy);
            g.ResetTransform();
            g.DrawLine(p, new PointF(Point.X + ScaleSize.Width - ScaleSize.Height, Point.Y + ScaleSize.Height), new PointF(Point.X + ScaleSize.Width, Point.Y + ScaleSize.Height));
            p.Dispose();
            base.Draw(g);
        }
    }

    public class WireCircuitImageInfo : CircuitImageInfo
    {
        public WireCircuitImageInfo(Wire obj)
        {
            if (obj != null)
            {
                Owner = obj;
                if (obj.Start != null)
                {
                    LinkEnds.Add(new Offset(obj.Start.P.X, obj.Start.P.Y));
                }
                if (obj.End != null)
                {
                    LinkEnds.Add(new Offset(obj.End.P.X, obj.End.P.Y));
                }
            }
        }
    }



    public class CircuitImageInfoCollection
    {
        private List<CircuitImageInfo> List { get; } = new List<CircuitImageInfo>();

        public void Add(CircuitImageInfo cci)
        {
            if (cci != null)
            {
                bool b = false;
                for (int i = 0; i < List.Count; i++)
                {
                    CircuitImageInfo e = (CircuitImageInfo)List[i];
                    if (e.Id == cci.Id) { b = true; }
                }
                if (!b)
                {
                    List.Add(cci);
                }
            }
            else
            {
                throw new ArgumentNullException("cci");
            }
        }

        public void Add<T>(T obj) where T : Element, new()
        {
            bool b = false;
            foreach (CircuitImageInfo circuit in List)
            {
                if (circuit.Owner != null && circuit.Owner.Id == obj.Id)
                {
                    b = true;
                }
            }
            if (!b)
            {
                Type t = typeof(T);
                if (t == typeof(Ammeter))
                {
                    AmmeterCircuitImageInfo a = new AmmeterCircuitImageInfo(obj);
                    List.Add(a);
                }
                else if (t == typeof(Voltmeter))
                {
                    VoltmeterCircuitImageInfo v = new VoltmeterCircuitImageInfo(obj);
                    List.Add(v);
                }
                else if (t == typeof(BatteryCase))
                {
                    BatteryCircuitImageInfo bc = new BatteryCircuitImageInfo(obj);
                    List.Add(bc);
                }
                else if (t == typeof(Lampstand))
                {
                    LampstandCircuitImageInfo l = new LampstandCircuitImageInfo(obj);
                    List.Add(l);
                }
                else if (t == typeof(Resistor))
                {
                    ResistorCircuitImageInfo r = new ResistorCircuitImageInfo(obj);
                    List.Add(r);
                }
                else if (t == typeof(Rheostat))
                {
                    RheostatCircuitImageInfo rh = new RheostatCircuitImageInfo(obj);
                    List.Add(rh);
                }
                else if (t == typeof(Switch))
                {
                    SwitchCircuitImageInfo s = new SwitchCircuitImageInfo(obj);
                    List.Add(s);
                }
                else if (t == typeof(Fan))
                {
                    FanCircuitImageInfo f = new FanCircuitImageInfo(obj);
                    List.Add(f);
                }
            }
        }

        /// <summary>
        /// 添加电路元件图
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p"></param>
        public void Add(ComponentType t, PointF p)
        {
            CircuitImageInfo c = null;
            switch (t)
            {
                case ComponentType.Ammeter:
                    c = new AmmeterCircuitImageInfo(null);
                    break;
                case ComponentType.Voltmeter:
                    c = new VoltmeterCircuitImageInfo(null);
                    break;
                case ComponentType.BatteryCase:
                    c = new BatteryCircuitImageInfo(null);
                    break;
                case ComponentType.Fan:
                    c = new FanCircuitImageInfo(null);
                    break;
                case ComponentType.Lampstand:
                    c = new LampstandCircuitImageInfo(null);
                    break;
                case ComponentType.Resistor:
                    c = new ResistorCircuitImageInfo(null);
                    break;
                case ComponentType.Rheostat:
                    c = new RheostatCircuitImageInfo(null);
                    break;
                case ComponentType.Switch:
                    c = new SwitchCircuitImageInfo(null);
                    break;
            }
            if (c != null) { c.Point = p; List.Add(c); }
        }

        /// <summary>
        /// 添加电路图元件集合
        /// </summary>
        /// <param name="elements"></param>
        public void AddRange(List<Element> elements)
        {
            if (elements != null && elements.Count > 0)
            {
                foreach (Element element in elements)
                {
                    Add(element.Type, element.WorldPoint);
                }
            }
        }

        private void AddCircuitImageInfoFromCircuit(Circuit circuit)
        {
            foreach (EleComponent ele in circuit.ElementAndWires)
            {
                if (ele.GetType().BaseType == typeof(Element))
                {
                    bool b = false;
                    foreach (CircuitImageInfo cii in List)
                    {
                        if (cii.Owner.Id == ele.Id) { b = true; }
                    }
                    if (!b)
                    {
                        Add((Element)ele);
                    }
                }
                else if (ele.Type == ComponentType.Wire)
                {
                    bool b = false;
                    foreach (CircuitImageInfo w in List)
                    {
                        if (w.Owner.Id == ele.Id)
                        {
                            b = true;
                        }
                    }
                    if (!b)
                    {
                        Add(new WireCircuitImageInfo((Wire)ele));
                    }
                }
            }
        }

        public void BuildFromCircuit(Circuit circuit)
        {
            Clear();
            AddCircuitImageInfoFromCircuit(circuit);
        }

        public void BuildFromCircuits(List<Circuit> circuits)
        {
            List.Clear();
            foreach (Circuit circuit in circuits)
            {
                AddCircuitImageInfoFromCircuit(circuit);
            }
        }

        /// <summary>
        /// 删除指定位置的电路图元件
        /// </summary>
        /// <param name="index"></param>
        public void DeleteAt(int index)
        {
            if (index >= 0 && index < List.Count)
            {
                List.RemoveAt(index);
            }
            else
            {
                throw new ArgumentOutOfRangeException("index");
            }
        }

        /// <summary>
        /// 删除电路图元件
        /// </summary>
        /// <param name="cci"></param>
        public void Delete(CircuitImageInfo cci)
        {
            if (cci != null)
            {
                for (int i = List.Count - 1; i >= 0; i--)
                {
                    CircuitImageInfo c = (CircuitImageInfo)List[i];
                    if (c.Id == cci.Id)
                    {
                        List.RemoveAt(i);
                    }
                }
            }
            else
            {
                throw new ArgumentNullException("cci");
            }
        }

        /// <summary>
        /// 删除选中的电路图元件
        /// </summary>
        public void DeleteSelected()
        {
            for (int i = List.Count - 1; i >= 0; i--)
            {
                CircuitImageInfo cir = (CircuitImageInfo)List[i];
                if (cir.IsSelected) { List.RemoveAt(i); }
            }
        }

        public void Clear()
        {
            List.Clear();
        }

        /// <summary>
        /// 获取整个工作台上所有电路符号的矩形区域
        /// </summary>
        public RectangleF FullRegion
        {
            get
            {
                float x0 = float.MaxValue, y0 = float.MaxValue;
                float x1 = float.MinValue, y1 = float.MinValue;
                foreach (CircuitImageInfo circuit in List)
                {
                    x0 = Math.Min(circuit.Point.X, x0);
                    y0 = Math.Min(circuit.Point.Y, y0);
                    x1 = Math.Max(circuit.Point.X + circuit.ScaleRegion.Width, x1);
                    y1 = Math.Max(circuit.Point.Y + circuit.ScaleRegion.Height, y1);
                }
                return new RectangleF(x0, y0, x1 - x0, y1 - y0);
            }
        }

        /// <summary>
        /// 所有电路符号进行平移
        /// </summary>
        /// <param name="offset"></param>
        public void MoveAll(Offset offset)
        {
            foreach (CircuitImageInfo circuit in List)
            {
                circuit.Point = new PointF(circuit.Point.X + offset.X, circuit.Point.Y + offset.Y);
            }
        }

        /// <summary>
        /// 缩放所有电路符号
        /// </summary>
        /// <param name="scale"></param>
        public void ScaleAll(float scale)
        {
            if (scale > 1 && scale < 5)
            {
                foreach (CircuitImageInfo circuit in List)
                {
                    circuit.Scale = scale;
                }
            }
        }

        /// <summary>
        /// 获取指定PointF处的电路图元件
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public CircuitImageInfo GetCircuitImageInfo(PointF location)
        {
            foreach (CircuitImageInfo circuit in List)
            {
                if (circuit.ScaleRegion.Contains(location))
                {
                    return circuit;
                }
            }
            return null;
        }

        /// <summary>
        /// 设置屏幕上某点的电路图元件为选中状态
        /// </summary>
        /// <param name="location"></param>
        public void SetSelected(PointF location)
        {
            foreach (CircuitImageInfo circuit in List)
            {
                if (circuit.ScaleRegion.Contains(location))
                {
                    circuit.IsSelected = true;
                }
                else { circuit.IsSelected = false; }
            }
        }

        /// <summary>
        /// 设置屏幕上某矩形区域中的电路图元件为选中状态
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="isIntersect">选择标准，是相交还是包含。true:相交，false:包含</param>
        public void SetSelected(RectangleF rect, bool isIntersect = true)
        {
            foreach (CircuitImageInfo circuit in List)
            {
                if (isIntersect)
                {
                    if (circuit.ScaleRegion.IntersectsWith(rect))
                    {
                        circuit.IsSelected = true;
                    }
                    else { circuit.IsSelected = false; }
                }
                else
                {
                    if (circuit.ScaleRegion.Contains(rect))
                    {
                        circuit.IsSelected = true;
                    }
                    else { circuit.IsSelected = false; }
                }
            }
        }


    }
}
