using System.Collections.Generic;
using System.Threading.Tasks;
using BleServer.Common.Models;

namespace BleServer.Common.Services.Ble
{
    public interface IBleService
    {
        Task<IEnumerable<BleDevice>> GetDevices();
        Task<BleDevice> GetDeviceById(string deviceId);
        Task<ServiceResponse<IEnumerable<BleGattService>>> GetGattServicesByDeviceId(string deviceId);
        Task<bool> UnpairDeviceById(string deviceId);

        Task<ServiceResponse<IEnumerable<BleGattCharacteristic>>> GetCharacteristics(string deviceId,
            string gattServiceId);

        Task<ServiceResponse<IEnumerable<byte>>> WriteToCharacteristic(string deviceUuid, string serviceUuid,
            string characteristicUuid, IEnumerable<byte> buffer);
    }
}