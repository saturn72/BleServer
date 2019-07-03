using System;
using ConnectivityServer.Common.Models;

namespace ConnectivityServer.Common.Services.Nfc
{
    public class NfcDeviceEventArgs : EventArgs
    {
        public NfcDeviceEventArgs(NfcDevice device)
        {
            Device = device;
        }
        public NfcDevice Device { get; }
    }
}
