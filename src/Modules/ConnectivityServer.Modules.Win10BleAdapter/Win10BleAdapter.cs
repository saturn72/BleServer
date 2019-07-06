using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using ConnectivityServer.Common.Models;
using ConnectivityServer.Common.Services.Ble;
using System.Collections.Concurrent;

namespace ConnectivityServer.Modules.Win10BleAdapter
{
    public class Win10BleAdapter : IBleAdapter
    {
        public async Task<bool> Unpair(string deviceId)
        {
            var d = _devices[deviceId];
            var unpairingResult = await d.DeviceInformation.Pairing.UnpairAsync();
            var result = unpairingResult.Status == DeviceUnpairingResultStatus.AlreadyUnpaired ||
                         unpairingResult.Status == DeviceUnpairingResultStatus.Unpaired;

            if (result)
            {
                ClearDevice(deviceId);
                OnDeviceDisconnected(new BleDeviceEventArgs(d.ToDomainModel()));
            }
            return result;
        }

        public async Task<IEnumerable<BleGattService>> GetGattServices(string deviceUuid)
        {
            var gattDeviceServices = await _devices[deviceUuid].GetGattServicesAsync();
            var result = new List<BleGattService>();
            foreach (var gds in gattDeviceServices.Services)
                result.Add(await ExtractDomainModel(gds));
            return result;
        }

        private async Task<GattDeviceService> GetGattServiceByUuid(string deviceUuid, string serviceUuid)
        {
            var srvKey = $"{deviceUuid}_{serviceUuid}";
            if (_services.TryGetValue(srvKey, out var service))
                return service;

            var gattServices = await _devices[deviceUuid].GetGattServicesForUuidAsync(Guid.Parse(serviceUuid), BluetoothCacheMode.Cached);
            service = gattServices.Services.First();

            _services[srvKey] = service;

            return service;
        }

        public event BleDeviceEventHandler DeviceDiscovered;
        public event BleDeviceEventHandler DeviceDisconnected;
        public event BluetoothDeviceValueChangedEventHandler DeviceValueChanged;

        public async Task<IEnumerable<byte>> ReadFromCharacteristic(string deviceUuid, string serviceUuid, string characteristicUuid)
        {
            var characteristic = await GetCharacteristicAsync(deviceUuid, serviceUuid, characteristicUuid);
            var gattReadResult = await characteristic.ReadValueAsync();

            if (gattReadResult.Status != GattCommunicationStatus.Success)
                return null;

            using (var reader = DataReader.FromBuffer(gattReadResult.Value))
            {
                var value = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(value);
                return value;
            }
        }

        public async Task<bool> WriteToCharacteristic(string deviceUuid, string serviceUuid, string characteristicUuid,
            IEnumerable<byte> buffer)
        {
            var characteristic = await GetCharacteristicAsync(deviceUuid, serviceUuid, characteristicUuid);
            using (var writer = new DataWriter())
            {
                writer.WriteBytes(buffer.ToArray());
                var status = await characteristic.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithResponse);
                return status == GattCommunicationStatus.Success;
            }
        }

        private async Task<GattCharacteristic> GetCharacteristicAsync(string deviceUuid, string serviceUuid,
            string characteristicUuid)
        {
            var chKey = $"{deviceUuid}_{serviceUuid}_{characteristicUuid}";

            if (_characteristics.TryGetValue(chKey, out var characteristic))
                return characteristic;

            var service = await GetGattServiceByUuid(deviceUuid, serviceUuid);
            var allCharacteristics = await service.GetCharacteristicsForUuidAsync(Guid.Parse(characteristicUuid), BluetoothCacheMode.Uncached);
            var result = allCharacteristics.Characteristics.First();
            _characteristics[chKey] = result;
            return result;
        }

        public async Task<bool> GetCharacteristicNotifications(string deviceUuid, string serviceUuid, string characteristicUuid)
        {
            var readCharacteristic = await GetCharacteristicAsync(deviceUuid, serviceUuid, characteristicUuid);

            var status =
                await readCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.Notify);
            var result = status == GattCommunicationStatus.Success;

            if (result) readCharacteristic.ValueChanged += ProcessAndNotify;

            return result;
        }
        private void ProcessAndNotify(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            byte[] buffer;
            using (var reader = DataReader.FromBuffer(args.CharacteristicValue))
            {
                buffer = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(buffer);
            }

            var newValue = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            var valueChanged = new BleDeviceValueChangedEventArgs(sender.Service.Session.DeviceId.Id,
                sender.Service.Uuid.ToString(),
                sender.Uuid.ToString(),
                newValue);

            OnValueChanged(valueChanged);
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
        private readonly IDictionary<string, BluetoothLEDevice> _devices = new ConcurrentDictionary<string, BluetoothLEDevice>();
        private readonly IDictionary<string, GattCharacteristic> _characteristics = new Dictionary<string, GattCharacteristic>();
        private readonly IDictionary<string, GattDeviceService> _services = new Dictionary<string, GattDeviceService>();

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
                bleDevice.ConnectionStatusChanged += RemoveDeviceFromCollection;
                return bleWatcher;
            };
        }
        private void RemoveDevicefromCollectionndPublishEvent(BluetoothLEDevice sender, object args)
        {
            if (sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected && 
                _devices.ContainsKey(bleDevice.DeviceId))
            {
                ClearDevice(bleDevice.DeviceId);
                OnDeviceDisconnected(new BleDeviceEventArgs(bleDevice.ToDomainModel()));
            }
        }
        private void ClearDevice(string deviceId)
        {
            var charsToRemove = _characteristics.Where(x => x.Key.StartsWith(deviceId)).ToArray();
            for (int i = 0; i < charsToRemove.Count(); i++)
                _characteristics.Remove(charsToRemove.ElementAt(i));

            var servicesToRemove = _services.Where(x => x.Key.StartsWith(deviceId)).ToArray();
            for (int i = 0; i < servicesToRemove.Count(); i++)
                _services.Remove(servicesToRemove.ElementAt(i));

            _devices.Remove(deviceId);
        }

        protected virtual void OnDeviceDiscovered(BleDeviceEventArgs args)
        {
            DeviceDiscovered?.Invoke(this, args);
        }
        protected virtual void OnDeviceDisconnected(BleDeviceEventArgs args)
        {
            DeviceDisconnected?.Invoke(this, args);
        }

        protected virtual void OnValueChanged(BleDeviceValueChangedEventArgs args)
        {
            DeviceValueChanged?.Invoke(this, args);
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
