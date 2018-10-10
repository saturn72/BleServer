using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BleServer.Common.Domain;
using BleServer.Common.Services.Ble;
using Moq;
using Shouldly;
using Xunit;

namespace BleServer.Common.Tests.Services.BLE
{
    public class BleManagerExtensionsTests
    {
        [Fact]
        public async Task GetGattService_DeviceNotFound()
        {
            var bleManager = new Mock<IBleManager>();
            bleManager.Setup(b => b.GetDeviceGattServices(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();

            var gs = await BleManagerExtensions.GetGattServiceById(bleManager.Object, "deviceId", "gatt-service-id");
            gs.ShouldBeNull();
        }
        [Fact]
        public async Task GetGattServiceById_ServiceNotFound()
        {
            var bleManager = new Mock<IBleManager>();
            bleManager.Setup(b => b.GetDeviceGattServices(It.IsAny<string>()))
                .ReturnsAsync(null as IEnumerable<BleGattService>);

            var gs = await BleManagerExtensions.GetGattServiceById(bleManager.Object, "deviceId", "gatt-service-id");
            gs.ShouldBeNull();
        }

      
        [Fact]
        public async Task GetGattService_ReturnService()
        {
            var gattServiceId = "DE9522AE-D0B0-49DD-A2E7-82C1D76DE7C0";
            var deviceId = "deviceId";

            var expGattService = new BleGattService
            {
                DeviceId = deviceId,
                Uuid = new Guid(gattServiceId)
            };
            var bleManager = new Mock<IBleManager>();
            bleManager.Setup(b => b.GetDeviceGattServices(It.IsAny<string>()))
                .ReturnsAsync(new[]{expGattService});


            var gs = await BleManagerExtensions.GetGattServiceById(bleManager.Object, deviceId, gattServiceId);
            gs.ShouldBe(expGattService);
        }
    }
}
