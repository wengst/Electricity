using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectricityDLL {
    /// <summary>
    /// 支路(一头进，一头出)
    /// </summary>
    public class Branch : ElePath {
        /// <summary>
        /// 根据IndexInCircuit从小到大排序
        /// </summary>
        public class BranchSortByIndex : IComparer<Branch> {
            public int Compare(Branch x, Branch y) {
                if (x != null && y != null) {
                    return x.IndexInCircuit - y.IndexInCircuit;
                }
                return 0;
            }
        }

        /// <summary>
        /// 是否是有效支路
        /// <para>在找有效支路时确定。用于在清除节点中的无效支路时不用再for循环ValidBranchs集合</para>
        /// </summary>
        public bool IsValidBranch { get; set; } = false;

        public bool HasDevice() {
            foreach (PathElement path in PathElements) {
                ComponentType t = path.ElementOrWire.Type;
                if (t == ComponentType.Fan || t == ComponentType.Lampstand || t == ComponentType.Resistor || t == ComponentType.Rheostat) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 这个属性用于确定在一个电路中，以初始电源的右接线柱为开始接线柱，通过左接线柱回到右接线柱的有效电路中，支路在其中的顺序。
        /// <para>这是List<Branch>的排序(Sort)依据</Branch></para>
        /// </summary>
        public int IndexInCircuit { get; set; } = -1;

        public AllowLink Allow {
            get {
                Terminal t1 = (Terminal)FirstEle;
                Terminal t2 = (Terminal)LastEle;
                bool NotLeft = t1.HasBranch;
                bool NotRight = t2.HasBranch;
                bool IsStart = t1.Owner.Type == ComponentType.BatteryCase && t1.Key == TerminalKey.Right;
                bool IsEnd = t2.Owner.Type == ComponentType.BatteryCase && t2.Key == TerminalKey.Right;

                if (NotLeft && IsEnd) { return AllowLink.NotAllow; }
                else if (NotRight && IsStart) { return AllowLink.NotAllow; }
                else if (NotLeft && NotRight) { return AllowLink.NotAllow; }
                else if (IsStart && IsEnd) { return AllowLink.NotAllow; }
                else if (NotLeft && !IsEnd && !NotRight) {
                    return AllowLink.Right;
                }
                else if (!NotLeft && !IsStart && NotRight) {
                    return AllowLink.Left;
                }
                else if (!NotLeft && !NotRight && !IsStart && !IsEnd) {
                    return AllowLink.LeftAndRight;
                }
                else {
                    return AllowLink.Right;
                }
            }
        }

        /// <summary>
        /// 该支路后面的支路
        /// </summary>
        public List<Branch> AfterBranchs { get; } = new List<Branch>();

        /// <summary>
        /// 支路前面的支路
        /// </summary>
        public List<Branch> BeforeBranchs { get; } = new List<Branch>();

        /// <summary>
        /// 并联的支路
        /// </summary>
        public List<Branch> Parallels { get; } = new List<Branch>();

        /// <summary>
        /// 支路索引
        /// <para>在进行KCL/KVL计算时，每条支路的电流用I1、I2、I3...表示</para>
        /// <para>而这个Index索引标识就是I下的这个数字</para>
        /// <para>所有有效支路必须有一个Index。并且这个Index必须连续</para>
        /// </summary>
        public int Index { get; set; } = -1;

        /// <summary>
        /// 支路被要求改向次数
        /// <para>被改向次数多少决定该支路是桥支路还是非桥支路。1次以下，非桥支路，2次以上是桥支路</para>
        /// </summary>
        private int reverseCount = 0;

        /// <summary>
        /// 是否被双向连接
        /// </summary>
        public bool IsTwoway { get; set; }

        /// <summary>
        /// 最小电路元素是否可连接到本支路
        /// </summary>
        /// <param name="pe"></param>
        /// <returns></returns>
        public bool IsConnectable(PathElement pe) {
            if (pe != null) {
                if (IsContainPathElement(pe) || pe.B != null) {
                    return false;
                }
                else {
                    if (pe.Right.Id == LastTerminal.Id || pe.Left.Id == LastTerminal.Id) {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 负极连接到的位置
        /// </summary>
        public LinkedArea NeigativeArea {
            get {
                Terminal t1 = (Terminal)FirstEle;
                Terminal t2 = (Terminal)LastEle;
                ComponentType o1 = t1.Owner.Type;
                ComponentType o2 = t2.Owner.Type;
                TerminalKey k1 = t1.Key;
                TerminalKey k2 = t2.Key;
                if (o1 == ComponentType.BatteryCase && k1 == TerminalKey.Left) {
                    return LinkedArea.First;
                }
                else if (o2 == ComponentType.BatteryCase && k2 == TerminalKey.Left) {
                    return LinkedArea.Last;
                }
                else {
                    return LinkedArea.UnKnow;
                }
            }
        }

        /// <summary>
        /// 正极连接到的位置
        /// </summary>
        public LinkedArea PositiveArea {
            get {
                Terminal t1 = (Terminal)FirstEle;
                Terminal t2 = (Terminal)LastEle;
                ComponentType o1 = t1.Owner.Type;
                ComponentType o2 = t2.Owner.Type;
                TerminalKey k1 = t1.Key;
                TerminalKey k2 = t2.Key;
                if (o1 == ComponentType.BatteryCase && k1 == TerminalKey.Right) {
                    return LinkedArea.First;
                }
                else if (o2 == ComponentType.BatteryCase && k2 == TerminalKey.Right) {
                    return LinkedArea.Last;
                }
                else {
                    return LinkedArea.UnKnow;
                }
            }
        }

        /// <summary>
        /// 是否是后串联支路
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool IsAfterSeries(Branch b) {
            //trace(b.IsTwoway);
            if ((b.FirstEle.Id == FirstEle.Id && b.LastEle.Id == LastEle.Id) ||
                (b.FirstEle.Id == LastEle.Id && b.LastEle.Id == FirstEle.Id)) {
                return false;
            }
            else {
                if (!b.IsTwoway) {
                    return b.FirstEle.Id == LastEle.Id;
                }
                else {
                    return b.LastEle.Id == LastEle.Id || b.FirstEle.Id == LastEle.Id;
                }
            }
        }

        /// <summary>
        /// 是否是前串联支路
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool IsBeforeSeries(Branch b) {
            if ((b.FirstEle.Id == FirstEle.Id && b.LastEle.Id == LastEle.Id) ||
                (b.FirstEle.Id == LastEle.Id && b.LastEle.Id == FirstEle.Id)) {
                return false;
            }
            else {
                if (!b.IsTwoway) {
                    return b.LastEle.Id == FirstEle.Id;
                }
                else {
                    return b.LastEle.Id == LastEle.Id || b.FirstEle.Id == LastEle.Id;
                }
            }
        }

        /// <summary>
        /// 是否本支路并联
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool IsParallel(Branch b) {
            bool r = false;
            if ((FirstNode == b.FirstNode && LastNode == b.LastNode) || (FirstNode == b.LastNode && LastNode == b.FirstNode)) {
                r = true;
            }
            else {
                r = false;
            }
            return r;
        }

        /// <summary>
        /// 定义的电流方向
        /// <para>用于基尔霍夫的节点电流KCL方程。对于一个节点，有流入的电流也有流出的电流，流入节点的电流等于流出的电流，即∑入-∑出=0</para>
        /// <para>这个属性的作用就是确定该支路的电流是流入电流还是流出</para>
        /// <para>这个属性在创建KCL时被使用</para>
        /// </summary>
        public int DefinitionDirection { get; set; } = 0;

        public bool IsEqu(Branch branch) {
            List<EleComponent> meObjs = EleComponents.GetRange(0, EleComponents.Count);
            List<EleComponent> itObjs = branch.EleComponents.GetRange(0, branch.EleComponents.Count);
            if (meObjs.ToString() == itObjs.ToString()) {
                return true;
            }
            else {
                meObjs.Reverse();
                string meStr = meObjs.ToString();
                string itStr = itObjs.ToString();
                return meStr == itStr;
            }
        }

        public void SetPathElementBranch() {
            for (int i = 0; i < PathElements.Count; i++) {
                PathElements[i].B = this;
            }
        }

        /// <summary>
        /// 将路径反向
        /// <para>参数changeDirectionCount要求是否计数改向次数。改向次数影响到桥电路的判断</para>
        /// <paramref name="changeDirectionCount"/>
        /// </summary>
        public void Reverse(bool changeDirectionCount = false) {
            
            foreach (PathElement path in PathElements) {
                path.Reverse();
            }
            PathElements.Reverse();
            EleComponents.Reverse();

            if (changeDirectionCount) {
                reverseCount++;
                if (reverseCount >= 1) { IsTwoway = true; }
            }
        }

        public List<double> GetAfter(List<Branch> bs) {
            double[] rs = new double[bs.Count];
            int i; Branch b;

            for (i = 0; i < bs.Count; i++) {
                b = bs[i];
                if (b != this) {
                    if (b.FirstEle.Id == LastEle.Id) {
                        rs[i] = 1;
                    }
                    else if (b.LastEle.Id == LastEle.Id) {
                        rs[i] = -1;
                    }
                }
            }
            return rs.ToList();
        }

        public float GetAfterA() {
            float r = float.NaN;
            foreach (Branch branch in AfterBranchs) {
                if (float.IsNaN(branch.A)) {
                    return float.NaN;
                }
                else {
                    if (float.IsNaN(r)) {
                        r = 0;
                    }
                    r += branch.A;
                }
            }
            return r;
        }

        public float GetBeforeA() {
            float r = float.NaN;
            foreach (Branch branch in BeforeBranchs) {
                if (float.IsNaN(branch.A)) {
                    return float.NaN;
                }
                else {
                    if (float.IsNaN(r)) {
                        r = 0;
                    }
                    r += branch.A;
                }
            }
            return r;
        }

        public List<double> GetBefore(List<Branch> bs) {
            double[] rs = new double[bs.Count];
            int i; Branch b;

            for (i = 0; i < bs.Count; i++) {
                b = bs[i];
                if (b != this) {
                    if (b.LastEle == FirstEle) {
                        rs[i] = 1;
                    }
                    else if (b.FirstEle == FirstEle) {
                        rs[i] = -1;
                    }
                }
            }
            return rs.ToList();
        }

        private bool LeftLink(PathElement p) {
            bool b = false;
            if (p != null) {
                //首接线柱与p.Left或p.Right相同
                if (FirstTerminal.Id == p.Left.Id && p.Left.Junctions.Count == 1) {
                    PathElements.Insert(0, p);
                    EleComponents.Insert(0, p.ElementOrWire);
                    EleComponents.Insert(0, p.Right);
                    p.B = this;
                    b = true;
                }
                else if (FirstTerminal.Id == p.Right.Id && p.Right.Junctions.Count == 1) {
                    PathElements.Insert(0, p);
                    EleComponents.Insert(0, p.ElementOrWire);
                    EleComponents.Insert(0, p.Left);
                    p.B = this;
                    b = true;
                }
                //Console.WriteLine("LeftLink=" + b);
            }
            return b;
        }

        private bool RightLink(PathElement p) {
            bool b = false;
            if (p != null) {
                //首接线柱与p.Left或p.Right相同
                if (LastTerminal.Id == p.Left.Id) {
                    PathElements.Add(p);
                    EleComponents.Add(p.ElementOrWire);
                    EleComponents.Add(p.Right);
                    b = true;
                }
                else if (LastTerminal.Id == p.Right.Id) {
                    PathElements.Add(p);
                    EleComponents.Add(p.ElementOrWire);
                    EleComponents.Add(p.Left);
                    b = true;
                }
            }
            return b;
        }

        public bool Link(PathElement p) {
            return RightLink(p);
        }

        public void SetPathElementUnLoked() {
            foreach (PathElement pathElement in PathElements) {
                pathElement.IsLocked = false;
            }
        }

        /// <summary>
        /// 定义的电流方向
        /// <para>通常情况，预定义的电流方向只有一个，但是对于电路桥，电流的方向是不确定的。实际的电流方向取决于桥两端的电位(电势)</para>
        /// </summary>
        public AD CurrentDirection { get; set; }

        public void SetCurrentDirection(AD ad) {
            if (ad != AD.UnKnow && ad != AD.TwoWay) {
                CurrentDirection |= ad;
            }
        }

        public Branch(PathElement pe) : base(pe) {
            Terminal t1 = FirstTerminal;
            Terminal t2 = LastTerminal;
            int IntF = 0;
            int IntL = Count - 1;

            if ((t1.Owner.Type == ComponentType.BatteryCase && t1.Key == TerminalKey.Right) ||
                (t2.Owner.Type == ComponentType.BatteryCase && t2.Key == TerminalKey.Left) ||
                (t1.HasBranch)
                ) {
                EleComponents[IntF] = t1;
                EleComponents[IntL] = t2;
            }
            else if ((t1.Owner.Type == ComponentType.BatteryCase && t1.Key == TerminalKey.Left) ||
                 (t2.Owner.Type == ComponentType.BatteryCase && t2.Key == TerminalKey.Right) ||
                 (t2.HasBranch)
                ) {
                EleComponents[IntF] = t2;
                EleComponents[IntL] = t1;
            }
        }

        public Branch() { }
    }
}
