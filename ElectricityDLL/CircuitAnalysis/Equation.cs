using System;
using System.Collections.Generic;
using System.Text;

namespace ElectricityDLL {
    public class Equation {
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// 方程类型
        /// </summary>
        public EquationType Type { get; set; } = EquationType.KCL;

        public string OrderName { get; set; }

        /// <summary>
        /// 系数数组
        /// </summary>
        public List<double> Coefficients { get; } = new List<double>();

        /// <summary>
        /// 向量
        /// </summary>
        public double Vector = 0;

        /// <summary>
        /// 有效数字的位数
        /// </summary>
        public int YXS = 2;

        /// <summary>
        /// 非零系数数量
        /// <para>如果非零系数只有1个，则可以直接计算解</para>
        /// </summary>
        public int NonZeroCoefficientAmount {
            get {
                int n = 0;
                foreach (double d in Coefficients) {
                    if (d != 0d && !double.IsNaN(d)) {
                        n++;
                    }
                }
                return n;
            }
        }

        /// <summary>
        /// 系数零的数量
        /// </summary>
        public int ZeroCoefficientAmount {
            get {
                int n = 0;
                foreach (double d in Coefficients) {
                    if (d == 0d) {
                        n++;
                    }
                }
                return n;
            }
        }

        /// <summary>
        /// 系数数组长度
        /// </summary>
        public int Cols { get { return Coefficients.Count; } }

        public Equation(uint rank) {
            for (int i = 0; i < rank; i++) {
                Coefficients.Add(0d);
            }
        }

        /// <summary>
        /// 根据回路创建KVL方程
        /// </summary>
        /// <param name="circuit"></param>
        /// <param name="equationBranchs"></param>
        public Equation(Circuit circuit, List<Branch> equationBranchs) {
            int n = equationBranchs.Count;
            for (int i = 0; i < n; i++) {
                Coefficients.Add(0d);
            }
            foreach (Branch branch in circuit.Branchs) {
                if (equationBranchs.Contains(branch)) {
                    Coefficients[branch.Index] = branch.R;
                }
            }
            Vector = circuit.V;
        }

        /// <summary>
        /// 根据节点创建KCL方程
        /// </summary>
        /// <param name="n"></param>
        /// <param name="equationBranchs"></param>
        public Equation(Node node, List<Branch> equationBranchs) {
            int n = equationBranchs.Count;

        }

        public int FirstNonZeroIndex {
            get {
                for (int i = 0; i < Coefficients.Count; i++) {
                    if (Coefficients[i] != 0d) {
                        return i;
                    }
                }
                return 0;
            }
        }

        public int LastNonZeroIndex {
            get {
                for (int i = Cols - 1; i >= 0; i--) {
                    if (Coefficients[i] != 0d) {
                        return i;
                    }
                }
                return 0;
            }
        }

