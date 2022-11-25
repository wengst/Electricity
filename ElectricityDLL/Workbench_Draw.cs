using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System;
using System.Text.Json.Serialization;

namespace ElectricityDLL
{
    public partial class Workbench
    {
        [JsonIgnore]
        private GraphicsPath DrawPath { get; set; } = new GraphicsPath();

        private void DrawElement(EleComponent ele, RectangleF rectangle)
        {
            if (ele != null)
            {
                switch (ele.Type)
                {
                    case ComponentType.Ammeter:
                        ((Ammeter)ele).Draw(G, BackColor, ForeColor, Font);
                        break;
                    case ComponentType.Voltmeter:
                        ((Voltmeter)ele).Draw(G, BackColor, ForeColor, Font);
                        break;
                    case ComponentType.BatteryCase:
                        ((BatteryCase)ele).Draw(G, BackColor, ForeColor, Font);
                        break;
                    case ComponentType.Fan:
                        Fan f = (Fan)ele;
                        f.FrameIndex = (FrameIndex % FPS);
                        f.Draw(G, BackColor, ForeColor, Font);
                        break;
                    case ComponentType.Lampstand:
                        ((Lampstand)ele).Draw(G, BackColor, ForeColor, Font);
                        break;
                    case ComponentType.Resistor:
                        ((Resistor)ele).Draw(G, BackColor, ForeColor, Font);
                        break;
                    case ComponentType.Rheostat:
                        ((Rheostat)ele).Draw(G, BackColor, ForeColor, Font);
                        break;
                    case ComponentType.Switch:
                        ((Switch)ele).Draw(G, BackColor, ForeColor, Font);
                        break;
                    case ComponentType.Wire:
                        //Console.WriteLine("Draw Wire");
                        ((Wire)ele).Draw(G, BackColor, ForeColor, Font);
                        break;
                }
            }
        }

        private void DrawElement2(Graphics g, EleComponent ele, RectangleF rectangle)
        {
            if (ele != null)
            {
                switch (ele.Type)
                {
                    case ComponentType.Ammeter:
                        ((Ammeter)ele).Draw(g, BackColor, ForeColor, Font);
                        break;
                    case ComponentType.Voltmeter:
                        ((Voltmeter)ele).Draw(g, BackColor, ForeColor, Font);
                        break;
                    case ComponentType.BatteryCase:
                        ((BatteryCase)ele).Draw(g, BackColor, ForeColor, Font);
                        break;
                    case ComponentType.Fan:
                        Fan f = (Fan)ele;
                        f.FrameIndex = (FrameIndex % FPS);
                        f.Draw(g, BackColor, ForeColor, Font);
                        break;
                    case ComponentType.Lampstand:
                        ((Lampstand)ele).Draw(g, BackColor, ForeColor, Font);
                        break;
                    case ComponentType.Resistor:
                        ((Resistor)ele).Draw(g, BackColor, ForeColor, Font);
                        break;
                    case ComponentType.Rheostat:
                        ((Rheostat)ele).Draw(g, BackColor, ForeColor, Font);
                        break;
                    case ComponentType.Switch:
                        ((Switch)ele).Draw(g, BackColor, ForeColor, Font);
                        break;
                    case ComponentType.Wire:
                        //Console.WriteLine("Draw Wire");
                        ((Wire)ele).Draw(g, BackColor, ForeColor, Font);
                        break;
                }
            }
        }

        private void _ReDrawRectangle(List<int> moveEleIds)
        {
            G.FillRectangle(new SolidBrush(BackColor), DDI.LastRectangle);
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                foreach (int guid in moveEleIds)
                {
                    if (Items[i].EleRectangleF.IntersectsWith(DDI.LastRectangle) && Items[i].Id != guid)
                    {
                        DrawElement(Items[i], DDI.LastRectangle);
                    }
                }
            }
        }

        private void Draw_Paint(object sender, PaintEventArgs e)
        {
            //if (IsDirty) {
            Graphics G = CreateGraphics();

            G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            G.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            G.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            //List<RectangleF> DirtyRectangleFs = GetDirtyRectangleFs();
            //foreach (RectangleF rectangle in DirtyRectangleFs) {
            G.FillRectangle(new SolidBrush(BackColor), R);
            //}
            //Console.WriteLine("Start Draw");
            foreach (Element ele in Elements)
            {
                DrawElement(ele, R);
                for (int i = 0; i < ele.Terminals.Count; i++)
                {
                    if (ele.Terminals[i].IsMouseUp)
                    {
                        Pen p = new Pen(Color.Blue);
                        p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        G.DrawRectangle(p, ele.Terminals[i].WR);
                    }
                }
            }
            foreach (Wire wire in Wires)
            {
                DrawElement(wire, R);
            }
            G.Dispose();
            //Console.WriteLine("End Draw");
            //}
        }

