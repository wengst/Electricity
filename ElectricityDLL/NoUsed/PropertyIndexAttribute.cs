using System;
using System.ComponentModel;

namespace ElectricityDLL {
    /*用于属性表格控件的属性排序，由于对于排序还没有搞懂，所以没有用这排序。2022/10/25 */


    /// <summary>
    /// 属性排序元数据类型
    /// </summary>
    public class PropertyIndexAttribute : Attribute {
        /*暂时还没有用上*/
        public int Index { get; } = 10;

        public PropertyIndexAttribute(int i) {
            Index = i;
        }
    }

    public class PropertyOrderComparer : IComparable<PropertyIndexAttribute> {
        private PropertyDescriptor _property;
        private string _name;
        private int _index;
        public PropertyOrderComparer(string name, int index, PropertyDescriptor pd) {
            _name = name;
            _index = index;
            _property = pd;
        }

        public int CompareTo(PropertyIndexAttribute other) {
            return 0;
        }
    }
}
