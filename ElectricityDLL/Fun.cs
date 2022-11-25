using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Security.Cryptography;

namespace ElectricityDLL {
    public static class Fun {
        public static string NSpace(int n) {
            string r = "";
            for (int i = 0; i <= n; i++) {
                r += " ";
            }
            return r;
        }

        public static Bitmap Rotate(Bitmap b, int angle, PointF cp) {
            angle = angle % 360;
            //弧度转换
            double radian = angle * Math.PI / 180.0;
            double cos = Math.Cos(radian);
            double sin = Math.Sin(radian);
            //原图的宽和高
            int w = b.Width;
            int h = b.Height;
            int W = (int)(Math.Max(Math.Abs(w * cos - h * sin), Math.Abs(w * cos + h * sin)));
            int H = (int)(Math.Max(Math.Abs(w * sin - h * cos), Math.Abs(w * sin + h * cos)));
            //目标位图
            Bitmap dsImage = new Bitmap(W, H);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(dsImage);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //计算偏移量
            Point Offset = new Point((W - w) / 2, (H - h) / 2);
            //构造图像显示区域：让图像的中心与窗口的中心点一致
            Rectangle rect = new Rectangle(Offset.X, Offset.Y, w, h);
            Point center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
            g.TranslateTransform(cp.X, cp.Y);
            g.RotateTransform(angle);
            //恢复图像在水平和垂直方向的平移
            g.TranslateTransform(-cp.X, -cp.Y);
            g.DrawImage(b, rect);
            //重至绘图的所有变换
            g.ResetTransform();
            g.Save();
            g.Dispose();
            //dsImage.Save("yuancd.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            return dsImage;
        }
        /// <summary>
        /// 获取元器件的中文名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetChineseName(ComponentType type) {
            switch (type) {
                case ComponentType.Ammeter:
                    return "电流表";
                case ComponentType.BatteryCase:
                    return "电源";
                case ComponentType.Fan:
                    return "电动机";
                case ComponentType.Lampstand:
                    return "小灯泡";
                case ComponentType.Meter:
                    return "仪表";
                case ComponentType.Ohmmeter:
                    return "欧姆表";
                case ComponentType.Other:
                    return "其他";
                case ComponentType.Resistor:
                    return "电阻器";
                case ComponentType.Rheostat:
                    return "变阻器";
                case ComponentType.Switch:
                    return "开关";
                case ComponentType.Voltmeter:
                    return "电压表";
            }
            return "";
        }

        /// <summary>
        /// 获取元器件的英文首字母
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetSymbol(ComponentType type) {
            switch (type) {
                case ComponentType.Ammeter:
                    return "A";
                case ComponentType.BatteryCase:
                    return "B";
                case ComponentType.Fan:
                    return "M";
                case ComponentType.Lampstand:
                    return "L";
                case ComponentType.Meter:
                    return "E";
                case ComponentType.Ohmmeter:
                    return "Ω";
                case ComponentType.Other:
                    return "O";
                case ComponentType.Resistor:
                    return "R";
                case ComponentType.Rheostat:
                    return "R";
                case ComponentType.Switch:
                    return "S";
                case ComponentType.Voltmeter:
                    return "V";
            }
            return "";
        }

        /// <summary>
        /// 计算屏幕上两个点之间的距离
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double CalculateDistance(PointF p1, PointF p2) {
            double w = p2.X - (double)p1.X;
            double h = p2.Y - (double)p1.Y;
            return Math.Abs(Math.Sqrt(Math.Pow(w, 2) + Math.Pow(h, 2)));
        }

