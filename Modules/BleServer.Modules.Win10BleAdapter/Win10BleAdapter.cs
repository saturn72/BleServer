using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using BleServer.Common.Services.Ble;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using BleServer.Common.Domain;
using System.Linq;
using Windows.Devices.Enumeration;

namespace BleServer.Modules.Win10BleAdapter
{
    public class Win10BleAdapter : IBleAdapter
    {
        #region fields

        private readonly BluetoothLEAdvertisementWatcher _bleWatcher;
        private readonly IDictionary<string, BluetoothLEDevice> _devices = new Dictionary<string, BluetoothLEDevice>();

        private object lockObj = new object();
        #endregion

        #region ctor

        public Win10BleAdapter()
        {
            _bleWatcher = InitBleWatcher();
        }

        private BluetoothLEAdvertisementWatcher InitBleWatcher()
        {
            var bleWatcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };
            bleWatcher.Received += async (w, btAdv) =>
                {
                    var bleDevice = await ExtractBleDevice(btAdv);
                    if (bleDevice == null)
                        return;

                    var bleDeviceId = bleDevice.DeviceId;
                    
                    lock (lockObj)
                    {
                        if (!_devices.ContainsKey(bleDeviceId))
                            _devices[bleDeviceId] = bleDevice;
                    }
                    OnDevicediscovered(new BleDeviceEventArgs(bleDevice.ToDomainModel()));
                   
                };
            return bleWatcher;
        }

        protected virtual void OnDevicediscovered(BleDeviceEventArgs args)
        {
            DeviceDiscovered?.Invoke(this, args);
        }

        private async Task<BluetoothLEDevice> ExtractBleDevice(BluetoothLEAdvertisementReceivedEventArgs btAdv)
        {
            try
            {
                return await BluetoothLEDevice.FromBluetoothAddressAsync(btAdv.BluetoothAddress);
            }
            catch (Exception)
            {
                return null;
            }
        }
    

        #endregion

        public void Start()
        {
            _bleWatcher.Start();
        }

        public async Task<bool> Unpair(string deviceId)
        {
            var unpairingResult = await _devices[deviceId].DeviceInformation.Pairing.UnpairAsync();
            var result = unpairingResult.Status == DeviceUnpairingResultStatus.AlreadyUnpaired ||
                   unpairingResult.Status == DeviceUnpairingResultStatus.Unpaired;

            if (result)
                _devices.Remove(deviceId);
            return result;
        }

        public async Task<IEnumerable<BleGattService>> GetGattServices(string deviceId)
        {
            var gattDeviceServices = await _devices[deviceId].GetGattServicesAsync(BluetoothCacheMode.Cached);
            var result = new List<BleGattService>();
            foreach (var gds in gattDeviceServices.Services)
                result .Add(await ExtractDomainModel(gds));
            return result;
        }

        private static async Task<BleGattService> ExtractDomainModel(GattDeviceService gattDeviceService)
        {
            var srvChars = await gattDeviceService.GetCharacteristicsAsync(BluetoothCacheMode.Cached);

            return new BleGattService
            {
                Uuid = gattDeviceService.Uuid,
                DeviceId = gattDeviceService.Session?.DeviceId?.Id ?? string.Empty,
                Characteristics = srvChars.Characteristics.AsEnumerable()
                    .Select(sc=> new BleGattCharacteristic(sc.Uuid, sc.UserDescription)).ToArray()
            };
        }

        public event BluetoothDeviceEventHandler DeviceDiscovered;
    }
}
