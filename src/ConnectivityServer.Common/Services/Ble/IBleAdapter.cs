using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectivityServer.Common.Models;

namespace ConnectivityServer.Common.Services.Ble
{
    public interface IBleAdapter
    {
        event BluetoothDeviceEventHandler DeviceDiscovered;
        event BluetoothDeviceEventHandler DeviceDisconnected;
        event BluetoothDeviceValueChangedEventHandler DeviceValueChanged;
        Task<IEnumerable<BleGattService>> GetGattServices(string deviceUuid);
        Task<bool> Unpair(string deviceId);
        Task<bool> WriteToCharacteristic(string deviceUuid, string serviceUuid, string characteristicUuid, IEnumerable<byte> buffer);
        Task<IEnumerable<byte>> ReadFromCharacteristic(string deviceUuid, string serviceUuid, string characteristicUuid);
        Task<bool> GetCharacteristicNotifications(string deviceUuid, string serviceUuid, string characteristicUuid);
    }

    public delegate void BluetoothDeviceEventHandler(IBleAdapter sender, BleDeviceEventArgs args);
    public delegate void BluetoothDeviceValueChangedEventHandler(IBleAdapter sender, BleDeviceValueChangedEventArgs args);
}
