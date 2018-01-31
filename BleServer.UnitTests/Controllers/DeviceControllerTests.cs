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

namespace BleServer.WebApi.Tests.Controllers
{
    public class DeviceControllerTests
    {
        #region DeviceController_GetDevices

        public static IEnumerable<object[]> EmptyBluetooLEDeviceCollection => new[]
        {
            new object[] {null},
            new object[] {new BluetoothDevice[] { }},
        };

        [Theory]
        [MemberData(nameof(EmptyBluetooLEDeviceCollection))]
        public async Task DeviceController_GetDevices_ReturnsEmptyCollectionFromservice(IEnumerable<BluetoothDevice> devices)
        {
            var bleSrv = new Mock<IBluetoothService>();
            bleSrv.Setup(bs => bs.GetDevices()).Returns(Task.FromResult(devices));
            var ctrl = new DeviceController(bleSrv.Object);
            var res = await ctrl.GetAllDevicesAsync();
            var t = res.ShouldBeOfType<OkObjectResult>();
            var d = t.Value as IEnumerable<BluetoothDevice>;
            d.ShouldNotBeNull();
            d.Count().ShouldBe(0);
        }
        [Fact]
        public async Task DeviceController_GetDevices_ReturnsCollectionFromService()
        {
            var serviceDevices = new[]
            {
                new BluetoothDevice{Id = "id_1",Name = "name_1"},
                new BluetoothDevice{Id = "id_2",Name = "name_2"},
                new BluetoothDevice{Id = "id_3",Name = "name_3"},
                new BluetoothDevice{Id = "id_4",Name = "name_4"},
            } as IEnumerable<BluetoothDevice>;

            var bleSrv = new Mock<IBluetoothService>();
            bleSrv.Setup(bs => bs.GetDevices()).Returns(Task.FromResult(serviceDevices));
            var ctrl = new DeviceController(bleSrv.Object);
            var res = await ctrl.GetAllDevicesAsync();
            var t = res.ShouldBeOfType<OkObjectResult>();
            var retDevices = t.Value as IEnumerable<BluetoothDevice>;
            retDevices.ShouldNotBeNull();
            retDevices.Count().ShouldBe(serviceDevices.Count());

            foreach (var rt in retDevices)
                serviceDevices.Any(d => d.Name == rt.Name && d.Id == rt.Id);
        }

        #endregion

        #region DeviceController_GetDeviceById

        [Fact]
        public async Task DeviceController_GetDeviceById_NotFoundInEmptyCollection()
        {
            var bleSrv = new Mock<IBluetoothService>();
            bleSrv.Setup(bs => bs.GetDeviceById(It.IsAny<string>())).Returns(Task.FromResult(null as BluetoothDevice));
            var ctrl = new DeviceController(bleSrv.Object);
            var deviceId = "id";
            var res = await ctrl.GetDeviceByIdAsync(deviceId);
            var t = res.ShouldBeOfType<NotFoundObjectResult>();
            GetPropertyValue(t.Value, "id").ShouldBe(deviceId);
        }

        private object GetPropertyValue(object obj, string propertyName)
        {
            var pi = obj.GetType().GetProperty(propertyName);
            return pi.GetValue(obj);
        }

        [Fact]
        public async Task DeviceController_GetDevices_ReturnsDevice()
        {
            var deviceId = "id_1";
            var srvResponse = new BluetoothDevice { Id = deviceId, Name = "name_1" };

            var bleSrv = new Mock<IBluetoothService>();
            bleSrv.Setup(bs => bs.GetDeviceById(It.IsAny<string>())).Returns(Task.FromResult(srvResponse));
            var ctrl = new DeviceController(bleSrv.Object);
            var res = await ctrl.GetDeviceByIdAsync(deviceId);
            var t = res.ShouldBeOfType<OkObjectResult>();
            t.Value.ShouldBe(srvResponse);
        }

        #endregion


    }
}
