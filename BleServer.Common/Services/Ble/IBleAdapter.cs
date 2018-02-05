using System.Collections.Generic;
using System.Threading.Tasks;
using BleServer.Common.Domain;

namespace BleServer.Common.Services.Ble
{
    public interface IBleAdapter
    {
        Task<IEnumerable<BleGattService>> GetGattServices(string deviceId, bool refresh = false);
        event BluetoothDeviceEventHandler DeviceDiscovered;
        Task<string> ReadCharacteristicValue(string deviceId, string gattServiceAssignedNumber,
            string gattCharacteristicAssignedNumber);
    }

    public delegate void BluetoothDeviceEventHandler(IBleAdapter sender, BleDeviceEventArgs args);
}
