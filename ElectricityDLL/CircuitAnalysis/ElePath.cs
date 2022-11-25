using System;
using System.Collections.Generic;
using System.Text;

namespace ElectricityDLL {
    /// <summary>
    /// 电路路径类
    /// </summary>
    public class ElePath {
        public Guid Id { get; } = Guid.NewGuid();

        protected List<PathElement> PathElements { get; } = new List<PathElement>();

        /// <summary>
        /// 是否包含原件
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool IsContainElement(EleComponent e) {
            foreach (PathElement path in PathElements) {
                if (path.ElementOrWire.Id == e.Id) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 路径的最后一个电路元素
        /// </summary>
        public PathElement LastPathElement {
            get {
                if (PathElements.Count > 0) return PathElements[PathElements.Count - 1];
                return null;
            }
        }

        /// <summary>
        /// 路径的第一个电路元素
        /// </summary>
        public PathElement FirstPathElement {
            get {
                if (PathElements.Count > 0) {
                    return PathElements[0];
                }
                else {
                    return null;
                }
            }
        }

        /// <summary>
        /// 第一个接线柱
        /// </summary>
        public Terminal FirstTerminal {
            get {
                if (EleComponents.Count >= 3) {
                    return (Terminal)EleComponents[0];
                }
                return null;
            }
        }

        /// <summary>
        /// 第二个接线柱
        /// </summary>
        public Terminal SecondTerminal {
            get {
                if (Count >= 3) {
                    return (Terminal)EleComponents[2];
                }
                return null;
            }
        }

        /// <summary>
        /// 最后一个接线柱
        /// </summary>
        public Terminal LastTerminal {
            get {
                if (EleComponents.Count > 0) {
                    return (Terminal)LastEle;
                }
                else {
                    return null;
                }
            }
        }

        /// <summary>
        /// 倒数第二个接线柱
        /// </summary>
        public Terminal SecondLastTerminal {
            get {
                if (Count >= 3) {
                    return (Terminal)EleComponents[EleComponents.Count - 3];
                }
                return null;
            }
        }

        /// <summary>
        /// 第一个接线柱所在元件的类型
        /// </summary>
        public ComponentType FirstType {
            get {
                if (FirstTerminal != null) {
                    return FirstTerminal.Owner.Type;
                }
                return ComponentType.Other;
            }
        }

        /// <summary>
        /// 获取最后一个接线柱所在元件的类型
        /// </summary>
        public ComponentType LastType {
            get {
                if (LastTerminal != null) {
                    return LastTerminal.Owner.Type;
                }
                return ComponentType.Other;
            }
        }

        public ComponentType SecondLastType {
            get {
                if (SecondLastTerminal != null) {
                    return SecondLastTerminal.Owner.Type;
                }
                return ComponentType.Other;
            }
        }

        /// <summary>
        /// 获取第一个接线柱的Key
        /// </summary>
        public TerminalKey FirstKey {
            get {
                if (FirstTerminal != null) {
                    return FirstTerminal.Key;
                }
                return TerminalKey.UnKnow;
            }
        }

        /// <summary>
        /// 获取最后一个接线柱的Key
        /// </summary>
        public TerminalKey LastKey {
            get {
                if (LastTerminal != null) {
                    return LastTerminal.Key;
                }
                return TerminalKey.UnKnow;
            }
        }

        public FaultType Fault { get; set; } = FaultType.无;

        public string Name { get; set; } = "";

        public bool IsIdeal { get; set; } = true;

        /// <summary>
        /// 是否连接到了电源的负极
        /// </summary>
        public bool IsLinkedNegativeElectrode {
            get {
                bool fbl = FirstType == ComponentType.BatteryCase && FirstKey == TerminalKey.Left;
                bool fbr = SecondLastType == ComponentType.BatteryCase && LastKey == TerminalKey.Left;

                return fbl || fbr;
            }
        }

        /// <summary>
        /// 是否连接到了电源的正极
        /// </summary>
        public bool IsLinkedPositiveElectrode {
            get {
                bool fbl = FirstType == ComponentType.BatteryCase && FirstKey == TerminalKey.Right;
                bool fbr = LastType == ComponentType.BatteryCase && LastKey == TerminalKey.Right;

                return fbl || fbr;
            }
        }

        private float a = float.NaN;
        /// <summary>
        /// 电路中的电流
        /// </summary>
        public float A {
            get { return a; }
            set {
                a = value;
                if (!Consts.IsZero(a) && !float.IsNaN(a) && !float.IsInfinity(a)) {
                    foreach (PathElement path in PathElements) {
                        path.ElementOrWire.Current = a;
                        if (path.ElementOrWire.Type != ComponentType.Switch && path.ElementOrWire.Type != ComponentType.BatteryCase) {
                            path.ElementOrWire.Stat = WorkStat.Working;
                        }
                        path.ElementOrWire.IsDirty = true;
                        if (path.ElementOrWire.Type != ComponentType.Wire) {
                            Element e = (Element)path.ElementOrWire;
                            e.SetTerminalPairCurrent(path, a);
                        }
                    }
                }
                else {
                    foreach (PathElement path in PathElements) {
                        path.ElementOrWire.Current = float.NaN;
                        path.ElementOrWire.IsDirty = true;
                        if (path.ElementOrWire.Type != ComponentType.Wire) {
                            Element e = (Element)path.ElementOrWire;
                            e.SetTerminalPairCurrent(path, a);
                        }
                        if (path.ElementOrWire.Type != ComponentType.BatteryCase && path.ElementOrWire.Type != ComponentType.Switch) {
                            path.ElementOrWire.Stat = WorkStat.StopOrOpen;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 路径中的元器件集合
        /// <para>该集合的元器件排序应该是类似于 电源1左接线柱->导线1->小灯泡左接线柱->小灯泡->小灯泡右接线柱->导线2->开关右接线柱->开关->开关左接线柱->导线3->电源1右接线柱</para>
        /// </summary>
        public List<EleComponent> EleComponents { get; } = new List<EleComponent>();

        /// <summary>
        /// EleComponents.Count
        /// </summary>
        public int Count { get { return EleComponents.Count; } }

        /// <summary>
        /// 如果路径能够闭合，则返回闭合处的电器元件或导线，否则返回null
        /// </summary>
        public EleComponent ClosureEle {
            get {
                if (Count >= 3) {
                    if (FirstEle.Owner != null && LastEle.Owner != null) {
                        if (FirstEle != LastEle && LastEle.Owner.Id == FirstEle.Owner.Id) {
                            //同一电器元件
                            return FirstEle.Owner;
                        }
                        else {
                            //同一导线
                            if (FirstEle.Type == ComponentType.Terminal && LastEle.Type == ComponentType.Terminal) {
                                Terminal t1 = (Terminal)FirstEle;
                                Terminal t2 = (Terminal)LastEle;
                                foreach (Junction junction in t1.Junctions) {
                                    if (junction.AnotherSideTerminal() == t2) {
                                        return junction.Owner;
                                    }
                                }
                            }
                        }
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// 路径的最后一个元件，如果路径无元件，则返回NULL
        /// </summary>
        public EleComponent FirstEle {
            get {
                if (EleComponents.Count > 0) { return EleComponents[0]; }
                return null;
            }
        }

        /// <summary>
        /// 路径的第一个元件，如果路径无元件，则返回NULL
        /// </summary>
        public EleComponent LastEle {
            get {
                if (EleComponents.Count > 0) {
                    return EleComponents[EleComponents.Count - 1];
                }
                return null;
            }
        }

        /// <summary>
        /// 如果第一个元件是接线柱，则返回接线柱位置的节点
        /// </summary>
        public Node FirstNode {
            get {
                if (FirstEle.Type == ComponentType.Terminal) {
                    Terminal terminal = (Terminal)FirstEle;
                    return terminal.JoinedNode;
                }
                return null;
            }
        }

        /// <summary>
        /// 如果最后一个元件是接线柱，则返回接线柱位置的节点
        /// </summary>
        public Node LastNode {
            get {
                if (LastEle.Type == ComponentType.Terminal) {
                    return ((Terminal)LastEle).JoinedNode;
                }
                return null;
            }
        }

        /// <summary>
        /// 第一个元件的符号名称，如果无元件，返回NULL
        /// </summary>
        public string FirstName {
            get {
                if (FirstEle != null) return FirstEle.SymbolName;
                return null;
            }
        }

        /// <summary>
        /// 最后一个元件的符号名称,如果无元件，则返回NULL
        /// </summary>
        public string LastName {
            get {
                if (LastEle != null) return LastEle.SymbolName;
                return null;
            }
        }

        /// <summary>
        /// 获取路径中倒数第二个元件的符号名称
        /// </summary>
        public string LastSecondName {
            get {
                if (Count >= 3) {
                    return EleComponents[Count - 2].SymbolName;
                }
                return null;
            }
        }

        /// <summary>
        /// 路径中的所有接线柱
        /// </summary>
        public List<Terminal> Terminals {
            get {
                List<Terminal> list = new List<Terminal>();
                foreach (EleComponent ele in EleComponents) {
                    if (ele.Type == ComponentType.Terminal) {
                        list.Add((Terminal)ele);
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// 路径中的所有节点
        /// </summary>
        public List<Node> Nodes {
            get {
                List<Node> nodes = new List<Node>();
                foreach (Terminal item in Terminals) {
                    if (item.JoinedNode != null) {
                        Fun.AddNode(item.JoinedNode, nodes);
                    }
                }
                return nodes;
            }
        }

        /// <summary>
        /// 获取路径中的所有电器元件和导线
        /// </summary>
        public List<EleComponent> ElementAndWires {
            get {
                List<EleComponent> list = new List<EleComponent>();
                foreach (EleComponent ele in EleComponents) {
                    if (ele.Type == ComponentType.Wire || ele.GetType().BaseType == typeof(Element)) {
                        Fun.AddEleComponents<EleComponent>(ele, list);
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// 路径中的元器件
        /// </summary>
        public List<Element> Elements {
            get {
                List<Element> vs = new List<Element>();
                foreach (EleComponent ele in EleComponents) {
                    if (ele.GetType().BaseType == typeof(Element)) {
                        vs.Add((Element)ele);
                    }
                }
                return vs;
            }
        }

        /// <summary>
        /// 路径中的电压表
        /// </summary>
        public List<BatteryCase> BatteryCases {
            get {
                List<BatteryCase> vs = new List<BatteryCase>();
                foreach (EleComponent ele in EleComponents) {
                    if (ele.Type == ComponentType.BatteryCase) {
                        vs.Add((BatteryCase)ele);
                    }
                }
                return vs;
            }
        }

        /// <summary>
        /// 路径中的电压表
        /// </summary>
        public List<Voltmeter> Voltmeters {
            get {
                List<Voltmeter> vs = new List<Voltmeter>();
                foreach (EleComponent ele in EleComponents) {
                    if (ele.Type == ComponentType.Voltmeter) {
                        vs.Add((Voltmeter)ele);
                    }
                }
                return vs;
            }
        }

        /// <summary>
        /// 电路中是否存在电压表
        /// </summary>
        public bool HasVoltmeter {
            get {
                foreach (EleComponent item in EleComponents) {
                    if (item.Type == ComponentType.Voltmeter) {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 获取电路的电阻，如返回float.NaN，表示电阻无穷大
        /// <para>如果电路中存在断路/开关未闭合等情况，返回float.NaN</para>
        /// </summary>
        public float R {
            get {
                if (PathElements.Count > 0) {
                    float f = 0f;
                    foreach (PathElement path in PathElements) {
                        if (path.R != 0 && !float.IsNaN(path.R)) {
                            f += path.R;
                        }
                        else if (float.IsNaN(path.R)) {
                            return float.NaN;
                        }
                    }
                    return f;
                }
                return float.NaN;
            }
        }

        /// <summary>
        /// 路径是否开始与指定电源的右接线柱，结束于指定电源的左接线柱
        /// </summary>
        public bool IsBothBatteryCase(BatteryCase bc) {
            if (Count >= 3) {
                if (FirstEle.Type == ComponentType.Terminal && LastEle.Type == ComponentType.Terminal) {
                    if (FirstEle.Owner.Id == LastEle.Owner.Id && FirstEle.Owner.Type == ComponentType.BatteryCase && FirstEle.Owner.Id == bc.Id) {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 是否闭合回路
        /// <para>闭合回路中可能是没有电源的</para>
        /// </summary>
        public bool IsCloseLoop {
            get {
                if (Count >= 3) {
                    if (LastEle.Type == ComponentType.Terminal && FirstEle.Type == ComponentType.Terminal) {
                        List<Terminal> lja = ((Terminal)LastEle).ConnectedTerminals;
                        foreach (Terminal item in lja) {
                            if (item.Id == FirstEle.Id) {
                                return true;
                            }
                        }
                        lja = ((Terminal)LastEle).OtherTerminals;
                        foreach (Terminal item in lja) {
                            if (item.Id == FirstEle.Id) {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 确定某段路径是否包含在另一端路径中
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsContainPath(ElePath path) {
            if (path != null && path.Count <= Count) {
                if (PathStr.IndexOf(path.PathStr) >= 0 || PathStr.IndexOf(path.ReversePathStr) >= 0) {
                    return true;
                }
            }
            return false;
        }

        [Obsolete]
        public float ResistanceBoth(int start, int end) {
            if (start >= 0 && end < Count && start < end) {

            }
            return 0f;
        }

        /// <summary>
        /// 是否包含路径元素
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsContainPathElement(PathElement p) {
            if (p != null) {
                if (PathElements.Count == 0) {
                    return false;
                }
                else {
                    foreach (PathElement pathElement in PathElements) {
                        if (pathElement.IsEqual(p)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 按从前向后的顺序将各元件的名称连接起来
        /// <para>名称字符串</para>
        /// </summary>
        public string PathStr {
            get {
                StringBuilder sb = new StringBuilder();
                foreach (EleComponent ele in EleComponents) {
                    sb.Append(ele.SymbolName);
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 按从后向前的顺序将各元件的名称连接起来
        /// <para>名称字符串</para>
        /// </summary>
        public string ReversePathStr {
            get {
                StringBuilder sb = new StringBuilder();
                for (int i = Count - 1; i >= 0; i--) {
                    sb.Append(EleComponents[i].SymbolName);
                }
                return sb.ToString();
            }
        }

        public string SortByName() {
            int i;
            //trace("按名称排序之对象数组："+Objs.toString());
            List<EleComponent> _objs = new List<EleComponent>();
            _objs.AddRange(EleComponents);
            _objs.RemoveAt(_objs.Count - 1);
            float com = 0;
            EleComponent e;
            EleComponent.EleComponentComparer comer = new EleComponent.EleComponentComparer();
            int n;
            do {
                n = 0;
                for (i = 0; i < _objs.Count - 1; i++) {
                    com = comer.Compare(_objs[i], _objs[i + 1]);
                    if (com > 0) {
                        e = _objs[i];
                        _objs[i] = _objs[i + 1];
                        _objs[i + 1] = e;
                        n++;
                    }
                }
            } while (n != 0);
            //trace("ObjLength="+_objs.length.toString());
            return _objs.ToString();
        }

        public bool IsEqu(ElePath circuit) {
            return PathStr == circuit.PathStr || PathStr == circuit.ReversePathStr;
        }

        /// <summary>
        /// 路径是否相同
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsEqual(ElePath path) {
            return path.PathStr == PathStr || path.PathStr == ReversePathStr;
        }

        public ElePath(List<EleComponent> path) {
            if (path != null) {
                EleComponents.AddRange(path);
            }
        }

        /// <summary>
        /// 以电路元素初始化电路
        /// </summary>
        /// <param name="pe"></param>
        public ElePath(PathElement pe) {
            PathElements.Add(pe);
            EleComponents.Add(pe.Left);
            EleComponents.Add(pe.ElementOrWire);
            EleComponents.Add(pe.Right);
        }

        public ElePath() { }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            foreach (EleComponent e in EleComponents) {
                if (e.Type == ComponentType.Wire) {
                    sb.Append(sb.Length > 0 ? "->" : "");
                    sb.Append(e.SymbolName);
                }
                else if (e.GetType().BaseType == typeof(Element)) {
                    Element ele = (Element)e;
                    sb.Append(sb.Length > 0 ? "->" : "");
                    sb.Append(e.SymbolName);
                }
                else if (e.Type == ComponentType.Terminal) {
                    Terminal t = (Terminal)e;
                    sb.Append(sb.Length > 0 ? "->" : "");
                    sb.Append(t.Owner.SymbolName + "." + t.Key.ToString());
                }
            }
            return sb.ToString();
        }
    }
}
