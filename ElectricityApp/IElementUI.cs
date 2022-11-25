using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ElectricityApp
{
    public interface IElementUI
    {
        /// <summary>
        /// 按钮图片
        /// </summary>
        Image ButtonImage { get; }
        /// <summary>
        /// 不通电图片
        /// </summary>
        Image InitImage { get; }
        /// <summary>
        /// 工作图片
        /// </summary>
        Image WorkingImage { get; set; }

        /// <summary>
        /// 元件的初始数量
        /// </summary>
        int InitAmount { get; }
    }
}
