using ElectricityDLL.CircuitAnalysis;
using System;
using System.Collections.Generic;

namespace ElectricityDLL {
    /// <summary>
    /// 表示电路中的节点
    /// </summary>
    public class Node {
        /// <summary>
        /// 节点排序比较器
        /// </summary>
        public class NodeComparer : IComparer<Node> {
            public int Compare(Node a, Node b) {
                if (a.BranchCount > b.BranchCount) {
                    return -1;
                }
                else if (a.BranchCount < b.BranchCount) {
                    return 1;
                }
                else {
                    return 0;
                }
            }
        }

        public Guid Id { get; } = Guid.NewGuid();

        public List<Element> Elements { get; } = new List<Element>();

        public int index = -1;

        public string Name { get; set; } = "";

        public List<Terminal> Terminals { get; } = new List<Terminal>();

        /// <summary>
        /// 节点电位
        /// </summary>
        public float Potential { get; set; } = float.NaN;

        public bool AddTerminal(Terminal t) {
            if (!IsContainTerminal(t)) {
                t.JoinedNode = this;
                Terminals.Add(t);
                return true;
            }
            return false;
        }

        public bool IsContainTerminal(Terminal t) {
            bool b = false;
            foreach (Terminal terminal in Terminals) {
                if (terminal.Id == t.Id) { b = true; }
            }
            return b;
        }

        private List<Terminal> GetNeighbor(Terminal t, Terminal et = null) {
            List<Terminal> ls = new List<Terminal>();
            foreach (Terminal terminal in Terminals) {
                if (IsNeighbor(t, terminal)) {
                    ls.Add(terminal);
                }
            }
            return ls;
        }

        private bool IsNeighbor(Terminal t1, Terminal t2) {
            foreach (Terminal terminal in t1.ConnectedTerminals) {
                if (terminal.Id == t2.Id) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 返回与接线柱位于同一节点的其他接线柱数组。忽略掉开关所在的接线柱
        /// </summary>
        /// <param name="j0"></param>
        /// <returns></returns>
        public List<Terminal> GetOtherTerminals(Terminal j0) {
            List<Terminal> ls = new List<Terminal>();
            foreach (Terminal terminal in Terminals) {
                if (terminal.Id != j0.Id) {
                    ls.Add(terminal);
                }
            }
            return ls;
        }

        public Node(Terminal t) {
            if (t != null) { Terminals.Add(t); }
        }

        public List<Branch> Branchs { get; } = new List<Branch>();

        /// <summary>
        /// 节点的支路
        /// </summary>
        public List<NodeBranchPair> NBPs { get; } = new List<NodeBranchPair>();

        private void addnbp(NodeBranchPair nbp) {
            bool b = false;
            foreach (NodeBranchPair node in NBPs) {
                if (node.B.Id == nbp.B.Id) {
                    b = true;
                    break;
                }
            }
            if (!b) {
                NBPs.Add(nbp);
            }
        }

        /// <summary>
        /// 添加支路
        /// </summary>
        /// <param name="b"></param>
        public void AddBranch(Branch b) {
            if (b.FirstNode.Id == Id) {
                NodeBranchPair nbp = new NodeBranchPair();
                nbp.D = AD.EndToStart;
                nbp.B = b;
                addnbp(nbp);
            }
            else if (b.LastNode.Id == Id) {
                NodeBranchPair nbp = new NodeBranchPair();
                nbp.D = AD.StartToEnd;
                nbp.B = b;
                addnbp(nbp);
            }
        }

        public int BranchCount { get { return Branchs.Count; } }

        public int ElementCount { get { return Elements.Count; } }

        /// <summary>
        /// 是否是起始于指定电源的节点
        /// </summary>
        /// <param name="bc"></param>
        /// <returns></returns>
        public bool IsFirstNode(BatteryCase bc) {
            foreach (Terminal terminal in Terminals) {
                if (terminal.Owner.Id == bc.Id && terminal.Key == TerminalKey.Right) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 是否是结束于指定电源的节点
        /// </summary>
        /// <param name="bc"></param>
        /// <returns></returns>
        public bool IsLastNode(BatteryCase bc) {
            foreach (Terminal terminal in Terminals) {
                if (terminal.Owner.Id == bc.Id && terminal.Key == TerminalKey.Left) {
                    return true;
                }
            }
            return false;
        }

        public Terminal GetTerminalAt(int index) {
            if (index >= 0 && index < Terminals.Count) { return Terminals[index]; }
            return null;
        }

        /// <summary>
        /// 获取该节点的所有接线柱数量
        /// </summary>
        public int TerminalCount {
            get { return Terminals.Count; }
        }

        public Terminal FirstTerminal {
            get {
                if (TerminalCount > 0) { return Terminals[0]; }
                return null;
            }
        }

        public int IndexAt(Terminal t) {
            if (t == null) return -1;
            for (int i = 0; i < TerminalCount; i++) {
                if (Terminals[i].Id == t.Id) return i;
            }
            return -1;
        }

        public int Index { get; set; }

        /// <summary>
        /// 创建节点电流公式（KCL）
        /// <para>节点电流公式形式：I1+I2+...+In=0</para>
        /// <para>如果某节点不存在某条支路，则该支路索引的电流定为0</para>
        /// <para>如果某支路的电流是流入该节点，则为1，否则为-1</para>
        /// </summary>
        /// <param name="cols">有效支路的总数</param>
        /// <returns></returns>
        public Equation GetEquation(uint cols) {
            Equation eq = new Equation(cols);
            eq.Type = EquationType.KCL;

            for (int i = 0; i < Branchs.Count; i++) {
                if (Branchs[i].LastNode == this) {
                    eq.Coefficients[Branchs[i].Index] = 1;
                }
                else {
                    eq.Coefficients[Branchs[i].Index] = -1;
                }
            }
            eq.Vector = 0;
            return eq;
        }

        public override string ToString() {
            return Id.ToString() + "[Potential=" + Potential.ToString("x2") + "](" + string.Join(",", Terminals) + ")";
        }
    }
}
