using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using BleServer.Common.Services.BLE;
using Windows.Devices.Bluetooth;

namespace BleServer.Modules.Win10BleAdapter
{
    public class Win10BleAdapter : IBluetoothAdapter
    {
        #region fields

        private readonly BluetoothLEAdvertisementWatcher _bleWatcher;

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

                    OnDevicediscovered(new BluetoothDeviceEventArgs(bleDevice.ToDomainModel()));
                };
            return bleWatcher;
        }

        protected virtual void OnDevicediscovered(BluetoothDeviceEventArgs args)
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

        public event BluetoothDeviceEventHandler DeviceDiscovered;
    }
}
