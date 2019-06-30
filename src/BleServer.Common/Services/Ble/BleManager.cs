using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BleServer.Common.Models;
using BleServer.Common.Services.Notifications;

namespace BleServer.Common.Services.Ble
{
    public partial class BleManager : IBleManager
    {
        private readonly INotifier _onDeviceValueChangedNotifier;

        #region Fields

        private object lockObject = new object();
        protected readonly IDictionary<string, ProxiesBluetoothDevice> Devices = new Dictionary<string, ProxiesBluetoothDevice>();
        #endregion

        #region ctor

        public BleManager(IEnumerable<IBleAdapter> bleAdapters, INotifier onDeviceValueChangedNotifier)
        {
            _onDeviceValueChangedNotifier = onDeviceValueChangedNotifier;

            foreach (var adapter in bleAdapters)
            {
                adapter.DeviceDiscovered += DeviceDiscoveredHandler;
                adapter.DeviceDisconnected += DeviceDisconnectedHandler;
                adapter.DeviceValueChanged += DeviceValueChangedHandler;
            }
        }

        private void DeviceDiscoveredHandler(IBleAdapter sender, BleDeviceEventArgs args)
        {
            var device = args.Device;
            var deviceId = device.Id;
            lock (lockObject)
            {
                if(!Devices.ContainsKey(deviceId))
                    Devices[deviceId] = new ProxiesBluetoothDevice(sender, device);
            }
        }
         private void DeviceDisconnectedHandler(IBleAdapter sender, BleDeviceEventArgs args)
        {
            var device = args.Device;
            var deviceId = device.Id;
            lock (lockObject)
            {
                Devices.Remove(deviceId);
            }
        }

        private void DeviceValueChangedHandler(IBleAdapter sender, BleDeviceValueChangedEventArgs args)
        {
            Task.Run(() => _onDeviceValueChangedNotifier.Push(args.DeviceUuid, args));
        }

        #endregion

        public virtual IEnumerable<BleDevice> GetDiscoveredDevices()
        {
            return Devices.Values.Select(v => v.Device);
        }

        public async Task<IEnumerable<BleGattService>> GetDeviceGattServices(string deviceId)
        {
            var bleAdapter = Devices[deviceId].Adapter;
            return await bleAdapter.GetGattServices(deviceId) ?? new BleGattService[] { };
        }

        public async Task<IEnumerable<BleGattCharacteristic>> GetDeviceCharacteristics(string deviceUuid, string serviceUuid)
        {
            var gattService = await this.GetGattServiceById(deviceUuid, serviceUuid);
            return gattService.Characteristics;
        }

        public async Task<bool> Unpair(string deviceUuid)
        {
            var res = await Devices[deviceUuid].Adapter.Unpair(deviceUuid);
            if (res)
                Devices.Remove(deviceUuid);
            return res;
        }

        public async Task<bool> WriteToCharacteristric(string deviceUuid, string serviceUuid, string characteristicUuid, IEnumerable<byte> buffer)
        {
            var bleAdapter = Devices[deviceUuid].Adapter;
            return await bleAdapter.WriteToCharacteristic(deviceUuid, serviceUuid, characteristicUuid, buffer);
        }

        public async Task<IEnumerable<byte>> ReadFromCharacteristic(string deviceUuid, string serviceUuid, string characteristicUuid)
        {
            var bleAdapter = Devices[deviceUuid].Adapter;
            return await bleAdapter.ReadFromCharacteristic(deviceUuid, serviceUuid, characteristicUuid);
        }


        public async Task<bool> RegisterToCharacteristicNotifications(string deviceUuid, string serviceUuid, string characteristicUuid)
        {
            var bleAdapter = Devices[deviceUuid].Adapter;
            return await bleAdapter.GetCharacteristicNotifications(deviceUuid, serviceUuid, characteristicUuid);
        }
    }
}
