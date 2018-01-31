using System.Collections.Generic;
using BleServer.Common.Domain;

namespace BleServer.Common.Services.BLE
{
    public interface IBluetoothAdapter
    {
        IEnumerable<BluetoothGattService> GetGattServices(string deviceId);
        event BluetoothDeviceEventHandler DeviceDiscovered;
    }

    public delegate void BluetoothDeviceEventHandler(IBluetoothAdapter sender, BluetoothDeviceEventArgs args);
}
