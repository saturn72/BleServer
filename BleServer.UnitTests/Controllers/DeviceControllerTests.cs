using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BleServer.Common.Domain;
using BleServer.Common.Services;
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



        private object GetPropertyValue(object obj, string propertyName)
        {
            var pi = obj.GetType().GetProperty(propertyName);
            return pi.GetValue(obj);
        }




        #region GetDeviceById
        [Fact]
        public async Task DeviceController_GetDeviceById_NotFoundInEmptyCollection()
        {
            var bleSrv = new Mock<IBleService>();
            bleSrv.Setup(bs => bs.GetDeviceById(It.IsAny<string>())).Returns(Task.FromResult(null as BleDevice));
            var ctrl = new DeviceController(bleSrv.Object);
            var deviceId = "id";
            var res = await ctrl.GetDeviceByIdAsync(deviceId);
            var t = res.ShouldBeOfType<NotFoundObjectResult>();
            GetPropertyValue(t.Value, "id").ShouldBe(deviceId);
        }
        #endregion
        #region GetDevices
        [Theory]
        [MemberData(nameof(EmptyBluetooLEDeviceCollection))]
        public async Task DeviceController_GetDevices_ReturnsEmptyCollectionFromservice(
            IEnumerable<BleDevice> devices)
        {
            var bleSrv = new Mock<IBleService>();
            bleSrv.Setup(bs => bs.GetDevices()).Returns(Task.FromResult(devices));
            var ctrl = new DeviceController(bleSrv.Object);
            var res = await ctrl.GetAllDevicesAsync();
            var t = res.ShouldBeOfType<OkObjectResult>();
            var d = t.Value as IEnumerable<BleDevice>;
            d.ShouldNotBeNull();
            d.Count().ShouldBe(0);
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
            var res = await ctrl.GetAllDevicesAsync();
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
            var srvResponse = new BleDevice { Id = deviceId, Name = "name_1" };

            var bleSrv = new Mock<IBleService>();
            bleSrv.Setup(bs => bs.GetDeviceById(It.IsAny<string>())).Returns(Task.FromResult(srvResponse));
            var ctrl = new DeviceController(bleSrv.Object);
            var res = await ctrl.GetDeviceByIdAsync(deviceId);
            var t = res.ShouldBeOfType<OkObjectResult>();
            t.Value.ShouldBe(srvResponse);
        }

        #endregion
        #region GetGattServicesByDeviceId

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
            var ctrl = new DeviceController(bleSrv.Object);
            var deviceId = "id";
            var res = await ctrl.GetGattServicesByDeviceId(deviceId);
            var t = res.ShouldBeOfType<OkObjectResult>();
            (t.Value as IEnumerable<BleGattService>).Count().ShouldBe(data?.Count() ?? 0);
        }
        [Fact]
        public async Task DeviceController_GetGattServicesByDeviceId_DeviceNotExists()
        {
            var srvRes = new ServiceResponse<IEnumerable<BleGattService>>
            {
                Result = ServiceResponseResult.NotFound
            };
            var bleSrv = new Mock<IBleService>();
            bleSrv.Setup(bs => bs.GetGattServicesByDeviceId(It.IsAny<string>())).Returns(Task.FromResult(srvRes));
            var ctrl = new DeviceController(bleSrv.Object);
            var deviceId = "id";
            var res = await ctrl.GetGattServicesByDeviceId(deviceId);

            var t = res.ShouldBeOfType<NotFoundObjectResult>();
            GetPropertyValue(t.Value, "id").ShouldBe(deviceId);
        }
        #endregion

        #region ReadCharacteristicValue

        [Fact]
        public async Task DeviceController_ReadCharacteristic_ReadsFromChatacteruistic()
        {
            var bleSrvResult = new ServiceResponse<string>
            {
                Data = "data-from-characteristic",
                Result = ServiceResponseResult.Success
            };
            var bleSrv = new Mock<IBleService>();
            bleSrv.Setup(b => b.ReadCharacteristicValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(bleSrvResult);
            var ctrl = new DeviceController(bleSrv.Object);
            string deviceId = "some-device-id",
                serviceAssignedNumber = "some-srv-assign-num",
                charAssignedNumber = "some-char-assign-num";
            var res = await ctrl.ReadCharacteristicValue(deviceId, serviceAssignedNumber, charAssignedNumber);
            res.ShouldBeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeviceController_ReadCharacteristic_FailesInService()
        {
            var bleSrvResult = new ServiceResponse<string>
            {
                ErrorMessage = "error from server",
                Result = ServiceResponseResult.Failed
            };
            var bleSrv = new Mock<IBleService>();
            bleSrv.Setup(b => b.ReadCharacteristicValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(bleSrvResult);
            var ctrl = new DeviceController(bleSrv.Object);
            string deviceId = "some-device-id",
                serviceAssignedNumber = "some-srv-assign-num",
                charAssignedNumber = "some-char-assign-num";
            var res = await ctrl.ReadCharacteristicValue(deviceId, serviceAssignedNumber, charAssignedNumber);
            res.ShouldBeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [MemberData(nameof(DeviceController_ReadCharacteristic_InvalidArgs_Data))]
        public async Task DeviceController_ReadCharacteristic_InvalidArgs(string deviceId, string serviceAssignedNumber, string charAssignedNumber)
        {
            var ctrl = new DeviceController(null);
            var res = await ctrl.ReadCharacteristicValue(deviceId, serviceAssignedNumber, charAssignedNumber);
            res.ShouldBeOfType<BadRequestObjectResult>();
        }


        public static IEnumerable<object[]> DeviceController_ReadCharacteristic_InvalidArgs_Data()
        {
            var result = new List<string[]>();
            var deviceIdOptions = new[] {null, "deviceId"};
            var srvANOptions = new[] {null, "srvAN"};
            var charANOptions = new[] {null, "charAN"};
            foreach (var di in deviceIdOptions)
            {
                foreach(var san in srvANOptions)
                {
                    foreach (var can in charANOptions)
                    {
                        if (di != null && san != null)
                            continue;
                        result.Add(new []{di, san, can});
                    }
                }
            }
            return result;
        }

        #endregion
        
    }
}