        /// <summary>
        /// 求解三次贝塞尔曲线的三个解
        /// </summary>
        /// <param name="v"></param>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <returns></returns>
        public static List<double> SolveBezier(float v, float v0, float v1, float v2, float v3) {
            List<double> ls = new List<double>();
            float a = v3 - 3 * v2 + 3 * v1 - v0;
            float b = 3 * v2 - 6 * v1 + 3 * v0;
            float c = 3 * v1 - 3 * v0;
            float d = v0 - v;
            float w = -0.5f;
            float p = (3 * a * c - b * b) / (3 * a * a);
            float q = (27 * a * a * d - 9 * a * b * c + 2 * b * b * b) / (27 * a * a * a);
            float s1 = -b / (3 * a);
            double s2, s3;
            double _s2 = -q / 2 + Math.Sqrt(q * q / 4 + p * p * p / 27);
            double _s3 = -q / 2 - Math.Sqrt(q * q / 4 + p * p * p / 27);
            double m = 1d / 3d;
            if (_s2 > 0) {
                s2 = Math.Pow(_s2, m);
            }
            else {
                s2 = Math.Pow(-_s2, m);
            }
            if (_s3 > 0) {
                s3 = Math.Pow(_s3, m);
            }
            else {
                s3 = Math.Pow(-_s3, m);
            }

            double x1, x2, x3, X2, X3;
            x1 = s1 + s2 + s3;
            x2 = s1 + w * s2 + w * w * s3;
            X2 = Math.Sqrt(3) / 2 * s2 + (-Math.Sqrt(3) / 2) * s3;
            x3 = s1 + w * w * s2 + w * s3;
            X3 = Math.Sqrt(3) / 2 * s3 + (-Math.Sqrt(3) / 2) * s2;
            ls.Add(x1);
            ls.Add(x2);
            ls.Add(x3);
            return ls;
        }

        /// <summary>
        /// 返回点P是否在由P0,P1,P2,P3所构成的那条贝塞尔曲线上
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public static bool PointInBezier(PointF p, PointF p0, PointF p1, PointF p2, PointF p3) {
            List<double> xs = SolveBezier(p.X, p0.X, p1.X, p2.X, p3.X);
            List<double> ys = SolveBezier(p.Y, p0.Y, p1.Y, p2.Y, p3.Y);
            bool b = true;
            foreach (double d in xs) {
                Console.WriteLine(d);
                if (d < 0 || d >= 1) { b = false; }
            }
            foreach (double d in ys) {
                Console.WriteLine(d);
                if (d < 0 || d >= 1) { b = false; }
            }
            return b;
        }

        /// <summary>
        /// 获取圆上一点绕圆心旋转一定角度后的新坐标
        /// </summary>
        /// <param name="C">圆心坐标</param>
        /// <param name="P">圆上一点坐标</param>
        /// <param name="A">旋转角度</param>
        /// <returns></returns>
        public static PointF GetRotatePoint(PointF C, PointF P, float A) {
            double x = (P.X - C.X) * Math.Cos(A) - (P.Y - C.Y) * Math.Sin(A) + C.X;
            double y = (P.Y - C.Y) * Math.Cos(A) + (P.X - C.X) * Math.Sin(A) + C.Y;
            return new PointF((float)x, (float)y);
        }

        /// <summary>
        /// 根据t值，计算贝塞尔曲线上的某点坐标值(x/y)
        /// </summary>
        /// <param name="t"></param>
        /// <param name="x0"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="x3"></param>
        /// <returns></returns>
        public static double GetBezierValue(float t, float x0, float x1, float x2, float x3) {
            double u = 1 - t;
            return x0 * Math.Pow(u, 3) + 3 * x1 * t * Math.Pow(u, 2) + 3 * x2 * Math.Pow(t, 2) * u + x3 * Math.Pow(t, 3);
        }

        public static PointF GetBezierPointF(float t, PointF p0, PointF p1, PointF p2, PointF p3) {
            double x = GetBezierValue(t, p0.X, p1.X, p2.X, p3.X);
            double y = GetBezierValue(t, p0.Y, p1.Y, p2.Y, p3.Y);
            return new PointF((float)x, (float)y);
        }

        /// <summary>
        /// 点p是否在点p1和p2连成的直线上
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static bool IsInLine(PointF p, PointF p1, PointF p2, float range) {
            if (p == p1 || p == p2) { return true; }
            double cross = (p2.X - p1.X) * (p.X - p1.X) + (p2.Y - p1.Y) * (p.Y - p1.Y);
            if (cross <= 0) return false;
            double d2 = (p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y);
            if (cross >= d2) return false;
            double r = cross / d2;
            double dx = p1.X + (p2.X - p1.X) * r;
            double dy = p1.Y + (p2.Y - p1.Y) * r;
            double l = Math.Sqrt((p.X - dx) * (p.X - dx) + (dy - p.Y) * (dy - p.Y));
            //Console.WriteLine("L=" + l);
            return l < range;
        }

