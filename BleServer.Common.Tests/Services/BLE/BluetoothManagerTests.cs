using System;
using System.Collections.Generic;
using System.Linq;
using BleServer.Common.Domain;
using BleServer.Common.Services.Ble;
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

            var bm = new BleManager(new[] {dummyAdapter});
            var device = new BleDevice
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

    public class DummyBluetoothAdapter : IBleAdapter
    {
        internal void RaiseDeviceDiscoveredEvent(BleDevice device)
        {
            var bdea = new BleDeviceEventArgs(device);
            DeviceDiscovered(this, bdea);
        }

        public IEnumerable<BleGattService> GetGattServices(string deviceId)
        {
            throw new NotImplementedException();
        }

        public event BluetoothDeviceEventHandler DeviceDiscovered;
    }
}
