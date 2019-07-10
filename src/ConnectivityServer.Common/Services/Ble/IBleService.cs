using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectivityServer.Common.Models;

namespace ConnectivityServer.Common.Services.Ble
{
    public interface IBleService
    {
        Task<IEnumerable<BleDevice>> GetDiscoveredDevices();
        Task<BleDevice> GetDiscoveredDeviceById(string deviceId);
        Task<ServiceResponse<IEnumerable<BleGattService>>> GetGattServicesByDeviceId(string deviceId);
        Task<bool> DisconnectDeviceById(string deviceId);

        Task<ServiceResponse<IEnumerable<BleGattCharacteristic>>> GetCharacteristics(string deviceId,
            string gattServiceId);

        Task<ServiceResponse<IEnumerable<byte>>> WriteToCharacteristic(string deviceUuid, string serviceUuid,
            string characteristicUuid, IEnumerable<byte> buffer);

        Task<ServiceResponse<IEnumerable<byte>>> ReadFromCharacteristic(string deviceUuid, string serviceUuid,
        string characteristicUuid);

        Task<ServiceResponse<string>> GetCharacteristicNotifications(string deviceUuid, string serviceUuid, string characteristicUuid);
    }
}