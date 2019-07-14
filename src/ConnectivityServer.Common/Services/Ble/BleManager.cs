using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConnectivityServer.Common.Models;
using ConnectivityServer.Common.Services.Notifications;
using EasyCaching.Core;
using Polly;
using Polly.Retry;

namespace ConnectivityServer.Common.Services.Ble
{
    public partial class BleManager : IBleManager
    {
        #region Fields
        private const string DiscoveredDeviceCachePrefix = "discovereddevice-";
        private const int TotalPolicyTime = 6000;
        private const int PolicySleepDuration = 1500;
        private object lockObject = new object();

        private readonly INotifier _onDeviceValueChangedNotifier;
        private readonly IEasyCachingProvider _cachingProvider;

        private static readonly TimeSpan DiscoveredDeviceCachingTime = TimeSpan.FromMilliseconds(5000);

        protected readonly IDictionary<string, ProxiedBleDevice> Devices = new Dictionary<string, ProxiedBleDevice>();
        private static readonly AsyncRetryPolicy<CacheValue<ProxiedBleDevice>> GetCachedProxiedBleDevicePolicy = Policy.HandleResult<CacheValue<ProxiedBleDevice>>(pbd => pbd?.Value == null)
                .WaitAndRetryAsync(TotalPolicyTime / PolicySleepDuration, i => TimeSpan.FromMilliseconds(PolicySleepDuration));
        #endregion

        #region ctor

        public BleManager(IEnumerable<IBleAdapter> bleAdapters, INotifier onDeviceValueChangedNotifier, IEasyCachingProvider cachingProvider)
        {
            _cachingProvider = cachingProvider;
            _onDeviceValueChangedNotifier = onDeviceValueChangedNotifier;

            foreach (var adapter in bleAdapters)
            {
                adapter.DeviceConnected += DeviceConnectedHandler;
                adapter.DeviceDiscovered += DeviceDiscoveredHandler;
                adapter.DeviceDisconnected += DeviceDisconnectedHandler;
                adapter.DeviceValueChanged += DeviceValueChangedHandler;
            }
        }
        private void DeviceConnectedHandler(IBleAdapter sender, BleDeviceEventArgs args)
        {
            var device = args.Device;
            var deviceId = device.Id;
            lock (lockObject)
            {
                if (!Devices.ContainsKey(deviceId))
                    Devices[deviceId] = new ProxiedBleDevice(sender, device);
            }
        }
        private void DeviceDiscoveredHandler(IBleAdapter sender, BleDeviceEventArgs args)
        {
            var device = args.Device;
            var deviceId = device.Id;
            var pd = new ProxiedBleDevice(sender, device);
            _cachingProvider.Set(GetDeviceCacheKey(deviceId), pd, DiscoveredDeviceCachingTime);
        }
        private void DeviceDisconnectedHandler(IBleAdapter sender, BleDeviceEventArgs args)
        {
            var device = args.Device;
            var deviceId = device.Id;
            lock (lockObject)
            {
                Devices.Remove(deviceId);
                _cachingProvider.RemoveByPrefix(DiscoveredDeviceCachePrefix);
            }
        }
        private void DeviceValueChangedHandler(IBleAdapter sender, BleDeviceValueChangedEventArgs args)
        {
            Task.Run(() => _onDeviceValueChangedNotifier.Push(args.DeviceUuid, args));
        }

        #endregion

        public virtual IEnumerable<BleDevice> GetDiscoveredDevices()
        {
            var all = (Devices.Select(d => d.Value.Device) ?? new BleDevice[] { }).ToList();
            var cached = _cachingProvider.GetByPrefix<ProxiedBleDevice>(DiscoveredDeviceCachePrefix).Select(v => v.Value.Value.Device);
            all.AddRange(cached);
            return all?.GroupBy(d => d.Id).Select(grp => grp.First()) ?? new BleDevice[] { };
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

        public async Task<bool> Disconnect(string deviceUuid)
        {
            return Devices.TryGetValue(deviceUuid, out ProxiedBleDevice pbd)
                && await pbd.Adapter.Disconnect(deviceUuid);
        }

        public async Task<bool> WriteToCharacteristric(string deviceUuid, string serviceUuid, string characteristicUuid, IEnumerable<byte> buffer)
        {
            var bleAdapter = await GetAdapterByDeviceId(deviceUuid);
            return await bleAdapter.WriteToCharacteristic(deviceUuid, serviceUuid, characteristicUuid, buffer);
        }

        public async Task<IEnumerable<byte>> ReadFromCharacteristic(string deviceUuid, string serviceUuid, string characteristicUuid)
        {
            var bleAdapter = await GetAdapterByDeviceId(deviceUuid);
            return await bleAdapter.ReadFromCharacteristic(deviceUuid, serviceUuid, characteristicUuid);
        }
        private async Task<IBleAdapter> GetAdapterByDeviceId(string deviceUuid)
        {
            if (!Devices.TryGetValue(deviceUuid, out ProxiedBleDevice pbd))
            {
                var cm = await GetCachedProxiedBleDevicePolicy.ExecuteAsync(() => _cachingProvider.GetAsync<ProxiedBleDevice>(GetDeviceCacheKey(deviceUuid)));
                pbd = cm.Value;
                if (pbd != null)
                    Devices[deviceUuid] = pbd;
            }
            return pbd?.Adapter;
        }
        private string GetDeviceCacheKey(string deviceUuid)
        {
            return DiscoveredDeviceCachePrefix + deviceUuid;
        }



        public async Task<bool> RegisterToCharacteristicNotifications(string deviceUuid, string serviceUuid, string characteristicUuid)
        {
            var bleAdapter = await GetAdapterByDeviceId(deviceUuid);
            return await bleAdapter.GetCharacteristicNotifications(deviceUuid, serviceUuid, characteristicUuid);
        }
    }
}
