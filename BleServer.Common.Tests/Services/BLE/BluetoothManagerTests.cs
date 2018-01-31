using System;
using System.Collections.Generic;
using System.Linq;
using BleServer.Common.Domain;
using BleServer.Common.Services.BLE;
using Moq;
using Shouldly;
using Xunit;

namespace BleServer.Common.Tests.Services.BLE
{
    public class BluetoothManagerTests
    {
        [Fact]
        public void BluetoothManager_RegisterToDiscoveredEvent()
        {
            var dummyAdapter = new DummyBluetoothAdapter();

            var bm = new BluetoothManager(new[] {dummyAdapter});
            var device = new BluetoothDevice
            {
                Id = "some-device-id",
                Name = "Some-device-Name"
            };
            dummyAdapter.RaiseDeviceDiscoveredEvent(device);
            var devices = bm.GetDiscoveredDevices();

            devices.Count().ShouldBe(1);
            var d = devices.First();
            d.Name = device.Name;
            d.Id = device.Id;
        }
    }

    public class DummyBluetoothAdapter : IBluetoothAdapter
    {
        internal void RaiseDeviceDiscoveredEvent(BluetoothDevice device)
        {
            var bdea = new BluetoothDeviceEventArgs(device);
            DeviceDiscovered(this, bdea);
        }

        public IEnumerable<BluetoothGattService> GetGattServices(string deviceId)
        {
            throw new NotImplementedException();
        }

        public event BluetoothDeviceEventHandler DeviceDiscovered;
    }
}
