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
        #region Ctor 

        [Fact]
        public void BleManager_FailsOnCreation_OnEmptyBleAdapterCollection()
        {
            Should.Throw<ArgumentException>(() => new BleManager(new IBleAdapter[] { }));
            Should.Throw<ArgumentNullException>(() => new BleManager(null));
        }

        #endregion 
        #region BleManager_GetDeviceServices

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
        #endregion

        #region BleManager_RegisterToDiscoveredEvent
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
        #endregion
        #region BleManager_ReadServiceCharacteristic

        [Fact]
        public void BleManager_ReadServiceCharacteristic_ReturnsNullOnDeviceNotExists()
        {
            var bleAdapter = new Mock<IBleAdapter>();
            var bm = new BleManager(new[] { bleAdapter.Object });

            bm.ReadServiceCharacteristic("device-not-exists", "srv", "char").ShouldBeNull();
        }
        [Fact]
        public async Task BleManager_ReadServiceCharacteristic_ReturnsValueFromDevice()
        {
            var dummyAdapter = new DummyBleAdapter();

            var bm = new BleManager(new[] { dummyAdapter });
            var device = new BleDevice
            {
                Id = "some-device-id",
                Name = "Some-device-Uuid"
            };
            dummyAdapter.RaiseDeviceDiscoveredEvent(device);
            var charValue = "some-value";
            dummyAdapter.SetCharacteristicValue(charValue);

            var cv = await bm.ReadServiceCharacteristic(device.Id, "srv", "char");
            cv.ShouldBe(charValue);
        }
        #endregion
    }

    public class DummyBleAdapter : IBleAdapter
    {
        private static readonly IDictionary<string, IEnumerable<BleGattService>> _gattServices =
            new Dictionary<string, IEnumerable<BleGattService>>();

        private string _charValue;

        public Task<IEnumerable<BleGattService>> GetGattServices(string deviceId, bool refresh = false)
        {
            return Task.FromResult(_gattServices[deviceId]);
        }

        public event BluetoothDeviceEventHandler DeviceDiscovered;
        public Task<string> ReadCharacteristicValue(string deviceId, string gattServiceAssignedNumber,
            string gattCharacteristicAssignedNumber)
        {
            return Task.FromResult(_charValue);
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

        public void SetCharacteristicValue(string charValue)
        {
            _charValue = charValue;
        }
    }

}