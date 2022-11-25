using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectricityDLL {
    /// <summary>
    /// 电路组(包含节点、电流回路、电源、用电器)
    /// </summary>
    public class CircuitGroup {
        /// <summary>
        /// 对工作平台的引用
        /// </summary>
        public Workbench Bench { get; private set; }
        /// <summary>
        /// 是否是理想电路
        /// </summary>
        public bool IsIdeal { get; set; } = true;

        /// <summary>
        /// 电路类型
        /// </summary>
        public CT Fault { get; set; } = CT.None;

        /// <summary>
        /// 是否现实电流
        /// </summary>
        public bool IsShowCurrent { get; set; } = false;

        public bool AllOpen { get { return Fault == CT.AllOpen; } }

        public bool IsPowerShort { get { return Fault == CT.PowerShort; } }

        public bool BatteryIsExists(BatteryCase b) {
            foreach (Circuit circuit in Circuits) {
                foreach (EleComponent component in circuit.EleComponents) {
                    if (component.Id == b.Id) { return true; }
                }
            }
            return false;
        }

        public bool ElementIsExists(Element e) {
            foreach (Circuit circuit in Circuits) {
                foreach (EleComponent component in circuit.EleComponents) {
                    if (component.Id == e.Id) { return true; }
                }
            }
            return false;
        }

        /// <summary>
        /// 电路组名称
        /// </summary>
        public string Name { get; set; } = "";

        public List<EleComponent> EleComponents {
            get {
                List<EleComponent> es = new List<EleComponent>();
                foreach (Circuit circuit in Circuits) {
                    foreach (EleComponent ele in circuit.EleComponents) {
                        bool b = false;
                        for (int i = 0; i < es.Count; i++) {
                            if (es[i].Id == ele.Id) { b = true; }
                        }
                        if (!b) {
                            es.Add(ele);
                        }
                    }
                }
                return es;
            }
        }

        /// <summary>
        /// 所有节点
        /// </summary>
        public List<Node> Nodes {
            get {
                List<Node> ns = new List<Node>();
                int i = 0; int j = 0;
                for (i = 0; i < Circuits.Count; i++) {
                    ns = Circuits[i].Nodes;
                    if (ns != null && ns.Count > 0) {
                        for (j = 0; j < ns.Count; j++) {
                            Fun.AddNode(ns[j], Nodes);
                        }
                    }
                }
                return ns;
            }
        }

        /// <summary>
        /// 所有回路
        /// </summary>
        public List<Circuit> Circuits { get; } = new List<Circuit>();

        /// <summary>
        /// 所有支路
        /// </summary>
        public List<Branch> Branchs { get; } = new List<Branch>();

        private List<BatteryCase> _bcs = null;

        /// <summary>
        /// 全部电源
        /// </summary>
        public List<BatteryCase> BatteryCases {
            get {
                if (_bcs == null) _bcs = new List<BatteryCase>();
                foreach (Circuit circuit in Circuits) {
                    foreach (BatteryCase batteryCase in circuit.BatteryCases) {
                        batteryCase.SCG = this;
                        Fun.AddEleComponents<BatteryCase>(batteryCase, _bcs);
                    }
                }
                return _bcs;
            }
        }

        public BatteryCase FirstBatterCase {
            get {
                if (BatteryCases.Count > 0) {
                    return BatteryCases[0];
                }
                return null;
            }
        }

        /// <summary>
        /// 电路中的所有元件（电源、开关、仪表、用电器）
        /// </summary>
        public List<Element> Elements {
            get {
                List<Element> ls = new List<Element>();
                foreach (Circuit circuit in Circuits) {
                    foreach (EleComponent ele in circuit.EleComponents) {
                        if (ele.GetType().BaseType == typeof(Element)) {
                            bool b = false;
                            for (int i = 0; i < ls.Count; i++) {
                                if (ls[i].Id == ele.Id) { b = true; }
                            }
                            if (!b) {
                                ls.Add((Element)ele);
                            }
                        }
                    }
                }
                return ls;
            }
        }

        /// <summary>
        /// 有效的电器元件
        /// </summary>
        public List<Element> EfficientElements {
            get {
                List<Element> ls = new List<Element>();
                foreach (Circuit circuit in Circuits) {
                    foreach (EleComponent ele in circuit.EleComponents) {
                        if (ele.GetType().BaseType == typeof(Element) && ele.Type != ComponentType.BatteryCase) {
                            bool b = false;
                            foreach (Element element in ls) {
                                if (ele.Id == element.Id) { b = true; }
                            }
                            if (!b) {
                                ls.Add((Element)ele);
                            }
                        }
                    }
                }
                return ls;
            }
        }

        /// <summary>
        /// 有效接线柱
        /// </summary>
        public List<Terminal> EfficientTerminals {
            get {
                List<Terminal> list = new List<Terminal>();
                foreach (Circuit circuit in Circuits) {
                    foreach (Terminal terminal in circuit.Terminals) {
                        Fun.AddTerminal(terminal, list);
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// 用于KVL方程的有效回路
        /// </summary>
        public List<Circuit> EqCircuits { get; } = new List<Circuit>();

        /// <summary>
        /// 应用于KCL方程的有效节点
        /// </summary>
        public List<Node> EqNodes { get; } = new List<Node>();

        /// <summary>
        /// KCL、KVL方程集合
        /// </summary>
        public List<Equation> Equations { get; } = new List<Equation>();


        /// <summary>
        /// 
        /// </summary>
        public List<Branch> EqBranchs { get; } = new List<Branch>();

        private void AddCircuitLoop(Circuit sourcecl) {
            bool b = false;
            foreach (Circuit circuit in Circuits) {
                if (circuit.IsContainPath(sourcecl)) {
                    b = true;
                }
            }
            if (!b) {
                Circuits.Add(sourcecl);
            }
        }

        /*==========================================================*/
        /**
		 * 从路径数组中排除某条路径
		 */
        private List<ElePath> ExcludePath(Circuit cl, List<ElePath> paths) {
            int i, j, n, l1, l2;
            ElePath path;
            l2 = cl.Count;
            for (i = paths.Count - 1; i >= 0; i--) {
                path = paths[i];
                l1 = path.Count;
                if (l1 <= l2) {
                    n = 0;
                    for (j = 0; j < l1; j++) {
                        if (path.EleComponents[j] == cl.EleComponents[l2 - j - 1]) {
                            n++;
                        }
                    }
                    if (n == l1) {
                        paths.RemoveAt(i);
                    }
                }
            }
            return paths;
        }

        /// <summary>
        /// 迭代查找电路的路径
        /// </summary>
        /// <param name="sourcecl"></param>
        private void FindFullCircuitDetail(Circuit sourcecl) {
            if (!sourcecl.HasCloseLoop) {
                Terminal t1 = (Terminal)sourcecl.LastEle;
                List<PathElement> pes = t1.GetAllPathElement(sourcecl.LastPathElement);
                Circuit newcl;
                int i;
                //trace(sourcecl.toStr());
                //trace(t1.FullName+" paths.length="+paths.length.toString());
                if (pes.Count > 0) {
                    for (i = 0; i < pes.Count; i++) {
                        if (!sourcecl.IsContainPathElement(pes[i])) {
                            newcl = sourcecl.Clone();
                            //newcl.Link(pes[i]);
                            FindFullCircuitDetail(newcl);
                        }
                    }
                }
            }
            else {
                if (sourcecl.BatteryCases.Count != 0 && !sourcecl.HasHalfway) {
                    AddCircuitLoop(sourcecl);
                }
            }
        }

        private void FindFullCircuit(BatteryCase bc) {
            Terminal Left = bc.GetTerminal(TerminalKey.Left);
            if (Left != null) {
                List<ElePath> paths = Left.AllNeighborPaths;
                List<PathElement> pes = Left.GetAllPathElement();
                for (int i = 0; i < pes.Count; i++) {
                    Circuit c = new Circuit(this, pes[i]);
                    FindFullCircuitDetail(c);
                }
            }
        }

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

        /// <summary>
        /// 把电流回路的起始接线柱移动到支路最多的位置
        /// </summary>
        public void MoveCircuitToMaxPaths() {
            for (int i = 0; i < Circuits.Count; i++) {
                Circuits[i].MoveToMaxPaths();
            }
        }

        public void AddNode(Node n) {
            if (Fun.AddNode(n, Nodes)) {
                n.Index = Nodes.Count;
            }
        }

        private void BuilderNodes() {
            List<Node> ns;
            int i, j;
            for (i = 0; i < Circuits.Count; i++) {
                ns = Circuits[i].Nodes;
                if (ns != null && ns.Count > 0) {
                    for (j = 0; j < ns.Count; j++) {
                        Fun.AddNode(ns[j], Nodes);
                    }
                }
            }
        }

        private Branch addBranch(Branch b) {
            if (b == null) return null;
            for (int i = 0; i < Branchs.Count; i++) {
                if (Branchs[i].IsEqu(b)) {
                    return Branchs[i];
                }
            }
            b.Name = "B" + Branchs.Count.ToString();
            b.IsIdeal = IsIdeal;
            Branchs.Add(b);
            return b;
        }

        private void FindBranch() {
            int i, j, n, index;
            Branch b1;
            Terminal t1;
            List<Circuit> cls;
            Circuit cl;

            /*查找支路*/
            cls = Circuits;
            if (cls != null && cls.Count > 0) {
                for (i = 0; i < cls.Count; i++) {
                    index = 0;
                    cl = cls[i];
                    for (j = 0; j < cl.Count; j += 2) {
                        t1 = (Terminal)(cl.EleComponents[j]);
                        n = t1.BothSideObjCount;
                        //trace("getBranchs result="+n.toString()+" , j="+j.toString()+" , cl.ObjLength="+cl.ObjLength.toString());
                        switch (n) {
                            case 2:/*在只有一条Circuit，且是回路的起点或终点的情况下会出现*/
                                if (j == cl.Count - 1) {
                                    b1 = addBranch(cl.GetSection(index, j));
                                    Fun.AddPath(b1, cl.Branchs);
                                }
                                break;
                            default:/*接线柱前后至少存在3个不同的对象*/
                                if (j > 0) {
                                    b1 = addBranch(cl.GetSection(index, j));
                                    Fun.AddPath(b1, cl.Branchs);
                                    index = j;
                                }
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 移除无效节点
        /// </summary>
        private void removeInvalidNodes() {
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
        /// 把支路数做多的节点电位设置为零
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
                    for (j = 0; j < Branchs.Count; j++) {
                        b = Branchs[j];
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

        private void BuilderEqCircuits() {
            int i, opens;
            opens = 0;
            float rsis = float.NaN;
            float vol = float.NaN;
            Circuit c;
            EqCircuits.Clear();

            if (Circuits != null && Circuits.Count > 1) {
                for (i = 0; i < Circuits.Count; i++) {
                    c = Circuits[i];
                    rsis = c.R;
                    vol = c.V;
                    //trace("该回路是否部分短路："+sectionShort.toString());
                    if (rsis > 0 && rsis < float.MaxValue && rsis != float.NaN && c.V != 0 && !c.HasSectionShort && !c.HasNodeCross) {
                        Fun.AddPath(c, EqCircuits);
                    }
                    else if (rsis == float.NaN) {
                        opens++;
                    }
                    else if (rsis == 0 && vol != 0) {
                        Fault = CT.PowerShort;
                    }
                }
                if (opens == Circuits.Count) {
                    Fault = CT.AllOpen;
                }
            }
            else if (Circuits != null && Circuits.Count == 1) {
                EqCircuits.Add(Circuits[0]);
                rsis = Circuits[0].R;
                vol = Circuits[0].V;
                if (rsis == 0 && vol != 0) {
                    Fault = CT.PowerShort;
                }
                else if (rsis == float.NaN || rsis == float.MaxValue) {
                    Fault = CT.AllOpen;
                }
            }
        }

        private void FindEqBranchs() {
            EqBranchs.Clear();
            int i = 0; int j = 0;
            Branch b1;

            for (i = 0; i < Branchs.Count; i++) {
                Branchs[i].Index = -1;
            }
            if (Branchs.Count > 1) {
                for (i = 0; i < EqCircuits.Count; i++) {
                    Circuit cl = EqCircuits[i];
                    if (cl.Fault != FaultType.断路 && cl.Fault != FaultType.短路 && !cl.HasSectionShort) {
                        for (j = 0; j < cl.Branchs.Count; j++) {
                            b1 = cl.Branchs[j];
                            if (b1.R != 0 && b1.R != float.NaN && b1.Index == -1) {
                                if (Fun.AddPath(b1, EqBranchs)) {
                                    b1.Index = EqBranchs.Count - 1;
                                    if (cl.EleComponents.ToString().IndexOf(b1.EleComponents.ToString()) >= 0 && b1.DefinitionDirection == 0) {
                                        b1.DefinitionDirection = 1;
                                    }
                                    else if (b1.DefinitionDirection == 0) {
                                        b1.DefinitionDirection = -1;
                                    }
                                }
                                else {
                                    int index = Fun.GetBranchIndex(b1, EqBranchs);
                                    if (index != -1) {
                                        b1 = EqBranchs[i];
                                        if (cl.EleComponents.ToString().IndexOf(b1.EleComponents.ToString()) >= 0 && b1.DefinitionDirection == 0) {
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
            else if (Branchs.Count == 1 && Branchs[0].R != float.NaN) {
                EqBranchs.Add(Branchs[0]);
                Branchs[0].Index = 0;
                Branchs[0].DefinitionDirection = 1;
            }
        }

        /// <summary>
        /// 以初始电源右接线柱开始查找完整电流回路和支路
        /// </summary>
        private void BuilderCircuit() {

            Circuits.Clear();

            int i;
            int firstCircuitCount;
            int secondCircuitCount;
            int kvlCircuits;
            /*1.以初始电源右接线柱开始查找完整电流回路和支路*/
            FindFullCircuit(FirstBatterCase);
            firstCircuitCount = Circuits.Count;
            //FindPowers();
            if (BatteryCases.Count > 1) {
                for (i = 1; i < BatteryCases.Count; i++) {
                    FindFullCircuit(BatteryCases[i]);
                }
            }
            secondCircuitCount = Circuits.Count;
            /*2.归集有效接线柱*/
            //BuilderValidJxzs();
            /*计算每个接线柱前后连接的对象数*/
            ComputeTerminalPaths();
            /*把电流回路中前后对象最多的那个接线柱设置为回路的起始接线柱*/
            MoveCircuitToMaxPaths();
            /*把接线柱所在的节点集中，便于后续引用*/
            BuilderNodes();
            //trace("节点数："+Nodes.length.toString());
            /*移除无效节点*/
            removeInvalidNodes();
            //trace("剩余节点数："+Nodes.length.toString());
            /*查找支路*/
            FindBranch();
            ///*定义支路数最多节点的电位为零*/
            DelimitZeroPotential();
            ///*归集可同于KVL计算的电流回路*/
            BuilderEqCircuits();
            ///*查找用于KVL计算的支路*/
            FindEqBranchs();
            kvlCircuits = EqCircuits.Count;
            //string outStr = "";
            //if (firstCircuits < secondCircuits) {
            //    //outStr = "从电源"+FirstBC.name+"开始找到"+firstCircuits.toString()+"条回路；以其他电源查找回路，找到"+(secondCircuits-firstCircuits).toString()+"条";
            //}
            //else {
            //    //outStr = "找到"+firstCircuits.toString()+"条回路";
            //}
            ////outStr += "，其中可用于KVL方程的回路有"+kvlCircuits.toString()+"条。";
            ////outStr += "全部支路数有"+Branchs.length.toString()+"条，其中可用于KVL方程的支路数有"+eqBranchs.length.toString()+"条。";
            ////outStr += "\n以下显示电流回路、支路和节点："
            ////trace(outStr);
            ////trace("-----------------全部回路----------------------");
            ////showCircuits(false);
            ////trace("------------------KVL回路----------------------");
            ////showCircuits();
            ////trace("-----------------全部支路----------------------");
            ////showBranchs();
            ////trace("-----------------KVL支路----------------------");
            ////showBranchs(true);
            ////showNodes();
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
                    current += result[i];
                }
            }
            if (current == 0) { return false; }
            return true;
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

            for (i = 0; i < EqCircuits.Count; i++) {
                EqCircuits[i].ComputeNodePotential();
            }
            for (i = 0; i < Branchs.Count; i++) {
                rsis = Branchs[i].R;
                //trace("支路"+Branchs[i].Name+"的电阻="+rsis.toString());
                isexists = false;
                for (j = 0; j < EqBranchs.Count; j++) {
                    if (Branchs[i].IsEqu(EqBranchs[j])) {
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
                        if (Fun.GetResistance(obj, t1, t2, false) != float.NaN) {
                            if (type != ComponentType.BatteryCase) {
                                if (float.IsNaN(t1.JoinedNode.Potential) && float.IsNaN(t2.JoinedNode.Potential)) {
                                    t2.JoinedNode.Potential = t1.JoinedNode.Potential;
                                }
                            }
                            else if (type == ComponentType.BatteryCase) {
                                bc = (BatteryCase)(obj);
                                if (float.IsNaN(t1.JoinedNode.Potential) && float.IsNaN(t2.JoinedNode.Potential)) {
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
                        if (Fun.GetResistance(obj, t1, t2, false) != float.NaN) {
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
        /// 绑定支路的连接关系
        /// </summary>
        private void bindLink() {

            int i, j;
            Branch b1, b2;

            for (i = 0; i < Branchs.Count; i++) {
                b1 = Branchs[i];
                if (b1.R == float.NaN) {
                    continue;
                }
                for (j = 0; j < Branchs.Count; j++) {
                    if (j != i) {
                        b2 = Branchs[j];
                        if (b2.R == float.NaN) { continue; }
                        if (b1.IsAfterSeries(b2)) {
                            /*b2串联在b1的后面*/
                            Fun.AddPath(b2, b1.AfterBranchs);
                            Fun.AddPath(b1, b2.BeforeBranchs);

                        }
                        if (b1.IsBeforeSeries(b2)) {
                            Fun.AddPath(b2, b1.BeforeBranchs);
                            Fun.AddPath(b1, b2.AfterBranchs);
                        }
                        if (b1.IsParallel(b2)) {
                            Fun.AddPath(b1, b2.Parallels);
                            Fun.AddPath(b2, b1.Parallels);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 绑定节点连接的有效支路
        /// </summary>
        private void BindNodeValidBranchs() {
            Node node; Branch b;

            if (Nodes != null && Nodes.Count > 0 && EqBranchs != null && EqBranchs.Count > 0) {
                for (int j = 0; j < Nodes.Count; j++) {
                    node = Nodes[j];
                    node.Branchs.Clear();
                    for (int i = 0; i < EqBranchs.Count; i++) {
                        b = EqBranchs[i];
                        if (b.FirstNode == node || b.LastNode == node) {
                            Fun.AddPath(b, node.Branchs);
                            node.AddBranch(b);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取用于矩阵求解的线性方程组
        /// </summary>
        /// <returns></returns>
        public void getMatrix() {
            Equations.Clear();
            List<Equation> matrix = GetKVLsByCL();
            //trace("KCLs="+matrix.length.toString());
            if (matrix.Count < EqBranchs.Count) {
                matrix.AddRange(GetKCLsByNode(EqBranchs.Count - matrix.Count));
            }
            Equations.AddRange(matrix);
        }

        public List<Equation> GetKCLsByNode(int needs) {
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
                eq = node.GetEquation((uint)EqBranchs.Count);
                //trace(eq.toString());
                if (Fun.AddEquation(eq, kcls)) {
                    kn++;
                }
                if (kn == needs) {
                    break;
                }
            }
            return kcls;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cl"></param>
        /// <param name="cls"></param>
        /// <returns></returns>
        private Circuit getOtherCL(Circuit cl, List<Circuit> cls) {
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

        public Equation getKVLByCL(Circuit cl) {
            int f = (int)cl.Fault;
            if (f == (int)FaultType.无) {
                //trace("参与计算的有效支路数="+eqBranchs.length.toString());
                Equation eq = new Equation((uint)EqBranchs.Count);
                eq.Type = EquationType.KVL;

                float resistance;
                Branch branch;
                int i;
                for (i = 0; i < cl.Branchs.Count; i++) {
                    branch = cl.Branchs[i];
                    //trace("支路索引="+branch.Index.toString());
                    if (branch.Index != -1) {
                        //trace("branch.索引="+branch.Index.toString()+" , 电阻="+branch.Resistance.toString()+" , 定义的方向="+branch.DefinitionDirection.toString());
                        resistance = branch.R;
                        eq.Coefficients[branch.Index] = branch.DefinitionDirection * resistance;
                    }
                }
                eq.Vector = cl.V;
                return eq;
            }
            else {
                return null;
            }

        }

        public List<Equation> GetKVLsByCL() {
            //showBranchs(true);
            List<Circuit> cls = EqCircuits.GetRange(0, EqCircuits.Count);
            Circuit cl;
            //trace("cls.length="+cls.length.toString());
            cls.Sort(new Circuit.CircuitComparer());
            //trace("GetKVLsByCL,Find CLs="+cls.length.toString());
            int row = cls.Count;
            List<Equation> kvls = new List<Equation>();
            if (row > 0) {
                cl = cls[0];
                //trace(cl.toString());
                Equation eq = getKVLByCL(cl);
                kvls.Add(eq);
                int n = 1;
                cls.RemoveAt(0);
                while (n < row) {
                    cl = getOtherCL(cl, cls);
                    if (cl != null && cl.V != 0) {
                        //trace(cl.toString());
                        eq = getKVLByCL(cl);
                        Fun.AddEquation(eq, kvls);
                        Circuit.RemoveCircuitLoop(cl, cls);
                    }
                    n++;
                }
            }
            return kvls;
        }

        private double GetCurrent(List<double> source, List<double> cons) {
            if (source.Count != cons.Count) return 0;

            int n = 0, m = 0;
            double c = 0d;
            for (int i = 0; i < source.Count; i++) {
                if (cons[i] != 0) {
                    m++;
                    if (source[i] != 0) {
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
            if (EqCircuits != null && EqCircuits.Count > 0) {
                for (int i = 0; i < EqCircuits.Count; i++) {
                    for (int j = 0; j < EqCircuits[i].Branchs.Count; j++) {
                        if (EqCircuits[i].Branchs[j].IsEqu(branch)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 是否存在桥电路
        /// </summary>
        public bool HasBridge {
            get {
                bool r = false;
                for (int i = 0; i < Branchs.Count; i++) {
                    if (Branchs[i].IsTwoway) {
                        r = true;
                        break;
                    }
                }
                return r;
            }
        }

        private bool noZero(List<double> source) {
            for (int i = 0; i < source.Count; i++) {
                if (source[i] == 0) {
                    return true;
                }
            }
            return false;
        }

        private void ComputeCurrent() {
            /*参与计算的支路必须确定是有电流流过的*/
            List<Branch> bs = new List<Branch>();
            int i; Branch b1;

            for (i = 0; i < Branchs.Count; i++) {
                b1 = Branchs[i];
                if (hasCurrent(Branchs[i])) {
                    bs.Add(Branchs[i]);
                }
            }
            float curr = 0f;

            if (EqCircuits.Count > 1) {
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
                        if (currents[i] == 0) {
                            //trace("Compute "+bs[i].Name+"'s Current");
                            curr = (float)GetCurrent(currents, bs[i].GetAfter(bs));
                            if (curr == 0) {
                                curr = (float)GetCurrent(currents, bs[i].GetBefore(bs));
                            }
                            if (curr != 0) {
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
            else if (EqCircuits.Count == 1) {
                for (i = 0; i < bs.Count; i++) {
                    if (bs[i].A != 0 && bs[i].A != float.NaN) {
                        curr = bs[i].A;
                        break;
                    }
                }
                for (i = 0; i < EqCircuits[0].Branchs.Count; i++) {
                    EqCircuits[0].Branchs[i].A = curr;
                }
            }
        }

        public CircuitGroup(BatteryCase bc, string name, Workbench bh, bool isIdeal = true) {
            Name = name;
            IsIdeal = isIdeal;
            bc.SCG = this;
            Bench = bh;
            if (_bcs == null) { _bcs = new List<BatteryCase>(); _bcs.Add(bc); }
            BuilderCircuit();
            if (!AllOpen && !IsPowerShort) {
                bindLink();
                FindEqBranchs();

                if (EqBranchs.Count > 0) {
                    getMatrix();
                    double[] result = Fun.ComputeEquations(Equations);

                    bool IsValid = ComputedIsValid(result.ToList());

                    if (IsValid) {
                        for (int i = 0; i < EqBranchs.Count; i++) {
                            if (result[i] != 0d) {
                                EqBranchs[i].A = (float)result[i];
                            }
                        }

                        ComputeCurrent();

                        ComputeNodeVoltage();

                        //ShowNodes();
                    }
                }
            }
            else if (AllOpen) {
                ComputeNodeVoltage();
                foreach (Branch branch in Branchs) {
                    if (branch.HasVoltmeter) {

                    }
                }
            }
            //int i, j;

        }
    }
}
