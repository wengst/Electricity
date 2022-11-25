using System;
using System.Collections.Generic;

namespace ElectricityDLL {
    public class Circuit : ElePath {
        /// <summary>
        /// 比较器
        /// </summary>
        public class CircuitComparer : IComparer<Circuit> {
            public int Compare(Circuit a, Circuit b) {
                float av = Math.Abs(a.V);
                float bv = Math.Abs(b.V);
                if (av > bv) {
                    return -1;
                }
                else if (av < bv) {
                    return 1;
                }
                else {
                    if (a.ValidBranchCount > b.ValidBranchCount) {
                        return -1;
                    }
                    else if (a.ValidBranchCount < b.ValidBranchCount) {
                        return 1;
                    }
                    else {
                        return 0;
                    }
                }
            }
        }
        /// <summary>
        /// 节点数
        /// </summary>
        public int NodeCount { get { return Nodes.Count; } }

        public int DeviceCount {
            get {
                int n = 0;
                foreach (PathElement path in PathElements) {
                    if (path.ElementOrWire.IsDevice) { n++; }
                }
                return n;
            }
        }

        public int PowerCount {
            get {
                int n = 0;
                foreach (PathElement path in PathElements) {
                    if (path.IsPower) { n++; }
                }
                return n;
            }
        }

        public CircuitGroup SCG { get; set; } = null;

        public List<Terminal> BatteryCaseTerminals { get; } = new List<Terminal>();

        public List<Branch> Branchs { get; } = new List<Branch>();

        /// <summary>
        /// 有效支路集合
        /// </summary>
        public List<Branch> ValidBranchs {
            get {
                List<Branch> bs = new List<Branch>();
                foreach (Branch branch in Branchs) {
                    if (branch.Index != -1 && branch.FirstNode != branch.LastNode) {
                        Fun.AddPath(branch, bs);
                    }
                }
                return bs;
            }
        }

        public List<int> VCBIndexs {
            get {
                List<int> Indexs = new List<int>();
                foreach (Branch branch in ValidBranchs) {
                    Indexs.Add(branch.Index);
                }
                return Indexs;
            }
        }

        /// <summary>
        /// 有效支路数
        /// </summary>
        public int ValidBranchCount {
            get { return ValidBranchs.Count; }
        }

        /// <summary>
        /// 不相同索引的数量
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public int NonEqualIndexCount(Circuit cl) {
            int r = 0;
            if (cl != null) {
                List<int> meIndexs = VCBIndexs;
                List<int> clIndexs = cl.VCBIndexs;
                foreach (int m in meIndexs) {
                    foreach (int c in clIndexs) {
                        if (m == c) { r++; }
                    }
                }
            }
            return r;
        }

