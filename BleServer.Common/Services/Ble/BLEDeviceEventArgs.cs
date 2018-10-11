using System;
using BleServer.Common.Models;

namespace BleServer.Common.Services.Ble
{
    public class BleDeviceEventArgs : EventArgs
    {
        public BleDeviceEventArgs(BleDevice device)
        {
            Device = device;
        }
        public BleDevice Device { get; }
    }
}