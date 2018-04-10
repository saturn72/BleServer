﻿using System.Collections.Generic;
using System.Threading.Tasks;
using BleServer.Common.Domain;

namespace BleServer.Common.Services.Ble
{
    public interface IBleManager
    {
        IEnumerable<BleDevice> GetDiscoveredDevices();
        Task<IEnumerable<BleGattService>> GetDeviceGattServices(string deviceId);
        Task<bool> Unpair(string deviceId);
    }
}