        public bool HasBatteryCaseFullLoop(BatteryCase bc) {
            if (FirstEle.Owner.Type == ComponentType.BatteryCase
                && FirstEle.Owner.Id == bc.Id
                && LastEle.Owner.Type == ComponentType.BatteryCase
                && LastEle.Owner.Id == bc.Id
                ) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 是否是完整的电路
        /// <para>完整电路的特征是由电源的右接线柱开始，回到了电源的左接线柱</para>
        /// </summary>
        public bool IsFullLoop {
            get {
                int id1 = FirstTerminal.Id, id2 = LastTerminal.Id, eid1 = FirstTerminal.Owner.Id, eid2 = SecondLastTerminal.Owner.Id;
                return (id1 == id2 && eid1 == eid2);
            }
        }

        /// <summary>
        /// 电路的第一个元件与最后一个是否相同
        /// </summary>
        public bool HasCloseLoop {
            get {
                return LastEle.Id == FirstEle.Id;
            }
        }

        public bool HasHalfway {
            get {
                int i, j;
                List<Node> ns = new List<Node>();
                for (i = 0; i < Count; i += 2) {
                    if (EleComponents[i].Type == ComponentType.Terminal) {
                        Terminal t = (Terminal)EleComponents[i];
                        Node n = t.JoinedNode;
                        if (ns.Count == 0 || ns[ns.Count - 1].Id != n.Id) {
                            ns.Add(n);
                        }
                    }
                }

                for (i = 0; i < ns.Count - 2; i++) {
                    Node n0 = ns[i];
                    for (j = i + 1; j < ns.Count - 1; j++) {
                        Node n1 = ns[j];
                        if (j != ns.Count - 1 && n1.Id == n0.Id) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 回路中的部分元件是否存在被短路的情况
        /// </summary>
        public bool HasSectionShort {
            get {
                for (int i = 1; i < Count - 1; i += 2) {
                    if (EleComponents[i - 1].Type == ComponentType.Terminal && EleComponents[i + 1].Type == ComponentType.Terminal) {
                        Terminal t1 = (Terminal)EleComponents[i - 1];
                        Terminal t2 = (Terminal)EleComponents[i + 1];
                        EleComponent e = EleComponents[i];
                        float r = Fun.GetResistance(e, t1, t2, IsIdeal);
                        if (r > 0 && t1.JoinedNode != null && t2.JoinedNode != null && t1.JoinedNode == t2.JoinedNode) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 是否电源短路
        /// </summary>
        public bool IsPowerShort {
            get {
                return Math.Abs(V) > 0 && IsShort;
            }
        }

        /// <summary>
        /// 是否短路
        /// </summary>
        public bool IsShort {
            get {
                return R == 0;
            }
        }

        /// <summary>
        /// 是否存在双向支路
        /// </summary>
        public bool IsTwoway {
            get {
                foreach (Branch branch in Branchs) {
                    if (branch.IsTwoway) { return true; }
                }
                return false;
            }
        }

        public List<EleComponent> BothSideObjs(Terminal t) {
            List<EleComponent> r = new List<EleComponent>();
            int index = Fun.GetObjIndex(t, EleComponents);
            if (index == 0) {
                r.Add(EleComponents[1]);
                r.Add(EleComponents[Count - 2]);
            }
            else if (index == Count - 1) {
                r.Add(EleComponents[Count - 2]);
                r.Add(EleComponents[1]);
            }
            else if (index != -1) {
                r.Add(EleComponents[index - 1]);
                r.Add(EleComponents[index + 1]);
            }
            return r;
        }

        public Circuit Clone() {
            Circuit c = new Circuit();
            c.PathElements.AddRange(PathElements);
            c.EleComponents.AddRange(EleComponents);
            c.SCG = SCG;
            return c;
        }

        public Circuit() { }

        public Circuit(CircuitGroup cg, List<EleComponent> list) {
            SCG = cg;
            if (list != null) {
                EleComponents.AddRange(list);
            }
        }

        public Circuit(CircuitGroup cg, PathElement pe) {
            if (pe != null) {
                PathElements.Add(pe);
                EleComponents.Add(pe.Left);
                EleComponents.Add(pe.ElementOrWire);
                EleComponents.Add(pe.Right);
            }
            SCG = cg;
        }

        public Circuit(PathElement pe) {
            if (pe != null) {
                PathElements.Add(pe);
                EleComponents.Add(pe.Left);
                EleComponents.Add(pe.ElementOrWire);
                EleComponents.Add(pe.Right);
            }
        }

        public void MoveToMaxPaths() {
            int i, index, max;
            max = 0;
            Terminal t;
            index = 0;
            for (i = 0; i < Count; i += 2) {
                t = (Terminal)(EleComponents[i]);
                if (t.BothSideObjCount > max) {
                    index = i;
                    max = t.BothSideObjCount;
                }
            }
            Movement(index);
        }

        /// <summary>
        /// 获取电路中的电压总和
        /// </summary>
        public float V {
            get {
                float r = 0f;
                foreach (PathElement path in PathElements) {
                    if (path.ElementOrWire.Type == ComponentType.BatteryCase) {
                        BatteryCase bc = (BatteryCase)path.ElementOrWire;
                        if (path.Left.Key == TerminalKey.Right && path.Right.Key == TerminalKey.Left) {
                            r += bc.Voltage;
                        }
                        else {
                            r -= bc.Voltage;
                        }
                    }
                }
                return r;
            }
        }

        /// <summary>
        /// 平移，即将index前的对象平移到对象数组的最后面
        /// </summary>
        /// <param name="index"></param>
        public void Movement(int index) {
            if (index <= 0 || index >= Count - 1 || (index % 2) == 1) return;
            List<EleComponent> objs = EleComponents.GetRange(0, index);
            //objs.Add(EleComponents[0]);
            //Link(new ElePath(objs));
        }

        public Branch GetSection(int begin = -1, int end = -1) {
            Branch b = new Branch();
            List<EleComponent> objs = new List<EleComponent>();
            if (begin == -1) { begin = 0; }
            if (end == -1) { end = Count - 1; }
            for (int i = begin; i <= end; i++) {
                objs.Add(EleComponents[i]);
            }
            b.EleComponents.AddRange(objs);
            return b;
        }

        /// <summary>
        /// 是否存在节点交叉的情况。一条正常的主干回路，只有起始节点和结束节点相同，中间节点不应回到以前的节点
        /// </summary>
        public bool HasNodeCross {
            get {
                int i;
                int currentIndex;
                int lastIndex = -1;
                List<Node> ns = Nodes;
                int smallers = 0;
                Terminal t;

                for (i = 0; i < Count; i += 2) {
                    t = (Terminal)(EleComponents[i]);
                    currentIndex = Fun.GetNodeIndex(t.JoinedNode, ns);
                    if (currentIndex > lastIndex) {
                        lastIndex = currentIndex;
                    }
                    else if (currentIndex < lastIndex) {
                        lastIndex = currentIndex;
                        smallers++;
                    }
                }
                if (smallers <= 1) {
                    return false;
                }
                else {
                    return true;
                }
            }
        }

        /// <summary>
        /// 是否等效
        /// </summary>
        /// <param name="cl"></param>
        /// <returns></returns>
        public bool IsEquivalent(Circuit cl) {
            List<Branch> meValidCBs = ValidBranchs;
            List<Branch> clValidCBs = cl.ValidBranchs;
            int n = 0;
            for (int i = 0; i < meValidCBs.Count; i++) {
                for (int j = 0; j < clValidCBs.Count; j++) {
                    if (clValidCBs[j] == meValidCBs[i]) {
                        n++;
                    }
                }
            }
            if (meValidCBs.Count == n && clValidCBs.Count == meValidCBs.Count) {
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// 是否可连接
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsConnectable(PathElement p) {
            if (p != null) {
                if (IsContainPathElement(p)) {
                    return false;
                }
                else {
                    if (p.Right.Id == LastTerminal.Id || p.Left.Id == LastTerminal.Id) {
                        return true;
                    }
                }
            }
            return false;
        }

        private float GetObjCurrent(EleComponent obj) {
            int i = 0; int j = 0;
            for (i = 0; i < Branchs.Count; i++) {
                for (j = 1; j < Branchs[i].Count; j += 2) {
                    if (Branchs[i].EleComponents[j] == obj) {
                        return Branchs[i].A;
                    }
                }
            }
            return float.NaN;
        }

        /// <summary>
        /// 按路径方向计算节点电位
        /// </summary>
        /// <param name="index"></param>
        private void ComputePotentialForward(int index) {
            int i;
            Terminal t1, t2;
            EleComponent obj;
            float r, a;
            ComponentType type;

            for (i = index + 1; i < Count; i += 2) {
                t1 = (Terminal)(EleComponents[i - 1]);
                t2 = (Terminal)(EleComponents[i + 1]);
                obj = EleComponents[i];
                type = obj.Type;
                if (type != ComponentType.BatteryCase && t2.JoinedNode != null && t2.JoinedNode != t1.JoinedNode && float.IsNaN(t2.JoinedNode.Potential) && !float.IsNaN(t1.JoinedNode.Potential)) {
                    r = Fun.GetResistance(obj, t1, t2, IsIdeal);
                    a = GetObjCurrent(obj);
                    if (r != 0 && (r < float.MaxValue || float.IsNaN(r)) && !float.IsNaN(a) && Math.Abs(a) > 0) {
                        t2.JoinedNode.Potential = t1.JoinedNode.Potential - a * r;
                    }
                }
                else if (type == ComponentType.BatteryCase && float.IsNaN(t2.JoinedNode.Potential)) {
                    if (t1.Key == TerminalKey.Left && t2.Key == TerminalKey.Right) {
                        t2.JoinedNode.Potential = t1.JoinedNode.Potential + ((BatteryCase)(obj)).Voltage;
                    }
                    else if (t1.Key == TerminalKey.Right && t2.Key == TerminalKey.Left) {
                        t2.JoinedNode.Potential = t1.JoinedNode.Potential - ((BatteryCase)(obj)).Voltage;
                    }
                }
            }
        }

        /// <summary>
        /// 反路径方向计算节点电位
        /// </summary>
        /// <param name="index"></param>
        private void ComputePotentialBackward(int index) {
            int i;
            Terminal t1, t2;
            EleComponent obj;
            float r, a;
            ComponentType type;
            for (i = index - 1; i >= 0; i -= 2) {
                t1 = (Terminal)(EleComponents[i + 1]);
                t2 = (Terminal)(EleComponents[i - 1]);
                obj = EleComponents[i];
                type = obj.Type;
                if (type != ComponentType.BatteryCase && t2.JoinedNode.Id != t1.JoinedNode.Id && float.IsNaN(t2.JoinedNode.Potential) && !float.IsNaN(t1.JoinedNode.Potential)) {
                    r = Fun.GetResistance(obj, t1, t2, IsIdeal);
                    a = GetObjCurrent(obj);
                    if (r != 0 && (r < float.MaxValue || !float.IsNaN(r)) && !float.IsNaN(a) && a != 0) {
                        t2.JoinedNode.Potential = t1.JoinedNode.Potential + a * r;
                    }
                }
                else if (type == ComponentType.BatteryCase && float.IsNaN(t2.JoinedNode.Potential)) {
                    if (t1.Key == TerminalKey.Left && t2.Key == TerminalKey.Right) {
                        t2.JoinedNode.Potential = t1.JoinedNode.Potential - ((BatteryCase)(obj)).Voltage;
                    }
                    else if (t1.Key == TerminalKey.Right && t2.Key == TerminalKey.Left) {
                        t2.JoinedNode.Potential = t1.JoinedNode.Potential + ((BatteryCase)(obj)).Voltage;
                    }
                }
            }
        }

        /// <summary>
        /// 计算节点电位（只有完整电路才计算节点电位）
        /// </summary>
        public void ComputeNodePotential() {

            int index = 0;
            Terminal t1;

            for (int i = 0; i < Count; i += 2) {
                t1 = (Terminal)(EleComponents[i]);
                if (t1.JoinedNode.Potential == 0) { index = i; break; }
            }
            //trace("ComputeNodePotential index="+index.toString());
            if (index == 0) {
                /*零节点位于回路的起始，电流沿回路流动，即后续节点电位小于零*/
                ComputePotentialForward(index);
            }
            else if (index == Count - 1) {
                /*零节点位于回路的终点，电流沿回路流动到此，即前面节点电位大于零*/
                ComputePotentialBackward(index);
            }
            else {
                ComputePotentialForward(index);
                ComputePotentialBackward(index);
            }
        }

        private int GetTerminalIndex(Terminal t) {
            for (int i = 0; i < EleComponents.Count; i++) {
                if (EleComponents[i].Id == t.Id) { return i; }
            }
            return -1;
        }

        /// <summary>
        /// 纠正支路方向
        /// </summary>
        public void CorrectBranchDirection() {
            for (int i = 0; i < Branchs.Count; i++) {
                int n1 = GetTerminalIndex(Branchs[i].FirstTerminal);
                int n2 = GetTerminalIndex(Branchs[i].LastTerminal);
                if (n2 < n1) {
                    Branchs[i].Reverse();
                }
            }
        }

        public void Link(PathElement p) {
            if (p != null) {
                //首接线柱与p.Left或p.Right相同
                if (LastTerminal.Id == p.Left.Id) {
                    PathElements.Add(p);
                    EleComponents.Add(p.ElementOrWire);
                    EleComponents.Add(p.Right);
                }
                else if (LastTerminal.Id == p.Right.Id) {
                    PathElements.Add(p);
                    EleComponents.Add(p.ElementOrWire);
                    EleComponents.Add(p.Left);
                }
            }
        }

        /// <summary>
        /// 获取支路在电路中的位置索引
        /// <para>该索引值只是支路路径字符串在电路路径支字符串中首次出现的位置</para>
        /// </summary>
        /// <param name="branch"></param>
        /// <returns>支路路径字符串在电路路径支字符串中首次出现的位置</returns>
        public int GetBranchIndex(Branch branch) {
            string s = branch.PathStr;
            string rs = branch.ReversePathStr;
            string me = PathStr;
            int index = me.IndexOf(s);
            if (index == -1) {
                index = me.IndexOf(rs);
            }
            if (index != -1) {
                branch.IndexInCircuit = index;
            }
            return index;
        }

        /// <summary>
        /// 获取电流方向
        /// </summary>
        /// <para>如果支路的开始接线柱与前一支路的最后接线柱相同，返回1</para>
        /// <para>如果支路的最后接线柱与前一支路的最后接线柱相同，返回-1</para>
        /// <para>其他情况返回1</para>
        /// <param name="b"></param>
        /// <returns></returns>
        public int GetCurrentDirection(Branch b) {
            bool e = false;
            int k = 0;
            for (int i = 0; i < Branchs.Count; i++) {
                Branch branch = Branchs[i];
                if (branch.Id == b.Id) { e = true; k = i; break; }
            }
            if (e) {
                if (k > 0) {
                    Branch bb = Branchs[k - 1];
                    if (bb.LastTerminal.Id == b.FirstTerminal.Id) {
                        return 1;
                    }
                    else if (bb.LastTerminal.Id == b.LastTerminal.Id) {
                        return -1;
                    }
                }
            }
            return 1;
        }

        #region static function
        public static bool RemoveCircuitLoop(Circuit cl, List<Circuit> cls) {
            bool r = false;
            for (int i = cls.Count - 1; i >= 0; i--) {
                if (cls[i].IsEqual(cl)) {
                    r = true;
                    cls.RemoveAt(i);
                    break;
                }
            }
            return r;
        }
        #endregion
    }
}
