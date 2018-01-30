using BleServer.Common.Domain;

namespace BleServer.Modules.Win10BleAdapter
{
    public static class MappingExtensions
    {
        internal static BluetoothLEDevice ToDomainModel(
            this Windows.Devices.Bluetooth.BluetoothLEDevice win10BluetoothLeDevice)
        {
            return new BluetoothLEDevice
            {
                Id = win10BluetoothLeDevice.DeviceId,
                Name = win10BluetoothLeDevice.Name
            };
        }
    }
}
