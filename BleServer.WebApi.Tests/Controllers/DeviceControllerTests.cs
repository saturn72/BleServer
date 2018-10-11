using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BleServer.Common.Models;
using BleServer.Common.Services.Ble;
using BleServer.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using Xunit;

namespace BleServer.WebApi.Tests.Controllers
{
    public class DeviceControllerTests
    {
        public static IEnumerable<object[]> EmptyBluetooLEDeviceCollection => new[]
        {
            new object[] {null},
            new object[] {new BleDevice[] { }}
        };

        [Theory]
        [MemberData(nameof(EmptyBluetooLEDeviceCollection))]
        public async Task DeviceController_GetDevices_ReturnsEmptyCollectionFromservice(
            IEnumerable<BleDevice> devices)
        {
            var bleSrv = new Mock<IBleService>();
            bleSrv.Setup(bs => bs.GetDevices()).Returns(Task.FromResult(devices));
            var ctrl = new DeviceController(bleSrv.Object);
            var res = await ctrl.GetAllDiscoveredDevicesAsync();
            var t = res.ShouldBeOfType<OkObjectResult>();
            var d = t.Value as IEnumerable<BleDevice>;
            d.ShouldNotBeNull();
            d.Count().ShouldBe(0);
        }

        [Fact]
        public async Task DeviceController_GetDeviceById_NotFoundInEmptyCollection()
        {
            var bleSrv = new Mock<IBleService>();
            bleSrv.Setup(bs => bs.GetDeviceById(It.IsAny<string>())).Returns(Task.FromResult(null as BleDevice));
            var ctrl = new DeviceController(bleSrv.Object);
            var deviceId = "id";
            var res = await ctrl.GetDeviceByIdAsync(deviceId);
            var t = res.ShouldBeOfType<NotFoundObjectResult>();
            TestUtil.GetPropertyValue(t.Value, "id").ShouldBe(deviceId);
        }

        [Fact]
        public async Task DeviceController_GetDevices_ReturnsCollectionFromService()
        {
            var serviceDevices = new[]
            {
                new BleDevice {Id = "id_1", Name = "name_1"},
                new BleDevice {Id = "id_2", Name = "name_2"},
                new BleDevice {Id = "id_3", Name = "name_3"},
                new BleDevice {Id = "id_4", Name = "name_4"}
            } as IEnumerable<BleDevice>;

            var bleSrv = new Mock<IBleService>();
            bleSrv.Setup(bs => bs.GetDevices()).Returns(Task.FromResult(serviceDevices));
            var ctrl = new DeviceController(bleSrv.Object);
            var res = await ctrl.GetAllDiscoveredDevicesAsync();
            var t = res.ShouldBeOfType<OkObjectResult>();
            var retDevices = t.Value as IEnumerable<BleDevice>;
            retDevices.ShouldNotBeNull();
            retDevices.Count().ShouldBe(serviceDevices.Count());

            foreach (var rt in retDevices)
                serviceDevices.Any(d => d.Name == rt.Name && d.Id == rt.Id);
        }

        [Fact]
        public async Task DeviceController_GetDevices_ReturnsDevice()
        {
            var deviceId = "id_1";
            var srvResponse = new BleDevice {Id = deviceId, Name = "name_1"};

            var bleSrv = new Mock<IBleService>();
            bleSrv.Setup(bs => bs.GetDeviceById(It.IsAny<string>())).Returns(Task.FromResult(srvResponse));
            var ctrl = new DeviceController(bleSrv.Object);
            var res = await ctrl.GetDeviceByIdAsync(deviceId);
            var t = res.ShouldBeOfType<OkObjectResult>();
            t.Value.ShouldBe(srvResponse);
        }

       

        [Theory]
        [InlineData(true, typeof(AcceptedResult))]
        [InlineData(false, typeof(ObjectResult))]
        public async Task DeviceController_DisconnectDeviceById(bool expResult, Type expREsponseType)
        {
            var bleSrv = new Mock<IBleService>();
            bleSrv.Setup(b => b.UnpairDeviceById(It.IsAny<string>())).ReturnsAsync(expResult);

            var ctrl = new DeviceController(bleSrv.Object);
            var res = await ctrl.DisconnectDeviceAsync("some-id");
            res.ShouldBeOfType(expREsponseType);

        }
    }
}