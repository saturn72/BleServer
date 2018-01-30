using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace BleServer
{
    public class BleManager
    {
        private static object lockObject = new object();

        private const GattClientCharacteristicConfigurationDescriptorValue CHARACTERISTIC_NOTIFICATION_TYPE = GattClientCharacteristicConfigurationDescriptorValue.Notify;
        private readonly IDictionary<string, BluetoothLEDevice> _deviceList = new Dictionary<string, BluetoothLEDevice>();

        public BluetoothLEAdvertisementWatcher _bleWatcher { get; private set; }

        public event NewDeviceEventHandler NewDeviceFound;

        public void StartScan()
        {
            _bleWatcher.Start();
        }
        public BleManager()
        {
            _bleWatcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            _bleWatcher.Received += async (w, btAdv) =>
            {
                try
                {
                    var device = await BluetoothLEDevice.FromBluetoothAddressAsync(btAdv.BluetoothAddress);

                    if (device == null)
                    {
                        return;
                    }

                    NewDeviceFound?.Invoke(this, new NewDeviceEventArgs()
                    {
                        Device = device
                    });

                    await Task.Run(() => AddBlueToothLeDeviceIfNotExists(device));

                }
                catch (Exception ex)
                {

                }
            };
        }

        private void AddBlueToothLeDeviceIfNotExists(BluetoothLEDevice device)
        {
            var deviceId = device.BluetoothDeviceId.Id;
            lock (lockObject)
            {
                if (!_deviceList.ContainsKey(deviceId))
                    _deviceList[deviceId] = device;
            }
        }

        public async Task<bool> SendRateAsync(BluetoothLEDevice device, int rate)
        {

            bool result = false;
            try
            {
                // SERVICES!!
                var gatt = await device.GetGattServicesAsync();

                // CHARACTERISTICS!!
                var service = await gatt.Services.Single(s => s.Uuid == (GattServiceUuids.HeartRate)).GetCharacteristicsAsync();

                GattCharacteristic gattCharacteristic =
                service.Characteristics.Single(c => c.Uuid.ToString() == "00003010-0000-1000-8000-00805f9b34fb");

                byte[] message = new byte[10];
                message[0] = 5; //command id
                message[1] = 4; // command len
                message[2] = 1; //ack needed
                byte[] buffer = IntToBitIndian(rate * 1000);
                buffer.CopyTo(message, 6);

                var writer = new DataWriter();

                writer.WriteBytes(message);

                await gattCharacteristic.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);

                result = true;

            }
            catch (Exception ex)
            {


            }
            return result;
        }

        byte[] Int2LittelIndian(int data)
        {
            byte[] b = new byte[4];
            b[0] = (byte)data;
            b[1] = (byte)(((uint)data >> 8) & 0xFF);
            b[2] = (byte)(((uint)data >> 16) & 0xFF);
            b[3] = (byte)(((uint)data >> 24) & 0xFF);
            return b;
        }
        byte[] IntToBitIndian(int data)
        {
            int intValue;
            byte[] intBytes = BitConverter.GetBytes(data);
            Array.Reverse(intBytes);
            byte[] result = intBytes;

            return result;
        }

    }

    public delegate void NewDeviceEventHandler(object sender, NewDeviceEventArgs e);

    public class NewDeviceEventArgs : EventArgs
    {
        public BluetoothLEDevice Device { get; set; }
    }
}
