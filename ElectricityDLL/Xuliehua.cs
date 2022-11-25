using System;

namespace ElectricityDLL {
    /// <summary>
    /// 序列化
    /// </summary>
    public class Xuliehua : Attribute {
        public string Name { get; set; }
        public Xuliehua(string name) { Name = name; }
    }
    public class IgnoreXuliehua:Attribute { }
}
