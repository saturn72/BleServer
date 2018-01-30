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
        public async Task BluetoothLEServiceTests_GetDevices(IEnumerable<BluetoothLEDevice> devices)
        {
            var bleAdapter = new Mock<IBleAdapter>();
            bleAdapter.Setup(b => b.GetDiscoveredDevices()).Returns(devices);
            var srv = new BluetoothLEService(bleAdapter.Object);
            var res = await srv.GetDevices();
            res.ShouldNotBeNull();
            res.Any().ShouldBeFalse();
        }
        #endregion
    }
}
