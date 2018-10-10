using System.Collections.Generic;
using System.Threading.Tasks;
using BleServer.Common.Domain;

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

        Task<ServiceResponse<IEnumerable<byte>>> WriteToCharacteristic(string deviceId, string gattServiceId,
            string characteristicId, IEnumerable<byte> buffer);
    }
}