using System.Collections.Generic;
using System.Threading.Tasks;
using BleServer.Common.Models;

namespace BleServer.Common.Services.Ble
{
    public interface IBleAdapter
    {
        Task<IEnumerable<BleGattService>> GetGattServices(string deviceUuid);
        Task<bool> Unpair(string deviceId);
        event BluetoothDeviceEventHandler DeviceDiscovered;
        event BluetoothDeviceValueChangedEventHandler DeviceValueChanged;
        Task<bool> WriteToCharacteristic(string deviceUuid, string serviceUuid, string characteristicUuid, IEnumerable<byte> buffer);
        Task<IEnumerable<byte>> ReadFromCharacteristic(string deviceUuid, string serviceUuid, string characteristicUuid);
        Task<bool> GetCharacteristicNotifications(string deviceUuid, string serviceUuid, string characteristicUuid);
    }

    public delegate void BluetoothDeviceEventHandler(IBleAdapter sender, BleDeviceEventArgs args);
    public delegate void BluetoothDeviceValueChangedEventHandler(IBleAdapter sender, BleDeviceValueChangedEventArgs args);
}
