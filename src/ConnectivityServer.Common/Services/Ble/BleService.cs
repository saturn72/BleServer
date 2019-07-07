using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConnectivityServer.Common.Models;

namespace ConnectivityServer.Common.Services.Ble
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

        public async Task<IEnumerable<BleDevice>> GetDiscoveredDevices()
        {
            return await Task.FromResult(_bluetoothManager.GetDiscoveredDevices() ?? new BleDevice[] { });
        }

        public virtual async Task<BleDevice> GetDiscoveredDeviceById(string deviceId)
        {
            var allDevices = await GetDiscoveredDevices();
            return allDevices.FirstOrDefault(x => x.Id == deviceId);
        }

        public async Task<ServiceResponse<IEnumerable<BleGattService>>> GetGattServicesByDeviceId(string deviceId)
        {
            var serviceResponse = new ServiceResponse<IEnumerable<BleGattService>>();

            var device = await GetDiscoveredDeviceById(deviceId);
            if (device == null)
            {
                serviceResponse.Result = ServiceResponseResult.NotFound;
                serviceResponse.Message = $"the requested deviceId:\'{deviceId}\' does not exists.";
                return serviceResponse;
            }

            var deviceGattServices = await _bluetoothManager.GetDeviceGattServices(deviceId) ?? new BleGattService[] { };
            serviceResponse.Data = deviceGattServices;
            serviceResponse.Result = ServiceResponseResult.Success;

            return serviceResponse;
        }

        public async Task<ServiceResponse<IEnumerable<BleGattCharacteristic>>> GetCharacteristics(string deviceId, string gattServiceId)
        {
            var response = new ServiceResponse<IEnumerable<BleGattCharacteristic>>();

            response.Data = await _bluetoothManager.GetDeviceCharacteristics(deviceId, gattServiceId);

            if (!response.Data.Any())
            {
                response.Result = ServiceResponseResult.NotFound;
                response.Message = $"Failed to fetch characteristics from device with Id: \'{deviceId}\' and service with Id: \'{gattServiceId}\'";
                return response;
            }

            response.Result = ServiceResponseResult.Success;
            return response;
        }

        public Task<bool> UnpairDeviceById(string deviceId)
        {
            return _bluetoothManager.Unpair(deviceId);
        }

        public async Task<ServiceResponse<IEnumerable<byte>>> WriteToCharacteristic(string deviceUuid,
            string serviceUuid, string characteristicUuid, IEnumerable<byte> buffer)
        {
            bool res;
            var errMessage = "";
            var response = new ServiceResponse<IEnumerable<byte>>
            {
                Data = buffer,
            };

            try
            {
                res = await _bluetoothManager.WriteToCharacteristric(deviceUuid, serviceUuid, characteristicUuid, buffer);
            }
            catch (Exception e)
            {
                res = false;
                errMessage = "\n" + e.Message;
                response.Result = ServiceResponseResult.NotAcceptable;
            }

            response.Result = response.Result != ServiceResponseResult.NotSet ?
                response.Result :
                res ? ServiceResponseResult.Success : ServiceResponseResult.Fail;


            if (!res)
                response.Message =
                    $"Failed to write to characteristic. device Id: \'{deviceUuid}\' gatt-service Id: \'{serviceUuid}\' characteristic id: \'{characteristicUuid}\' buffer: \'{buffer}\'{errMessage}";
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<byte>>> ReadFromCharacteristic(string deviceUuid,
                    string serviceUuid, string characteristicUuid)
        {
            var errMessage = "";
            var response = new ServiceResponse<IEnumerable<byte>>();

            try
            {
                response.Data = await _bluetoothManager.ReadFromCharacteristic(deviceUuid, serviceUuid, characteristicUuid);

                var successFlag = response.Data != null;
                response.Result = successFlag ? ServiceResponseResult.Success : ServiceResponseResult.Fail;
                if (successFlag)
                    return response;
            }
            catch (Exception e)
            {
                errMessage = "\n" + e.Message;
            }

            if (response.Result == ServiceResponseResult.NotSet)
                response.Result = ServiceResponseResult.NotAcceptable;

            response.Message = $"Failed to read from characteristic. device Id: \'{deviceUuid}\' gatt-service Id: \'{serviceUuid}\' characteristic id: \'{characteristicUuid}\'.{errMessage}";
            return response;
        }

        public async Task<ServiceResponse<string>> GetCharacteristicNotifications(string deviceUuid, string serviceUuid, string characteristicUuid)
        {
            bool res;
            var errMessage = "";
            var response = new ServiceResponse<string>
            {
                Data = characteristicUuid,
            };

            try
            {
                res = await _bluetoothManager.RegisterToCharacteristicNotifications(deviceUuid, serviceUuid, characteristicUuid);
            }
            catch (Exception e)
            {
                res = false;
                errMessage = "\nInternal Error: " + e.Message;
                response.Result = ServiceResponseResult.NotAcceptable;
            }

            response.Result = response.Result != ServiceResponseResult.NotSet ?
                response.Result :
                res ? ServiceResponseResult.Success : ServiceResponseResult.Fail;


            if (!res)
                response.Message =
                    $"Failed to register to characteristic notifications. device Id: \'{deviceUuid}\' gatt-service Id: \'{serviceUuid}\' characteristic id: \'{characteristicUuid}\'{errMessage}";
            return response;
        }
    }
}
