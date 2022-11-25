using System.Drawing;

namespace ElectricityDLL {
    /// <summary>
    /// 元器件数量限制数据类型
    /// </summary>
    public class ElementConfigItem {
        /// <summary>
        /// 元器件类型
        /// </summary>
        public ComponentType Type { get; private set; }

        /// <summary>
        /// 元器件最大数量
        /// </summary>
        public int MaxAmount { get; private set; }

        /// <summary>
        /// 工具图片
        /// </summary>
        public Image ToolImage { get; internal set; }

        /// <summary>
        /// 元器件的中文名
        /// <para>暂时不用 2022.10.25</para>
        /// </summary>
        public string ChineseName { get; internal set; }

        public ElementConfigItem(ComponentType t, int m) {
            Type = t; MaxAmount = m;
        }
    }
}
