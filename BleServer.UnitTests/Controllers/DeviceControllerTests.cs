using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BleServer.Common.Domain;
using BleServer.Common.Services.BLE;
using BleServer.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using Xunit;

namespace BleServer.UnitTests.Controllers
{
    public class DeviceControllerTests
    {
        #region DeviceController_GetDevices

        public static IEnumerable<object[]> EmptyBluetooLEDeviceCollection => new[]
        {
            new object[] {null},
            new object[] {new BluetoothLEDevice[] { }},
        };

        [Theory]
        [MemberData(nameof(EmptyBluetooLEDeviceCollection))]
        public async Task DeviceController_GetDevices_ReturnsEmptyCollectionFromservice(IEnumerable<BluetoothLEDevice> devices)
        {
            var bleSrv = new Mock<IBluetoothLEService>();
            bleSrv.Setup(bs => bs.GetDevices()).Returns(Task.FromResult(devices));
            var ctrl = new DeviceController(bleSrv.Object);
            var res = await ctrl.GetAllDevicesAsync();
            var t = res.ShouldBeOfType<OkObjectResult>();
            var d = t.Value as IEnumerable<BluetoothLEDevice>;
            d.ShouldNotBeNull();
            d.Count().ShouldBe(0);
        }
        [Fact]
        public async Task DeviceController_GetDevices_ReturnsCollectionFromService()
        {
            var serviceDevices = new[]
            {
                new BluetoothLEDevice{Id = "id_1",Name = "name_1"},
                new BluetoothLEDevice{Id = "id_2",Name = "name_2"},
                new BluetoothLEDevice{Id = "id_3",Name = "name_3"},
                new BluetoothLEDevice{Id = "id_4",Name = "name_4"},
            } as IEnumerable<BluetoothLEDevice>;

            var bleSrv = new Mock<IBluetoothLEService>();
            bleSrv.Setup(bs => bs.GetDevices()).Returns(Task.FromResult(serviceDevices));
            var ctrl = new DeviceController(bleSrv.Object);
            var res = await ctrl.GetAllDevicesAsync();
            var t = res.ShouldBeOfType<OkObjectResult>();
            var retDevices = t.Value as IEnumerable<BluetoothLEDevice>;
            retDevices.ShouldNotBeNull();
            retDevices.Count().ShouldBe(serviceDevices.Count());

            foreach (var rt in retDevices)
                serviceDevices.Any(d => d.Name == rt.Name && d.Id == rt.Id);
        }

        #endregion
    }
}
