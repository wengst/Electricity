using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectricityDLL {
    /*想用自己的属性表格呈现电路数据，但是已经使用了属性表格控件，所以这些就不用。另外，这个功能也没有完成。2022.10.25*/

    public abstract class EditableAttribute : Attribute {
        /// <summary>
        /// 是否合并列
        /// </summary>
        public bool IsMergeCol { get; set; } = false;

        /// <summary>
        /// 提示文本
        /// </summary>
        public string Caption { get; set; }

        public bool IsReadOnly { get; set; } = false;

        public int Order { get; set; } = 1;

        public class EditableComparer : IComparer<EditableAttribute> {
            public int Compare(EditableAttribute x, EditableAttribute y) {
                return x.Order - y.Order;
            }
        }
    }

    /// <summary>
    /// 数字输入框
    /// </summary>
    public class NumberUpDownAttribute : EditableAttribute {
        /// <summary>
        /// 最小值
        /// </summary>
        public float MinValue { get; set; } = 0f;
        /// <summary>
        /// 最大值
        /// </summary>
        public float MaxValue { get; set; } = 100f;
        /// <summary>
        /// 每次鼠标点击，改变的数值大小
        /// </summary>
        public float MouseUpDownValue { get; set; }
        /// <summary>
        /// 初始数据
        /// </summary>
        public float DefaultValue { get; set; }
        public NumberUpDownAttribute(string caption, float min = 1f, float max = 100f, float mudv = 1f, float defaultValue = 5f) {
            Caption = caption;
            MinValue = min;
            MaxValue = max;
            MouseUpDownValue = mudv;
            DefaultValue = defaultValue;
        }
    }

    /// <summary>
    /// 文本输入框
    /// </summary>
    public class TextInputAttribute : EditableAttribute {
        public TextInputAttribute(string caption) {
            Caption = caption;
        }
    }

    /// <summary>
    /// 下拉框
    /// </summary>
    public class DropdownAttribute : EditableAttribute {
        public List<string> Items { get; set; }
        public string DefaultValue { get; set; }
        public DropdownAttribute(string caption, string items, string defaultValue = "") {
            Caption = caption;
            Items = items.Split(',').ToList();
            DefaultValue = defaultValue;
        }
    }

    /// <summary>
    /// 图片显示框
    /// </summary>
    public class ImageAttribute : EditableAttribute {
        public ImageAttribute(string caption = "") {
            IsMergeCol = true;
            IsReadOnly = true;
            Caption = caption;
        }
    }

    /// <summary>
    /// 仅仅只是作为显示用
    /// </summary>
    public class LabelAttribute : EditableAttribute {
        public LabelAttribute(string caption) {
            Caption = caption;
            IsMergeCol = false;
            IsReadOnly = false;
        }
    }

    /// <summary>
    /// 单选按钮组
    /// </summary>
    public class RadioButtons : EditableAttribute {
        /// <summary>
        /// 每个单选按钮旁的文本
        /// </summary>
        public List<string> Items { get; set; }
    }
}
