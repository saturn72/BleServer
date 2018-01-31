using System.Collections.Generic;
using System.Threading.Tasks;
using BleServer.Common.Domain;

namespace BleServer.Common.Services.BLE
{
    public interface IBluetoothService
    {
        Task<IEnumerable<BluetoothDevice>> GetDevices();
        Task<BluetoothDevice> GetDeviceById(string deviceId);
    }
}
