using System;

namespace ElectricityDLL {
    public class DragDropElementEventArgs : EventArgs {
        /// <summary>
        /// 拖放信息
        /// </summary>
        public DragDropInfo DDI { get; set; }
    }

    public delegate void DragDropHandler(object sender, DragDropElementEventArgs e);
}
