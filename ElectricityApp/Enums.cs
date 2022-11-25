namespace ElectricityApp {
    /// <summary>
    /// 预置的主题
    /// </summary>
    public enum Theme {
        浅色 = 1,
        深色 = 2,
        粉色 = 3,
        蓝色 = 4,
        灰色 = 5
    }

    /// <summary>
    /// 放大倍率
    /// </summary>
    public enum Scales {
        一倍 = 1,
        两倍 = 2,
        三倍 = 3,
        四倍 = 4
    }

    /// <summary>
    /// 鼠标移动操作枚举
    /// </summary>
    public enum MouseMoveOperate { 

        UnKnow,
        /// <summary>
        /// 绘制新导线
        /// </summary>
        DrawNewWire,
        /// <summary>
        /// 按住导线一端拖拽
        /// </summary>
        DrawWrie,
        /// <summary>
        /// 拖拽元件
        /// </summary>
        DragDropElement,
        /// <summary>
        /// 按住导线中间，整体移动导线
        /// </summary>
        DragDropWire,
        /// <summary>
        /// 按住导线的贝塞尔曲线操作柄端，更改导线的贝塞尔曲线
        /// </summary>
        DragDropWireHandle,
        /// <summary>
        /// 移动滑动变阻器上的滑片
        /// </summary>
        DragDropVane
    }
}
