using System;
using BleServer.Common.Models;

namespace BleServer.Common.Services.Ble
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
            string changedValue)
        {
            DeviceUuid = deviceUuid;
            ServiceUuid = serviceUuid;
            CharacteristicUuid = characteristicUuid;
            ChangedValue = changedValue;
        }

        public string DeviceUuid { get; }
        public string ServiceUuid { get; }
        public string CharacteristicUuid { get; }
        public string ChangedValue { get; }
    }
}