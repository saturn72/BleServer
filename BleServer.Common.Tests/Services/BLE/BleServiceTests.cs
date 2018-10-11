using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BleServer.Common.Models;
using BleServer.Common.Services;
using BleServer.Common.Services.Ble;
using Moq;
using Shouldly;
using Xunit;

namespace BleServer.Common.Tests.Services.BLE
{
    public class BleServiceTests
    {
        public static IEnumerable<object[]> EmptyBluetooLEDeviceCollection => new[]
        {
            new object[] {null},
            new object[] {new BleDevice[] { }},
        };

        #region BleService_GetDevices

        [Theory]
        [MemberData(nameof(EmptyBluetooLEDeviceCollection))]
        public async Task BleServiceTests_GetDevices_ReturnsEmpty(IEnumerable<BleDevice> devices)
        {
            var bMgr = new Mock<IBleManager>();
            bMgr.Setup(b => b.GetDiscoveredDevices()).Returns(devices);
            var srv = new BleService(bMgr.Object);
            var res = await srv.GetDevices();
            res.ShouldNotBeNull();
            res.Any().ShouldBeFalse();
        }

        [Fact]
        public async Task BleServiceTests_GetDevices_ReturnsDevices()
        {
            var serviceDevices = new[]
            {
                new BleDevice{Id = "id_1",Name = "name_1"},
                new BleDevice{Id = "id_2",Name = "name_2"},
                new BleDevice{Id = "id_3",Name = "name_3"},
                new BleDevice{Id = "id_4",Name = "name_4"},
            } as IEnumerable<BleDevice>;
            var bMgr = new Mock<IBleManager>();
            bMgr.Setup(b => b.GetDiscoveredDevices()).Returns(serviceDevices);
            var srv = new BleService(bMgr.Object);
            var devices = await srv.GetDevices();
            devices.ShouldNotBeNull();
            devices.Count().ShouldBe(serviceDevices.Count());

            foreach (var rd in devices)
                serviceDevices.Any(d => d.Name == rd.Name && d.Id == rd.Id);
        }

        #endregion

        #region BleService_GetDeviceById

        [Theory]
        [MemberData(nameof(EmptyBluetooLEDeviceCollection))]
        public async Task BleServiceTests_GetDeviceById_ReturnsNull(IEnumerable<BleDevice> devices)
        {
            var bMgr = new Mock<IBleManager>();
            bMgr.Setup(b => b.GetDiscoveredDevices()).Returns(devices);
            var srv = new BleService(bMgr.Object);
            var res = await srv.GetDeviceById("deviceId");
            res.ShouldBeNull();
        }

        [Fact]
        public async Task BleServiceTests_GetDeviceById_ReturnsDevice()
        {
            var deviceId = "id_1";
            var bMgrResponse = new BleDevice { Id = deviceId, Name = "name_1" };

            var bMgr = new Mock<IBleManager>();
            bMgr.Setup(b => b.GetDiscoveredDevices()).Returns(new[] { bMgrResponse });
            var srv = new BleService(bMgr.Object);

            var res = await srv.GetDeviceById(deviceId);
            res.ShouldBe(bMgrResponse);
        }
        #endregion

        #region BleService_GetCharacteristics

