using System;
using System.Linq;

namespace ElectricityDLL {
    public static class Consts {

        #region PropertyGrid DisplayName Description Category
        //Category Name
        public const string PGC_cat1 = "设计参数";
        public const string PGC_cat2 = "运行结果";
        public const string PGC_cat3 = "名称标识";
        public const string PGC_cat4 = "电路连接";
        public const string PGC_Action = "行为";
        public const string PGC_Property = "属性";
        public const string PGC_Sharp = "形状数据";
        public const string PGDN_Size = "尺寸";
        public const string PGDN_Point = "坐标";
        public const string PGDN_LineColor = "线条颜色";
        public const string PGDN_LineWidth = "线条宽度";
        public const string PGDN_TextOffset = "名称偏移比例";
        public const string PGDN_LeftToRight = "左侧滑片";
        public const string PGDN_SymbolFont = "文本字体";

        //Property DisplayName
        public const string PGC_unknow = "陷入沉思";
        public const string PG_RatedPower = "额定功率";
        public const string PG_Power = "功率";
        public const string PG_RatedVoltage = "额定电压";
        public const string PG_RatedVoltage_Description = "额定电压，设计时根据使用环境确定。比如以汽车蓄电池为电源的，额定电压必须是12V。";
        public const string PG_Voltage = "电压";
        public const string PG_PowerVoltage = "电源电压";
        public const string PG_PowerVoltage_Description = "电源电压，单位伏特(V)。可更改电源电压。更改后需重新计算，再单击需要查看的元件可获得更新后数据";
        public const string PG_RatedCurrent = "额定电流";
        public const string PG_Current = "电流";
        public const string PG_Resistance = "电阻";
        public const string PG_InductiveReactance = "感抗";
        public const string PG_InductiveReactance_Description = "当线圈中有电流通过时，就会在线圈中形成感应电磁场，而感应电磁场又会在线圈中产生感应电流来抵制通过线圈中的电流。因此，我们把这种电流与线圈之间的相互作用称其为电的感抗，也就是电路中的电感。";
        public const string PG_MaxResistance = "最大阻值";
        public const string PG_LeftResistance = "左侧电阻";
        public const string PG_RightResistance = "右侧电阻";
        public const string PG_LeftVoltage = "左侧电压";
        public const string PG_RightVoltage = "右侧电压";
        public const string PG_Resistivity = "电阻率";
        public const string PG_Length = "长度";
        public const string PG_Thickness = "粗细";
        public const string PG_Efficiency = "效率";
        public const string PG_LinkedCircuit = "连接到电路";
        public const string PG_LinkedCircuit_Description = "是否是参与电路计算的一部分";
        public const string PG_Fault = "故障";
        public const string PG_ElementName = "器件名称";
        public const string PG_ElementName_Description = "电器元件的名称";
        public const string PG_ElementId = "器件Id";
        public const string PG_Range = "量程";
        public const string PG_MeterDisplay = "示数";
        public const string PGDV_FaultStrs = "无,短路,断路";
        public const string PGDV_FaultStrs_Description = "电器元件是否存在某种故障";
        #endregion

        #region Math
        public const float ZeroValue = 0.0001f;
        public const string Accuracy1 = "f1";
        public const string Accuracy2 = "f2";
        #endregion

        #region Json Name
        public const string Json_ShowElementName = "ShowElementName";
        public const string Json_ShowWireName = "ShowWireName";
        public const string Json_AutoAnalyze = "AutoAnalyze";
        public const string Json_Locked = "Locked";
        public const string Json_Ideal = "Ideal";
        public const string Json_Wires = "Wires";
        public const string Json_Elements = "Elements";

        public const string Json_Junctions = "Junctions";
        public const string Json_Terminal = "Terminal";
        public const string Json_Id = "Id";
        public const string Json_X = "X";
        public const string Json_Y = "Y";
        public const string Json_Type = "Type";
        public const string Json_Name = "Name";
        public const string Json_Scale = "Scale";
        public const string Json_Stat = "Stat";
        public const string Json_Fault = "Fault";
        public const string Json_Points = "Points";
        #endregion

        #region Convert
        /// <summary>
        /// Json占位符
        /// </summary>
        public const string ZWF = "[-]";

        /// <summary>
        /// String To Int
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int StrToInt(string s) {
            int n = 0;
            if (int.TryParse(s, out n)) {
                return n;
            }
            return n;
        }

        /// <summary>
        /// String To Float
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static float StrToFloat(string s) {
            float n = 0;
            if (float.TryParse(s, out n)) {
                return n;
            }
            return n;
        }

        /// <summary>
        /// String To Boolean
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool StrToBool(string s) {
            return s == "1" ? true : false;
        }

        /// <summary>
        /// Boolean To String
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string BoolToStr(bool b) {
            return b ? "1" : "0";
        }

        /// <summary>
        /// String To ComponentType
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static ComponentType StrToType(string s) {
            int t;
            if (int.TryParse(s, out t)) {
                return (ComponentType)t;
            }
            return ComponentType.Other;
        }

        /// <summary>
        /// ComponentType To String
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string TypeToStr(ComponentType type) {
            return ((int)type).ToString();
        }
        #endregion

        public static string DeleteFirstChar(string source, string c) {
            int cl = c.Length;
            int sl = source.Length;
            if (source.Substring(0, cl) == c) {
                source = source.Substring(cl, sl - cl);
            }
            return source;
        }

        public static string DeleteLastChar(string source, string c) {
            int cl = c.Length;
            int sl = source.Length;
            if (source.Substring(sl - cl, cl) == c) {
                source = source.Substring(0, sl - cl);
            }
            return source;
        }

        /// <summary>
        /// 删除两端的指定字符
        /// </summary>
        /// <param name="source"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string DeleteBothEndChar(string source, string start, string end) {
            source = DeleteFirstChar(source, start);
            source = DeleteLastChar(source, end);
            return source;
        }

        public static bool IsZero(double d) {
            return Math.Abs(d) <= ZeroValue;
        }

        public static bool IsZero(float f) {
            return Math.Abs(f) <= ZeroValue;
        }

        public static bool IsZero(decimal d) { return (float)Math.Abs(d) <= ZeroValue; }
    }
}
