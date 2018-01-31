using BleServer.Common.Domain;

namespace BleServer.Modules.Win10BleAdapter
{
    public static class MappingExtensions
    {
        internal static BluetoothDevice ToDomainModel(
            this Windows.Devices.Bluetooth.BluetoothLEDevice win10BluetoothLeDevice)
        {
            return new BluetoothDevice
            {
                Id = win10BluetoothLeDevice.DeviceId,
                Name = win10BluetoothLeDevice.Name
            };
        }
    }
}
