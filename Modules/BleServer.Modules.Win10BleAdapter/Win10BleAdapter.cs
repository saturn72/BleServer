using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using BleServer.Common.Domain;
using BleServer.Common.Services.Ble;

namespace BleServer.Modules.Win10BleAdapter
{
    public class Win10BleAdapter : IBleAdapter
    {
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
                result.Add(await ExtractDomainModel(gds));
            return result;
        }

        public event BluetoothDeviceEventHandler DeviceDiscovered;
        public event BluetoothDeviceEventHandler DeviceStopped;

        public void Start()
        {
            _bleWatcher.Start();
        }

        private static async Task<BleGattService> ExtractDomainModel(GattDeviceService gattDeviceService)
        {
            var srvChars = await gattDeviceService.GetCharacteristicsAsync(BluetoothCacheMode.Cached);

            return new BleGattService
            {
                Uuid = gattDeviceService.Uuid,
                DeviceId = gattDeviceService.Session?.DeviceId?.Id ?? string.Empty,
                Characteristics = srvChars.Characteristics.AsEnumerable()
                    .Select(sc => new BleGattCharacteristic(sc.Uuid, sc.UserDescription)).ToArray()
            };
        }

        #region fields

        private readonly BluetoothLEAdvertisementWatcher _bleWatcher;
        private readonly IDictionary<string, BluetoothLEDevice> _devices = new Dictionary<string, BluetoothLEDevice>();

        private readonly object lockObj = new object();

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
                var bleDevice = await ExtractBleDeviceByBluetoothAddress(btAdv.BluetoothAddress);
                if (bleDevice == null)
                    return;

                var bleDeviceId = bleDevice.DeviceId;

                lock (lockObj)
                {
                    if (!_devices.ContainsKey(bleDeviceId))
                        _devices[bleDeviceId] = bleDevice;
                }

                OnDeviceDiscovered(new BleDeviceEventArgs(bleDevice.ToDomainModel()));
            };

            return bleWatcher;
        }

        protected virtual void OnDeviceDiscovered(BleDeviceEventArgs args)
        {
            DeviceDiscovered?.Invoke(this, args);
        }

        private static async Task<BluetoothLEDevice> ExtractBleDeviceByBluetoothAddress(ulong bluetoothAddres)
        {
            try
            {
                return await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddres);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion
    }
}