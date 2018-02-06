using System.Collections.Generic;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleServer.Modules.Win10BleAdapter
{
    internal class BluetoothLEDeviceData
    {
        internal BluetoothLEDevice Device { get; set; }

        internal IEnumerable<GattDeviceService> GattDeviceServices { get; set; } = new List<GattDeviceService>();
        internal IEnumerable<GattCharacteristic> GattDeviceCharacteristics { get; set; } = new List<GattCharacteristic>();
    }
}