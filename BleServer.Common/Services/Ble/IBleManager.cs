using System.Collections.Generic;
using System.Threading.Tasks;
using BleServer.Common.Domain;

namespace BleServer.Common.Services.Ble
{
    public interface IBleManager
    {
        IEnumerable<BleDevice> GetDiscoveredDevices();
        Task<IEnumerable<BleGattService>> GetDeviceGattServices(string deviceId);
        Task<IEnumerable<BleGattCharacteristic>> GetDeviceCharacteristics(string deviceId, string gattServiceId);
        Task<bool> Unpair(string deviceId);
        Task<bool> WriteToCharacteristric(string deviceId, string gattServiceId, string characteristicId, IEnumerable<byte> buffer);
    }
}