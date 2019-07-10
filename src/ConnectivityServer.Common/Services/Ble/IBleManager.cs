using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectivityServer.Common.Models;

namespace ConnectivityServer.Common.Services.Ble
{
    public interface IBleManager
    {
        IEnumerable<BleDevice> GetDiscoveredDevices();
        Task<IEnumerable<BleGattService>> GetDeviceGattServices(string deviceId);
        Task<IEnumerable<BleGattCharacteristic>> GetDeviceCharacteristics(string deviceUuid, string serviceUuid);
        Task<bool> Disconnect(string deviceUuid);
        Task<bool> WriteToCharacteristric(string deviceUuid, string serviceUuid, string characteristicUuid, IEnumerable<byte> buffer);
        Task<IEnumerable<byte>> ReadFromCharacteristic(string deviceUuid, string serviceUuid, string characteristicUuid);
        Task<bool> RegisterToCharacteristicNotifications(string deviceUuid, string serviceUuid, string characteristicUuid);
    }
}