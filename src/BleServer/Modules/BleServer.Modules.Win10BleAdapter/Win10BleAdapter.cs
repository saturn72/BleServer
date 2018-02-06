using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using BleServer.Common.Services.Ble;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using BleServer.Common.Domain;
using System.Linq;
using Windows.Storage.Streams;

namespace BleServer.Modules.Win10BleAdapter
{
    public class Win10BleAdapter : IBleAdapter
    {
        #region fields

        private readonly BluetoothLEAdvertisementWatcher _bleWatcher;
        private readonly IDictionary<string, BluetoothLEDeviceData> _deviceDatas = new Dictionary<string, BluetoothLEDeviceData>();

        private object lockObj = new object();
        #endregion

        #region Events

        public event BluetoothDeviceEventHandler DeviceDiscovered;

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
                        if (!_deviceDatas.ContainsKey(bleDeviceId))
                            _deviceDatas[bleDeviceId] = new BluetoothLEDeviceData
                            {
                                Device = bleDevice
                            };
                    }
                    OnDeviceDiscovered(new BleDeviceEventArgs(bleDevice.ToDomainModel()));

                };
            return bleWatcher;
        }

        protected virtual void OnDeviceDiscovered(BleDeviceEventArgs args)
        {
            DeviceDiscovered?.Invoke(this, args);
        }

        private static async Task<BluetoothLEDevice> ExtractBleDevice(BluetoothLEAdvertisementReceivedEventArgs btAdv)
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

        #region GetGattServices
        public async Task<IEnumerable<BleGattService>> GetGattServices(string deviceId, bool refresh = false)
        {
            var deviceData = _deviceDatas[deviceId];

            if (!deviceData.GattDeviceServices.Any() || refresh)
            {
                var deviceGattServicesResult = await deviceData.Device.GetGattServicesAsync(BluetoothCacheMode.Cached);
                deviceData.GattDeviceServices = deviceGattServicesResult.Services;
            }

            var allCahracteristics = new List<GattCharacteristic>();
            var result = new List<BleGattService>();
            foreach (var gds in deviceData.GattDeviceServices)
            {
                var characteristicsResult = await gds.GetCharacteristicsAsync(BluetoothCacheMode.Cached);
                allCahracteristics.AddRange(characteristicsResult.Characteristics);
                result.Add(ExtractDomainModel(gds, deviceData.GattDeviceCharacteristics));
            }
            deviceData.GattDeviceCharacteristics = allCahracteristics;
            return result;
        }

        private static BleGattService ExtractDomainModel(GattDeviceService gattDeviceService,
            IEnumerable<GattCharacteristic> serviceCharacteristics)
        {
            return new BleGattService
            {
                AssignedNumber = gattDeviceService.AttributeHandle,
                Uuid = gattDeviceService.Uuid,
                DeviceId = gattDeviceService.Session?.DeviceId?.Id ?? string.Empty,
                Characteristics = serviceCharacteristics
                    .Select(sc => new BleGattCharacteristic
                    {
                        Uuid = sc.Uuid,
                        Description = sc.UserDescription,
                        AssignedNumber = sc.AttributeHandle
                    }).ToArray()
            };
        }

        #endregion

        #region read Characteristics
        public async Task<string> ReadCharacteristicValue(string deviceId, string gattServiceAssignedNumber,
            string gattCharacteristicAssignedNumber)
        {
            var device = _deviceDatas[deviceId];
            var srvGattPrefix = MitigateGattAssignedNumer(gattServiceAssignedNumber);
            var service = device.GattDeviceServices.FirstOrDefault(s => s.Uuid.ToString().StartsWith(srvGattPrefix));
            if (service == null)
                return null;

            var charGattPrefix = MitigateGattAssignedNumer(gattCharacteristicAssignedNumber);
            var characteristic = device.GattDeviceCharacteristics.FirstOrDefault(c => c.Service == service && c.Uuid.ToString().StartsWith(charGattPrefix));
            if (characteristic == null)
                return null;

            var data = await characteristic.ReadValueAsync();
            var dataReader = DataReader.FromBuffer(data.Value);
            return dataReader.ReadString(data.Value.Length);
        }

        private string MitigateGattAssignedNumer(string gattAssignedNumber)
        {
            var withoutHexPrefix =
                gattAssignedNumber.Substring(gattAssignedNumber.IndexOf("x",
                    StringComparison.InvariantCultureIgnoreCase)+1);
            return "0000" + withoutHexPrefix;
        }

        #endregion
    }
}
