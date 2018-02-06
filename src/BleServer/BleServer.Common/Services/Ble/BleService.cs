using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BleServer.Common.Domain;
using BleServer.Common.Extensions;

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

        public async Task<ServiceResponse<IEnumerable<BleGattService>>> GetGattServicesByDeviceId(string deviceId)
        {
            var serviceResponse = new ServiceResponse<IEnumerable<BleGattService>>();

            var device = await GetDeviceById(deviceId);
            if (device == null)
            {
                serviceResponse.Result = ServiceResponseResult.NotFound;
                serviceResponse.ErrorMessage = "the Given deviceId does not exists";
                return serviceResponse;
            }

            var deviceGattServices = await _bluetoothManager.GetDeviceGattServices(deviceId) ?? new BleGattService[]{};
            serviceResponse.Data = deviceGattServices;
            serviceResponse.Result = ServiceResponseResult.Success;

            return serviceResponse;
        }

        public async Task<ServiceResponse<string>> ReadCharacteristicValue(string deviceId, string serviceAssignedNumber, string characteristicAssignedNumber)
        {
            var srvResponse = new ServiceResponse<string>();
            if (!deviceId.HasValue() || !serviceAssignedNumber.HasValue() || !characteristicAssignedNumber.HasValue())
            {
                srvResponse.Result = ServiceResponseResult.Failed;
                srvResponse.ErrorMessage =
                    "Missing data. Please specify all fields: deviceId, serviceAssignedNumber, characteristicAssignedNumber";
                return srvResponse;
            }

            var data = await _bluetoothManager.ReadServiceCharacteristic(deviceId, serviceAssignedNumber,
                characteristicAssignedNumber);
            srvResponse.Result = ServiceResponseResult.Success;
            srvResponse.Data = data;
            return srvResponse;
        }
    }
}
