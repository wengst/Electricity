using ElectricityDLL.Mouse;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Text.Json.Serialization;

namespace ElectricityDLL {
    public partial class Workbench {

        #region Properties
        /// <summary>
        /// 鼠标拖拽数据
        /// </summary>
        [Browsable(false)]
        [JsonIgnore]
        public DragDropInfo DDI { get; } = new DragDropInfo();
        #endregion

        /// <summary>
        /// 获取鼠标移动到其上方的元件索引
        /// </summary>
        /// <returns></returns>
        private int GetMoveUpIndex() {
            int r = -1;
            for (int i = Items.Count - 1; i >= 0; i--) {
                if (Items[i].IsMouseUp) { r = i; break; }
            }
            return r;
        }

        /// <summary>
        /// 将所有元件的鼠标移动到其上方属性置false;
        /// </summary>
        private void SetAllEleNotMoveUp() {
            for (int i = 0; i < Items.Count; i++) {
                EleComponent item = Items[i];
                item.IsMouseUp = false;
            }
        }

        /// <summary>
        /// 设置所有元件为不选中状态
        /// </summary>
        private void SetAllEleNoSelected() {
            foreach (EleComponent ele in AllEleComponents) {
                ele.IsDirty = ele.IsSelected;
                ele.IsSelected = false;
            }
        }

        /// <summary>
        /// 设置鼠标移动到元件上方。返回是否需要刷新界面
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool SetMouseMoveUpStat(PointF point) {
            SetAllEleNotMoveUp();
            int nowMoveUpIndex = -1;
            for (int i = Items.Count - 1; i >= 0; i--) {
                EleComponent e = Items[i];
                if (e.GetIsMoveUp(point)) {
                    nowMoveUpIndex = i;
                    e.IsMouseUp = true;
                    break;
                }
            }
            //Console.WriteLine("LastIndex:" + LastMouseUpIndex.ToString() + "  ;  NowIndex:" + nowMoveUpIndex.ToString());
            if (LastMouseUpIndex != nowMoveUpIndex) {
                LastMouseUpIndex = nowMoveUpIndex;
                return true;
            }
            else {
                return false;
            }
        }


        private void SetAllEleMouseMoveUp(PointF p) {
            foreach (Element item in Elements) {
                item.MouseMove(p);
            }
        }

        /// <summary>
        /// 工作台某类器材的数量
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetElementCountByName(string symbol) {
            int m = 0;
            foreach (EleComponent eleComponent in Items) {
                if (eleComponent.Symbol == symbol) {
                    m++;
                }
            }
            return m;
        }

        public int GetElementCountByType(ComponentType type) {
            int m = 0;
            foreach (EleComponent eleComponent in Items) {
                if (eleComponent.Type == type) {
                    m++;
                }
            }
            return m;
        }

        private void SetMultipleChoiceObjs() {
            if (DDI.Action == DragDropOperate.MultipleChoice) {
                foreach (EleComponent ele in Items) {
                    if (ele.Region.IntersectsWith(DDI.MultipleRectangle)) {
                        if (!ele.IsSelected) { ele.IsDirty = true; } else { ele.IsDirty = false; }
                        ele.IsSelected = true;
                    }
                    else {
                        if (ele.IsSelected) {
                            ele.IsDirty = true;
                        }
                        else {
                            ele.IsDirty = false;
                        }
                        ele.IsSelected = false;
                    }
                }
            }
        }

        /// <summary>
        /// 鼠标在元件上方停留时发生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Mouse_Hover(object sender, MouseEventArgs e) {
            bool mu = false;
            //遍历下接线柱，看看鼠标有没有停留在接线柱上方
            foreach (Terminal terminal in Terminals) {
                mu = terminal.Contains(e.Location);
                terminal.IsDirty = terminal.IsMouseUp != mu;
                terminal.IsMouseUp = mu;
                if (mu) {
                    ((Element)terminal.Owner).MouseUpTerminal = terminal;
                    DDI.Target = terminal;
                }
                else {
                    ((Element)terminal.Owner).MouseUpTerminal = null;
                }
            }
            //遍历下滑片，看看鼠标有没有停留在滑片上方
            if (!mu) {
                foreach (Vane vane in Vanes) {
                    mu = vane.Contains(e.Location);
                    vane.IsDirty = vane.IsMouseUp != mu;
                    vane.IsMouseUp = mu;
                    if (mu) {
                        DDI.Target = vane;
                    }
                    else {
                        DDI.Target = null;
                    }
                }
            }