        /// <summary>
        /// 点是否在多边形区域内
        /// </summary>
        /// <param name="p"></param>
        /// <param name="Vertexs"></param>
        /// <returns></returns>
        public static bool IsInPolygon(PointF p, List<PointF> Vertexs) {
            Region r = new Region();
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.Reset();
            path.AddPolygon(Vertexs.ToArray());
            r.MakeEmpty();
            r.Union(path);
            return r.IsVisible(p);
        }

        /// <summary>
        /// 将方程添加到方程数组。如果添加的方程与已有方程存在相关性，则不添加。添加成功返回true，添加不成功返回false
        /// </summary>
        /// <param name="eq"></param>
        /// <param name="eqs"></param>
        /// <returns></returns>
        public static bool AddEquation(Equation eq, List<Equation> eqs) {
            if (eq == null) return false;
            if (eqs == null) eqs = new List<Equation>();
            bool b = false;
            for (int i = 0; i < eqs.Count; i++) {
                if (eqs[i].IsRelated(eq)) {
                    b = true;
                    break;
                }
            }
            if (!b) {
                eqs.Add(eq);
                return true;
            }
            return false;
        }

        public static bool AddIntToVector(int n, List<int> ns) {
            bool r = false;
            if (ns == null) ns = new List<int>();
            for (int i = 0; i < ns.Count; i++) {
                if (ns[i] == n) {
                    r = true;
                    break;
                }
            }
            if (!r) {
                ns.Add(n);
                r = true;
            }
            return r;
        }

        public static bool AddTerminal(Terminal t, List<Terminal> ts) {
            if (t == null) return false;
            if (ts == null) ts = new List<Terminal>();
            bool b = false;
            for (int i = 0; i < ts.Count; i++) {
                if (ts[i].Id == t.Id) {
                    b = true;
                    break;
                }
            }
            if (!b) {
                ts.Add(t);
            }
            return !b;
        }

        public static bool AddNode(Node node, List<Node> nodes) {
            bool b = false;
            if (node == null) return false;
            if (nodes == null) {
                nodes = new List<Node>();
                nodes.Add(node);
            }
            else {
                for (int i = 0; i < nodes.Count; i++) {
                    if (nodes[i].Id == node.Id) {
                        b = true;
                        break;
                    }
                }
                if (!b) {
                    nodes.Add(node);
                }
            }
            return !b;
        }

        public static bool AddEleComponents<T>(T t, List<T> ts) where T : EleComponent {
            if (t == null) return false;
            if (ts == null) {
                ts = new List<T>();
            }
            bool b = false;
            foreach (EleComponent ele in ts) {
                if (ele.Id == t.Id) { b = true; break; }
            }
            if (!b) {
                ts.Add(t);
            }
            return !b;
        }

        public static int GetNodeIndex(Node node, List<Node> nodes) {
            if (nodes == null || node == null) return -1;
            for (int i = 0; i < nodes.Count; i++) {
                if (node.Id == nodes[i].Id) { return i; }
            }
            return -1;
        }

        public static bool AddPath<T>(T b, List<T> bs) where T : ElePath {
            if (b != null && b.Count > 2) {
                if (bs == null) { bs = new List<T>(); }
                for (int i = 0; i < bs.Count; i++) {
                    if (bs[i].IsEqu(b)) {
                        return false;
                    }
                }
                bs.Add(b);
                return true;
            }
            else {
                return false;
            }
        }

        public static void AddBranchs<T>(T sourceBS, T targetBS) where T : List<ElePath> {
            if (sourceBS == null || targetBS == null) return;
            for (int i = 0; i < sourceBS.Count; i++) {

            }
            for (int i = 0; i < sourceBS.Count; i++) {
                AddPath(sourceBS[i], targetBS);
            }
        }

