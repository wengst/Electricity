using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ElectricityApp {
    public partial class frmOptions : Form {
        public ElectricityDLL.Workbench Bench { get; set; }

        public frmOptions() {
            InitializeComponent();
        }

        private void frmOptions_Load(object sender, EventArgs e) {
            if (Bench != null) {
                chkShowElementName.Checked = Bench.IsShowElementName;
                chkShowWireName.Checked = Bench.IsShowWireName;
                chkAutoAnalyze.Checked = Bench.IsAutoAnalyze;
            }
        }

        private void chkShowElementName_CheckedChanged(object sender, EventArgs e) {
            if (Bench != null) {
                Bench.IsShowElementName = chkShowElementName.Checked;
                Bench.Draw();
            }
        }

        private void chkShowWireName_CheckedChanged(object sender, EventArgs e) {
            if (Bench != null) {
                Bench.IsShowWireName = chkShowWireName.Checked;
                Bench.Draw();
            }
        }

        private void chkAutoAnalyze_CheckedChanged(object sender, EventArgs e) {
            if (Bench != null) {
                Bench.IsAutoAnalyze = chkAutoAnalyze.Checked;
                ((Main)Owner).toolAutoAnalyaze.Visible = !Bench.IsAutoAnalyze;
                if (chkAutoAnalyze.Checked) {
                    Bench.DoTurnOnCircuit();
                }
                Bench.Draw();
            }
        }
    }
}
