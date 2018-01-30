using System.Collections.Generic;
using System.Threading.Tasks;
using BleServer.Common.Domain;

namespace BleServer.Common.Services.BLE
{
    public interface IBleAdapter
    {
        Task<IEnumerable<BluetoothLEDevice>> GetDiscoveredDevices();
    }
}