        [Fact]
        public async Task BleServiceTests_GetCharacteristics_NotFound()
        {
            var deviceId = "device-not-exists";
            var serviceId = "gatt-service-id";
            var bMgr = new Mock<IBleManager>();
            bMgr.Setup(b => b.GetDeviceCharacteristics(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new BleGattCharacteristic[] { });
            var srv = new BleService(bMgr.Object);

            var res = await srv.GetCharacteristics(deviceId, serviceId);
            res.Result.ShouldBe(ServiceResponseResult.NotFound);
            res.Message.ShouldContain("\'" + deviceId + "\'");
            res.Message.ShouldContain("\'" + serviceId + "\'");
        }

        [Fact]
        public async Task BleServiceTests_GetCharacteristics()
        {
            var deviceId = "device-not-exists";
            var serviceId = "gatt-service-id";
            var bMgrResponse = new[] { new BleGattCharacteristic(Guid.NewGuid(), "some-description") };
            var bMgr = new Mock<IBleManager>();
            bMgr.Setup(b => b.GetDeviceCharacteristics(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(bMgrResponse);
            var srv = new BleService(bMgr.Object);

            var res = await srv.GetCharacteristics(deviceId, serviceId);
            res.Result.ShouldBe(ServiceResponseResult.Success);
            res.Data.ShouldBe(bMgrResponse);
        }

        #endregion

        #region BleService_GetGattServicesByDeviceId

        [Fact]
        public async Task BleServiceTests_GetGattServicesByDeviceId_deviceNotExists()
        {
            var deviceId = "device-not-exists";
            var bMgrResponse = new BleDevice { Id = "dId", Name = "name_1" };

            var bMgr = new Mock<IBleManager>();
            bMgr.Setup(b => b.GetDiscoveredDevices()).Returns(new[] { bMgrResponse });
            var srv = new BleService(bMgr.Object);

            var res = await srv.GetGattServicesByDeviceId(deviceId);
            res.Result.ShouldBe(ServiceResponseResult.NotFound);
        }

        [Fact]
        public async Task BleServiceTests_GetGattServicesByDeviceId_Returns_BleGattServices()
        {
            var deviceId = "deviceId";
            var bMgrDevices = new BleDevice { Id = deviceId, Name = "name_1" };
            var bMgerDeviceGattServices = new[]
            {
                new BleGattService {DeviceId = "gatt-service-1"},
                new BleGattService {DeviceId= "gatt-service-2"},
                new BleGattService {DeviceId = "gatt-service-3"},
            };

            var bMgr = new Mock<IBleManager>();

            bMgr.Setup(b => b.GetDiscoveredDevices()).Returns(new[] { bMgrDevices });
            bMgr.Setup(b => b.GetDeviceGattServices(It.IsAny<string>()))
                .Returns(Task.FromResult((IEnumerable<BleGattService>)bMgerDeviceGattServices));
            var srv = new BleService(bMgr.Object);

            var res = await srv.GetGattServicesByDeviceId(deviceId);
            res.Result.ShouldBe(ServiceResponseResult.Success);
            res.Data.Count().ShouldBe(bMgerDeviceGattServices.Length);
            foreach (var gt in res.Data)
                bMgerDeviceGattServices.Any(t => t.Uuid == gt.Uuid).ShouldBeTrue();
        }
        #endregion


        #region BleService_Unpair

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task BleServiceTests_Unpair(bool expUnpairResult)
        {
            var bleMock = new Mock<IBleManager>();
            bleMock.Setup(b => b.Unpair(It.IsAny<string>())).ReturnsAsync(expUnpairResult);
            var srv = new BleService(bleMock.Object);

            var res = await srv.UnpairDeviceById("some-device-id");
            res.ShouldBe(expUnpairResult);
        }

        #endregion

        #region BleService_WriteToCharacteristic

        [Fact]
        public async Task BleServiceTests_WriteToCharacteristic_Failed()
        {
            var bleMock = new Mock<IBleManager>();
            bleMock.Setup(b => b.WriteToCharacteristric(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<byte>>())).ReturnsAsync(false);
            var srv = new BleService(bleMock.Object);

            var deviceId = "some-device-id";
            var gattServiceId = "some-gatt-service-id";
            var characteristicId = "some-characteristic-id";

            var res = await srv.WriteToCharacteristic(deviceId, gattServiceId, characteristicId, new byte[] { });
            res.Result.ShouldBe(ServiceResponseResult.Fail);
            res.Message.ShouldContain("\'" + deviceId + "\'");
            res.Message.ShouldContain("\'" + gattServiceId + "\'");
            res.Message.ShouldContain("\'" + characteristicId + "\'");
        }
        [Fact]
        public async Task BleServiceTests_WriteToCharacteristic_Throws()
        {
            var bleMock = new Mock<IBleManager>();
            bleMock.Setup(b => b.WriteToCharacteristric(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<byte>>()))
                .ThrowsAsync(new NullReferenceException());
            var srv = new BleService(bleMock.Object);

            var deviceId = "some-device-id";
            var gattServiceId = "some-gatt-service-id";
            var characteristicId = "some-characteristic-id";

            var res = await srv.WriteToCharacteristic(deviceId, gattServiceId, characteristicId, new byte[] { });
            res.Result.ShouldBe(ServiceResponseResult.NotAcceptable);
            res.Message.ShouldContain("\'" + deviceId + "\'");
            res.Message.ShouldContain("\'" + gattServiceId + "\'");
            res.Message.ShouldContain("\'" + characteristicId + "\'");
        }

        [Fact]
        public async Task BleServiceTests_WriteToCharacteristic_Success()
        {
            var bleMock = new Mock<IBleManager>();
            bleMock.Setup(b => b.WriteToCharacteristric(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<byte>>())).ReturnsAsync(true);
            var srv = new BleService(bleMock.Object);

            var deviceId = "some-device-id";
            var gattServiceId = "some-gatt-service-id";
            var characteristicId = "some-characteristic-id";

            var res = await srv.WriteToCharacteristic(deviceId, gattServiceId, characteristicId, new byte[] { });
            res.Result.ShouldBe(ServiceResponseResult.Success);
            res.Message.ShouldBeNull();
        }
        #endregion

        #region BleService_SubscribeToCharacteristic
        [Fact]
        public async Task BleService_SubscribeToCharacteristic_Failed()
        {
            var bleMock = new Mock<IBleManager>();
            bleMock.Setup(b => b.ReadFromCharacteristic(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            var srv = new BleService(bleMock.Object);

            var deviceId = "some-device-id";
            var gattServiceId = "some-gatt-service-id";
            var characteristicId = "some-characteristic-id";

            var res = await srv.SubscribeToCharacteristic(deviceId, gattServiceId, characteristicId);
            res.Result.ShouldBe(ServiceResponseResult.Fail);
            res.Message.ShouldContain("\'" + deviceId + "\'");
            res.Message.ShouldContain("\'" + gattServiceId + "\'");
            res.Message.ShouldContain("\'" + characteristicId + "\'");
        }
        [Fact]
        public async Task BleService_SubscribeToCharacteristic_Throws()
        {
            var bleMock = new Mock<IBleManager>();
            bleMock.Setup(b => b.ReadFromCharacteristic(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new NullReferenceException());
            var srv = new BleService(bleMock.Object);

            var deviceId = "some-device-id";
            var gattServiceId = "some-gatt-service-id";
            var characteristicId = "some-characteristic-id";

            var res = await srv.SubscribeToCharacteristic(deviceId, gattServiceId, characteristicId);
            res.Result.ShouldBe(ServiceResponseResult.NotAcceptable);
            res.Message.ShouldContain("\'" + deviceId + "\'");
            res.Message.ShouldContain("\'" + gattServiceId + "\'");
            res.Message.ShouldContain("\'" + characteristicId + "\'");
        }

        [Fact]
        public async Task BleService_SubscribeToCharacteristic_Success()
        {
            var bleMock = new Mock<IBleManager>();
            bleMock.Setup(b => b.ReadFromCharacteristic(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            var srv = new BleService(bleMock.Object);

            var deviceId = "some-device-id";
            var gattServiceId = "some-gatt-service-id";
            var characteristicId = "some-characteristic-id";

            var res = await srv.SubscribeToCharacteristic(deviceId, gattServiceId, characteristicId);
            res.Result.ShouldBe(ServiceResponseResult.Success);
            res.Message.ShouldBeNull();
        }
        #endregion
    }
}
