using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BleServer.Common.Domain;
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
                .Returns(Task.FromResult((IEnumerable<BleGattService>) bMgerDeviceGattServices));
            var srv = new BleService(bMgr.Object);

            var res = await srv.GetGattServicesByDeviceId(deviceId);
            res.Result.ShouldBe(ServiceResponseResult.Success);
            res.Data.Count().ShouldBe(bMgerDeviceGattServices.Length);
            foreach (var gt in res.Data)
                bMgerDeviceGattServices.Any(t => t.Uuid == gt.Uuid).ShouldBeTrue();
        }
        #endregion

        #region BleService_ReadCharacteristicValue

        [Fact]
        public async Task BleService_ReadCharacteristicValue_ReadFromDevice()
        {
            var charData = "result from device";
            var btMgr = new Mock<IBleManager>();
            btMgr.Setup(bm => bm.ReadServiceCharacteristic(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(charData);

            var srv = new BleService(btMgr.Object);
            var res = await srv.ReadCharacteristicValue("deviceId", "serviceAssignedNumber", "charAssignedNumber");

            res.Data.ShouldBe(charData);
            res.HasErrors().ShouldBeFalse();
        }

        [Theory]
        [MemberData(nameof(BleService_ReadCharacteristicValue_InvalidArgs_Data))]
        public async Task BleService_ReadCharacteristicValue_InvalidArguments(string deviceId, string serviceAssignedNumber, string charAssignedNumber)
        {
            var srv = new BleService(null);
            var res = await srv.ReadCharacteristicValue(deviceId, serviceAssignedNumber, charAssignedNumber);
            res.HasErrors().ShouldBeTrue();
        }
        public static IEnumerable<object[]> BleService_ReadCharacteristicValue_InvalidArgs_Data()
        {
            var result = new List<string[]>();
            var deviceIdOptions = new[] { null, "deviceId" };
            var srvANOptions = new[] { null, "srvAN" };
            var charANOptions = new[] { null, "charAN" };
            foreach (var di in deviceIdOptions)
            {
                foreach (var san in srvANOptions)
                {
                    foreach (var can in charANOptions)
                    {
                        if (di != null && san != null)
                            continue;
                        result.Add(new[] { di, san, can });
                    }
                }
            }
            return result;
        }
        #endregion
    }
}
