using System.Collections.Generic;

namespace ElectricityDLL {
    public class EquationComparer : IComparer<Equation> {
        public int Compare(Equation a, Equation b) {
            int r = 0;
            if (a.FirstNonZeroIndex < b.FirstNonZeroIndex) {
                r = -1;
            }
            else if (a.FirstNonZeroIndex > b.FirstNonZeroIndex) {
                r = 1;
            }
            else {
                if (a.MaxCoefficients > b.MaxCoefficients) {
                    r = -1;
                }
                else if (a.MaxCoefficients < b.MaxCoefficients) {
                    r = 1;
                }
                else {
                    r = 0;
                }
            }
            return r; ;
        }
    }
}
