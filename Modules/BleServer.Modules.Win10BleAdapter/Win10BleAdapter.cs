using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using BleServer.Common.Services.Ble;
using Windows.Devices.Bluetooth;
using BleServer.Common.Domain;

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

        public IEnumerable<BleGattService> GetGattServices(string deviceId)
        {

            if (_devices.ContainsKey(deviceId))
            {
                var t = _devices[deviceId].GetGattServicesAsync();
                throw new NotImplementedException();
            }
            throw new InvalidOperationException("The provided device Id does not exist in collection: " + deviceId);
        }

        public event BluetoothDeviceEventHandler DeviceDiscovered;
    }
}
