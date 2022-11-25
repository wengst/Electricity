namespace ElectricityDLL {
    /// <summary>
    /// 用于装载元器件数量、类型等数据的类型
    /// </summary>
    public class ElementData {
        int BL = 3;
        public ComponentType Type { get; set; }
        public string Name {
            get {
                return PublicFunction.GetChineseName(Type);
            }
        }
        public string Symbol {
            get {
                return PublicFunction.GetSymbol(Type);
            }
        }
        public int InitAmount {
            get {
                switch (Type) {
                    case ComponentType.Ammeter:
                    case ComponentType.BatteryCase:
                    case ComponentType.Voltmeter:
                    case ComponentType.Fan:
                    case ComponentType.Rheostat:
                        return BL * 1;
                    case ComponentType.Switch:
                        return BL * 3;
                    case ComponentType.Lampstand:
                    case ComponentType.Resistor:
                        return BL * 2;

                }
                return 1 * BL;
            }
        }
    }
}