        public static bool AddCircuitGroup(CircuitGroup cg, List<CircuitGroup> cgs) {
            bool b = false;
            if (cgs == null) { cgs = new List<CircuitGroup>(); }
            for (int i = 0; i < cgs.Count; i++) {
                if (cgs[i].Name == cg.Name) {
                    b = true;
                    break;
                }
            }
            if (!b) {
                cgs.Add(cg);
            }
            return b;
        }

        public static void RemoveTerminal(Terminal t, List<Terminal> ts) {
            if (t != null && ts.Count > 0) {
                for (int i = ts.Count - 1; i >= 0; i--) {
                    if (ts[i].Id == t.Id) {
                        ts.RemoveAt(i);
                    }
                }
            }
        }

        public static EleComponent GetDisplayObject(int start, int end, List<Terminal> terminals) {
            int length = terminals.Count;
            if (Math.Abs(start - end) != 1 || start < 0 || start >= length || end < 0 || end >= length) { return null; }
            EleComponent o1 = terminals[start].Owner;
            EleComponent o2 = terminals[end].Owner;
            EleComponent r = null;
            if (o1 != o2) {
                List<Junction> cs = terminals[start].Junctions;
                bool b = false;
                for (int i = 0; i < cs.Count; i++) {
                    if (cs[i].AnotherSideTerminal().Id == (terminals[end]).Id) {
                        r = cs[i].Owner;
                        b = true;
                        break;
                    }
                }
                if (!b) {
                    r = null;
                }
            }

            else {
                r = o1;
            }
            return r;
        }

        /// <summary>
        /// 获取某电器元件上两个接线柱之间的电阻
        /// </summary>
        /// <param name="ele"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="isContainPower">是否包含电源</param>
        /// <param name="IsIdeal">是否理想状态</param>
        /// <returns></returns>
        public static float GetResistance(EleComponent ele, Terminal t1, Terminal t2, bool IsIdeal = true) {
            float r = float.NaN;
            float t = float.NaN;
            if (ele != null) {
                if (ele.Type == ComponentType.Wire) {
                    if (ele.Fault != FaultType.断路) {
                        r = 0f;
                    }
                }
                else if (ele.GetType().BaseType == typeof(Element)) {
                    switch (ele.Type) {
                        case ComponentType.Switch:
                            t = ((Switch)ele).GetResistance(t1, t2);
                            break;
                        case ComponentType.Resistor:
                            t = ((Resistor)ele).GetResistance(t1, t2);
                            break;
                        case ComponentType.Ammeter:
                            t = ((Ammeter)ele).GetResistance(t1, t2);
                            break;
                        case ComponentType.BatteryCase:
                            t = ((BatteryCase)ele).GetResistance(t1, t2);
                            break;
                        case ComponentType.Fan:
                            t = ((Fan)ele).GetResistance(t1, t2);
                            break;
                        case ComponentType.Rheostat:
                            t = ((Rheostat)ele).GetResistance(t1, t2);
                            break;
                        case ComponentType.Lampstand:
                            t = ((Lampstand)ele).GetResistance(t1, t2);
                            break;
                        case ComponentType.Voltmeter:
                            t = ((Voltmeter)ele).GetResistance(t1, t2);
                            break;
                    }
                    if (t != 0 && !float.IsNaN(t)) {
                        if (IsIdeal) {
                            if (ele.Type == ComponentType.BatteryCase || ele.Type == ComponentType.Ammeter) {
                                r = 0;
                            }
                            else if (ele.Type == ComponentType.Voltmeter) {
                                r = float.NaN;
                            }
                            else {
                                r = t;
                            }
                        }
                        else {
                            r = t;
                        }
                    }
                    else {
                        r = t;
                    }
                }
                else {
                    r = float.NaN;
                }
            }
            return r;
        }

