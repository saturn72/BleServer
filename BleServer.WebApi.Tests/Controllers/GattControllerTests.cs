using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BleServer.Common.Models;
using BleServer.Common.Services;
using BleServer.Common.Services.Ble;
using BleServer.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;
using Xunit;

namespace BleServer.WebApi.Tests.Controllers
{
    public class GattControllerTests
    {
        [Fact]
        public async Task DeviceController_GetGattServicesByDeviceId_DeviceNotExists()
        {
            var srvRes = new ServiceResponse<IEnumerable<BleGattService>>
            {
                Result = ServiceResponseResult.NotFound
            };
            var bleSrv = new Mock<IBleService>();
            bleSrv.Setup(bs => bs.GetGattServicesByDeviceId(It.IsAny<string>())).Returns(Task.FromResult(srvRes));
            var ctrl = new ServiceController(bleSrv.Object);
            var deviceId = "id";
            var res = await ctrl.GetGattServicesByDeviceId(deviceId);

            var t = res.ShouldBeOfType<NotFoundObjectResult>();
            TestUtil.GetPropertyValue(t.Value, "id").ShouldBe(deviceId);
        }

        [Theory]
        [MemberData(nameof(DeviceController_GetGattServicesByDeviceId_ReturnsServices_Data))]
        public async Task DeviceController_GetGattServicesByDeviceId_ReturnsServices(
            IEnumerable<BleGattService> data)
        {
            var srvRes = new ServiceResponse<IEnumerable<BleGattService>>
            {
                Result = ServiceResponseResult.Success,
                Data = data
            };
            var bleSrv = new Mock<IBleService>();
            bleSrv.Setup(bs => bs.GetGattServicesByDeviceId(It.IsAny<string>())).Returns(Task.FromResult(srvRes));
            var ctrl = new ServiceController(bleSrv.Object);
            var deviceId = "id";
            var res = await ctrl.GetGattServicesByDeviceId(deviceId);
            var t = res.ShouldBeOfType<OkObjectResult>();
            (t.Value as IEnumerable<BleGattService>).Count().ShouldBe(data?.Count() ?? 0);
        }

        public static IEnumerable<object[]> DeviceController_GetGattServicesByDeviceId_ReturnsServices_Data =>
            new[]
            {
                new object[] {null},
                new object[]
                {
                    new[]
                    {
                        new BleGattService {DeviceId = "some-gatt-service-name-1"},
                        new BleGattService {DeviceId = "some-gatt-service-name-2"},
                        new BleGattService {DeviceId = "some-gatt-service-name-3"},
                        new BleGattService {DeviceId = "some-gatt-service-name-4"}
                    }
                }
            };
    }
}