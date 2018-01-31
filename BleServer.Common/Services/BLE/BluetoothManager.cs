using System;
using System.Collections.Generic;
using BleServer.Common.Domain;

namespace BleServer.Common.Services.BLE
{
    public class BluetoothManager : IBluetoothManager
    {
        #region Fields

        private static object lockObject = new object();
        private readonly IEnumerable<IBluetoothAdapter> _bleAdapters;
        protected static readonly IDictionary<string, BluetoothDevice> Devices = new Dictionary<string, BluetoothDevice>();
        #endregion

        #region ctor

        public BluetoothManager(IEnumerable<IBluetoothAdapter> bleAdapters)
        {
            _bleAdapters = bleAdapters;

            foreach (var adapter in bleAdapters)
                adapter.DeviceDiscovered += DeviceDiscoveredHandler;
        }

        private static void DeviceDiscoveredHandler(IBluetoothAdapter sender, BluetoothDeviceEventArgs args)
        {
            var device = args.Device;
            var deviceId = device.Id;
            lock (lockObject)
            {
                if (!Devices.ContainsKey(deviceId))
                    Devices[deviceId] = device;
            }
        }
        
        #endregion

        public virtual IEnumerable<BluetoothDevice> GetDiscoveredDevices()
        {
            return Devices.Values;
        }
    }
}
