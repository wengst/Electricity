namespace ElectricityDLL {
    public static class PublicFunction {
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
    }
}
