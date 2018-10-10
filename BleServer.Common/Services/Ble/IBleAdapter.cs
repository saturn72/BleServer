using System.Collections.Generic;
using System.Threading.Tasks;
using BleServer.Common.Domain;

namespace BleServer.Common.Services.Ble
{
    public interface IBleAdapter
    {
        Task<IEnumerable<BleGattService>> GetGattServices(string deviceId);
        Task<bool> Unpair(string deviceId);
        event BluetoothDeviceEventHandler DeviceDiscovered;
        Task<bool> Write(string gattServiceUuid, string characteristicUuid, IEnumerable<byte> buffer);
    }

    public delegate void BluetoothDeviceEventHandler(IBleAdapter sender, BleDeviceEventArgs args);
}