            if (!mu) {
                foreach (Knify knify in Knifies) {
                    mu = knify.Contains(e.Location);
                    knify.IsDirty = knify.IsMouseUp != mu;
                    knify.IsMouseUp = true;
                    if (mu) {
                        DDI.Target = knify;
                    }
                    else {
                        DDI.Target = null;
                    }
                }
            }

            //如果鼠标既没有停留在接线柱上方，也没有停留在滑片上方，那就看看鼠标有没有停留在元件上方
            if (!mu) {
                foreach (EleComponent ele in Items) {
                    mu = ele.Contains(e.Location);
                    ele.IsDirty = (ele.IsMouseUp != mu);
                    ele.IsMouseUp = mu;
                    if (mu) {
                        DDI.Target = ele;
                    }
                    else {
                        DDI.Target = null;
                    }
                }
            }
            //如果任何元件或部件的鼠标停留状态发生了改变，就重绘工作台
            if (IsDirty) {
                Draw();
            }
            if (OnDragDropElement != null) {
                OnDragDropElement(this, new DragDropElementEventArgs() { DDI = DDI });
            }
        }

        private void Mouse_Move_Element() {
            if (DDI != null) DDI.MoveElement();
            Draw();
        }

        private void Mouse_Move_Wire(Wire wire, PointF point) {

        }

        #region 鼠标事件方法
        [JsonIgnore]
        public StatusChangedHandler OnStatusChanged;

        [JsonIgnore]
        public DragDropHandler OnDragDropElement;

        /// <summary>
        /// 当选中某个元件时发生
        /// </summary>
        [JsonIgnore]
        public OnSelectedElementHandler OnSelectedElement;

