using System;

namespace ElectricityDLL {
    public class OnSelectedElementEventArgs : EventArgs {
        public EleComponent SelectedObject { get; internal set; }
    }
    public delegate void OnSelectedElementHandler(object sender, OnSelectedElementEventArgs e);
}
