using System;
using System.Drawing;
using System.Windows.Forms;

namespace ElectricityDLL {
    /// <summary>
    /// 拖拽数据
    /// </summary>
    public class DragDropInfo {

        public MouseButtons Button { get; set; } = MouseButtons.None;
        /// <summary>
        /// 最小移动间距
        /// </summary>
        public float MinMoveSpacing {
            get; set;
        }
        /// <summary>
        /// 鼠标左键按下时的坐标
        /// </summary>
        public PointF DownPoint { get; set; }

        /// <summary>
        /// 当前坐标
        /// </summary>
        public PointF CurrentPoint { get; set; }

        /// <summary>
        /// 上一次鼠标位置
        /// </summary>
        public PointF LastPoint { get; private set; }

        /// <summary>
        /// 操作
        /// </summary>
        public DragDropOperate Action { get; set; }
        /// <summary>
        /// 被操作的目标对象
        /// </summary>
        public EleComponent Target { get; set; }
        /// <summary>
        /// 被拖动对象的类型
        /// </summary>
        public ComponentType Type {
            get {
                if (Target != null) return Target.Type;
                return ComponentType.Other;
            }
        }
        /// <summary>
        /// 其他需要临时保存的数据
        /// </summary>
        public object Tag { get; set; }
        /// <summary>
        /// 拖拽开始时的对象坐标
        /// </summary>
        public PointF ObjectStartPoint { get; set; } = PointF.Empty;

        public void ContinueMove() {
            LastPoint = CurrentPoint;
        }

        public RectangleF LastRectangle { get; set; }

        /// <summary>
        /// 获取最近一次移动的偏移量
        /// </summary>
        public Offset Offset {
            get {
                Offset offset = new Offset(CurrentPoint.X - LastPoint.X, CurrentPoint.Y - LastPoint.Y);
                return offset;
            }
        }

        /// <summary>
        /// 获取从按下鼠标位置开始到现在位置的一个矩形
        /// </summary>
        public RectangleF MultipleRectangleF {
            get {
                float x = Math.Min(CurrentPoint.X, DownPoint.X);
                float y = Math.Min(CurrentPoint.Y, DownPoint.Y);
                float w = Math.Abs(CurrentPoint.X - DownPoint.X);
                float h = Math.Abs(CurrentPoint.Y - DownPoint.Y);
                return new RectangleF(x, y, w, h);
            }
        }

        /// <summary>
        /// 多选时的矩形
        /// </summary>
        public Rectangle MultipleRectangle {
            get {
                if (CurrentPoint != PointF.Empty) {
                    float x = Math.Min(DownPoint.X, CurrentPoint.X);
                    float y = Math.Min(DownPoint.Y, CurrentPoint.Y);
                    float w = Math.Abs(DownPoint.X - CurrentPoint.X);
                    float h = Math.Abs(DownPoint.Y - CurrentPoint.Y);
                    return new Rectangle((int)x, (int)y, (int)w, (int)h);
                }
                else {
                    return Rectangle.Empty;
                }
            }
        }

        /// <summary>
        /// 获取鼠标按下鼠标左键开始到当前位置的距离
        /// </summary>
        public float AllDistance {
            get {
                if (CurrentPoint != PointF.Empty) {
                    return (float)Fun.CalculateDistance(CurrentPoint, DownPoint);
                }
                return float.NaN;
            }
        }

        /// <summary>
        /// 获取最近一次拖拽或移动的距离
        /// </summary>
        public float MoveDistance {
            get {
                return (float)Math.Abs(Fun.CalculateDistance(LastPoint, CurrentPoint));
            }
        }

        public void MoveElement() {
            if (Action == DragDropOperate.DragDropElement && Target != null) {
                LastRectangle = Target.RegionWithText;
                Target.X = ObjectStartPoint.X + Offset.X;
                Target.Y = ObjectStartPoint.Y + Offset.Y;
                Target.IsDirty = true;
            }
        }

        public DragDropInfo(PointF point) {
            DownPoint = point;
            LastPoint = point;
            CurrentPoint = point;
        }

        public DragDropInfo() { }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset() {
            DownPoint = LastPoint = CurrentPoint = PointF.Empty;
            Target = null;
            Tag = null;
            Action = DragDropOperate.OnlyMove;
            Button = MouseButtons.None;
            MinMoveSpacing = 4;
        }

        /// <summary>
        /// 开始拖拽
        /// </summary>
        /// <param name="p"></param>
        public void StartDragDrop(PointF p) {
            DownPoint = LastPoint = CurrentPoint = p;
            Target = null;
            Tag = null;
            Action = DragDropOperate.OnlyMove;
            Button = MouseButtons.Left;
            MinMoveSpacing = 5;
        }

        public override string ToString() {
            string r = "Operate:" + Action.ToString();
            if (Action == DragDropOperate.MultipleChoice) {
                r += " StartPoint:{X=" + DownPoint.X.ToString() + ",Y=" + DownPoint.Y.ToString() + "}";
                r += " EndPoint:{X=" + CurrentPoint.X.ToString() + ",Y=" + CurrentPoint.Y.ToString() + "}";
            }
            else if (Target != null) {
                r += " Type:" + Type.ToString();
                r += " Id:" + Target.Id.ToString();
                r += " X:" + Target.X.ToString();
                r += " Y:" + Target.Y.ToString();
                r += " Width:" + Target.Width.ToString();
                r += " Height:" + Target.Height.ToString();
            }
            return r;
        }
    }
}
