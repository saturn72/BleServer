using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BleServer.Common.Domain;
using BleServer.Common.Services.BLE;
using Moq;
using Shouldly;
using Xunit;

namespace BleServer.Common.Tests.Services.BLE
{
    public class BluetoothLEServiceTests
    {
        #region BluetoothLEService_GetDevices

        public static IEnumerable<object[]> EmptyBluetooLEDeviceCollection => new[]
        {
            new object[] {null},
            new object[] {new BluetoothLEDevice[] { }},
        };

        [Theory]
        [MemberData(nameof(EmptyBluetooLEDeviceCollection))]
        public async Task BluetoothLEServiceTests_GetDevices_ReturnsEmpty(IEnumerable<BluetoothLEDevice> devices)
        {
            var bleAdapter = new Mock<IBleAdapter>();
            bleAdapter.Setup(b => b.GetDiscoveredDevices()).Returns(devices);
            var srv = new BluetoothLEService(bleAdapter.Object);
            var res = await srv.GetDevices();
            res.ShouldNotBeNull();
            res.Any().ShouldBeFalse();
        }

        [Fact]
        public async Task BluetoothLEServiceTests_GetDevices_ReturnsDevices()
        {
            var serviceDevices = new[]
            {
                new BluetoothLEDevice{Id = "id_1",Name = "name_1"},
                new BluetoothLEDevice{Id = "id_2",Name = "name_2"},
                new BluetoothLEDevice{Id = "id_3",Name = "name_3"},
                new BluetoothLEDevice{Id = "id_4",Name = "name_4"},
            } as IEnumerable<BluetoothLEDevice>;
            var bleAdapter = new Mock<IBleAdapter>();
            bleAdapter.Setup(b => b.GetDiscoveredDevices()).Returns(serviceDevices);
            var srv = new BluetoothLEService(bleAdapter.Object);
            var devices = await srv.GetDevices();
            devices.ShouldNotBeNull();
            devices.Count().ShouldBe(serviceDevices.Count());

            foreach (var rd in devices)
                serviceDevices.Any(d => d.Name == rd.Name && d.Id == rd.Id);
        }

        #endregion
    }
}
