using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BleServer.Common.Models;
using BleServer.Common.Services.Ble;
using BleServer.Common.Services.Notifications;
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

            var bm = new BleManager(new[] { bleAdapter }, null);
            bleAdapter.SetGetGattServices(device, gattServices);

            var res = await bm.GetDeviceGattServices(device.Id);
            res.Count().ShouldBe(gattServices?.Count() ?? 0);
            foreach (var r in res)
                gattServices.Any(s => s.Uuid == r.Uuid).ShouldBeTrue();
        }

        #region GetDeviceCharacteristics
        [Fact]
        public void BleManager_GetDeviceCharacteristics_NotFound()
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

            var bm = new BleManager(new[] { bleAdapter }, null);
            bleAdapter.SetGetGattServices(device, new[] { gs });

            var task = bm.GetDeviceCharacteristics(deviceId, "not-exists-gatt-service-id");

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

            var bm = new BleManager(new[] { bleAdapter }, null);
            bleAdapter.SetGetGattServices(device, new[] { gs });

            var res = await bm.GetDeviceCharacteristics(deviceId, gsUuid.ToString());
            res.ShouldBe(gs.Characteristics);
        }

        #endregion

        #region WriteToCharacteristic
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task BleManager_WriteToCharacteristic(bool writeResult)
        {
            var deviceId = "device-Id";
            var gattServiceId = "4C088D33-76C6-4094-8C4A-65A80430678A";
            var characteristicId = "some-characteristic-id";
            var gs = new BleGattService { DeviceId = deviceId, Uuid = Guid.Parse(gattServiceId) };
            gs.Characteristics = new BleGattCharacteristic[] { };

            var bleAdapter = new DummyBleAdapter();
            var device = new BleDevice
            {
                Id = deviceId,
                Name = "some-device-name"
            };

            var bm = new BleManager(new[] { bleAdapter }, null);
            bleAdapter.SetGetGattServices(device, new[] { gs });
            bleAdapter.WriteToCharacteristicResult = writeResult;
            var res = await bm.WriteToCharacteristric(deviceId, gattServiceId, characteristicId, new List<byte>());
            res.ShouldBe(writeResult);
        }
        #endregion
        #region ReadFromCharacteristic
        [Theory]
        [MemberData(nameof(BleManager_BleManager_ReadFrom))]
        public async Task BleManager_ReadFrom(IEnumerable<byte> readResult)
        {
            var deviceId = "device-Id";
            var gattServiceId = "4C088D33-76C6-4094-8C4A-65A80430678A";
            var characteristicId = "some-characteristic-id";
            var gs = new BleGattService { DeviceId = deviceId, Uuid = Guid.Parse(gattServiceId) };
            gs.Characteristics = new BleGattCharacteristic[] { };

            var bleAdapter = new DummyBleAdapter();
            var device = new BleDevice
            {
                Id = deviceId,
                Name = "some-device-name"
            };

            var bm = new BleManager(new[] { bleAdapter }, null);
            bleAdapter.SetGetGattServices(device, new[] { gs });
            bleAdapter.ReadFromCharacteristicResult = readResult;
            var res = await bm.ReadFromCharacteristic(deviceId, gattServiceId, characteristicId);
            res.ShouldBe(readResult);
        }

        public static IEnumerable<object[]> BleManager_BleManager_ReadFrom => new[]
        {
            new object[]{null as IEnumerable<byte>},
            new object[]{new byte[]{1,1,1,0,0,1}},
        };


        #endregion


        #region RegisterToCharacteristicNotifications
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task BleManager_RegisterToCharacteristicNotifications_Fails(bool readResult)
        {
            var deviceId = "device-Id";
            var gattServiceId = "4C088D33-76C6-4094-8C4A-65A80430678A";
            var characteristicId = "some-characteristic-id";
            var gs = new BleGattService { DeviceId = deviceId, Uuid = Guid.Parse(gattServiceId) };
            gs.Characteristics = new BleGattCharacteristic[] { };

            var bleAdapter = new DummyBleAdapter();
            var device = new BleDevice
            {
                Id = deviceId,
                Name = "some-device-name"
            };

            var bm = new BleManager(new[] { bleAdapter }, null);
            bleAdapter.SetGetGattServices(device, new[] { gs });
            bleAdapter.BleNotificationResult = readResult;
            var res = await bm.RegisterToCharacteristicNotifications(deviceId, gattServiceId, characteristicId);
            res.ShouldBe(readResult);
        }

        [Fact]
        public void BleManager_RegisterToCharacteristicNotificationsasses()
        {
            const string deviceUuid = "device-Id";
            const string serviceUuid = "4C088D33-76C6-4094-8C4A-65A80430678A";
            const string characteristicUuid = "some-characteristic-id";
            const string message = "this is notifucation content";
            var gs = new BleGattService { DeviceId = deviceUuid, Uuid = Guid.Parse(serviceUuid) };
            gs.Characteristics = new BleGattCharacteristic[] { };

            var bleAdapter = new DummyBleAdapter();
            var device = new BleDevice
            {
                Id = deviceUuid,
                Name = "some-device-name"
            };

            var notifier = new Mock<INotifier>();
            var bm = new BleManager(new[] { bleAdapter }, notifier.Object);
            bleAdapter.SetGetGattServices(device, new[] { gs });

            bleAdapter.RaiseDeviceValueChangedEvent(deviceUuid, serviceUuid, characteristicUuid, message);
            notifier.Verify(n => n.Push(It.Is<string>(s => s == deviceUuid), It.Is<BleDeviceValueChangedEventArgs>(
                b =>
                    b.DeviceUuid == deviceUuid
                    && b.ServiceUuid == serviceUuid
                    && b.CharacteristicUuid == characteristicUuid
                    && b.Message == message)), Times.Once);
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
        public void BleManager_ConnectAndDisconnectDevice()
        {
            var dummyAdapter = new DummyBleAdapter();

            var bm = new BleManager(new[] { dummyAdapter }, null);
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

              dummyAdapter.RaiseDeviceDisconnectedEvent(device);
             devices = bm.GetDiscoveredDevices();
            devices.Count().ShouldBe(0);

        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task BleManager_Unpair(bool expUnpairResult)
        {
            var dummyAdapter = new DummyBleAdapter { UnpairResult = expUnpairResult };

            var bm = new BleManager(new[] { dummyAdapter }, null);
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

        internal bool WriteToCharacteristicResult { get; set; }
        internal IEnumerable<byte> ReadFromCharacteristicResult { get; set; }

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
        public event BluetoothDeviceEventHandler DeviceDisconnected;
        public event BluetoothDeviceValueChangedEventHandler DeviceValueChanged;

        public Task<bool> WriteToCharacteristic(string deviceUuid, string serviceUuid, string characteristicUuid,
            IEnumerable<byte> buffer)
        {
            return Task.FromResult(WriteToCharacteristicResult);
        }
        public Task<IEnumerable<byte>> ReadFromCharacteristic(string deviceUuid, string serviceUuid, string characteristicUuid)
        {
            return Task.FromResult(ReadFromCharacteristicResult);
        }

        public Task<bool> GetCharacteristicNotifications(string deviceUuid, string serviceUuid, string characteristicUuid)
        {
            return Task.FromResult(BleNotificationResult);
        }


        public bool BleNotificationResult { get; set; }

        internal void RaiseDeviceValueChangedEvent(string deviceUuid, string serviceUuid, string characteristicUuid, string message)
        {
            var args = new BleDeviceValueChangedEventArgs(deviceUuid, serviceUuid, characteristicUuid, message);
            DeviceValueChanged(this, args);
        }
        internal void RaiseDeviceDiscoveredEvent(BleDevice device)
        {
            var bdea = new BleDeviceEventArgs(device);
            DeviceDiscovered(this, bdea);
        } 
        internal void RaiseDeviceDisconnectedEvent(BleDevice device)
        {
            var bdea = new BleDeviceEventArgs(device);
            DeviceDisconnected(this, bdea);
        }

        internal void SetGetGattServices(BleDevice device, IEnumerable<BleGattService> gattServices)
        {
            RaiseDeviceDiscoveredEvent(device);
            _gattServices[device.Id] = gattServices?.ToArray();
        }


    }
}