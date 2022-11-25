using System;

namespace ElectricityDLL {
    /// <summary>
    /// 路径元素
    /// <para>每条路径都是由路径元素连接而成</para>
    /// </summary>
    public class PathElement {

        public Guid Id { get; } = Guid.NewGuid();
        /// <summary>
        /// 临时锁定
        /// </summary>
        public bool IsLocked { get; set; } = false;
        public Guid LockedId { get; set; }
        /// <summary>
        /// 所属支路/干路
        /// </summary>
        public Branch B { get; set; } = null;

        /// <summary>
        /// 是否是用电器电路元素
        /// </summary>
        public bool IsDevice {
            get { return ElementOrWire.IsDevice; }
        }

        /// <summary>
        /// 是否是电压表电路元素
        /// </summary>
        public bool IsVoltmeter {
            get { return ElementOrWire.IsVoltmater; }
        }

        /// <summary>
        /// 是否是电流表电路元素
        /// </summary>
        public bool IsAmmeter {
            get { return ElementOrWire.IsAmmeter; }
        }

        /// <summary>
        /// 是否是导线电路元素
        /// </summary>
        public bool IsWire {
            get { return ElementOrWire.Type == ComponentType.Wire; }
        }

        /// <summary>
        /// 是否是电源电路元素
        /// </summary>
        public bool IsPower { get { return ElementOrWire.IsPower; } }

        public float R {
            get {
                float r = float.NaN;
                if (Left != null && Right != null && ElementOrWire != null) {
                    switch (ElementOrWire.Type) {
                        case ComponentType.Wire:
                            r = ElementOrWire.Fault == FaultType.断路 ? float.NaN : 0f;
                            break;
                        case ComponentType.Ammeter:
                            r = ((Ammeter)ElementOrWire).GetResistance(Left, Right);
                            break;
                        case ComponentType.Voltmeter:
                            r = ((Voltmeter)ElementOrWire).GetResistance(Left, Right);
                            break;
                        case ComponentType.Fan:
                            r = ((Fan)ElementOrWire).GetResistance(Left, Right);
                            break;
                        case ComponentType.Lampstand:
                            r = ((Lampstand)ElementOrWire).GetResistance(Left, Right);
                            break;
                        case ComponentType.Resistor:
                            r = ((Resistor)ElementOrWire).GetResistance(Left, Right);
                            break;
                        case ComponentType.Rheostat:
                            r = ((Rheostat)ElementOrWire).GetResistance(Left, Right);
                            break;
                        case ComponentType.Switch:
                            r = ((Switch)ElementOrWire).GetResistance(Left, Right);
                            break;
                        case ComponentType.BatteryCase:
                            r = ((BatteryCase)ElementOrWire).GetResistance(Left, Right);
                            break;
                    }
                }
                return r;
            }
        }

        /// <summary>
        /// 接线柱1
        /// </summary>
        public Terminal Left { get; internal set; }

        /// <summary>
        /// 接线柱之间的元件或导线
        /// </summary>
        public EleComponent ElementOrWire { get; set; }

        /// <summary>
        /// 接线柱2
        /// </summary>
        public Terminal Right { get; internal set; }

        /// <summary>
        /// (int)Left.Key | (int)Right.Key
        /// </summary>
        public int Keys {
            get {
                int r = 0;
                r |= Left != null ? (int)Left.Key : 0;
                r |= Right != null ? (int)Right.Key : 0;
                return r;
            }
        }

        public bool IsCircuitStart {
            get {
                return Left.Owner.Type == ComponentType.BatteryCase && Left.Key == TerminalKey.Right && ElementOrWire.Type == ComponentType.Wire;
            }
        }

        public bool IsCircuitEnd {
            get {
                return ElementOrWire.Type == ComponentType.BatteryCase;
            }
        }

        public bool IsBadPath {
            get {
                Element e1 = (Element)Left.Owner;
                Element e2 = (Element)Right.Owner;
                return e1.IsBadPath || e2.IsBadPath;
            }
        }

        public PathElement(Terminal t1, EleComponent e, Terminal t2) {
            if (t1 != null && t2 != null && e != null && (e.Type == ComponentType.Wire || e.GetType().BaseType == typeof(Element))) {
                Left = t1;
                ElementOrWire = e;
                Right = t2;
                if (t2.Owner.Type == ComponentType.BatteryCase && t2.Key == TerminalKey.Right) {
                    Left = t2;
                    Right = t1;

                }
                if (t1.Owner.Type == ComponentType.BatteryCase && t1.Key == TerminalKey.Left) {
                    Left = t2;
                    Right = t1;
                }
            }
            else {
                throw new Exception("接线柱为NULL或者接线柱间的不是元件或导线");
            }
        }

        /// <summary>
        /// 获取反向的路径元素
        /// </summary>
        /// <returns></returns>
        public void Reverse() {
            Terminal t = Right;
            Right = Left;
            Left = t;
        }

        /// <summary>
        /// 是否在Start接线柱或End接线柱存在分支电路
        /// </summary>
        public bool HasBranch {
            get {
                return Left.HasBranch || Right.HasBranch;
            }
        }

        /// <summary>
        /// 是否等同
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsEqual(PathElement p) {
            if (p != null) {
                string s = ToString();
                Reverse();
                string rs = ToString();
                Reverse();
                string sp = p.ToString();
                return sp == s || sp == rs;
            }
            return false;
        }

        /// <summary>
        /// 是否可连接
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsConnectable(PathElement p, AllowLink allow = AllowLink.LeftAndRight) {
            if (!IsEqual(p)) {
                if (allow == AllowLink.Left) {
                    return p.Left.Id == Left.Id || p.Right.Id == Left.Id;
                }
                else if (allow == AllowLink.Right) {
                    return p.Left.Id == Right.Id || p.Right.Id == Right.Id;
                }
                else {
                    return p.Left.Id == Left.Id || p.Left.Id == Right.Id || p.Right.Id == Left.Id || p.Right.Id == Right.Id;
                }
            }
            return false;
        }

        public override string ToString() {
            return Left.Owner.SymbolName + "." + Left.Key.ToString() + "->" + ElementOrWire.SymbolName + "->" + Right.Owner.SymbolName + "." + Right.Key.ToString();
        }
    }
}
