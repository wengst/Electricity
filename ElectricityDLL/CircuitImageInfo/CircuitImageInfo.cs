using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ElectricityDLL
{
    /// <summary>
    /// 电路图抽象类
    /// </summary>
    public abstract class CircuitImageInfo
    {
        internal Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// 元件实物对象
        /// </summary>
        [Browsable(false)]
        public EleComponent Owner { get; set; }

        /// <summary>
        /// 是否选中
        /// </summary>
        [Browsable(false)]
        public bool IsSelected { get; set; }

        /// <summary>
        /// 选中框的颜色
        /// </summary>
        [Browsable(false)]
        protected Color SelectedBoxColor { get; } = Color.Gray;

        /// <summary>
        /// 连接点的边框色
        /// </summary>
        [Browsable(false)]
        protected Color LinkEndDrawColor { get; } = Color.Green;

        /// <summary>
        /// 连接点的填充色
        /// </summary>
        [Browsable(false)]
        protected Color LinkEndFillColor { get; } = Color.White;

        /// <summary>
        /// 连接点的矩形框大小
        /// </summary>
        protected SizeF LinkEndBoxSize { get; } = new SizeF(4, 4);

        /// <summary>
        /// 线条颜色
        /// </summary>
        [Category(Consts.PGC_Sharp), DisplayName(Consts.PGDN_LineColor)]
        public Color LineColor { get; set; } = Color.FromArgb(255, 0, 0, 0);

        /// <summary>
        /// 线条宽度
        /// </summary>
        [Category(Consts.PGC_Sharp), DisplayName(Consts.PGDN_LineWidth)]
        public float LineWidth { get; set; } = 2f;

        /// <summary>
        /// 符号字体
        /// </summary>
        [Category(Consts.PGC_Sharp), DisplayName(Consts.PGDN_SymbolFont)]
        public Font SymbolFont { get; set; } = new Font("Times New Roman", 12, FontStyle.Regular);

        /// <summary>
        /// 世界坐标
        /// </summary>
        [Category(Consts.PGC_Sharp), DisplayName(Consts.PGDN_Point)]
        public PointF Point { get; set; }

        /// <summary>
        /// 名称位置
        /// <para>名称字符串框中心点相对于该元件的位置偏移比例</para>
        /// </summary>
        [Category(Consts.PGC_Sharp), DisplayName(Consts.PGDN_TextOffset)]
        public Offset TextOffset { get; set; } = new Offset(0.5f, 1.1f);

        /// <summary>
        /// 文本区域
        /// </summary>
        public RectangleF TextRegion { get; protected set; }

        /// <summary>
        /// 大小
        /// </summary>
        [Category(Consts.PGC_Sharp), DisplayName(Consts.PGDN_Size)]
        public SizeF Size { get; set; }

        [Browsable(false)]
        public SizeF ScaleSize
        {
            get
            {
                return new SizeF(Size.Width * Scale, Size.Height * Scale);
            }
        }

        /// <summary>
        /// 原始元件绘制区
        /// </summary>
        public RectangleF OriRegion { get { return new RectangleF(Point, Size); } }

        /// <summary>
        /// 缩放后的区域
        /// </summary>
        public RectangleF ScaleRegion { get { return new RectangleF(Point, ScaleSize); } }

        /// <summary>
        /// 最外侧的矩形区域
        /// </summary>
        public RectangleF OuterRegion
        {
            get
            {
                float x0 = Math.Min(ScaleRegion.X, TextRegion.X), y0 = Math.Min(ScaleRegion.Y, TextRegion.Y);
                float x1 = Math.Min(ScaleRegion.X + ScaleRegion.Width, TextRegion.X + TextRegion.Width);
                float y1 = Math.Max(ScaleRegion.Y + ScaleRegion.Height, TextRegion.Y + TextRegion.Height);
                return new RectangleF(x0, y0, x1, y1);
            }
        }

        /// <summary>
        /// 放大倍数
        /// </summary>
        public float Scale { get; set; } = 1;

        /// <summary>
        /// 连接点坐标
        /// </summary>
        public List<Offset> LinkEnds { get; } = new List<Offset>();

        protected Rectangle RectangleF2Rectangle(RectangleF r)
        {
            return new Rectangle((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);
        }

        protected void DrawLikeAmmeterSymbol(Graphics g, string s = null)
        {
            SizeF strSize = g.MeasureString(s, SymbolFont);
            float w = Math.Max(strSize.Width + 4, strSize.Height + 4);
            float h = Math.Max(w, ScaleSize.Height);
            Size = new SizeF(3 * h, h);

            PointF p0 = new PointF(Point.X, Point.Y + h * 0.5f);
            PointF p1 = new PointF(Point.X + h, p0.Y);
            PointF p2 = new PointF(p1.X, Point.Y);
            SizeF e_size = new SizeF(h, h);
            PointF p3 = new PointF(p1.X + h, p0.Y);
            PointF p4 = new PointF(p3.X + h, p3.Y);

            Pen p = new Pen(new SolidBrush(LineColor), LineWidth);

            g.DrawLine(p, p0, p1);
            g.DrawLine(p, p3, p4);
            RectangleF r0 = new RectangleF(p2, e_size);
            g.DrawEllipse(new Pen(LineColor, LineWidth), r0);
            if (s != null && s != "×")
            {
                PointF strPoint = new PointF(p2.X + (h - strSize.Width) / 2, p2.Y + (h - strSize.Height) / 2);
                g.DrawString(s, SymbolFont, new SolidBrush(LineColor), strPoint);
            }
            else if (s == "×")
            {
                p0 = new PointF(-h * 0.5f, 0);
                p1 = new PointF(h * 0.5f, 0);

                p2 = new PointF(0, -0.5f * h);
                p3 = new PointF(0, 0.5f * h);

                float dx = Point.X + 1.5f * h;
                float dy = Point.Y + 0.5f * h;
                g.TranslateTransform(dx, dy);
                g.RotateTransform(45);
                g.DrawLine(p, p0, p1);
                g.DrawLine(p, p2, p3);
                g.TranslateTransform(-dx, -dy);
                g.ResetTransform();
            }
        }

        protected void DrawSymbolName(Graphics g)
        {
            if (Owner != null)
            {
                Font f = new Font(SymbolFont.FontFamily, 10, FontStyle.Italic | FontStyle.Bold);

                string zm = Owner.SymbolName.Substring(0, 1);
                string sz = Owner.SymbolName.Substring(1, Owner.SymbolName.Length - 1);
                float x, y;
                float fs = f.Size * 0.7f;
                SizeF zmsf = g.MeasureString(zm, f);
                Font f_sz = new Font(f.FontFamily, fs, FontStyle.Italic | FontStyle.Bold);
                SizeF szsf = g.MeasureString(sz, f_sz);
                SizeF sf = new SizeF(zmsf.Width + szsf.Width, zmsf.Height + szsf.Height * 0.3f);

                if (TextRegion.IsEmpty)
                {
                    x = Point.X + (ScaleSize.Width - sf.Width) / 2;
                    y = Point.Y + ScaleSize.Height + 1;
                }
                else
                {
                    x = TextRegion.Location.X;
                    y = TextRegion.Location.Y;
                }

                Brush b = new SolidBrush(LineColor);
                g.DrawString(zm, f, b, x, y);
                x += zmsf.Width / 2 + 3;
                y += szsf.Height * 0.4f;
                g.DrawString(sz, f_sz, b, x, y);
            }
        }

        public virtual void Draw(Graphics g)
        {
            if (Owner != null && Owner.IsSelected)
            {
                Pen p0 = new Pen(SelectedBoxColor, 1f);
                GraphicsPath gp = new GraphicsPath();
                g.DrawRectangle(p0, RectangleF2Rectangle(ScaleRegion));
                foreach (Offset offset in LinkEnds)
                {
                    float x0 = Point.X + offset.X * ScaleRegion.Width - LinkEndBoxSize.Width / 2;
                    float y0 = Point.Y + offset.Y * ScaleRegion.Height - LinkEndBoxSize.Height / 2;
                    gp.AddRectangle(new RectangleF(x0, y0, LinkEndBoxSize.Width, LinkEndBoxSize.Height));
                }
                g.FillPath(new SolidBrush(LinkEndFillColor), gp);
                g.DrawPath(new Pen(LinkEndDrawColor), gp);
            }
            DrawSymbolName(g);
        }
    }
}
