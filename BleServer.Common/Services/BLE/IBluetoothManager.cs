using System.Collections.Generic;
using BleServer.Common.Domain;

namespace BleServer.Common.Services.BLE
{
    public interface IBluetoothManager
    {
        IEnumerable<BluetoothDevice> GetDiscoveredDevices();
        IEnumerable<BluetoothService> GetDeviceServices(string deviceId);
    }
}