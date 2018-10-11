using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using BleServer.Common.Models;
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

        public async Task<IEnumerable<BleGattService>> GetGattServices(string deviceUuid)
        {
            var gattDeviceServices = await _devices[deviceUuid].GetGattServicesAsync(BluetoothCacheMode.Cached);
            var result = new List<BleGattService>();
            foreach (var gds in gattDeviceServices.Services)
                result.Add(await ExtractDomainModel(gds));
            return result;
        }

        public event BluetoothDeviceEventHandler DeviceDiscovered;
        public async Task<bool> Write(string deviceUuid, string serviceUuid, string characteristicUuid,
            IEnumerable<byte> buffer)
        {
            var srvUuid= Guid.Parse(serviceUuid);
                var gattServices = await _devices[deviceUuid].GetGattServicesForUuidAsync(srvUuid,BluetoothCacheMode.Cached);
            var writeService = gattServices.Services.FirstOrDefault(x => x.Uuid == srvUuid);
            if (writeService == default(GattDeviceService))
                return false;

            var chrUuid = Guid.Parse(characteristicUuid);
            var allCharacteristics = await  writeService.GetCharacteristicsForUuidAsync(chrUuid, BluetoothCacheMode.Cached);
            var writeCharacteristic = allCharacteristics.Characteristics.FirstOrDefault(ch => ch.Uuid == chrUuid);
            if (writeCharacteristic == null)
                return false;

            var writer = new DataWriter();
            writer.WriteBytes(buffer.ToArray());
            var status = await writeCharacteristic.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);

            return status == GattCommunicationStatus.Success;
        }

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
 