        /// <summary>
        /// 获取某段电路的电阻
        /// </summary>
        /// <param name="terminals"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="isIdeal"></param>
        /// <returns></returns>
        public static float GetCircuitResistance(List<Terminal> terminals, int start, int end, bool isIdeal = true) {
            if (terminals == null || terminals.Count < 2) return float.NaN;
            if (start < 0) start = 0;
            if (end == -1 || end >= terminals.Count) end = terminals.Count - 1;
            float r = float.NaN;
            float t = float.NaN;
            for (int i = start; i < end; i++) {
                EleComponent obj = GetDisplayObject(i, i + 1, terminals);
                t = GetResistance(obj, terminals[i], terminals[i + 1], isIdeal);
                if (float.IsNaN(t)) {
                    r = t;
                    break;
                }
                else {
                    r += t;
                }
            }
            return r;
        }

        public static int GetEleIndex<T>(T obj, List<T> objs) where T : EleComponent {
            if (obj == null || objs == null || objs.Count == 0) {
                return -1;
            }
            else {
                for (int i = 0; i < objs.Count; i++) {
                    if (objs[i].Id == obj.Id) {
                        return i;
                    }
                }
            }
            return -1;
        }

        public static int GetBranchIndex(Branch b, List<Branch> bs) {
            int r = -1;
            if (bs != null) {
                for (int i = 0; i < bs.Count; i++) {
                    if (bs[i].PathStr == b.PathStr || bs[i].PathStr == b.ReversePathStr) {
                        return i;
                    }
                }
            }
            return r;
        }

        public static int GetObjIndex(EleComponent obj, List<EleComponent> objs) {
            if (obj == null || objs == null || objs.Count == 0) {
                return -1;
            }
            else {
                for (int i = 0; i < objs.Count; i++) {
                    if (objs[i].Id == obj.Id) {
                        return i;
                    }
                }
            }
            return -1;
        }

        #region Math
        /// <summary>
        /// 获取用于消元计算的增广矩阵数组
        /// </summary>
        /// <param name="eqs"></param>
        /// <returns></returns>
        private static List<List<double>> getComputeData(List<Equation> eqs) {
            List<List<double>> a = new List<List<double>>();
            int i, j;
            for (i = 0; i < eqs.Count; i++) {
                Equation eq = eqs[i];
                List<double> item = new List<double>();
                for (j = 0; j < eq.Coefficients.Count; j++) {
                    item.Add(eq.Coefficients[j]);
                }
                item.Add(eq.Vector);
                a.Add(item);
            }
            return a;
        }

        private static string GetEquationString(List<Equation> eqs) {
            StringBuilder sb = new StringBuilder();
            foreach (Equation item in eqs) {
                sb.Append(item.ToString());
            }
            return sb.ToString();
        }

        public static double[] ComputeGaussColumns(List<Equation> eqs) {
            eqs.Sort(new EquationComparer());
            List<List<double>> a = getComputeData(eqs);
            const double e = 0.000031;
            int row = eqs.Count;
            int col = eqs[0].Coefficients.Count;
            double[] x = new double[row];
            int i, j, k;
            double kk;
            for (k = 0; k < row - 1; k++) {
                //消元计算
                for (i = k + 1; i < row; i++) {
                    kk = a[i][k] / a[k][k];
                    for (j = k; j < col; j++) {
                        a[i][j] = kk * a[k][j];
                    }
                }

                if (k < row - 2) { continue; }
                else {
                    if (Math.Abs(a[row - 1][row - 1]) < e) {
                        break;
                    }
                    else {
                        //求解
                        for (i = row - 1; i >= 0; i--) {
                            x[i] = a[i][col - 1];
                            for (j = i + 1; j < col - 1; j++) {
                                x[i] -= a[i][i] * x[j];
                            }
                            x[i] /= a[i][i];
                        }
                    }
                }
            }
            return x;
        }

