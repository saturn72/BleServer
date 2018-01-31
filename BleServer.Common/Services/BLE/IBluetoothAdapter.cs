
using System;

namespace BleServer.Common.Services.BLE
{
    public interface IBluetoothAdapter
    {
        event BluetoothDeviceEventHandler DeviceDiscovered;
    }

    public delegate void BluetoothDeviceEventHandler(IBluetoothAdapter sender, BluetoothDeviceEventArgs args);
}
