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
    }
}
