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
        public static IEnumerable<object[]> EmptyBluetooLEDeviceCollection => new[]
        {
            new object[] {null},
            new object[] {new BluetoothDevice[] { }},
        };

        #region BluetoothLEService_GetDevices

        [Theory]
        [MemberData(nameof(EmptyBluetooLEDeviceCollection))]
        public async Task BluetoothLEServiceTests_GetDevices_ReturnsEmpty(IEnumerable<BluetoothDevice> devices)
        {
            var bMgr = new Mock<IBluetoothManager>();
            bMgr.Setup(b => b.GetDiscoveredDevices()).Returns(devices);
            var srv = new BluetoothService(bMgr.Object);
            var res = await srv.GetDevices();
            res.ShouldNotBeNull();
            res.Any().ShouldBeFalse();
        }

        [Fact]
        public async Task BluetoothLEServiceTests_GetDevices_ReturnsDevices()
        {
            var serviceDevices = new[]
            {
                new BluetoothDevice{Id = "id_1",Name = "name_1"},
                new BluetoothDevice{Id = "id_2",Name = "name_2"},
                new BluetoothDevice{Id = "id_3",Name = "name_3"},
                new BluetoothDevice{Id = "id_4",Name = "name_4"},
            } as IEnumerable<BluetoothDevice>;
            var bMgr = new Mock<IBluetoothManager>();
            bMgr.Setup(b => b.GetDiscoveredDevices()).Returns(serviceDevices);
            var srv = new BluetoothService(bMgr.Object);
            var devices = await srv.GetDevices();
            devices.ShouldNotBeNull();
            devices.Count().ShouldBe(serviceDevices.Count());

            foreach (var rd in devices)
                serviceDevices.Any(d => d.Name == rd.Name && d.Id == rd.Id);
        }

        #endregion

        #region BluetoothLEService_GetDeviceById

        [Theory]
        [MemberData(nameof(EmptyBluetooLEDeviceCollection))]
        public async Task BluetoothLEServiceTests_GetDeviceById_ReturnsNull(IEnumerable<BluetoothDevice> devices)
        {
            var bMgr = new Mock<IBluetoothManager>();
            bMgr.Setup(b => b.GetDiscoveredDevices()).Returns(devices);
            var srv = new BluetoothService(bMgr.Object);
            var res = await srv.GetDeviceById("deviceId");
            res.ShouldBeNull();
        }

        [Fact]
        public async Task BluetoothLEServiceTests_GetDeviceById_ReturnsDevice()
        {
            var deviceId = "id_1";
            var bMgrResponse = new BluetoothDevice { Id = deviceId, Name = "name_1" };

            var bMgr = new Mock<IBluetoothManager>();
            bMgr.Setup(b => b.GetDiscoveredDevices()).Returns(new[] { bMgrResponse });
            var srv = new BluetoothService(bMgr.Object);

            var res = await srv.GetDeviceById(deviceId);
            res.ShouldBe(bMgrResponse);
        }
        #endregion
    }
}
