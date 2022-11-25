using System.Collections.Generic;

namespace ElectricityDLL {
    /*
     * 应用程序配置
     */
    public static class ElectricityConfig {
        /// <summary>
        /// 元器件配置
        /// </summary>
        public static List<ElementConfigItem> ElementConfigs { get; } = new List<ElementConfigItem> {
            new ElementConfigItem(ComponentType.Ammeter,3){ ToolImage=Properties.Resources.btnAmmeter},
            new ElementConfigItem(ComponentType.BatteryCase,1){ ToolImage=Properties.Resources.btnBatteryCase},
            new ElementConfigItem(ComponentType.Fan,2){ ToolImage=Properties.Resources.btnFan},
            new ElementConfigItem(ComponentType.Lampstand,4){ ToolImage=Properties.Resources.btnLampstand},
            new ElementConfigItem(ComponentType.Resistor,4){ ToolImage=Properties.Resources.btnRsistance},
            new ElementConfigItem(ComponentType.Rheostat,2){ ToolImage=Properties.Resources.btnRheostat},
            new ElementConfigItem(ComponentType.Switch,6){ ToolImage=Properties.Resources.btnSwitch},
            new ElementConfigItem(ComponentType.Voltmeter,3){ToolImage=Properties.Resources.btnVoltmeter }
        };

        /// <summary>
        /// 获取某种元器件的最大数量
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int ElementMaxAmount(ComponentType type) {
            int m = 1024;
            foreach (ElementConfigItem item in ElementConfigs) {
                if (item.Type == type) {
                    m = item.MaxAmount;
                }
            }
            return m;
        }

        /// <summary>
        /// 当在工作台拖拽元器件时，为尽量减少CPU计算量和屏幕抖动，设定了最小拖拽量，以像素为单位。
        /// <para>当前这个配置值在程序中还没有被使用</para>
        /// </summary>
        public static int MinDragDropDistance = 5;
    }
}
