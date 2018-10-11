using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BleServer.Common.Models;
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

        #region GetDeviceCharacteristics
        [Fact]
        public async Task BleManager_GetDeviceCharacteristics_NotFound()
        {
            var deviceId = "device-Id";
            var gsUuid = Guid.Parse("4C088D33-76C6-4094-8C4A-65A80430678A");
            var gs = new BleGattService { DeviceId = deviceId, Uuid = gsUuid };
            gs.Characteristics = new BleGattCharacteristic[] { };

            var bleAdapter = new DummyBleAdapter();
            var device = new BleDevice
            {
                Id = deviceId,
                Name = "some-device-name"
            };

            var bm = new BleManager(new[] { bleAdapter });
            bleAdapter.SetGetGattServices(device, new[] { gs });

            var task =  bm.GetDeviceCharacteristics(deviceId, "not-exists-gatt-service-id");

            task.Exception.InnerExceptions.First().ShouldBeOfType<NullReferenceException>();
        }

        [Fact]
        public async Task BleManager_GetDeviceCharacteristics_Found()
        {
            var deviceId = "device-Id";
            var gsUuid = Guid.Parse("4C088D33-76C6-4094-8C4A-65A80430678A");
            var gs = new BleGattService { DeviceId = deviceId, Uuid = gsUuid };
            gs.Characteristics = new BleGattCharacteristic[] { };

            var bleAdapter = new DummyBleAdapter();

            var device = new BleDevice
            {
                Id = deviceId,
                Name = "some-device-name"
            };

            var bm = new BleManager(new[] { bleAdapter });
            bleAdapter.SetGetGattServices(device, new[] { gs });

            var res = await bm.GetDeviceCharacteristics(deviceId, gsUuid.ToString());
            res.ShouldBe(gs.Characteristics);
        }

        #endregion

        #region WriteToCharacteristic
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task BleManager_WriteToCharacteristic_Fails(bool writeResult)
        {
            var deviceId = "device-Id";
            var gattServiceId = "4C088D33-76C6-4094-8C4A-65A80430678A";
            var characteristicId = "some-characteristic-id";
            var gs = new BleGattService { DeviceId = deviceId, Uuid = Guid.Parse(gattServiceId)};
            gs.Characteristics = new BleGattCharacteristic[] { };

            var bleAdapter = new DummyBleAdapter();
            var device = new BleDevice
            {
                Id = deviceId,
                Name = "some-device-name"
            };

            var bm = new BleManager(new[] { bleAdapter });
            bleAdapter.SetGetGattServices(device, new[] { gs });
            bleAdapter.WriteResult = writeResult;
            var res = await  bm.WriteToCharacteristric(deviceId, gattServiceId, characteristicId, new List<byte>());
            res.ShouldBe(writeResult);
        }

        [Fact]
        public async Task BleManager_WriteToCharacteristic_Success()
        {
            throw new NotImplementedException();
            var deviceId = "device-Id";
            var gsUuid = Guid.Parse("4C088D33-76C6-4094-8C4A-65A80430678A");
            var gs = new BleGattService { DeviceId = deviceId, Uuid = gsUuid };
            gs.Characteristics = new BleGattCharacteristic[] { };

            var bleAdapter = new DummyBleAdapter();
            var device = new BleDevice
            {
                Id = deviceId,
                Name = "some-device-name"
            };

            var bm = new BleManager(new[] { bleAdapter });
            bleAdapter.SetGetGattServices(device, new[] { gs });

            var task = bm.GetDeviceCharacteristics(deviceId, "not-exists-gatt-service-id");

            task.Exception.InnerExceptions.First().ShouldBeOfType<NullReferenceException>();
        }
        #endregion

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
        [Trait("Category", "non-deterministic")]
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
        public DummyBleAdapter()
        {
            _gattServices.Clear();
        }

        private static readonly IDictionary<string, IEnumerable<BleGattService>> _gattServices =
            new Dictionary<string, IEnumerable<BleGattService>>();

        internal bool WriteResult { get; set; }

        public Task<IEnumerable<BleGattService>> GetGattServices(string deviceUuid)
        {
            return Task.FromResult(_gattServices[deviceUuid]);
        }

        internal bool UnpairResult { get; set; }

        public Task<bool> Unpair(string deviceId)
        {
            return Task.FromResult(UnpairResult);
        }

        public event BluetoothDeviceEventHandler DeviceDiscovered;
        public Task<bool> Write(string deviceUuid, string serviceUuid, string characteristicUuid,
            IEnumerable<byte> buffer)
        {
            return Task.FromResult(WriteResult);
        }

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