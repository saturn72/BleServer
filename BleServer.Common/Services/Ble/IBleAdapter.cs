using System.Collections.Generic;
using BleServer.Common.Domain;

namespace BleServer.Common.Services.Ble
{
    public interface IBleAdapter
    {
        IEnumerable<BleGattService> GetGattServices(string deviceId);
        event BluetoothDeviceEventHandler DeviceDiscovered;
    }

    public delegate void BluetoothDeviceEventHandler(IBleAdapter sender, BleDeviceEventArgs args);
}
