using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BleServer.Common.Models;

namespace BleServer.Common.Services.Ble
{
    public partial class BleManager : IBleManager
    {
        #region Fields

        private static object lockObject = new object();
        protected static readonly IDictionary<string, ProxiesBluetoothDevice> Devices = new Dictionary<string, ProxiesBluetoothDevice>();
        #endregion

        #region ctor

        public BleManager(IEnumerable<IBleAdapter> bleAdapters)
        {
            foreach (var adapter in bleAdapters)
                adapter.DeviceDiscovered += DeviceDiscoveredHandler;
        }

        private static void DeviceDiscoveredHandler(IBleAdapter sender, BleDeviceEventArgs args)
        {
            var device = args.Device;
            var deviceId = device.Id;
            lock (lockObject)
            {
                Devices[deviceId] = new ProxiesBluetoothDevice(sender, device);
            }
        }

        #endregion

        public virtual IEnumerable<BleDevice> GetDiscoveredDevices()
        {
            return Devices.Values.Select(v => v.Device);
        }

        public async Task<IEnumerable<BleGattService>> GetDeviceGattServices(string deviceId)
        {
            var bleAdapter = Devices[deviceId].Adapter;
            return  await bleAdapter.GetGattServices(deviceId) ?? new BleGattService[] { };
        }

        public async Task<IEnumerable<BleGattCharacteristic>> GetDeviceCharacteristics(string deviceId, string gattServiceId)
        {
            var gattService = await this.GetGattServiceById(deviceId, gattServiceId);
            return gattService.Characteristics;
        }

        public async Task<bool> Unpair(string deviceId)
        {
            var res = await Devices[deviceId].Adapter.Unpair(deviceId);
            if (res)
                Devices.Remove(deviceId);
            return res;
        }

        public async Task<bool> WriteToCharacteristric(string deviceId, string gattServiceId, string characteristicId, IEnumerable<byte> buffer)
        {
            var bleAdapter = Devices[deviceId].Adapter;
            return await bleAdapter.Write(deviceId, gattServiceId, characteristicId, buffer);
        }
    }
}
