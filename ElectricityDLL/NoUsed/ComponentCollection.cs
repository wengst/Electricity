using System.Collections.Generic;
using System.Drawing;

namespace ElectricityDLL {
    public class ComponentCollection {
        /// <summary>
        /// 获取所有元件集合
        /// </summary>
        public List<EleComponent> Items { get; } = new List<EleComponent>();

        public int Count { get { return Items.Count; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public EleComponent GetComponentByPoint(PointF point) {
            for (int i = Items.Count - 1; i >= 0; i--) {
                if (Items[i].GetIsMoveUp(point)) { return Items[i]; }
            }
            return null;
        }

        /// <summary>
        /// 获取元件索引
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public int GetComponentIndex(EleComponent c) {
            for (int i = 0; i < Items.Count; i++) {
                if (Items[i].Id == c.Id) return i;
            }
            return -1;
        }

        /// <summary>
        /// 是否属于动画
        /// </summary>
        public bool IsAnimation {
            get {
                for (int i = 0; i < Items.Count; i++) {
                    if (Items[i].IsAnimation) return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 根据Index获取元件
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public EleComponent GetComponentByIndex(int index) {
            if (index >= 0 && index < Items.Count) { return Items[index]; }
            return null;
        }

        /// <summary>
        /// 非导线类元件
        /// </summary>
        public List<Element> Elements {
            get {
                List<Element> list = new List<Element>();
                for (int i = 0; i < Items.Count; i++) {
                    if (Items[i].GetType().BaseType == typeof(Element)) {
                        list.Add((Element)Items[i]);
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// 非导线类元件总数
        /// </summary>
        public int ElementCount {
            get {
                return Elements.Count;
            }
        }

        public void Add(EleComponent e) {
            Items.Add(e);
        }
    }
}
