using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text.Json.Serialization;

namespace ElectricityDLL {
    [Serializable]
    public class EleComponent {
        [JsonIgnore]
        public static int Identity = 100;

        private static int GetEleComponentId() {
            Identity++;
            return Identity;
        }

        /// <summary>
        /// 字符串数据转float
        /// </summary>
        /// <param name="s"></param>
        /// <param name="u"></param>
        /// <param name="dv"></param>
        /// <returns></returns>
        protected float StrToFloat(string s, string u, float dv = float.NaN) {
            float a = dv;
            if (!string.IsNullOrEmpty(s)) {
                int i = s.IndexOf(u);
                if (i == -1) {
                    if (!float.TryParse(s, out a)) {
                        a = dv;
                    }
                }
                else if (i == s.Length - 1) {
                    s = s.Substring(0, s.Length - 1);
                    if (!float.TryParse(s, out a)) {
                        a = dv;
                    }
                }
            }
            return a;
        }

        /// <summary>
        /// 格式化数字
        /// </summary>
        /// <param name="a"></param>
        /// <param name="u"></param>
        /// <returns></returns>
        protected string FormatNumeric(float a, string u) {
            if (Consts.IsZero(a)) {
                return "0 " + u;
            }
            else if (float.IsNaN(a)) {
                return "-- " + u;
            }
            else if (float.IsInfinity(a)) {
                return "∞ " + u;
            }
            else {
                float x1 = (int)a;
                float x10 = (int)(a * 10.001f);
                float x100 = (int)(a * 100.0001f);
                Console.WriteLine("a*10=" + a * 10f);
                Console.WriteLine("x10=" + x10);
                Console.WriteLine("x10-a*10=" + (x10 - a * 10f));
                if (Consts.IsZero(a - x1)) {
                    return a.ToString() + " " + u;
                }
                else if (Consts.IsZero(a * 10f - x10)) {
                    return a.ToString("f1") + " " + u;
                }
                else if (Consts.IsZero(a * 100f - x100)) {
                    return a.ToString("f2") + " " + u;
                }
                else {
                    return a.ToString("f2") + " " + u;
                }
            }
        }

        /// <summary>
        /// 对工作台的引用
        /// </summary>
        [Browsable(false)]
        [JsonIgnore]
        public Workbench Bench { get; internal set; }

