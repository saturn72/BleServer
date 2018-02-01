using System.Collections.Generic;
using BleServer.Common.Domain;

namespace BleServer.Common.Services.Ble
{
    public interface IBleManager
    {
        IEnumerable<BleDevice> GetDiscoveredDevices();
        IEnumerable<BleService> GetDeviceServices(string deviceId);
    }
}