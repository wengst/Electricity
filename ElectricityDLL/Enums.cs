using System;

namespace ElectricityDLL {
    [Flags]
    public enum FaultType {
        /// <summary>
        /// 无故障
        /// </summary>
        无 = 0,
        /// <summary>
        /// 短路
        /// </summary>
        短路 = 2,
        /// <summary>
        /// 开路
        /// </summary>
        断路 = 4
    }

    /// <summary>
    /// 电源极性
    /// </summary>
    public enum Polarity {
        /// <summary>
        /// 未设置
        /// </summary>
        Notset,
        /// <summary>
        /// 正极
        /// </summary>
        Positive,
        /// <summary>
        /// 负极
        /// </summary>
        Negative
    }

    /// <summary>
    /// 方程类型
    /// </summary>
    public enum EquationType {
        /// <summary>
        /// 节点电流
        /// </summary>
        KCL = 0,
        /// <summary>
        /// 环路电压
        /// </summary>
        KVL = 1
    }

    [Flags]
    public enum TerminalKey {
        UnKnow = 0,
        Left = 2,
        Right = 4,
        Middle = 8,
        LeftUp = 16,
        LeftDown = 32,
        RightUp = 64,
        RightDown = 128,
        MiddleUp = 256,
        MiddleDown = 512
    }

    /// <summary>
    /// 支路允许连接位置
    /// </summary>
    public enum AllowLink {
        /// <summary>
        /// 只允许左连接新支路
        /// </summary>
        Left,
        /// <summary>
        /// 只允许右连接新支路
        /// </summary>
        Right,
        /// <summary>
        /// 左右两边都可以连接新支路
        /// </summary>
        LeftAndRight,
        /// <summary>
        /// 不允许连接支路
        /// </summary>
        NotAllow
    }

    /// <summary>
    /// 电器元件类型
    /// </summary>
    public enum ComponentType {
        /// <summary>
        /// 导线
        /// </summary>
        Wire,
        /// <summary>
        /// 电源
        /// </summary>
        BatteryCase,
        /// <summary>
        /// 单刀单掷开关
        /// </summary>
        Switch,
        /// <summary>
        /// 单刀双掷开关
        /// </summary>
        Switch2,
        /// <summary>
        /// 电流表
        /// </summary>
        Ammeter,
        /// <summary>
        /// 电压表
        /// </summary>
        Voltmeter,
        /// <summary>
        /// 欧姆表
        /// </summary>
        Ohmmeter,
        /// <summary>
        /// 电能表
        /// </summary>
        Meter,
        /// <summary>
        /// 电阻器
        /// </summary>
        Resistor,
        /// <summary>
        /// 电阻丝
        /// </summary>
        ResistanceWire,
        /// <summary>
        /// 变阻器
        /// </summary>
        Rheostat,
        /// <summary>
        /// 小灯泡
        /// </summary>
        Lampstand,
        /// <summary>
        /// 发光二极管
        /// </summary>
        LED,
        /// <summary>
        /// 电动机
        /// </summary>
        Fan,
        /// <summary>
        /// 滑片
        /// </summary>
        Vane,
        /// <summary>
        /// 接线柱
        /// </summary>
        Terminal,
        /// <summary>
        /// 导线的贝塞尔曲线控制点
        /// </summary>
        WireControlPoint,
        /// <summary>
        /// 导线端点
        /// </summary>
        WireJunction,
        /// <summary>
        /// 开关的刀
        /// </summary>
        Knify,
        /// <summary>
        /// 双口插座
        /// </summary>
        DualPortSocket,
        /// <summary>
        /// 三口插座
        /// </summary>
        ThreePortSocket,
        /// <summary>
        /// 保险丝盒
        /// </summary>
        FuseBox,
        /// <summary>
        /// 其他
        /// </summary>
        Other
    }

    /// <summary>
    /// 元器件的工作状态
    /// <para>对于元件来说只有正常或工作，对于开关来说就只有闭合和断开</para>
    /// </summary>
    [Flags]
    public enum WorkStat {
        /// <summary>
        /// 正在工作
        /// </summary>
        Working = 2,
        /// <summary>
        /// 停止工作或开关出于断开状态
        /// </summary>
        StopOrOpen = 4
    }

    /// <summary>
    /// 鼠标移动操作枚举
    /// </summary>
    public enum DragDropOperate {

        UnKnow,
        /// <summary>
        /// 仅Move
        /// </summary>
        OnlyMove,
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
        /// 拖拽导线端点
        /// </summary>
        DragDropWirePoint,
        /// <summary>
        /// 移动滑动变阻器上的滑片
        /// </summary>
        DragDropVane,
        /// <summary>
        /// 多选(通过在绘图区画一个矩形，圈选元件，实现多选)
        /// </summary>
        MultipleChoice
    }

    /// <summary>
    /// 导线区域
    /// </summary>
    public enum WireArea {
        No,
        /// <summary>
        /// 导线身体
        /// </summary>
        Body,
        /// <summary>
        /// 贝塞尔手柄控制点1
        /// </summary>
        StartHandle,
        /// <summary>
        /// 贝塞尔手柄控制点1
        /// </summary>
        EndHandle,
        /// <summary>
        /// 终点
        /// </summary>
        EndPoint,
        /// <summary>
        /// 起点
        /// </summary>
        StartPoint
    }

    /// <summary>
    /// 电流方向
    /// </summary>
    [Flags]
    public enum AD {
        /// <summary>
        /// 未知
        /// </summary>
        UnKnow = 0,
        StartToEnd = 2,
        /// <summary>
        /// 流入
        /// </summary>
        EndToStart = 4,
        /// <summary>
        /// 流出
        /// </summary>
        TwoWay = 6
    }

    /// <summary>
    /// 电路类型
    /// </summary>
    [Flags]
    public enum CT {
        UnKnow = -1,
        /// <summary>
        /// 非电路（无电源或无通路或无电器元件）
        /// </summary>
        None = 0,
        /// <summary>
        /// 存在电源短路
        /// </summary>
        PowerShort = 2,
        /// <summary>
        /// 存在未闭合电路
        /// </summary>
        SectionOpen = 4,
        /// <summary>
        /// 电路全部断开
        /// </summary>
        AllOpen = 8,
        /// <summary>
        /// 电路存在故障
        /// </summary>
        HasFault = 16,
        /// <summary>
        /// 基本电路（1电源，1电器，1条路）
        /// </summary>
        Base = 1024,
        /// <summary>
        /// 串联电路
        /// </summary>
        Series = 2048,
        /// <summary>
        /// 并联电路
        /// </summary>
        Parallel = 4096,
        /// <summary>
        /// 桥电路
        /// </summary>
        Bridge = 8192,
        /// <summary>
        /// 混联电路
        /// </summary>
        Mix = 16384
    }

    /// <summary>
    /// 正负极连接的位置
    /// </summary>
    public enum LinkedArea {
        /// <summary>
        /// 未知
        /// </summary>
        UnKnow,
        /// <summary>
        /// 线路开始
        /// </summary>
        First,
        /// <summary>
        /// 线路尾部
        /// </summary>
        Last
    }
}