        /// <summary>
        /// 是否用电器
        /// </summary>
        [Browsable(false)]
        [JsonIgnore]
        public bool IsDevice {
            get {
                switch (Type) {
                    case ComponentType.Fan:
                    case ComponentType.Lampstand:
                    case ComponentType.Resistor:
                    case ComponentType.Rheostat:
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 是否是仪表
        /// </summary>
        [Browsable(false)]
        [JsonIgnore]
        public bool IsMeter {
            get {
                switch (Type) {
                    case ComponentType.Ammeter:
                    case ComponentType.Voltmeter:
                    case ComponentType.Meter:
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 是否是电源
        /// </summary>
        [Browsable(false)]
        [JsonIgnore]
        public bool IsPower {
            get {
                switch (Type) {
                    case ComponentType.BatteryCase:
                        return true;
                }
                return false;
            }

        }

        /// <summary>
        /// 是否是电压表
        /// </summary>
        [Browsable(false)]
        [JsonIgnore]
        public bool IsVoltmater {
            get {
                return Type == ComponentType.Voltmeter;
            }
        }

        /// <summary>
        /// 是否是电流表
        /// </summary>
        [Browsable(false)]
        [JsonIgnore]
        public bool IsAmmeter {
            get { return Type == ComponentType.Ammeter; }
        }

        [Category(Consts.PGC_cat4), DisplayName(Consts.PG_LinkedCircuit), Description(Consts.PG_LinkedCircuit_Description)]
        [JsonIgnore]
        public virtual string LinkedCircuit {
            get {
                if (Bench != null) {
                    List<Circuit> cs = Bench.EquationCircuits;
                    if (cs.Count > 0) {
                        foreach (Circuit circuit in cs) {
                            if (circuit.IsContainElement(this)) {
                                return "已经连接到电路";
                            }
                        }
                    }
                }
                return Consts.PGC_unknow;
            }
        }

        [Browsable(false)]
        [JsonPropertyName(Consts.Json_Ideal)]
        [JsonIgnore]
        public bool IsIdeal { get; set; } = true;

        [JsonIgnore]
        [Browsable(false)]
        public bool IsShowMainProperties { get; set; } = false;

        /// <summary>
        /// 故障
        /// </summary>
        [JsonPropertyName(Consts.Json_Fault)]
        [Dropdown(Consts.PG_Fault, Consts.PGDV_FaultStrs), Category(Consts.PGC_Property), DisplayName(Consts.PG_Fault), Description(Consts.PGDV_FaultStrs_Description), ReadOnly(true)]
        [Browsable(false)]
        [Xuliehua(Consts.Json_Fault)]
        public virtual FaultType Fault { get; set; } = FaultType.无;

        /// <summary>
        /// 帧率
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public int FPS { get; set; } = 10;

        /// <summary>
        /// 帧索引
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public int FrameIndex { get; set; } = 0;

        /// <summary>
        /// 流经电器元件的电流强度
        ///电流可以是负值；
        ///如果计算得到的电流是负数，表示电流方向与参考电流方向相反
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public float Current { get; set; }

        /// <summary>
        /// 符号名前导字符
        /// </summary>
        [JsonPropertyName("sb")]
        [Browsable(false)]
        [JsonIgnore]
        public string Symbol { get; protected set; }

        /// <summary>
        /// 符号名
        /// </summary>
        [JsonPropertyName("sn")]
        [Label("符号名", Order = 0)]
        [Description(Consts.PG_ElementName_Description), DisplayName(Consts.PG_ElementName), Category(Consts.PGC_Property)]
        [Browsable(false)]
        [Xuliehua(Consts.Json_Name)]
        public virtual string SymbolName { get; set; }

        [Category(Consts.PGC_cat3), DisplayName("元件名")]
        [JsonIgnore]
        public virtual string CnName {
            get {
                return Fun.GetChineseName(Type) + SymbolName;
            }
        }

        [Category(Consts.PGC_cat3), DisplayName("理想模型"), Description("理想模型即不考虑电源内阻、导线电阻、电流表内阻为零、电压表内阻无穷大、温度变化、电感变化等情况的电路模型")]
        [JsonIgnore]
        public string IsIdealStr {
            get {
                return IsIdeal ? "是" : "否";
            }
        }

        /// <summary>
        /// 全局唯一ID
        /// </summary>
        [JsonIgnore]
        [Category(Consts.PGC_cat3), DisplayName("元件Id"), Description("该元件的唯一Id属性，使用者无需关心")]
        [Xuliehua(Consts.Json_Id)]
        public int Id { get; internal set; } = GetEleComponentId();

        /// <summary>
        /// 元件所有者
        /// </summary>
        [Browsable(false)]
        [JsonIgnore]
        public EleComponent Owner { get; set; } = null;

        /// <summary>
        /// 元器件类型
        /// </summary>
        [JsonPropertyName(Consts.Json_Type)]
        [Browsable(false)]
        [Xuliehua(Consts.Json_Type)]
        public virtual ComponentType Type { get; protected set; }

        /// <summary>
        /// 元件的工作状态
        /// </summary>
        [JsonPropertyName(Consts.Json_Stat)]
        [Browsable(false)]
        [Xuliehua(Consts.Json_Stat)]
        public virtual WorkStat Stat { get; set; } = WorkStat.StopOrOpen;

        /// <summary>
        /// 当前元器件是否出于选中状态
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public bool IsSelected { get; set; }

        /// <summary>
        /// 是否是鼠标坐标点下最上方的元件
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public bool IsMouseUp { get; set; }

        /// <summary>
        /// 形状是否需要重绘
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public bool IsDirty { get; set; }

        /// <summary>
        /// 最终输出到屏幕的图片
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public Image OutputImage { get; protected set; }

        [JsonIgnore]
        [Browsable(false)]
        public virtual bool IsAnimation { get; protected set; }

        [JsonPropertyName(Consts.Json_X)]
        [Browsable(false)]
        [Xuliehua(Consts.Json_X)]
        public virtual float X { get; set; }

        [JsonPropertyName(Consts.Json_Y)]
        [Browsable(false)]
        [Xuliehua(Consts.Json_Y)]
        public virtual float Y { get; set; }

        /// <summary>
        /// 宽
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public virtual float Width { get; set; }

        /// <summary>
        /// 高
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public virtual float Height { get; set; }

        /// <summary>
        /// 图形区域
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public virtual RectangleF Region {
            get { return new RectangleF(X, Y, Width, Height); }
            set { X = value.X; Y = value.Y; Width = value.Width; Height = value.Height; }
        }

        [JsonIgnore]
        [Browsable(false)]
        public RectangleF TextRegion { get; set; }

        [JsonIgnore]
        [Browsable(false)]
        public RectangleF RegionWithText {
            get {
                RectangleF rect = new RectangleF(Math.Min(X, TextRegion.X), Math.Min(Y, TextRegion.Y), Width, Height + 1 + TextRegion.Height);
                rect.Inflate(4, 4);
                return rect;
            }
        }

        public virtual bool Contains(PointF point) {
            return Region.Contains(point);
        }

        public virtual void Move(Offset offset) {
            X += offset.X;
            Y += offset.Y;
            IsDirty = true;
        }

        /// <summary>
        /// 缩放比例
        /// </summary>
        [JsonPropertyName(Consts.Json_Scale)]
        [Browsable(false)]
        [Xuliehua(Consts.Json_Scale)]
        public virtual int Scale { get; set; }

        /// <summary>
        /// 元件左上角的工作区坐标
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public virtual PointF WorldPoint { get { return new PointF(X, Y); } }

        /// <summary>
        /// 元件在工作区的矩形
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public virtual RectangleF EleRectangleF { get { return new RectangleF(X, Y, Width, Height); } }

        /// <summary>
        /// 将本地矩形转换为世界矩形
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        protected RectangleF L2W_RectangleF(float x, float y, float w, float h) {
            return new RectangleF(X + x, Y + y, w, h);
        }

        protected RectangleF L2W_RectangleF(PointF localPoint, SizeF sizef) {
            return new RectangleF(new PointF(X + localPoint.X, Y + localPoint.Y), sizef);
        }

        /// <summary>
        /// 将本地矩形转换为世界矩形
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        protected Rectangle L2W_Rectangle(float x, float y, float w, float h) {
            return new Rectangle((int)(X + x), (int)(Y + y), (int)w, (int)h);
        }

        protected Rectangle L2W_Rectangle(PointF localPoint, SizeF sizef) {
            return new Rectangle(new Point((int)(X + localPoint.X), (int)(Y + localPoint.Y)), new Size((int)sizef.Width, (int)sizef.Height));
        }

        /// <summary>
        /// 返回选中时的矩形区
        /// </summary>
        [JsonIgnore]
        [Browsable(false)]
        public RectangleF GetSelectedRectangleF {
            get {
                RectangleF rect = EleRectangleF;
                rect.Inflate(2, 2);
                return rect;
            }
        }

        [JsonIgnore]
        [Browsable(false)]
        public Rectangle SelectedRectangle {
            get {
                return GetInflateRect(2, 2);
            }
        }

        /// <summary>
        /// 返回扩边后的矩形
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        [Browsable(false)]
        public Rectangle GetInflateRect(int w, int h) {
            Rectangle rect = new Rectangle((int)X, (int)Y, (int)Width, (int)Height);
            rect.Inflate(w, h);
            return rect;
        }

        [JsonIgnore]
        [Browsable(false)]
        public SizeF EleSize { get { return new SizeF(Width, Height); } }

        /// <summary>
        /// 鼠标是否移动到其上方
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public bool GetIsMoveUp(PointF worldPoint) { return EleRectangleF.Contains(worldPoint); }

        public virtual void Draw(Graphics g, Color BC, Color FC, Font f) { }

        public class EleComponentComparer : IComparer<EleComponent> {
            public int Compare(EleComponent a, EleComponent b) {
                string aStr = a.ToString();
                string bStr = b.ToString();
                int len = Math.Min(aStr.Length, bStr.Length);
                char[] achar = aStr.ToCharArray();
                char[] bchar = bStr.ToCharArray();
                for (int i = 0; i < len; i++) {
                    if (achar[i] < bchar[i]) {
                        return -1;
                    }
                    else if (achar[i] > bchar[i]) {
                        return 1;
                    }
                }
                if (aStr.Length > bStr.Length) {
                    return 1;
                }
                else if (aStr.Length < bStr.Length) {
                    return -1;

                }
                else {
                    return 0;
                }
            }
        }
        public EleComponent() { }

        public static bool IsEquPotential(Terminal t1, Terminal t2) {
            if (t1 != null && t2 != null) {
                EleComponent e1 = t1.Owner;
                EleComponent e2 = t2.Owner;
                int k = (int)t1.Key + (int)t2.Key;
                if (e1.Id == e2.Id) {
                    //两接线柱是同一元件的两端
                    switch (e1.Type) {
                        case ComponentType.Switch:
                            Switch sw = (Switch)e1;
                            if (sw.Fault == FaultType.短路 ||
                                (sw.Fault == FaultType.无 && sw.Stat == WorkStat.Working)) {
                                return true;
                            }
                            else {
                                return false;
                            }
                        case ComponentType.BatteryCase:
                            return false;
                        case ComponentType.Ammeter:
                            if ((e1.IsIdeal && (k == 6 || k == 10) || e1.Fault == FaultType.短路)) {
                                return true;
                            }
                            else {
                                return false;
                            }
                        case ComponentType.Fan:
                        case ComponentType.Lampstand:
                        case ComponentType.Resistor:
                            if (e1.Fault == FaultType.短路) {
                                return true;
                            }
                            else {
                                return false;
                            }
                        case ComponentType.Rheostat:
                            Rheostat rheostat = (Rheostat)e1;
                            if (rheostat.GetResistance(t1, t2) == 0f) {
                                return true;
                            }
                            else {
                                return false;
                            }
                        case ComponentType.Voltmeter:
                            return false;
                    }
                }
                else {
                    //两者不是同一元件的接线柱
                    foreach (Junction j1 in t1.Junctions) {
                        foreach (Junction j2 in t2.Junctions) {
                            if (j1.Owner.Id == j2.Owner.Id && j1.Owner.Fault != FaultType.断路) {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
