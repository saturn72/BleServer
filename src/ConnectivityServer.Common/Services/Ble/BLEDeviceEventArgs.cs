using System;
using ConnectivityServer.Common.Models;

namespace ConnectivityServer.Common.Services.Ble
{
    public class BleDeviceEventArgs : EventArgs
    {
        public BleDeviceEventArgs(BleDevice device)
        {
            Device = device;
        }
        public BleDevice Device { get; }
    }

    public class BleDeviceValueChangedEventArgs : EventArgs
    {
        public BleDeviceValueChangedEventArgs(string deviceUuid, string serviceUuid, string characteristicUuid,
            string message)
        {
            DeviceUuid = deviceUuid;
            ServiceUuid = serviceUuid;
            CharacteristicUuid = characteristicUuid;
            Message = message;
        }

        public string DeviceUuid { get; }
        public string ServiceUuid { get; }
        public string CharacteristicUuid { get; }
        public string Message { get; }
    }
}