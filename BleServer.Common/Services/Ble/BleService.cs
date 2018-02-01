using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BleServer.Common.Domain;

namespace BleServer.Common.Services.Ble
{
    public class BleService : IBleService
    {
        private readonly IBleManager _bluetoothManager;

        #region ctor
        public BleService(IBleManager bluetoothManager)
        {
            _bluetoothManager = bluetoothManager;
        }

        #endregion

        public async Task<IEnumerable<BleDevice>> GetDevices()
        {
            return await Task.FromResult(_bluetoothManager.GetDiscoveredDevices() ?? new BleDevice[] { });
        }

        public async Task<BleDevice> GetDeviceById(string deviceId)
        {
            var allDevices = await GetDevices();
            return allDevices.FirstOrDefault(x => x.Id == deviceId);
        }

        public Task<ServiceResponse<IEnumerable<BleGattService>>> GetGattServicesByDeviceId(string deviceId)
        {
            throw new System.NotImplementedException();
        }
    }
}
