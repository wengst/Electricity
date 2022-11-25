using System;
using System.Windows.Forms;

namespace ElectricityApp {
    public partial class frmSaveImage : Form {
        public object SelectedObject { get { return cmbSaveRegion.SelectedItem; } }
        public frmSaveImage() {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void frmSaveImage_Load(object sender, EventArgs e) {
            cmbSaveRegion.SelectedIndex = 1;
        }
    }
}
