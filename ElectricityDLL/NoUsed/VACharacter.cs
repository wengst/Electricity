using System;

namespace ElectricityDLL {
    public class VACharacter {
        /// <summary>
        /// 电压
        /// </summary>
        public float V { get; private set; }
        /// <summary>
        /// 电流
        /// </summary>
        public float A { get; private set; }
        /// <summary>
        /// 功率
        /// </summary>
        public float P { get; private set; }
        /// <summary>
        /// 电阻
        /// </summary>
        public float R { get; private set; }

        public VACharacter() { }

        /// <summary>
        /// 以电压电流初始化
        /// </summary>
        /// <param name="v"></param>
        /// <param name="a"></param>
        public static VACharacter VA(float v, float a) {
            VACharacter va = new VACharacter {
                V = v,
                A = a,
                P = v * a,
                R = v / a
            };
            return va;
        }

        /// <summary>
        /// 以电压功率初始化
        /// </summary>
        /// <param name="v"></param>
        /// <param name="p"></param>
        public static VACharacter VP(float v, float p) {
            VACharacter va = new VACharacter() {
                V = v,
                P = p,
                R = v * v / p,
                A = p / v
            };
            return va;
        }

        /// <summary>
        /// 以功率电阻初始化
        /// </summary>
        /// <param name="p"></param>
        /// <param name="r"></param>
        public static VACharacter PR(float p, float r) {
            VACharacter va = new VACharacter() {
                P = p,
                R = r,
                V = (float)Math.Sqrt(p * r),
                A = (float)Math.Sqrt(p / r)
            };
            return va;
        }

        /// <summary>
        /// 以电流电阻初始化
        /// </summary>
        /// <param name="a"></param>
        /// <param name="r"></param>
        public static VACharacter IR(float a, float r) {
            VACharacter va = new VACharacter() {
                P = a * a * r,
                V = a * r,
                A = a,
                R = r
            };
            return va;
        }
    }
}
