using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConnectivityServer.Common.Models;
using ConnectivityServer.Common.Services.Notifications;

namespace ConnectivityServer.Common.Services.Nfc
{
    public partial class NfcManager : INfcManager
    {
        private readonly INotifier _onDeviceValueChangedNotifier;

        #region Fields

        private object lockObject = new object();
        protected readonly IDictionary<string, ProxiedNfcDevice> Devices = new Dictionary<string, ProxiedNfcDevice>();
        #endregion

        #region ctor

        public NfcManager(IEnumerable<INfcAdapter> nfcAdapters, INotifier onDeviceValueChangedNotifier)
        {
            _onDeviceValueChangedNotifier = onDeviceValueChangedNotifier;

            foreach (var adapter in nfcAdapters)
            {
                adapter.DeviceArrived += DeviceArrivedHandler;
                adapter.DeviceDeparted += DeviceDepartedHandler;
            }
        }

        private void DeviceArrivedHandler(INfcAdapter sender, NfcDeviceEventArgs args)
        {
            var device = args.Device;
            var deviceId = device.Id;
            lock (lockObject)
            {
                if (!Devices.ContainsKey(deviceId))
                    Devices[deviceId] = new ProxiedNfcDevice(sender, device);
            }
        }
        private void DeviceDepartedHandler(INfcAdapter sender, NfcDeviceEventArgs args)
        {
            var device = args.Device;
            var deviceId = device.Id;
            lock (lockObject)
            {
                Devices.Remove(deviceId);
            }
        }

        #endregion

        public virtual IEnumerable<NfcDevice> GetDiscoveredDevices()
        {
            return Devices.Values.Select(v => v.Device);
        }
        public Task<bool> SubscribeForMessage(string messageType, Action action)
        {
            throw new System.NotImplementedException();
        }
    }
}
