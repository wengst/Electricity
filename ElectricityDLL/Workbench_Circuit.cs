using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ElectricityDLL {
    public partial class Workbench {
        #region public fields
        /// <summary>
        /// 自增整型，用于给元件赋Id
        /// </summary>

        /// <summary>
        /// 电路连接是否发生改变
        /// </summary>
        [JsonIgnore]
        public bool IsCircuitChanged { get; set; } = false;
        /// <summary>
        /// 程序当前迭代的深度
        /// </summary>
        [JsonIgnore]
        int deepIndex = 0;

        /// <summary>
        /// 程序最多进行迭代调用的深度，超过这个深度表示程序很可能陷入了死循环，必须无条件终止循环
        /// </summary>
        [JsonIgnore]
        int maxDeep = 100;

        /// <summary>
        /// 最多电路数
        /// </summary>
        [JsonIgnore]
        int overCircuits = 50;

        /// <summary>
        /// 有效电路数
        /// </summary>
        [JsonIgnore]
        private int VCC { get { return ValidCircuits.Count; } }

        /// <summary>
        /// 方程回路数
        /// </summary>
        [JsonIgnore]
        private int ECC { get { return EquationCircuits.Count; } }

        /// <summary>
        /// 有效支路数
        /// </summary>
        [JsonIgnore]
        private int VBC { get { return ValidBranchs.Count; } }

        /// <summary>
        /// 方程支路数
        /// </summary>
        [JsonIgnore]
        private int EBC { get { return EquationBranchs.Count; } }

        /// <summary>
        /// 获取电路状态
        /// </summary>
        /// <returns></returns>
        public CT GetCircuitState() {
            CT ct = CT.UnKnow;
            Circuit circuit;
            if (VCC == 1) {
                circuit = ValidCircuits[0];
                if (circuit.PowerCount > 0) {
                    if (float.IsNaN(circuit.R) || float.IsInfinity(circuit.R)) {
                        ct = CT.AllOpen;
                    }
                    else if (Consts.IsZero(circuit.R)) {
                        ct = CT.PowerShort;
                    }
                    else {
                        if (circuit.DeviceCount == 1) {
                            ct = CT.Base;
                        }
                        else {
                            ct = CT.Series;
                        }
                    }
                }
            }
            else {
                int battaryCount = 0;
                bool zeroC = false;
                int openCount = 0;
                foreach (Circuit c in ValidCircuits) {
                    battaryCount += c.PowerCount;
                    if (Consts.IsZero(c.R)) {
                        zeroC = true;
                    }
                    if (float.IsNaN(c.R) || float.IsInfinity(c.R)) {
                        openCount++;
                    }
                }
                if (battaryCount > 0) {
                    if (zeroC) {
                        ct = CT.PowerShort;
                    }
                    else if (openCount == VCC) {
                        ct = CT.AllOpen;
                    }
                    else {
                        if (HasBridge) {
                            ct = CT.Bridge;
                        }
                        else if (hasSeriesParallel()) {
                            ct = CT.Mix;
                        }
                        else {
                            ct = CT.Parallel;
                        }
                    }
                }
            }

            return ct;
        }

        public CT CircuitType {
            get {
                CT c = CT.UnKnow;
                int cc = EquationCircuits.Count;
                int bc = EquationBranchs.Count;
                int b = 0, k = 0, o = 0;
                foreach (EleComponent ele in Items) {
                    switch (ele.Type) {
                        case ComponentType.BatteryCase:
                            b++;
                            break;
                        case ComponentType.Switch:
                            k++;
                            break;
                        case ComponentType.Lampstand:
                        case ComponentType.Resistor:
                        case ComponentType.Rheostat:
                        case ComponentType.Fan:
                            o++;
                            break;
                    }
                }
                int n = 0;
                if (cc > 0) {
                    foreach (Circuit circuit in EquationCircuits) {
                        if (circuit.R == 0f) {
                            c = CT.PowerShort; break;
                        }
                        else if (float.IsNaN(circuit.R)) { n++; }
                    }
                }
                if (n == cc) {
                    c = CT.AllOpen;
                }
                else if (bc == 1) {
                    if (cc == 1) {
                        if (b == 1 && o == 1 && k == 1) {
                            c = CT.Base;
                        }
                        else {
                            c = CT.Series;
                        }
                    }
                }
                else if (bc > 1) {
                    c = CT.Parallel;
                    if (HasBridge) {
                        c = CT.Bridge;
                    }
                    else if (hasSeriesParallel()) {
                        c = CT.Mix;
                    }
                }

                return c;
            }
        }

        ///<summary>
        /// 是否存在并联的支路与另一并联的支路串联的电路
        ///</summary>
        private bool hasSeriesParallel() {
            if (Branchs == null || Branchs.Count < 3) return false;
            foreach (Branch b1 in Branchs) {
                if (b1.Parallels != null && b1.Parallels.Count > 1 && b1.AfterBranchs != null && b1.AfterBranchs.Count > 1) {
                    foreach (Branch b2 in b1.Parallels) {
                        if (b2.AfterBranchs != null && b2.AfterBranchs.Count > 1) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 有效接线柱
        /// </summary>
        [JsonIgnore]
        private List<Terminal> EfficientTerminals { get; } = new List<Terminal>();

        /// <summary>
        /// 是否存在桥电路
        /// </summary>
        [JsonIgnore]
        public bool HasBridge {
            get {
                bool r = false;
                for (int i = 0; i < ValidBranchs.Count; i++) {
                    if (Branchs[i].CurrentDirection == AD.TwoWay) {
                        r = true;
                        break;
                    }
                }
                return r;
            }
        }

        public List<Branch> GetBridge() {
            List<Branch> list = new List<Branch>();
            for (int i = 0; i < ValidBranchs.Count; i++) {
                if (ValidBranchs[i].CurrentDirection == AD.TwoWay) {
                    list.Add(ValidBranchs[i]);
                }
            }
            return list;
        }
        #endregion

        #region Terminals
        public void ComputeTerminalPaths() {
            List<EleComponent> objs, tmpObjs;
            foreach (Terminal t in EfficientTerminals) {
                objs = new List<EleComponent>();
                foreach (Circuit circuit in Circuits) {
                    tmpObjs = circuit.BothSideObjs(t);
                    if (tmpObjs != null) {
                        foreach (EleComponent ele in tmpObjs) {
                            Fun.AddEleComponents(ele, objs);
                        }
                    }
                }
                t.BothSideObjCount = objs.Count;
                //trace(t.FullName+"接线柱的前后对象数="+objs.length.toString());
            }
        }
        #endregion

        #region Nodes
        [JsonIgnore]
        private List<Node> _nodes = new List<Node>();
        /// <summary>
        /// 全部节点
        /// <para>每个接线柱上LinkedNode的唯一集合</para>
        /// </summary>
        [JsonIgnore]
        internal List<Node> Nodes {
            get {
                if (IsCircuitChanged) {
                    _nodes.Clear();
                    foreach (Circuit circuit in Circuits) {
                        foreach (Node node in circuit.Nodes) {
                            Fun.AddNode(node, _nodes);
                        }
                    }
                }
                IsCircuitChanged = false;
                return _nodes;
            }
        }

        /// <summary>
        /// 有效节点。即在有效电路（ValidCircuits）中的节点
        /// </summary>
        [JsonIgnore]
        private List<Node> ValidNodes { get; } = new List<Node>();

        /// <summary>
        /// 清除所有节点
        /// </summary>
        internal void ClearNodes() {
            _nodes.Clear();
            SetTerminalJoinedNodeEquNull();
        }

        /// <summary>
        /// 向Nodes添加节点
        /// </summary>
        /// <param name="n"></param>
        private void AddNode(Node n, List<Node> ns) {
            bool b = false;
            foreach (Node node in ns) {
                if (node.Id == n.Id) {
                    b = true;
                }
            }
            if (!b) {
                ns.Add(n);
            }
        }

        /// <summary>
        /// 从Nodes中移除节点指定节点
        /// </summary>
        /// <returns>剩余具有相同</returns>
        /// <param name="n"></param>
        internal void RemoveNode(Node n) {
            for (int i = Nodes.Count - 1; i >= 0; i--) {
                if (Nodes[i].Id == n.Id) {
                    Nodes.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// 移除无效节点
        /// </summary>
        private void RemoveInvalidNodes() {
            int i, j;
            Terminal t;
            Node node;
            for (i = Nodes.Count - 1; i >= 0; i--) {
                node = Nodes[i];
                for (j = node.TerminalCount - 1; j >= 0; j--) {
                    t = node.GetTerminalAt(j);
                    if (Fun.GetEleIndex(t, EfficientTerminals) == -1) {
                        node.Terminals.RemoveAt(j);
                    }
                }
                if (node.TerminalCount == 0) {
                    Nodes.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 通过有效电路集合中的电路查找有效的节点，把找到的节点放入ValidNodes集合
        /// <para>此方法会先清空ValidNodes集合</para>
        /// </summary>
        private void FindValidNodes() {
            ValidNodes.Clear();
            foreach (Circuit circuit in ValidCircuits) {
                foreach (EleComponent ele in circuit.EleComponents) {
                    if (ele.Type == ComponentType.Terminal) {
                        Terminal t = (Terminal)ele;
                        if (t.JoinedNode != null) {
                            Fun.AddNode(t.JoinedNode, ValidNodes);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 遍历所有接线柱，将JoinedNode属性为n1的替换成n2
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        internal void ReplaceNode(Node n1, Node n2) {
            foreach (Terminal terminal in Terminals) {
                if (terminal.JoinedNode.Id == n1.Id) {
                    terminal.JoinedNode = n2;
                }
            }
        }

        private void reset() {
            Nodes.Clear();
            List<Element> elements = new List<Element>();
            elements.AddRange(BCs);
            foreach (BatteryCase batteryCase in BCs) {
                batteryCase.SCG = null;
            }
            foreach (Element item in Elements) {
                item.Reset();
            }
            foreach (Wire wire in Wires) {
                wire.ResetAssumeDirection();
                wire.CurrentStrength = 0f;
                wire.Stat = WorkStat.StopOrOpen;
            }
            CircuitGroups.Clear();
            Draw_Paint(null, null);
        }

        [JsonIgnore]
        private string nodeName {
            get {
                string s = "N";
                if (Nodes.Count == 0) {
                    s += "0";
                }
                else {
                    string tmp = Nodes[Nodes.Count - 1].Name;
                    tmp = tmp.Substring(1, tmp.Length - 1);
                    s += ((int.Parse(tmp)) + 1).ToString();
                }
                return s;
            }
        }

        private void SetNode(Terminal t, Node value) {

            string n1Name = "", nName = "";

            if (t.JoinedNode == null) {
                value.AddTerminal(t);
                t.JoinedNode = value;
            }
            if (t.JoinedNode != null) { nName = t.JoinedNode.Name; }

            int i;
            Terminal t1; Node node;
            /*邻居接线柱一定位于同一节点*/
            List<Junction> jts = t.Junctions;
            //trace("接线柱t的接头数="+jts.length.toString());
            Junction jt;
            for (i = 0; i < jts.Count; i++) {
                jt = jts[i];
                t1 = jt.AnotherSideTerminal();
                //trace("接头"+jt.name+"的接线柱为"+t1.FullName+",导线故障="+jt.Parent.Fault.toString());
                if (t1 != null && jt.Owner.Fault == FaultType.无) {
                    n1Name = "null";
                    if (t1.JoinedNode != null) {
                        n1Name = t1.JoinedNode.Name;
                    }
                    //trace(i.toString()+"设置邻居接线柱节点，接线柱(t)"+t.FullName+"与接线柱(t1)"+t1.FullName+"被一段导线连接。t的CurrentNode="+nName+" , t1的CurrentNode="+n1Name);
                    if (t1.JoinedNode == null) {
                        SetNode(t1, value);
                    }
                    else if (t1.JoinedNode != value) {
                        Fun.RemoveTerminal(t1, t1.JoinedNode.Terminals);
                        t1.JoinedNode = null;
                        SetNode(t1, value);
                    }
                }
                else if (t1 != null && jt.Owner.Fault == FaultType.断路) {
                    //trace(t1.FullName);
                    if (t1.JoinedNode == null) {
                        node = new Node(t1);
                        node.Name = nodeName;
                        Fun.AddNode(node, Nodes);
                        SetNode(t1, node);
                    }
                }
            }
            /*同元件的接线柱不一定不在同一节点*/

            List<Terminal> jxzs = t.OtherTerminals;
            for (i = 0; i < jxzs.Count; i++) {
                t1 = jxzs[i];
                float rsistance = ((Element)t.Owner).GetRsistance(t, t1);
                n1Name = "null";
                if (t1.JoinedNode != null) {
                    n1Name = t1.JoinedNode.Name;
                }
                //trace(i.toString()+"设置同元件接线柱节点，接线柱(t)"+t.FullName+"与(t1)"+t1.FullName+"之间的电阻="+rsistance.toString()+",t的CurrentNode="+nName+" , t1的CurrentNode="+n1Name);
                if (rsistance == 0 && t1.Owner.Type != ComponentType.BatteryCase) {
                    if (t1.JoinedNode != null && t1.JoinedNode != value) {
                        Fun.RemoveTerminal(t1, t1.JoinedNode.Terminals);
                        t1.JoinedNode = null;
                    }
                    if (t1.JoinedNode == null) {
                        SetNode(t1, value);
                    }
                }
                else {
                    if (t1.JoinedNode == null) {
                        node = new Node(t1);
                        node.Name = nodeName;
                        Fun.AddNode(node, Nodes);
                        SetNode(t1, node);
                    }
                }
            }
        }

        /// <summary>
        /// 迭代找等电位节点
        /// </summary>
        private void IterateFindNodes(Terminal t0) {
            List<Terminal> UnJoinedTerminals = GetUnJoinTerminals();
            for (int i = 0; i < UnJoinedTerminals.Count; i++) {
                Terminal t1 = UnJoinedTerminals[i];
                if (t1.JoinedNode == null && t1.Junctions.Count > 0 && EleComponent.IsEquPotential(t0, t1)) {
                    t0.JoinedNode.AddTerminal(t1);
                    IterateFindNodes(t1);
                }
            }
        }

        /// <summary>
        /// 设置所有接线柱的JoinedNode属性为null
        /// </summary>
        private void SetTerminalJoinedNodeEquNull() {
            foreach (Terminal terminal in Terminals) {
                terminal.JoinedNode = null;
                terminal.Potential = float.NaN;
            }
        }

        /// <summary>
        /// 合并等电位的接线柱，把这些接线柱的JoinedNode置为相同
        /// </summary>
        private void MergeEquPotentialTerminals() {
            foreach (Terminal t in Terminals) {
                if (t.JoinedNode == null && t.Junctions.Count > 0) {
                    t.JoinedNode = new Node(t);
                    IterateFindNodes(t);
                }
            }
        }

        private void ClearUpNodes() {
            for (int i = Nodes.Count - 1; i >= 0; i--) {
                Node node = Nodes[i];
                foreach (Circuit circuit in Circuits) {

                }
            }
        }

        /// <summary>
        /// 把支路数最多的节点电位设置为零
        /// </summary>
        public void DelimitZeroPotential() {
            if (Nodes.Count > 0) {
                int i, j, index, maxb, n;
                maxb = 0;
                Node node; Branch b;
                index = 0;
                for (i = 0; i < Nodes.Count; i++) {
                    node = Nodes[i];
                    n = 0;
                    for (j = 0; j < ValidBranchs.Count; j++) {
                        b = ValidBranchs[j];
                        if ((b.LastNode == node || b.FirstNode == node) && b.R > 0 && !float.IsNaN(b.R)) {
                            n++;
                        }
                    }
                    if (n > maxb) {
                        index = i;
                        maxb = n;
                    }
                }
                Nodes[index].Potential = 0;
            }
        }
        #endregion

        #region PathElements
        [Browsable(false)]
        [JsonIgnore]
        public List<PathElement> PathElements { get; } = new List<PathElement>();

        /// <summary>
        /// 构建最短的电路段集合
        /// </summary>
        private void BuildPathElements() {

            PathElements.Clear();

            foreach (Element element in Elements) {
                PathElements.AddRange(element.GetPathElements());
            }
            foreach (Wire wire in Wires) {
                PathElement p = wire.PathElement;
                if (p != null) {
                    PathElements.Add(p);
                }
            }
            //List<Terminal> ls = new List<Terminal>();
            //查找所有有导线连接的接线柱
            //foreach (Terminal t1 in Terminals) {
            //    if (t1.Junctions.Count > 0) {
            //        ls.Add(t1);
            //    }
            //}
            //foreach (Terminal t2 in ls) {
            //    List<PathElement> pes = t2.GetAllPathElement();
            //    foreach (PathElement pathElement in pes) {
            //        bool b = false;
            //        foreach (PathElement pe in PathElements) {
            //            if (pe.IsEqual(pathElement)) { b = true; }
            //        }
            //        if (!b) {
            //            pathElement.B = null;
            //            if (pathElement.Left.Owner.Type == ComponentType.BatteryCase && pathElement.Left.Key == TerminalKey.Right) {
            //                PathElements.Insert(0, pathElement);
            //            }
            //            else {
            //                PathElements.Add(pathElement);
            //            }
            //        }
            //    }
            //}
        }
        #endregion

        #region Branchs
        [Browsable(false)]
        [JsonIgnore]
        private List<Branch> Branchs { get; } = new List<Branch>();

        [JsonIgnore]
        private List<Branch> ValidBranchs { get; } = new List<Branch>();

        /// <summary>
        /// 有效支路，用于电路计算
        /// <para>有效支路是指支路在有效的回路中，且电阻不为零，也不为无穷大</para>
        /// </summary>
        [JsonIgnore]
        private List<Branch> EquationBranchs { get; } = new List<Branch>();

        /// <summary>
        /// 电路元素是否已经包含到找到的支路中
        /// </summary>
        /// <param name="pe"></param>
        /// <returns></returns>
        private bool PathElementInBranchs(PathElement pe) {
            for (int i = 0; i < Branchs.Count; i++) {
                if (Branchs[i].IsContainPathElement(pe)) {
                    return true;
                }
            }
            return false;
        }

        private void FindBranch(Branch b) {

            if (deepIndex >= maxDeep) {
                throw new Exception("连接的电路导致程序陷入了无限循环");
            }

            if (b == null || b.LastPathElement == null) return;
            AllowLink allow = b.Allow;
            bool fj = b.IsLinkedNegativeElectrode;
            bool zj = b.IsLinkedPositiveElectrode;
            if (allow == AllowLink.NotAllow) {
                WriteLine("※※※※{" + b.ToString() + "}已经不可再连接支路，添加到支路集合");
                AddBranchs(b);
            }
            WriteLine("{" + b.ToString() + "，Allow=" + allow.ToString() + "，负极=" + fj + "，正极=" + zj);


            List<PathElement> ls = GetConnectables(b);
            Branch b2 = null;
            WriteLine("找到支路" + ls.Count + "条，分别是：");
            for (int i = 0; i < ls.Count; i++) {
                WriteLine(i.ToString() + " = {" + ls[i].ToString() + "}");
            }
            if (deepIndex == 20) return;
            foreach (PathElement path in ls) {
                if (path.B == null) {
                    if (allow != AllowLink.NotAllow) {
                        WriteLine("{" + b.ToString() + "} 连接路径元素{" + path.ToString() + "}");
                        b.Link(path);
                        WriteLine("===>FindBranch({" + b.ToString() + "}) deep=" + deepIndex.ToString());
                        deepIndex++;
                        FindBranch(b);
                        deepIndex--;
                        WriteLine("<===FindBranch({" + b.ToString() + "}) deep=" + deepIndex.ToString());
                    }
                    else {
                        b2 = new Branch(path);
                        WriteLine("创建新支路{" + b2.ToString() + "}");
                        WriteLine("=>FindBranch({" + b2.ToString() + "}) deep=" + deepIndex.ToString());
                        deepIndex++;
                        FindBranch(b2);
                        deepIndex--;
                        WriteLine("<=findBranch({" + b2.ToString() + "}) deep=" + deepIndex.ToString());
                    }
                }
            }

        }

        /// <summary>
        /// 将支路添加到支路集合
        /// </summary>
        /// <param name="b0"></param>
        /// <returns></returns>
        private bool AddBranchs(Branch b0) {
            if (Branchs.Count < 500) {
                bool b = false;
                for (int i = 0; i < Branchs.Count; i++) {
                    Branch branch = Branchs[i];
                    if (branch.Id == b0.Id || branch.IsContainPath(b0)) {
                        b = true;
                    }
                    else if (!branch.IsContainPath(b0) && b0.IsContainPath(branch)) {
                        Branchs[i] = b0;
                        b = true;
                    }
                }
                if (!b) {
                    Branchs.Add(b0);
                    b0.SetPathElementBranch();
                    return true;
                }
                return false;
            }
            else {
                throw new Exception("当前连接的电路导致程序发生了错误");
            }
        }

        /// <summary>
        /// 获取可连接的最小路径元素
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<PathElement> GetConnectables(Branch path) {
            List<PathElement> ls = new List<PathElement>();
            //Console.WriteLine("FirstTerminal{"+TN(path.FirstTerminal)+"}.HasBranch=" + path.FirstTerminal.HasBranch + " ; LastTerminal{"+TN(path.LastTerminal)+"}.HasBranch=" + path.LastTerminal.HasBranch);
            foreach (PathElement pathElement in PathElements) {
                if (path.IsConnectable(pathElement) && !PathElementInBranchs(pathElement) && pathElement.B == null) {
                    ls.Add(pathElement);
                }
            }
            return ls;
        }

        private void BeginFindBranch() {
            Branchs.Clear();
            List<PathElement> ls = new List<PathElement>();
            deepIndex = 0;
            foreach (PathElement pathElement in PathElements) {
                if (pathElement.IsCircuitStart && !pathElement.IsCircuitEnd) {
                    ls.Add(pathElement);
                }
            }

            foreach (PathElement path in ls) {
                if (path.B == null) {
                    string np = Fun.NSpace(deepIndex);
                    Branch c = new Branch(path);
                    Console.WriteLine(np + "Into findBranch(" + c.ToString() + ") deep=" + deepIndex.ToString());
                    deepIndex++;
                    FindBranch(c);
                    deepIndex--;
                    Console.WriteLine(np + "Out findBranch deep=" + deepIndex.ToString());
                }
            }
        }

        /// <summary>
        /// 清理无效支路
        /// <para>在找支路过程中，会将一些无效的支路添加到支路集合，必须将这些支路清理掉</para>
        /// <para>判断依据是，凡是不包含在电路中的支路均被看作是无效支路</para>
        /// </summary>
        private void FindValidBranchs() {
            ValidBranchs.Clear();
            foreach (Branch branch in Branchs) {
                bool isexists = false;
                foreach (Circuit circuit in ValidCircuits) {
                    if (circuit.IsContainPath(branch)) {
                        isexists = true;
                    }
                }
                if (isexists) {
                    branch.IsValidBranch = true;
                    ValidBranchs.Add(branch);
                }
            }
        }

        /// <summary>
        /// 整理排列支路
        /// <para>将支路按照电路中出现的顺序，进行有序排列</para>
        /// <para>找出谁是谁的前面的支路，谁是谁的后面支路</para>
        /// <para>整理排序后的支路放在每个电路的Branchs集合中</para>
        /// </summary>
        private void ArrangeBranchs() {
            foreach (Circuit circuit in ValidCircuits) {
                circuit.Branchs.Clear();
                foreach (Branch branch in ValidBranchs) {
                    if (circuit.GetBranchIndex(branch) != -1) {
                        circuit.Branchs.Add(branch);
                    }
                }

                circuit.Branchs.Sort(new Branch.BranchSortByIndex());
                Console.WriteLine("Circuit : " + circuit.ToString());
                foreach (Branch branch1 in circuit.Branchs) {
                    Console.WriteLine("  " + branch1.ToString());
                }
                for (int i = 1; i < circuit.Branchs.Count; i++) {

                    Branch b0 = circuit.Branchs[i - 1];
                    Branch b1 = circuit.Branchs[i];

                    if (b1.LastTerminal.Id == b0.LastTerminal.Id) {
                        b1.Reverse(true);
                    }

                    if (b0.IsAfterSeries(b1)) {
                        Fun.AddPath(b1, b0.AfterBranchs);
                    }
                    if (b1.IsBeforeSeries(b0)) {
                        Fun.AddPath(b0, b1.BeforeBranchs);
                    }
                }
            }
        }

        /// <summary>
        /// 在有效支路内查询各个支路之间的并联关系
        /// </summary>
        private void BuildConnections() {
            int i;
            Branch b1, b2;

            for (i = 0; i < ValidBranchs.Count - 1; i++) {
                b1 = ValidBranchs[i];
                b2 = ValidBranchs[i + 1];
                if (float.IsNaN(b1.R) || float.IsNaN(b2.R)) { continue; }
                if (b1.IsParallel(b2)) {
                    Fun.AddPath(b1, b2.Parallels);
                    Fun.AddPath(b2, b1.Parallels);
                }
            }
        }
        #endregion

        #region Circuits
        /// <summary>
        /// 所有电路（包含正确的电路和不正确的电路）
        /// </summary>
        [Browsable(false)]
        [JsonIgnore]
        private List<Circuit> Circuits { get; } = new List<Circuit>();

        /// <summary>
        /// 正确电路的集合
        /// </summary>
        [Browsable(false)]
        [JsonIgnore]
        private List<Circuit> ValidCircuits { get; } = new List<Circuit>();

        /// <summary>
        /// 有效电路。既不含电阻为零或无穷大的电路的电路集合，用于电路计算
        /// </summary>
        [JsonIgnore]
        internal List<Circuit> EquationCircuits { get; } = new List<Circuit>();

        private void AddCircuit(Circuit c) {
            if (Circuits.Count < overCircuits) {
                bool b = false;
                foreach (Circuit circuit in Circuits) {
                    if (circuit.IsEqual(c)) { b = true; }
                }
                if (!b) {
                    Circuits.Add(c);
                    Console.WriteLine("电路总数=" + Circuits.Count.ToString());
                }
            }
            else {
                throw new Exception("由于电路过于复杂，导致程序陷入迷茫");
            }
        }

        /// <summary>
        /// 获取可连接的最小路径元素
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<PathElement> GetConnectables(Circuit path) {
            List<PathElement> ls = new List<PathElement>();
            foreach (PathElement pathElement in PathElements) {
                if (path.IsConnectable(pathElement)) {
                    ls.Add(pathElement);
                }
            }
            return ls;
        }

        private void FindCircuit(Circuit c1) {

            if (deepIndex >= maxDeep) {
                throw new Exception("连接的电路导致程序陷入了无限循环");
            }

            if (c1 == null || c1.LastPathElement == null) return;

            if (c1.IsFullLoop) {
                Console.WriteLine("电路已经完整，添加到电路集合");
                AddCircuit(c1);
            }
            else {

                List<PathElement> ls = GetConnectables(c1);
                Console.WriteLine("电路【" + c1.ToString() + "】第" + deepIndex.ToString() + "迭代，后面的路径元素有" + ls.Count + "条");
                if (ls.Count == 1) {
                    Console.WriteLine(ls[0].ToString());
                    c1.Link(ls[0]);
                    deepIndex++;
                    FindCircuit(c1);
                    deepIndex--;
                }
                else if (ls.Count > 1) {
                    foreach (PathElement path in ls) {
                        Console.WriteLine("克隆出新电路,连接【" + path.ToString() + "】");
                        Circuit c2 = c1.Clone();
                        c2.Link(path);
                        deepIndex++;
                        FindCircuit(c2);
                        deepIndex--;
                    }
                }
                else {
                    //无路可连，返回
                }
            }
        }

        /// <summary>
        /// 查找所有的电路
        /// </summary>
        private void FindCircuits() {

            Circuits.Clear();
            List<PathElement> ls = new List<PathElement>();
            deepIndex = 0;

            foreach (PathElement pathElement in PathElements) {
                if (pathElement.IsCircuitStart) {
                    ls.Add(pathElement);
                }
            }
            foreach (PathElement path in ls) {
                Circuit c = new Circuit(path);
                deepIndex++;
                FindCircuit(c);
                deepIndex--;
            }
        }

        /// <summary>
        /// 从所有电路中查找合法的电路
        /// </summary>
        private void FindValidCircuits() {
            ValidCircuits.Clear();
            foreach (Circuit circuit in Circuits) {
                if (!circuit.HasHalfway) {
                    circuit.CorrectBranchDirection();
                    ValidCircuits.Add(circuit);
                }
            }
        }

        /// <summary>
        /// 从合法的电路中查找能用于计算的电路
        /// </summary>
        private void FindEquationCircuits() {
            int opens;
            opens = 0;
            float rsis = float.NaN;
            float vol = float.NaN;
            EquationCircuits.Clear();

            if (ValidCircuits.Count > 1) {
                foreach (Circuit c in ValidCircuits) {
                    rsis = c.R;
                    vol = c.V;
                    //trace("该回路是否部分短路："+sectionShort.toString());
                    if (c.R > 0 && c.R < float.MaxValue && !float.IsNaN(c.R) && c.V != 0 && !c.HasSectionShort && !c.HasNodeCross) {
                        Fun.AddPath(c, EquationCircuits);
                    }
                    else if (float.IsNaN(c.R)) {
                        opens++;
                    }
                    //else if (c.R == 0f && c.V != 0) {
                    //    Fault = CT.PowerShort;
                    //}
                }
                //if (opens == Circuits.Count) {
                //    Fault = CT.AllOpen;
                //}
            }
            else if (Circuits.Count == 1) {
                EquationCircuits.Add(Circuits[0]);
                rsis = Circuits[0].R;
                vol = Circuits[0].V;
                //if (rsis == 0 && vol != 0) {
                //    Fault = CT.PowerShort;
                //}
                //else if (float.IsNaN(rsis) || rsis == float.MaxValue) {
                //    Fault = CT.AllOpen;
                //}
            }
        }

        /// <summary>
        /// 把电流回路的起始接线柱移动到支路最多的位置
        /// </summary>
        private void MoveCircuitToMaxPaths() {
            for (int i = 0; i < Circuits.Count; i++) {
                Circuits[i].MoveToMaxPaths();
            }
        }
        #endregion

        #region Equations
        /// <summary>
        /// KCL、KVL方程集合
        /// </summary>
        [JsonIgnore]
        public List<Equation> Equations { get; } = new List<Equation>();

        /// <summary>
        /// 所有的KCL和KVL方程
        /// </summary>
        [JsonIgnore]
        public List<Equation> AllEquations { get; } = new List<Equation>();

        /// <summary>
        /// 绑定节点连接的有效支路
        /// </summary>
        private void BindNodeValidBranchs() {
            Node node; Branch b;
            if (Nodes != null && Nodes.Count > 0 && EquationBranchs != null && EquationBranchs.Count > 0) {
                for (int j = 0; j < Nodes.Count; j++) {
                    node = Nodes[j];
                    node.Branchs.Clear();
                    for (int i = 0; i < EquationBranchs.Count; i++) {
                        b = EquationBranchs[i];
                        if (b.FirstNode == node || b.LastNode == node) {
                            Fun.AddPath(b, node.Branchs);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 创建节点电流公式（KCL）集合
        /// </summary>
        /// <para>KCL方程：节点电流方程，即所有进入某点的电流总和等于所有离开节点的电流总和</para>
        /// <para>即：I1+I2+(-I3)+...+In=0</para>
        /// <param name="needs"></param>
        /// <returns></returns>
        private List<Equation> GetKCLsByNode(int needs) {
            //trace("获取KCLs From Node,还需要"+needs.toString());
            List<Equation> kcls = new List<Equation>();
            BindNodeValidBranchs();
            List<Node> ns = Nodes.GetRange(0, Nodes.Count);
            ns.Sort(new Node.NodeComparer());
            Node node;
            Equation eq;
            int kn = 0;

            for (int i = 0; i < ns.Count; i++) {
                node = ns[i];
                eq = node.GetEquation((uint)EquationBranchs.Count);
                eq.OrderName = "L" + (AllEquations.Count + 1).ToString();
                AllEquations.Add(eq.Clone());
                //trace(eq.toString());
                if (kn < needs) {
                    if (Fun.AddEquation(eq, kcls)) {
                        kn++;
                    }
                }
            }
            return kcls;
        }

        /// <summary>
        /// 根据单条电路创建一个KVL方程
        /// </summary>
        /// <para>基尔霍夫电压定律：沿着闭合回路所有器件两端的电势差(电压)的代数和等于零</para>
        /// <para>既V1+V2+V3+...+Vn=0；而V=IR，I是要计算的量，所以式子变成R1+R2+R3+...+Rn</para>
        /// <para>每条支路的电阻不能为零，也不能为无穷大</para>
        /// <param name="cl"></param>
        /// <returns></returns>
        private Equation GetKVLByCircuit(Circuit cl) {
            int f = (int)cl.Fault;
            if (f == (int)FaultType.无) {
                //trace("参与计算的有效支路数="+eqBranchs.length.toString());
                Equation eq = new Equation((uint)EquationBranchs.Count);
                Branch branch;
                int i;
                for (i = 0; i < cl.Branchs.Count; i++) {
                    branch = cl.Branchs[i];
                    //trace("支路索引="+branch.Index.toString());
                    if (branch.Index != -1) {
                        //trace("branch.索引="+branch.Index.toString()+" , 电阻="+branch.Resistance.toString()+" , 定义的方向="+branch.DefinitionDirection.toString());
                        int cd = cl.GetCurrentDirection(branch);
                        if (cd == 1) {
                            branch.CurrentDirection |= AD.StartToEnd;
                        }
                        else if (cd == -1) {
                            branch.CurrentDirection |= AD.EndToStart;
                        }
                        eq.Coefficients[branch.Index] = cd * branch.R;
                    }
                }
                eq.Vector = cl.V;
                return eq;
            }
            else {
                return null;
            }

        }

        /// <summary>
        /// 创建KVL集合
        /// </summary>
        /// <returns></returns>
        private List<Equation> GetKVLsByCircuits() {
            //showBranchs(true);
            List<Circuit> cls = new List<Circuit>();
            Circuit cl;
            foreach (Circuit circuit in EquationCircuits) {
                bool b = false;
                for (int i = 0; i < cls.Count; i++) {
                    if (cls[i].IsEqual(circuit)) {
                        b = true;
                    }
                }
                if (!b) {
                    cls.Add(circuit);
                }
            }

            //trace("cls.length="+cls.length.toString());
            cls.Sort(new Circuit.CircuitComparer());
            //trace("GetKVLsByCL,Find CLs="+cls.length.toString());
            int row = cls.Count;
            List<Equation> kvls = new List<Equation>();
            if (row > 0) {
                cl = cls[0];
                //trace(cl.toString());
                Equation eq = GetKVLByCircuit(cl);
                eq.Type = EquationType.KVL;
                eq.OrderName = "L1";
                kvls.Add(eq);
                AllEquations.Add(eq);
                int n = 1;
                cls.RemoveAt(0);
                while (n < row) {
                    cl = GetOtherCL(cl, cls);
                    if (cl != null && cl.V != 0 && !float.IsNaN(cl.V)) {
                        //trace(cl.toString());
                        eq = GetKVLByCircuit(cl);
                        eq.OrderName = "L" + (kvls.Count + 1).ToString();
                        eq.Type = EquationType.KVL;
                        AllEquations.Add(eq.Clone());
                        Fun.AddEquation(eq, kvls);
                        Circuit.RemoveCircuitLoop(cl, cls);
                    }
                    n++;
                }
            }
            return kvls;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cl"></param>
        /// <param name="cls"></param>
        /// <returns></returns>
        private Circuit GetOtherCL(Circuit cl, List<Circuit> cls) {
            int index, i, max;
            index = -1; max = -1;
            for (i = 0; i < cls.Count; i++) {
                if (cls[i].NonEqualIndexCount(cl) > max && !cl.IsEquivalent(cls[i])) {
                    index = i;
                }
            }
            if (index >= 0 && index <= cls.Count) {
                return cls[index];
            }
            else {
                return null;
            }
        }

        /// <summary>
        /// 找出所有适合用于方程的支路
        /// </summary>
        private void FindEquationBranchs() {
            EquationBranchs.Clear();
            int i = 0; int j = 0;
            Branch b1;

            for (i = 0; i < Branchs.Count; i++) {
                Branchs[i].Index = -1;
            }
            if (Branchs.Count > 1) {
                for (i = 0; i < EquationCircuits.Count; i++) {
                    Circuit cl = EquationCircuits[i];
                    if (cl.Fault != FaultType.断路 && cl.Fault != FaultType.短路 && !cl.HasSectionShort) {
                        for (j = 0; j < cl.Branchs.Count; j++) {
                            b1 = cl.Branchs[j];
                            if (b1.R != 0 && !float.IsNaN(b1.R) && b1.Index == -1) {
                                if (Fun.AddPath(b1, EquationBranchs)) {
                                    b1.Index = EquationBranchs.Count - 1;
                                    if (cl.PathStr.IndexOf(b1.PathStr) >= 0 && b1.DefinitionDirection == 0) {
                                        b1.DefinitionDirection = 1;
                                    }
                                    else if (b1.DefinitionDirection == 0) {
                                        b1.DefinitionDirection = -1;
                                    }
                                }
                                else {
                                    int index = Fun.GetBranchIndex(b1, EquationBranchs);
                                    if (index != -1) {
                                        b1 = EquationBranchs[i];
                                        if (cl.PathStr.IndexOf(b1.PathStr) >= 0 && b1.DefinitionDirection == 0) {
                                            b1.DefinitionDirection = 1;

                                        }
                                        else if (b1.DefinitionDirection == 0) {
                                            b1.DefinitionDirection = -1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (Branchs.Count == 1 && !float.IsNaN(Branchs[0].R)) {
                EquationBranchs.Add(Branchs[0]);
                Branchs[0].Index = 0;
                Branchs[0].DefinitionDirection = 1;
            }
        }

        /// <summary>
        /// 根据EquationBranchs的总数创建Equation集合
        /// <para>先用回路电压法建立KVL方程，如果不够，就用节点电流法</para>
        /// <para>纯并联或者纯串联只用KVL（回路电压方程）就可以。复杂电路才既需要KVL，也需要KCL</para>
        /// </summary>
        public void BuildEquations() {
            Equations.Clear();

        }

        /// <summary>
        /// 获取用于矩阵求解的线性方程组
        /// </summary>
        /// <returns></returns>
        private void GetMatrix() {
            Equations.Clear();
            Console.WriteLine("Do GetMatrix()");
            List<Equation> matrix = GetKVLsByCircuits();
            //trace("KCLs="+matrix.length.toString());
            if (matrix.Count < EquationBranchs.Count) {
                matrix.AddRange(GetKCLsByNode(EquationBranchs.Count - matrix.Count));
            }

            Equations.AddRange(matrix);
            Console.WriteLine("Equations.Count=" + Equations.Count);
        }

        private bool ComputedIsValid(List<float> result) {
            float current = 0f;
            int i;
            for (i = 0; i < result.Count; i++) {
                if (float.IsNaN(result[i])) {
                    return false;
                }
                else {
                    current += result[i];
                }
            }
            if (current == 0) { return false; }
            return true;
        }

        private bool ComputedIsValid(List<double> result) {
            double current = 0d;
            int i;
            for (i = 0; i < result.Count; i++) {
                if (double.IsNaN(result[i])) {
                    return false;
                }
                else {
                    current += Math.Abs(result[i]);
                }
            }
            if (current == 0) { return false; }
            return true;
        }

        /// <summary>
        /// 根据节点计算各支路的电路
        /// </summary>
        private void ComputeCurrent3() {
            /*计算方法：
             * 循环计算节点的支路，直到所有支路的电流不为float.NaN或不为无穷大
             * 定义一个整数n，表示有效支路的总数
             * 循环
             * 令整数k=n
             * foreach循环
             *  如果支路电流不为float.NaN或不为无穷大，则k--
             * 如果k==0，则退出do循环
             * 如果k>0，则循环每个节点的每条支路
             *  如果节点支路电流不为float.NaN或不为无穷大，则累加电流a
             *  循环完节点的全部支路后，如果只有一条支路的电流是float.NaN或无穷大，则令该支路的电流等于0-累加电流a
             *  
             */
            int n = ValidBranchs.Count, m = 0, k = n;
            do {
                k = n;
                foreach (Branch branch in ValidBranchs) {
                    if (!float.IsNaN(branch.A) && !float.IsInfinity(branch.A)) {
                        k--;
                    }
                }
                if (k > 0) {
                    foreach (Node node in ValidNodes) {
                        ///电流为float.NaN的支路数量
                        int nb = 0;
                        int naIndex = -1;
                        float a = 0;
                        for (int i = 0; i < node.BranchCount; i++) {
                            Branch b = node.Branchs[i];
                            if (!float.IsNaN(b.A) && !float.IsInfinity(b.A)) {
                                a += b.DefinitionDirection * b.A;
                            }
                            else {
                                naIndex = i;
                                nb++;
                            }
                        }
                        if (nb == 1) {
                            node.Branchs[naIndex].A = -a;
                        }
                    }
                }
                m++;
            } while (k != 0 && m < 100);
        }

        /// <summary>
        /// 根据已经计算得到的方程支路的电流计算其他支路的电流
        /// </summary>
        private void ComputeCurrent2() {
            int m = 0;
            do {
                int n = ValidBranchs.Count;
                foreach (Branch branch in ValidBranchs) {
                    float aa = branch.GetAfterA();
                    float ba = branch.GetBeforeA();
                    float a = branch.A;
                    if (float.IsNaN(a)) {
                        if (!float.IsNaN(ba) && float.IsNaN(aa)) {
                            branch.A = ba;
                        }
                        else if (float.IsNaN(ba) && !float.IsNaN(aa)) {
                            branch.A = aa;
                        }
                        else if (!float.IsNaN(ba) && ba == aa) {
                            branch.A = ba;
                        }
                    }
                    else {
                        n--;
                    }
                }
                m++;
                Console.WriteLine("n=" + n.ToString() + " ; m=" + m.ToString());
                if (n == 0) { break; }
            } while (m < 100);
        }

        private void ComputeCurrent() {
            /*参与计算的支路必须确定是有电流流过的*/
            List<Branch> bs = new List<Branch>();
            int i; Branch b1;

            for (i = 0; i < ValidBranchs.Count; i++) {
                b1 = ValidBranchs[i];
                if (hasCurrent(ValidBranchs[i])) {
                    bs.Add(ValidBranchs[i]);
                }
            }
            float curr = 0f;

            if (EquationCircuits.Count > 1) {
                List<double> currents = new List<double>();
                List<int> cons = new List<int>();
                int len = bs.Count;

                for (i = 0; i < len; i++) {
                    currents.Add(bs[i].A);
                }
                int m = 0;
                //trace("准备进行无电阻支路电流计算");
                //trace(Func.ToNumbersString(currents,2));
                do {
                    for (i = 0; i < len; i++) {
                        if (currents[i] == 0 || double.IsNaN(currents[i])) {
                            //trace("Compute "+bs[i].Name+"'s Current");
                            curr = (float)GetCurrent(currents, bs[i].GetAfter(bs));
                            if (curr == 0) {
                                curr = (float)GetCurrent(currents, bs[i].GetBefore(bs));
                            }
                            if (curr != 0 && !float.IsNaN(curr)) {
                                currents[i] = curr;
                                bs[i].A = curr;

                            }
                        }
                    }
                    //trace(Func.ToNumbersString(currents,2));
                    m++;
                } while (noZero(currents) && m < len);
                //trace("结算结果");
                //trace(Func.ToNumbersString(currents,2));
            }
            else if (EquationCircuits.Count == 1) {
                for (i = 0; i < bs.Count; i++) {
                    if (bs[i].A != 0 && !float.IsNaN(bs[i].A)) {
                        curr = bs[i].A;
                        break;
                    }
                }
                for (i = 0; i < EquationCircuits[0].Branchs.Count; i++) {
                    EquationCircuits[0].Branchs[i].A = curr;
                }
            }
        }

        private double GetCurrent(List<double> source, List<double> cons) {
            if (source.Count != cons.Count) return 0;
            int n = 0, m = 0;
            double c = 0d;
            for (int i = 0; i < source.Count; i++) {
                if (cons[i] != 0d) {
                    m++;
                    if (source[i] != 0d) {
                        c += source[i] * cons[i];
                        n++;
                    }
                }
            }
            if (m == n && m != 0) {
                return c;
            }
            else {
                return 0;
            }
        }

        private bool hasCurrent(Branch branch) {
            if (EquationCircuits != null && EquationCircuits.Count > 0) {
                for (int i = 0; i < EquationCircuits.Count; i++) {
                    for (int j = 0; j < EquationCircuits[i].Branchs.Count; j++) {
                        if (EquationCircuits[i].Branchs[j].IsEqual(branch)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool noZero(List<double> source) {
            for (int i = 0; i < source.Count; i++) {
                if (source[i] == 0) {
                    return true;
                }
            }
            return false;
        }

        private void ComputeNodeVoltage() {
            //trace("开始计算节点电位，eqCircuits.length="+eqCircuits.length.toString());
            int i = 0; int j = 0;
            Terminal t1; Terminal t2;
            EleComponent obj;
            ComponentType type;
            BatteryCase bc;
            List<EleComponent> Objs;
            bool isexists = false;
            float rsis = float.NaN;

            //先计算方程支路的节点电位
            for (i = 0; i < EquationCircuits.Count; i++) {
                EquationCircuits[i].ComputeNodePotential();
            }
            //再计算非方程支路的节点电位
            for (i = 0; i < Branchs.Count; i++) {
                rsis = Branchs[i].R;
                //trace("支路"+Branchs[i].Name+"的电阻="+rsis.toString());
                isexists = false;
                for (j = 0; j < EquationBranchs.Count; j++) {
                    if (Branchs[i].IsEqual(EquationBranchs[j])) {
                        isexists = true;
                        break;
                    }
                }
                if (!isexists) {
                    Objs = Branchs[i].EleComponents;
                    //trace("正向设置节点电位……");
                    for (j = 1; j < Objs.Count; j += 2) {
                        t1 = (Terminal)(Objs[j - 1]);
                        t2 = (Terminal)(Objs[j + 1]);
                        obj = Objs[j];
                        type = obj.Type;
                        if (!float.IsNaN(Fun.GetResistance(obj, t1, t2, Opt.IsIdeal))) {
                            if (type != ComponentType.BatteryCase) {
                                if (!float.IsNaN(t1.JoinedNode.Potential) && float.IsNaN(t2.JoinedNode.Potential)) {
                                    t2.JoinedNode.Potential = t1.JoinedNode.Potential;
                                }
                            }
                            else if (type == ComponentType.BatteryCase) {
                                bc = (BatteryCase)(obj);
                                if (!float.IsNaN(t1.JoinedNode.Potential) && float.IsNaN(t2.JoinedNode.Potential)) {
                                    if (t1.Key == TerminalKey.Left && t2.Key == TerminalKey.Right) {
                                        t2.JoinedNode.Potential = t1.JoinedNode.Potential - bc.Voltage;
                                    }
                                    else if (t1.Key == TerminalKey.Right && t2.Key == TerminalKey.Left) {
                                        t2.JoinedNode.Potential = t1.JoinedNode.Potential + bc.Voltage;
                                    }
                                }
                                else if (float.IsNaN(t1.JoinedNode.Potential) && !float.IsNaN(t2.JoinedNode.Potential)) {
                                    if (t1.Key == TerminalKey.Left && t2.Key == TerminalKey.Right) {
                                        t2.JoinedNode.Potential = t1.JoinedNode.Potential + bc.Voltage;
                                    }
                                    else if (t1.Key == TerminalKey.Right && t2.Key == TerminalKey.Left) {
                                        t2.JoinedNode.Potential = t1.JoinedNode.Potential - bc.Voltage;
                                    }
                                }
                            }
                        }
                        else {
                            break;
                        }
                    }
                    //trace("反向设置节点电位");
                    for (j = Objs.Count - 2; j >= 1; j -= 2) {
                        t1 = (Terminal)(Objs[j + 1]);
                        t2 = (Terminal)(Objs[j - 1]);
                        obj = Objs[j];
                        type = obj.Type;
                        if (!float.IsNaN(Fun.GetResistance(obj, t1, t2, false))) {
                            if (type != ComponentType.BatteryCase) {
                                if (!float.IsNaN(t1.JoinedNode.Potential) && float.IsNaN(t2.JoinedNode.Potential)) {
                                    t2.JoinedNode.Potential = t1.JoinedNode.Potential;
                                }
                            }
                            else if (type == ComponentType.BatteryCase) {
                                bc = (BatteryCase)(obj);
                                if (!float.IsNaN(t1.JoinedNode.Potential) && float.IsNaN(t2.JoinedNode.Potential)) {
                                    if (t1.Key == TerminalKey.Left && t2.Key == TerminalKey.Right) {
                                        t2.JoinedNode.Potential = t1.JoinedNode.Potential + bc.Voltage;
                                    }
                                    else if (t1.Key == TerminalKey.Right && t2.Key == TerminalKey.Left) {
                                        t2.JoinedNode.Potential = t1.JoinedNode.Potential - bc.Voltage;
                                    }
                                }
                                else if (float.IsNaN(t1.JoinedNode.Potential) && !float.IsNaN(t2.JoinedNode.Potential)) {
                                    if (t1.Key == TerminalKey.Left && t2.Key == TerminalKey.Right) {
                                        t2.JoinedNode.Potential = t1.JoinedNode.Potential - bc.Voltage;
                                    }
                                    else if (t1.Key == TerminalKey.Right && t2.Key == TerminalKey.Left) {
                                        t2.JoinedNode.Potential = t1.JoinedNode.Potential + bc.Voltage;
                                    }
                                }
                            }
                        }
                        else {
                            break;
                        }
                    }
                }
            }
            float min = 0f;

            for (i = 0; i < Nodes.Count; i++) {
                if (!float.IsNaN(Nodes[i].Potential) && Nodes[i].Potential < min) {
                    min = Nodes[i].Potential;
                }
            }
            if (min < 0) {
                for (i = 0; i < Nodes.Count; i++) {
                    if (!float.IsNaN(Nodes[i].Potential)) {
                        Nodes[i].Potential += Math.Abs(min);
                    }
                }
            }
        }

        /// <summary>
        /// 设置元件接线柱对之间的电压
        /// </summary>
        private void SetTerminalPairVoltage() {
            foreach (PathElement path in PathElements) {
                Node n1 = path.Left.JoinedNode;
                Node n2 = path.Right.JoinedNode;
                if (path.ElementOrWire.GetType().BaseType == typeof(Element)) {
                    Element e = (Element)path.ElementOrWire;
                    e.SetTerminalPairVoltage(n1, n2);
                }
            }
        }

        #endregion

        #region CircuitGroups
        [Browsable(false)]
        [JsonIgnore]
        private List<CircuitGroup> CircuitGroups { get; } = new List<CircuitGroup>();
        #endregion
    }
}