        public bool ColumnToNumberOne(int col) {
            int n = -1;
            double factor = 0d;
            int i;
            for (i = 0; i < Cols; i++) {
                if (Coefficients[i] == 0d && i != col) {
                    if (n == -1) {
                        n = 1;
                    }
                    else {
                        n++;
                    }
                }
                if (i == col) {
                    factor = Coefficients[i];
                }
            }
            if (n == Cols - 1 && factor != 0d && factor != 1) {
                Coefficients[i] = Coefficients[i] / factor;
                Vector = Vector / factor;
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// 是否可消元
        /// <para>是否可消元取决于在同一个索引位置，它们的系数是不是都不为零，但又不会导致已经消元的部分重新有不为零系数</para>
        /// </summary>
        /// <param name="eq"></param>
        /// <returns></returns>
        public bool IsCanElimination(Equation eq) {
            if (eq.Cols != Cols) return false;
            int m = 0;
            for (int i = 0; i < Cols; i++) {
                double me = Coefficients[i];
                double it = eq.Coefficients[i];
                if (me != 0d && it != 0d) {
                    m++;
                }
            }
            return m > 0;
        }

        public bool Solution(List<double> result) {
            if (result != null || Cols != result.Count) return false;
            int i = 0;
            int SolutionIndex = -1;
            double tmp = 0;
            int n = 0;
            for (i = 0; i < Cols; i++) {
                if (Coefficients[i] != 0 && double.IsNaN(result[i])) {
                    n++;
                    SolutionIndex = i;
                }
            }
            if (n == 1) {
                for (i = 0; i < Cols; i++) {
                    if (i != SolutionIndex && double.IsNaN(result[i])) {
                        tmp += result[i] * Coefficients[i];
                    }
                }
                result[SolutionIndex] = (Vector - tmp) / Coefficients[SolutionIndex];
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// 代入求解
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool Substitute(List<double> result) {
            Equation e = new Equation((uint)result.Count);
            e.Coefficients.AddRange(Coefficients);
            e.Vector = Vector;
            for (int i = 0; i < Cols; i++) {
                if (!double.IsNaN(result[i]) && e.Coefficients[i] != 0d) {
                    e.Vector -= e.Coefficients[i] * result[i];
                    e.Coefficients[i] = 0;
                }
            }
            if (e.NonZeroCoefficientAmount == 1) {
                result[e.FirstNonZeroIndex] = e.Vector / e.Coefficients[e.FirstNonZeroIndex];
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// 方程乘
        /// </summary>
        /// <param name="multiplier"></param>
        public void MultiplyNumber(double multiplier) {
            for (int i = 0; i < Cols; i++) {
                Coefficients[i] = Coefficients[i] * multiplier;
            }
            Vector = Vector * multiplier;
        }

        /// <summary>
        /// 方程加
        /// </summary>
        /// <param name="eq"></param>
        /// <returns></returns>
        public Equation Plus(Equation eq) {
            if (eq.Cols != Cols) return this;
            for (int i = 0; i < Cols; i++) {
                Coefficients[i] += eq.Coefficients[i];
            }
            Vector += eq.Vector;
            return this;
        }

        /// <summary>
        /// 消元
        /// </summary>
        /// <param name="index"></param>
        /// <param name="eq"></param>
        /// <returns></returns>
        public Equation Elimination(int index, Equation eq) {
            if (eq.Cols != Cols || Coefficients[index] == 0) return eq;
            if (eq.Coefficients[index] == 0) return eq;
            double factor = Coefficients[index] / eq.Coefficients[index];
            eq.MultiplyNumber(factor);
            for (int i = 0; i < Cols; i++) {
                eq.Coefficients[i] = eq.Coefficients[i] - Coefficients[i];
            }
            eq.Vector = eq.Vector - Vector;
            return eq;
        }

        public void Elimination2(int col, Equation eq) {
            if (eq.Cols != Cols || col < 0 || col > Cols - 1 || Coefficients[col] == 0d || eq.Coefficients[col] == 0d) return;
            double me = Coefficients[col];
            double it = eq.Coefficients[col];
            MultiplyNumber(it);
            eq.MultiplyNumber(me);
            for (int i = 0; i < Cols; i++) {
                Coefficients[i] -= eq.Coefficients[i];
            }
            Vector -= eq.Vector;
        }

        /// <summary>
        /// 方程减
        /// </summary>
        /// <param name="eq"></param>
        /// <returns></returns>
        public Equation Subtract(Equation eq) {
            if (eq.Cols != Cols) return this;
            for (int i = 0; i < Cols; i++) {
                Coefficients[i] -= eq.Coefficients[i];
            }
            Vector -= eq.Vector;
            return this;
        }

        public Equation Clone() {
            Equation e = new Equation((uint)Cols);
            for (int i = 0; i < Cols; i++) {
                e.Coefficients[i] = Coefficients[i];
            }
            e.Vector = Vector;
            e.Type = Type;
            e.YXS = YXS;
            e.OrderName = OrderName;
            return e;
        }

        public double MaxCoefficients {
            get {
                double max = double.MinValue;
                for (int i = 0; i < Cols; i++) {
                    max = Math.Max(max, Math.Abs(Coefficients[i]));
                }
                return max;
            }
        }

        public int ZeroCount {
            get {
                int n = 0;
                for (int i = 0; i < Cols; i++) {
                    if (Coefficients[i] == 0d) {
                        n++;
                    }
                }
                return n;
            }
        }

        /// <summary>
        /// 返回非零系数的索引标志位
        /// <para>如果数字是2，表示第1位非零；如果数字是6，表示第1和第2位置的系数非零，以此类推</para>
        /// </summary>
        public int Indexs {
            get {
                int r = 0;
                for (int i = 0; i < Coefficients.Count; i++) {
                    if (Coefficients[i] != 0d) {
                        r += (int)Math.Pow(2, i);
                    }
                }
                return r;
            }
        }

        /// <summary>
        /// 两个方程是否具有相关性
        /// </summary>
        /// <param name="eq"></param>
        /// <returns></returns>
        public bool IsRelated(Equation eq) {
            if (eq.Cols == Cols) {
                double me = 0;
                double it = 0;
                int n = 0;
                double[] ns = new double[Cols + 1];
                int i = 0;
                for (i = 0; i < Cols; i++) {
                    if (eq.Coefficients[i] + Coefficients[i] == 0) {
                        n++;
                    }
                }
                if (n == Cols) {
                    return true;
                }
                n = 0;
                for (i = 0; i < Cols; i++) {
                    if (eq.Coefficients[i] - Coefficients[i] == 0) {
                        n++;
                    }
                }
                if (n == Cols) {
                    return true;
                }

                for (i = 0; i < Cols; i++) {
                    me = Coefficients[i];
                    it = eq.Coefficients[i];
                    if (me + it == 0d && Type == EquationType.KVL && eq.Type == EquationType.KVL && me != 0d) {
                        return true;
                    }
                }

                //两方程是否系数全部成比例
                for (i = 0; i < Cols; i++) {
                    me = Coefficients[i];
                    it = eq.Coefficients[i];
                    if (me != 0d && it != 0d) {
                        ns[i] = it / me;
                    }
                    else if ((me == 0d && it != 0d) || (me != 0d && it == 0d)) {
                        return false;
                    }
                }
                ns[Cols] = eq.Vector / Vector;
                me = 0d;
                for (int j = 0; j < ns.Length; j++) {
                    if (ns[j] != 0d && me != 0d) {
                        me = ns[j];
                    }
                    else if (ns[j] != 0d && me != 0d && me != ns[j]) {
                        return false;
                    }
                }
            }
            else {
                return false;
            }
            return true;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            string fn = "f" + YXS.ToString();
            foreach (double d in Coefficients) {
                string e = "[" + Type + "]" + OrderName + " : ";
                if (i > 0) {
                    e = " + ";
                }
                if (d < 0 && i != 0) {
                    e += "(" + d.ToString(fn) + ")×I" + (i + 1).ToString();
                }
                else {
                    e += d.ToString(fn) + "×I" + (i + 1).ToString();
                }

                sb.Append(e);
                i++;
            }
            sb.Append(" = " + Vector.ToString(fn));
            return sb.ToString();
        }

        public string EquationStr {
            get {
                StringBuilder sb = new StringBuilder();
                int i = 0;
                foreach (double d in Coefficients) {
                    sb.Append(d.ToString("f2") + "x" + (i + 1).ToString() + (i < Coefficients.Count - 1 ? "+" : ""));
                    i++;
                }
                sb.Append("=" + Vector.ToString("f2"));
                return sb.ToString();
            }
        }

        /// <summary>
        /// 按非零系数的数量排序
        /// <para>非零数量越少就越排在前面</para>
        /// </summary>
        public class EquationComparer : IComparer<Equation> {
            public int Compare(Equation x, Equation y) {
                if (x.Cols != y.Cols) {
                    throw new Exception("两个方程的列数不一致");
                }
                for (int i = 0; i < x.Cols; i++) {
                    double a = x.Coefficients[i];
                    double b = y.Coefficients[i];
                    if (a != 0d && b == 0d) {
                        return -1;
                    }
                    else if (a == 0d && b != 0d) {
                        return 1;
                    }
                    else if (a != 0d && b != 0d) {
                        if (Math.Abs(a) > Math.Abs(b)) {
                            return -1;
                        }
                        else if (Math.Abs(a) < Math.Abs(b)) {
                            return 1;
                        }
                        else {
                            continue;
                        }
                    }
                    else { continue; }
                }
                return 0;
            }
        }

        public class OrderByMaxCoefficient : IComparer<Equation> {
            public int Compare(Equation a, Equation b) {
                int r = 0;
                if (a.FirstNonZeroIndex < b.FirstNonZeroIndex) {
                    r = -1;
                }
                else if (a.FirstNonZeroIndex > b.FirstNonZeroIndex) {
                    r = 1;
                }
                else {
                    if (a.MaxCoefficients > b.MaxCoefficients) {
                        r = -1;
                    }
                    else if (a.MaxCoefficients < b.MaxCoefficients) {
                        r = 1;
                    }
                    else {
                        r = 0;
                    }
                }
                return r;
            }
        }
    }

    public class EquationCollection {
        /// <summary>
        /// 线性方程组
        /// </summary>
        public List<Equation> Equations { get; } = new List<Equation>();

        public List<Equation> EQS { get; } = new List<Equation>();

        public int Row { get { return Equations.Count; } }

        public int Col { get { return Equations.Count > 0 ? Equations[0].Cols : 0; } }

        /// <summary>
        /// 方程组的解
        /// </summary>
        public List<double> Solver { get; } = new List<double>();

        private Equation GetEquLastNonZeroIndexEquation(int index, Guid id) {
            foreach (Equation equation in Equations) {
                if (equation.Id != id && equation.LastNonZeroIndex == index) {
                    return equation;
                }
            }
            return null;
        }

        /// <summary>
        /// 简单求解
        /// <para>如果所有解都算出来了，返回true，否则返回false</para>
        /// </summary>
        private bool SimpleSolve() {
            /*简单求解的逻辑是：如果方程组中有一元一次方程，那么就从一元一次方程开始计算出其中一个解，然后依次代入求解。
             * 直至没有可以求解的元
             * 如果所有的解都被解出，那么返回true
             * 否则返回false
             */
            int n;
            do {
                n = 0;
                for (int i = 0; i < Row; i++) {
                    Equation e = Equations[i];
                    if (e.Substitute(Solver)) {
                        n++;
                    }
                }
            } while (n != 0);
            n = Col;
            foreach (double d in Solver) {
                if (!double.IsNaN(d)) {
                    n--;
                }
            }
            return n == 0;
        }

        private List<List<double>> getComputeData(List<Equation> eqs) {
            List<List<double>> a = new List<List<double>>();
            int i, j;
            Equation eq;
            for (i = 0; i < eqs.Count; i++) {
                eq = eqs[i];
                List<double> item = new List<double>();
                for (j = 0; j < eq.Coefficients.Count; j++) {
                    item.Add(eq.Coefficients[j]);
                }
                item.Add(eq.Vector);
                a.Add(item);
            }
            return a;
        }

        /// <summary>
        /// 三角消元法。不好用
        /// </summary>
        /// <returns></returns>
        public List<double> Solve() {
            if (Equations.Count == 0 || Equations[0].Coefficients.Count != Equations.Count) {
                throw new Exception("不合法的线性方程组");
            }
            else {
                Console.WriteLine("消元前");
                int k = 1;
                foreach (Equation equation2 in Equations) {
                    equation2.OrderName = "L" + k.ToString();
                    Console.WriteLine(equation2.ToString());
                    k++;
                }
                Solver.Clear();
                EQS.AddRange(Equations);
                EQS.Sort(new EquationComparer());
                Console.WriteLine("排序后");
                foreach (Equation equation3 in EQS) {
                    Console.WriteLine(equation3.ToString());
                }

                for (int i = 0; i < Col; i++) {
                    Solver.Add(double.NaN);
                }

                if (SimpleSolve()) {
                    return Solver;
                }

                Equation me, it;
                int m = 0, n = 0;
                ///消成上三角
                ///从第2行开始，将第N行的第N列前的系数消为零
                do {
                    m = 0; n = 0;
                    for (int r = 1; r < Row; r++) {
                        me = EQS[r];
                        for (int c = 0; c < r; c++) {
                            n++;
                            it = EQS[c];
                            double me_r_c = me.Coefficients[c];
                            double it_r_c = it.Coefficients[c];
                            if (me_r_c == 0d || it_r_c == 0) { m++; continue; }
                            double f = it_r_c / me_r_c;
                            Console.WriteLine("当前方程" + me.ToString() + " 乘 " + f.ToString("f2"));
                            me.MultiplyNumber(f);
                            Console.WriteLine("得到新方程" + me.ToString() + " 再减去方程 " + it.ToString());
                            me.Subtract(it);
                        }
                    }
                } while (m != n);
                Console.WriteLine("上三角");
                foreach (Equation equation in EQS) {
                    Console.WriteLine(equation.ToString());
                }
                ///消成对角线
                ///从倒数第2行开始，消去第N行第N列
                EQS.Sort(new EquationComparer());
                do {
                    m = 0; n = 0;
                    for (int r = Row - 2; r >= 0; r--) {
                        me = EQS[r];

                        for (int c = Col - 1; c > r; c--) {
                            it = EQS[c];
                            n++;
                            double me_r_c = me.Coefficients[c];
                            double it_r_c = it.Coefficients[c];
                            if (me_r_c == 0d || it_r_c == 0) { m++; continue; }
                            double f = it_r_c / me_r_c;
                            me.MultiplyNumber(f);
                            me.Subtract(it);
                        }
                    }
                } while (m != n);
                Console.WriteLine("对角线");
                foreach (Equation equation1 in EQS) {
                    Console.WriteLine(equation1.ToString());
                }
                ///求解
                for (int i = 0; i < Col; i++) {
                    Solver[i] = EQS[i].Vector / EQS[i].Coefficients[i];
                }
            }
            return Solver;
        }
    }
}
