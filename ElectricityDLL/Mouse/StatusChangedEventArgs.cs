using System;

namespace ElectricityDLL.Mouse {
    public class StatusChangedEventArgs : EventArgs {
        public string Status { get; set; }
    }

    public delegate void StatusChangedHandler(object sender, StatusChangedEventArgs e);
}