        public static double[] ComputeGaussRows(List<Equation> eqs) {
            eqs.Sort(new EquationComparer());
            List<List<double>> a = getComputeData(eqs);
            int row = a.Count;
            int col = a[0].Count;
            int L = row - 1;
            double e = 0.005;
            int i, j, l, n, m, k;
            double[] x = new double[row];
            i = 0; j = 0; l = 0; n = 0; m = 0; k = 0;
            //将增广矩阵消成上三角形式
            do {
                n = 0;
                for (l = k; l < L; l++) {
                    x[l] = a[l + 1][k] / a[k][k];
                    for (m = 0, i = k + 1; i < row; i++, m++) {
                        for (j = k; j < col; j++) {
                            a[i][j] -= x[m] * a[k][j];
                        }
                    }
                    k++;
                }
            } while (k < row);

            //将矩阵消成对角形式，并且重新给k赋值,最后只剩下对角线和最后一列的数，其它都为0

            do {
                n = 0;
                for (l = k; l >= 0; l--)
                    x[n++] = a[k - l][k + 1] / a[k + 1][k + 1];
                for (m = 0, i = k; i >= 0; i--, m++) {
                    for (j = k; j < col; j++)
                        a[k - i][j] -= x[m] * a[k + 1][j];
                }
                k--;
            } while (k >= 0);
            //准备输出结果
            double[] result = new double[row];
            for (i = 0; i < row; i++) {
                if ((a[i][i]) != 0) {
                    double v = (a[i][row]) / (a[i][i]);
                    result[i] = v;
                    if (double.IsNaN(result[i]) && Math.Abs(result[i]) < e) {
                        result[i] = 0;
                    }
                }
                else {
                    result[i] = 0;
                }
            }
            return result;
        }

        private static List<Equation> orderEquations(List<Equation> eqs) {
            if (eqs.Count > 0) {
                int cols = eqs.Count;
                int col, row;
                Equation tmpEq;
                int n = 0, m = 0;
                do {
                    n = 0;
                    for (row = 0; row < cols - 1; row++) {
                        for (col = row; col < cols; col++) {
                            if (eqs[row].Coefficients[col] == 0 && eqs[row + 1].Coefficients[col] != 0) {
                                n++;
                                tmpEq = eqs[row];
                                eqs[row] = eqs[row + 1];
                                eqs[row + 1] = tmpEq;
                                break;
                            }
                        }
                    }
                    m++;
                } while (n > 0 && m < eqs.Count);
            }
            return eqs;
        }

        public static Vector<double> SolveEquations(List<Equation> eqs) {
            double[,] A = new double[eqs.Count, eqs[0].Cols];
            double[] B = new double[eqs.Count];
            for (int i = 0; i < eqs.Count; i++) {
                for (int j = 0; j < eqs[0].Cols; j++) {
                    A[i, j] = eqs[i].Coefficients[j];
                }
                B[i] = eqs[i].Vector;
            }
            Matrix<double> matrix = DenseMatrix.OfArray(A);
            DenseVector b = DenseVector.OfArray(B);
            return matrix.LU().Solve(b);
        }

        public static double[] ComputeEquations(List<Equation> equations) {
            //List<Equation> eqs = orderEquations(equations);
            List<Equation> eqs = new List<Equation>();
            eqs.AddRange(equations);
            eqs.Sort(new Equation.EquationComparer());
            int rows = eqs.Count;
            int cols = rows;
            int i, j;
            double[] result = new double[cols];
            double eps = Math.Pow(10, -6);

            for (i = 0; i < cols; i++) {
                result[i] = double.NaN;
            }
            /*消元成上三角*/
            for (i = 0; i < rows - 1; i++) {
                for (j = i + 1; j < rows; j++) {
                    eqs[j] = eqs[i].Elimination(i, eqs[j]);
                }
            }
            Console.WriteLine("消元成上三角");
            for (i = 0; i < eqs.Count; i++) {
                Console.WriteLine(eqs[i].EquationStr);
            }
            /*消元成对角线*/
            for (i = rows - 1; i >= 0; i--) {
                eqs[i].Solution(result.ToList<double>());
            }
            Console.WriteLine("消元成对角线");
            for (i = 0; i < eqs.Count; i++) {
                Console.WriteLine(eqs[i].EquationStr);
            }
            for (i = 0; i < rows; i++) {

                result[i] = eqs[i].Vector / eqs[i].Coefficients[i];
                Console.WriteLine("result[" + i + "]=" + result[i]);
            }
            return result;
        }
        #endregion
    }
}
