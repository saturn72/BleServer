using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BleServer.Common.Domain;
using BleServer.Common.Services.Ble;
using Moq;
using Shouldly;
using Xunit;

namespace BleServer.Common.Tests.Services.BLE
{
    public class BleManagerTests
    {
        [Theory]
        [MemberData(nameof(BleManager_GetDeviceServices_Services))]
        public async Task BleManager_GetDeviceGattServices(IEnumerable<BleGattService> gattServices)
        {
            var bleAdapter = new DummyBleAdapter();
            var device = new BleDevice
            {
                Id = "some-device-id",
                Name = "some-device-name"
            };

            var bm = new BleManager(new[] { bleAdapter });
            bleAdapter.SetGetGattServices(device, gattServices);

            var res = await bm.GetDeviceGattServices(device.Id);
            res.Count().ShouldBe(gattServices?.Count() ?? 0);
            foreach (var r in res)
                gattServices.Any(s => s.Uuid == r.Uuid).ShouldBeTrue();
        }

        public static IEnumerable<object[]> BleManager_GetDeviceServices_Services => new[]
        {
            new object[] {null},
            new object[] {new BleGattService[] { }},
            new object[] {new[]
                {
                    new BleGattService {DeviceId = "gatt-service-name-1"},
                    new BleGattService {DeviceId= "gatt-service-name-2"},
                    new BleGattService {DeviceId = "gatt-service-name-3"},
                    new BleGattService {DeviceId= "gatt-service-name-4"},
                    new BleGattService {DeviceId= "gatt-service-name-5"}
                }
            }
        };

        [Fact]
        public void BleManager_RegisterToDiscoveredEvent()
        {
            var dummyAdapter = new DummyBleAdapter();

            var bm = new BleManager(new[] { dummyAdapter });
            var device = new BleDevice
            {
                Id = "some-device-id",
                Name = "Some-device-Uuid"
            };
            dummyAdapter.RaiseDeviceDiscoveredEvent(device);
            var devices = bm.GetDiscoveredDevices();

            devices.Count().ShouldBe(1);
            var d = devices.First();
            d.Name = device.Name;
            d.Id = device.Id;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task BleManager_Unpair(bool expUnpairResult)
        {
            var dummyAdapter = new DummyBleAdapter{UnpairResult = expUnpairResult};

            var bm = new BleManager(new[] { dummyAdapter });
            var device = new BleDevice
            {
                Id = "some-device-id_" + DateTime.Now.ToString("yyyy-MMMM-dd_hh:mm:ss.fffZ"),
                Name = "Some-device-Uuid"
            };
            dummyAdapter.RaiseDeviceDiscoveredEvent(device);
            var unpairResult = await bm.Unpair(device.Id);
            unpairResult.ShouldBe(expUnpairResult);
        }
    }

    public class DummyBleAdapter : IBleAdapter
    {
        private static readonly IDictionary<string, IEnumerable<BleGattService>> _gattServices =
            new Dictionary<string, IEnumerable<BleGattService>>();

        public Task<IEnumerable<BleGattService>> GetGattServices(string deviceId)
        {
            return Task.FromResult(_gattServices[deviceId]);
        }

        internal bool UnpairResult { get; set; }
        public Task<bool> Unpair(string deviceId)
        {
            return Task.FromResult(UnpairResult);
        }

        public event BluetoothDeviceEventHandler DeviceDiscovered;

        internal void RaiseDeviceDiscoveredEvent(BleDevice device)
        {
            var bdea = new BleDeviceEventArgs(device);
            DeviceDiscovered(this, bdea);
        }

        internal void SetGetGattServices(BleDevice device, IEnumerable<BleGattService> gattServices)
        {
            RaiseDeviceDiscoveredEvent(device);
            _gattServices[device.Id] = gattServices?.ToArray();
        }
    }
}