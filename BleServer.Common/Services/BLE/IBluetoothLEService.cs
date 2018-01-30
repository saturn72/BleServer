using System.Collections.Generic;
using System.Threading.Tasks;
using BleServer.Common.Domain;

namespace BleServer.Common.Services.BLE
{
    public interface IBluetoothLEService
    {
        Task<IEnumerable<BluetoothLEDevice>> GetDevices();
        Task<BluetoothLEDevice> GetDeviceById(string deviceId);
    }
}
