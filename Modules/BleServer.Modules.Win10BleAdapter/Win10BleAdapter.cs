using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using BleServer.Common.Domain;
using BleServer.Common.Services.BLE;
using Win10BluetoothLEDevice = Windows.Devices.Bluetooth.BluetoothLEDevice;

namespace BleServer.Modules.Win10BleAdapter
{
    public class Win10BleAdapter: IBleAdapter
    {
        #region fields

        private static object lockObject = new object();

        private readonly BluetoothLEAdvertisementWatcher _bleWatcher;
        private readonly IDictionary<string, Win10BluetoothLEDevice> _discoveredDevices;

        #endregion

        #region ctor

        public Win10BleAdapter()
        {
            _bleWatcher = InitBleWatcher();
            _discoveredDevices = new Dictionary<string, Win10BluetoothLEDevice>();
        }

        private BluetoothLEAdvertisementWatcher InitBleWatcher()
        {
            var bleWatcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };
            bleWatcher.Received += async (w, btAdv) =>
            {
                try
                {
                    var device = await Win10BluetoothLEDevice.FromBluetoothAddressAsync(btAdv.BluetoothAddress);

                    if (device == null)
                    {
                        return;
                    }

                    await Task.Run(() => AddBlueToothLeDeviceIfNotExists(device));
                }
                catch (Exception ex)
                {
                }
            };
            return bleWatcher;
        }

        private void AddBlueToothLeDeviceIfNotExists(Win10BluetoothLEDevice device)
        {
            var deviceId = device.BluetoothDeviceId.Id;
            lock (lockObject)
            {
                if (!_discoveredDevices.ContainsKey(deviceId))
                    _discoveredDevices[deviceId] = device;
            }
        }
        #endregion

        public IEnumerable<BluetoothLEDevice> GetDiscoveredDevices()
        {
            lock (lockObject)
            {
                return _discoveredDevices.Values.Select(v => v.ToDomainModel());
            }
        }

        public void Start()
        {
            _bleWatcher.Start();
        }
    }
}