        /// <summary>
        /// 工作区鼠标单击事件方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Mouse_Click(object sender, MouseEventArgs e) {
            //Console.WriteLine("Mouse_Click MousePoint=" + e.Location.ToString());
            bool b = false;
            for (int i = Items.Count - 1; i >= 0; i--) {
                EleComponent ele = Items[i];
                switch (ele.Type) {
                    case ComponentType.Wire:
                        Wire w = (Wire)ele;
                        b = w.Contains(e.Location);
                        w.IsDirty = w.IsSelected != b;
                        w.IsSelected = b;
                        break;
                    case ComponentType.Ammeter:
                    case ComponentType.BatteryCase:
                    case ComponentType.Fan:
                    case ComponentType.Lampstand:
                    case ComponentType.Meter:
                    case ComponentType.Ohmmeter:
                    case ComponentType.Other:
                    case ComponentType.Resistor:
                    case ComponentType.Rheostat:
                    case ComponentType.Switch:
                    case ComponentType.Voltmeter:
                        b = ele.Contains(e.Location);
                        ele.IsDirty = ele.IsSelected != b;
                        ele.IsSelected = b;
                        if (OnSelectedElement != null) {
                            OnSelectedElement(this, new OnSelectedElementEventArgs() { SelectedObject = (Element)ele }); ;
                        }
                        break;
                }
            }
            if (IsDirty) {
                Draw();
            }
        }

        /// <summary>
        /// 鼠标左键被按下事件方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Mouse_Down(object sender, MouseEventArgs e) {

            StopTimer();

            EleComponent component = GetComponentByPoint(e.Location);

            if (e.Button == MouseButtons.Left) {
                DDI.StartDragDrop(e.Location);
                DDI.Target = component;
                if (component == null) {
                    DDI.Action = DragDropOperate.MultipleChoice;
                    SetAllEleNoSelected();
                    if (IsDirty) {
                        Draw();
                    }
                }
            }

            if (OnStatusChanged != null) {
                StatusChangedEventArgs _e = new StatusChangedEventArgs() { Status = "" };

                OnStatusChanged(sender, _e);
            }
        }

        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Mouse_Move(object sender, MouseEventArgs e) {
            Wire w = null;
            WireArea a = WireArea.No;
            Terminal t = null;
            Junction j = null;
            Element ele = null;
            if (DDI.Button == MouseButtons.Left) {
                DDI.CurrentPoint = e.Location;
                //说明按下了鼠标左键并移动
                if (DDI.Target == null) {
                    //说明鼠标左键被按下的地方没有任何元件和部件（如接线柱、导线主体、导线控制端、导线端点、元件、滑片），那么此时就认为是在圈选
                    DDI.Action = DragDropOperate.MultipleChoice;
                    if (DDI.MoveDistance > DDI.MinMoveSpacing) {
                        SetMultipleChoiceObjs();
                        DDI.CurrentPoint = e.Location;
                        Draw();
                        Pen pen = new Pen(Color.Gray);
                        pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        G.DrawRectangle(pen, DDI.MultipleRectangle);
                    }
                }
                else {
                    switch (DDI.Type) {
                        case ComponentType.Terminal:
                            if (DDI.MoveDistance > MinWireLength) {
                                DDI.Action = DragDropOperate.DragDropWirePoint;
                                w = new Wire((Terminal)DDI.Target);
                                w.Bench = this;
                                w.SymbolName = GetSymbolName(w.Symbol);
                                w.SetJunction(e.Location, WireArea.EndPoint);
                                w.IsDirty = true;
                                w.IsSelected = true;
                                DDI.Target = w;
                                DDI.Tag = WireArea.EndPoint;
                                Items.Add(w);
                                Draw();
                                DDI.ContinueMove();
                            }
                            break;
                        case ComponentType.Wire:
                            switch (DDI.Action) {
                                case DragDropOperate.DragDropWirePoint:
                                    t = GetClosedTerminal(e.Location);
                                    a = (WireArea)DDI.Tag;
                                    //Console.WriteLine(a);
                                    w = (Wire)DDI.Target;
                                    w.IsDirty = true;
                                    if (DDI.MoveDistance > MinElementMoveDistance || t != null) {
                                        if (t != null) {
                                            w.SetJunction(t, a);
                                        }
                                        else {
                                            w.RemoveJunction(e.Location, a);
                                        }
                                        DDI.ContinueMove();
                                    }
                                    Draw();
                                    break;
                                case DragDropOperate.DragDropWireHandle:
                                    w = (Wire)DDI.Target;
                                    a = (WireArea)DDI.Tag;
                                    if (DDI.MoveDistance > MinElementMoveDistance) {
                                        w.SetJunction(e.Location, a);
                                        DDI.ContinueMove();
                                    }
                                    break;
                                case DragDropOperate.DragDropWire:
                                    if (DDI.MoveDistance > MinElementMoveDistance) {
                                        w = (Wire)DDI.Target;
                                        a = (WireArea)DDI.Tag;
                                        if (a == WireArea.Body) {
                                            w.Move(DDI.Offset);
                                        }
                                        DDI.ContinueMove();
                                    }
                                    break;
                            }
                            break;
                        case ComponentType.WireJunction:
                            if (DDI.MoveDistance > MinElementMoveDistance) {
                                j = (Junction)DDI.Target;
                                w = (Wire)j.Owner;
                                if (j.Area == WireArea.EndPoint || j.Area == WireArea.StartPoint) {
                                    t = GetClosedTerminal(e.Location);
                                    if (t != null) {
                                        w.SetJunction(j, t);
                                    }
                                    else {
                                        if (j.T != null) {
                                            w.RemoveJunction(j, e.Location);
                                        }
                                        else {
                                            w.SetJunction(j, e.Location);
                                        }
                                    }
                                }
                                else if (j.Area == WireArea.EndHandle || j.Area == WireArea.StartHandle) {
                                    w.SetJunction(j, e.Location);
                                }
                                DDI.ContinueMove();
                                Draw();
                            }
                            break;
                        case ComponentType.Vane:
                            Vane v = (Vane)DDI.Target;
                            Rheostat rheostat = (Rheostat)v.Owner;
                            rheostat.MoveVane(DDI.Offset);
                            DDI.ContinueMove();
                            if (IsAutoAnalyze) {
                                DoTurnOnCircuit();
                            }
                            else {
                                Draw();
                            }
                            break;
                        case ComponentType.Ammeter:
                        case ComponentType.BatteryCase:
                        case ComponentType.Fan:
                        case ComponentType.Lampstand:
                        case ComponentType.Meter:
                        case ComponentType.Ohmmeter:
                        case ComponentType.Other:
                        case ComponentType.Resistor:
                        case ComponentType.Rheostat:
                        case ComponentType.Switch:
                        case ComponentType.Voltmeter:
                            ele = (Element)DDI.Target;
                            ele.Move(DDI.Offset);
                            //Console.WriteLine(DDI.Offset);
                            DDI.ContinueMove();
                            Draw();
                            break;
                    }
                }

                if (OnStatusChanged != null) {
                    OnStatusChanged(sender, new StatusChangedEventArgs());
                }
            }
            else {

                Mouse_Hover(sender, e);
                DDI.ContinueMove();
            }
            if (OnDragDropElement != null) {
                if (DDI.Target != null) {
                    OnDragDropElement(this, new DragDropElementEventArgs() { DDI = DDI });
                }
                else {
                    OnDragDropElement(this, new DragDropElementEventArgs());
                }
            }
        }

        /// <summary>
        /// 鼠标左键被弹起事件方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Mouse_Up(object sender, MouseEventArgs e) {
            if (OnSelectedElement != null) {
                if (DDI != null && DDI.Target != null) {
                    OnSelectedElement(this, new OnSelectedElementEventArgs() { SelectedObject = DDI.Target });
                }
                else {
                    OnSelectedElement(this, new OnSelectedElementEventArgs() { SelectedObject = null });
                }
            }
            if (DDI.Target != null) {
                switch (DDI.Type) {
                    case ComponentType.Ammeter:
                    case ComponentType.BatteryCase:
                    case ComponentType.Fan:
                    case ComponentType.Lampstand:
                    case ComponentType.Meter:
                    case ComponentType.Ohmmeter:
                    case ComponentType.Other:
                    case ComponentType.Resistor:
                    case ComponentType.Rheostat:
                    case ComponentType.Switch:
                    case ComponentType.Voltmeter:
                        DDI.Reset();
                        break;
                    case ComponentType.Wire:
                        DDI.Reset();
                        break;
                    case ComponentType.WireJunction:
                        Junction j = (Junction)DDI.Target;
                        Wire w = (Wire)j.Owner;
                        w.IsSelected = true;
                        DDI.Reset();
                        break;
                    case ComponentType.Knify:
                        Knify k = (Knify)DDI.Target;
                        Switch s = (Switch)k.Owner;
                        s.ChangeStat();
                        if (IsAutoAnalyze) {
                            DoTurnOnCircuit();
                        }
                        break;
                    case ComponentType.Vane:
                        DDI.Reset();
                        break;
                    case ComponentType.Terminal:

                        break;
                }
            }
            else {
                DDI.Reset();
                Draw();
            }
            if (DDI != null && DDI.Button != MouseButtons.None) {
                DDI.Button = MouseButtons.None;
            }
            if (IsDirty) {
                Draw();
            }
        }

        public void Mouse_WHEEL(object sender, MouseEventArgs e) {
            EleComponent ele = GetComponentByPoint(e.Location);
            if (ele != null && e.Delta != 0) {
                if (ele.Type == ComponentType.Ammeter || ele.Type == ComponentType.Voltmeter) {
                    Console.WriteLine("Delta=" + e.Delta);
                    int scale = ele.Scale;
                    scale += e.Delta / 120;
                    if (scale > 4) scale = 4;
                    if (scale < 1) scale = 1;
                    Console.WriteLine("scale=" + scale);
                    if (ele.Scale != scale) {
                        ele.Scale = scale;
                        ele.IsDirty = true;
                    }
                }
            }
            if (IsDirty) {
                Draw();
            }
        }
        #endregion
    }
}
