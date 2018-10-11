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
        Task<bool> Write(string deviceUuid, string serviceUuid, string characteristicUuid, IEnumerable<byte> buffer);
    }

    public delegate void BluetoothDeviceEventHandler(IBleAdapter sender, BleDeviceEventArgs args);
}
