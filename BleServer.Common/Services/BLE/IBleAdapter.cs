using System.Collections.Generic;
using BleServer.Common.Domain;

namespace BleServer.Common.Services.BLE
{
    public interface IBleAdapter
    {
        IEnumerable<BluetoothLEDevice> GetDiscoveredDevices();
    }
}
