using System;
using BleServer.Common.Domain;

namespace BleServer.Common.Services.BLE
{
    public class BluetoothDeviceEventArgs : EventArgs
    {
        public BluetoothDeviceEventArgs(BluetoothDevice device)
        {
            Device = device;
        }
        public BluetoothDevice Device { get; }
    }
}