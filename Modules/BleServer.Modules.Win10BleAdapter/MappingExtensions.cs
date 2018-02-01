﻿using Windows.Devices.Bluetooth;
using BleServer.Common.Domain;

namespace BleServer.Modules.Win10BleAdapter
{
    public static class MappingExtensions
    {
        internal static BleDevice ToDomainModel(
            this BluetoothLEDevice win10BluetoothLeDevice)
        {
            return new BleDevice
            {
                Id = win10BluetoothLeDevice.DeviceId,
                Name = win10BluetoothLeDevice.Name
            };
        }
    }
}