        private void Draw_Paint2(object sender, PaintEventArgs e)
        {

            TargetImg = new Bitmap((int)R.Width, (int)R.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics tg = Graphics.FromImage(TargetImg);
            tg.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            tg.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            tg.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            tg.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            tg.FillRectangle(new SolidBrush(BackColor), R);
            //}
            //Console.WriteLine("Start Draw");
            foreach (Element ele in Elements)
            {
                DrawElement2(tg, ele, R);
                for (int i = 0; i < ele.Terminals.Count; i++)
                {
                    if (ele.Terminals[i].IsMouseUp)
                    {
                        Pen p = new Pen(Color.Blue);
                        p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        tg.DrawRectangle(p, ele.Terminals[i].WR);
                    }
                }
            }
            foreach (Wire wire in Wires)
            {
                DrawElement2(tg, wire, R);
            }
            tg.Dispose();

            //if (IsDirty) {
            Graphics G = CreateGraphics();
            G.DrawImage(TargetImg, 0, 0, TargetImg.Width, TargetImg.Height);
            G.Dispose();
            //Console.WriteLine("End Draw");
            //}
        }

        public void Draw()
        {
            if (IsAnimation)
            {
                StartTimer();
            }
            else
            {
                Draw_Paint2(null, null);
            }
        }

        public void DrawCircuitImage()
        {
            Graphics G = CreateGraphics();
            G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            G.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            G.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            foreach (Element element in Elements)
            {
                switch (element.Type)
                {
                    case ComponentType.Resistor:
                        ((Resistor)element).CircuitImageInfo.Draw(G);
                        break;
                    case ComponentType.Rheostat:
                        ((Rheostat)element).CircuitImageInfo.Draw(G);
                        break;
                    case ComponentType.BatteryCase:
                        ((BatteryCase)element).CircuitImageInfo.Draw(G);
                        break;
                    case ComponentType.Fan:
                        ((Fan)element).CircuitImageInfo.Draw(G);
                        break;
                    case ComponentType.Ammeter:
                        ((Ammeter)element).CircuitImageInfo.Draw(G);
                        break;
                    case ComponentType.Voltmeter:
                        ((Voltmeter)element).CircuitImageInfo.Draw(G);
                        break;
                    case ComponentType.Lampstand:
                        ((Lampstand)element).CircuitImageInfo.Draw(G);
                        break;
                    case ComponentType.Switch:
                        ((Switch)element).CircuitImageInfo.Draw(G);
                        break;
                }
            }
            G.Dispose();
        }

        /// <summary>
        /// 获取实物图
        /// </summary>
        /// <param name="fileType"></param>
        /// <param name="AllWorkbench"></param>
        /// <returns></returns>
        public Image GetImage(string fileType = "jpg", bool AllWorkbench = false)
        {
            float x1 = float.MaxValue, x2 = 0, y1 = float.MaxValue, y2 = 0;
            Rectangle rect;
            Color bgColor = Color.White;

            switch (fileType)
            {
                case "jpg":
                    bgColor = Color.White;
                    break;
                case "png":
                    bgColor = Color.Transparent;
                    break;
            }
            Bitmap Img = new Bitmap((int)R.Width, (int)R.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Graphics tg = Graphics.FromImage(Img);
            tg.InterpolationMode = InterpolationMode.NearestNeighbor;
            tg.SmoothingMode = SmoothingMode.HighQuality;
            tg.PixelOffsetMode = PixelOffsetMode.HighQuality;
            tg.CompositingQuality = CompositingQuality.HighQuality;
            tg.FillRectangle(new SolidBrush(bgColor), R);
            foreach (Element ele in Elements)
            {
                DrawElement2(tg, ele, R);
            }
            foreach (Wire wire in Wires)
            {
                DrawElement2(tg, wire, R);
            }
            tg.Dispose();
            if (!AllWorkbench)
            {
                foreach (EleComponent ele in Items)
                {
                    if (ele.GetType().BaseType == typeof(Element))
                    {
                        x1 = Math.Min(x1, ele.X);
                        x2 = Math.Max(x2, ele.X + ele.Width);
                        y1 = Math.Min(y1, ele.Y);
                        y2 = Math.Max(y2, ele.Y + ele.Height);
                    }
                    else if (ele.Type == ComponentType.Wire)
                    {
                        Wire w = (Wire)ele;
                        x1 = Math.Min(x1, w.RectF.X);
                        x2 = Math.Max(x2, w.RectF.X + w.RectF.Width);
                        y1 = Math.Min(y1, w.RectF.Y);
                        y2 = Math.Max(y2, w.RectF.Y + w.RectF.Height);
                    }
                }
                rect = new Rectangle((int)x1, (int)y1, (int)(x2 - x1), (int)(y2 - y1));
                Img = Img.Clone(rect, Img.PixelFormat);
            }

            return Img;
        }
    }
}
