using System;
using System.Collections.Generic;
using System.Linq;
using BleServer.Common.Domain;

namespace BleServer.Common.Services.Ble
{
    public partial class BleManager : IBleManager
    {
        #region Fields

        private static object lockObject = new object();
        private readonly IEnumerable<IBleAdapter> _bleAdapters;
        protected static readonly IDictionary<string, ProxiesBluetoothDevice> Devices = new Dictionary<string, ProxiesBluetoothDevice>();
        #endregion

        #region ctor

        public BleManager(IEnumerable<IBleAdapter> bleAdapters)
        {
            _bleAdapters = bleAdapters;

            foreach (var adapter in bleAdapters)
                adapter.DeviceDiscovered += DeviceDiscoveredHandler;
        }

        private static void DeviceDiscoveredHandler(IBleAdapter sender, BleDeviceEventArgs args)
        {
            var device = args.Device;
            var deviceId = device.Id;
            lock (lockObject)
            {
                if (!Devices.ContainsKey(deviceId))
                    Devices[deviceId] = new ProxiesBluetoothDevice(sender, device);
            }
        }
        
        #endregion

        public virtual IEnumerable<BleDevice> GetDiscoveredDevices()
        {
            return Devices.Values.Select(v=>v.Device);
        }

        public IEnumerable<BleService> GetDeviceServices(string deviceId)
        {
            throw new NotImplementedException("GetDeviceServices");
        }
    }
}
