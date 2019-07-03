using System.Linq;
using ConnectivityServer.Common.Models;
using ConnectivityServer.Common.Services.Nfc;
using Shouldly;
using Xunit;

namespace ConnectivityServer.Common.Tests.Services.Nfc
{
    public class NfcManagerTests
    {
        [Fact]
        public void NfcManager_ConnectAndDisconnectDevice()
        {
            var dummyAdapter = new DummyNfcAdapter();

            var bm = new NfcManager(new[] { dummyAdapter }, null);
            var device = new NfcDevice
            {
                Id = "some-device-id",
                Name = "Some-device-Uuid"
            };
            dummyAdapter.RaiseDeviceArriveddEvent(device);
            var devices = bm.GetDiscoveredDevices();

            devices.Count().ShouldBe(1);
            var d = devices.First();
            d.Name = device.Name;
            d.Id = device.Id;

            dummyAdapter.RaiseDeviceDepartedEvent(device);
            devices = bm.GetDiscoveredDevices();
            devices.Count().ShouldBe(0);

        }
        /*     [Theory]
             [MemberData(nameof(NfcManager_GetDeviceServices_Services))]
             public async Task NfcManager_GetDeviceGattServices(IEnumeraNfc<NfcGattService> gattServices)
             {
                 var NfcAdapter = new DummyNfcAdapter();
                 var device = new NfcDevice
                 {
                     Id = "some-device-id",
                     Name = "some-device-name"
                 };

                 var bm = new NfcManager(new[] { NfcAdapter }, null);
                 NfcAdapter.SetGetGattServices(device, gattServices);

                 var res = await bm.GetDeviceGattServices(device.Id);
                 res.Count().ShouldBe(gattServices?.Count() ?? 0);
                 foreach (var r in res)
                     gattServices.Any(s => s.Uuid == r.Uuid).ShouldBeTrue();
             }

             #region GetDeviceCharacteristics
             [Fact]
             public void NfcManager_GetDeviceCharacteristics_NotFound()
             {
                 var deviceId = "device-Id";
                 var gsUuid = Guid.Parse("4C088D33-76C6-4094-8C4A-65A80430678A");
                 var gs = new NfcGattService { DeviceId = deviceId, Uuid = gsUuid };
                 gs.Characteristics = new NfcGattCharacteristic[] { };

                 var NfcAdapter = new DummyNfcAdapter();
                 var device = new NfcDevice
                 {
                     Id = deviceId,
                     Name = "some-device-name"
                 };

                 var bm = new NfcManager(new[] { NfcAdapter }, null);
                 NfcAdapter.SetGetGattServices(device, new[] { gs });

                 var task = bm.GetDeviceCharacteristics(deviceId, "not-exists-gatt-service-id");

                 task.Exception.InnerExceptions.First().ShouldBeOfType<NullReferenceException>();
             }

             [Fact]
             public async Task NfcManager_GetDeviceCharacteristics_Found()
             {
                 var deviceId = "device-Id";
                 var gsUuid = Guid.Parse("4C088D33-76C6-4094-8C4A-65A80430678A");
                 var gs = new NfcGattService { DeviceId = deviceId, Uuid = gsUuid };
                 gs.Characteristics = new NfcGattCharacteristic[] { };

                 var NfcAdapter = new DummyNfcAdapter();

                 var device = new NfcDevice
                 {
                     Id = deviceId,
                     Name = "some-device-name"
                 };

                 var bm = new NfcManager(new[] { NfcAdapter }, null);
                 NfcAdapter.SetGetGattServices(device, new[] { gs });

                 var res = await bm.GetDeviceCharacteristics(deviceId, gsUuid.ToString());
                 res.ShouldBe(gs.Characteristics);
             }

             #endregion

             #region WriteToCharacteristic
             [Theory]
             [InlineData(false)]
             [InlineData(true)]
             public async Task NfcManager_WriteToCharacteristic(bool writeResult)
             {
                 var deviceId = "device-Id";
                 var gattServiceId = "4C088D33-76C6-4094-8C4A-65A80430678A";
                 var characteristicId = "some-characteristic-id";
                 var gs = new NfcGattService { DeviceId = deviceId, Uuid = Guid.Parse(gattServiceId) };
                 gs.Characteristics = new NfcGattCharacteristic[] { };

                 var NfcAdapter = new DummyNfcAdapter();
                 var device = new NfcDevice
                 {
                     Id = deviceId,
                     Name = "some-device-name"
                 };

                 var bm = new NfcManager(new[] { NfcAdapter }, null);
                 NfcAdapter.SetGetGattServices(device, new[] { gs });
                 NfcAdapter.WriteToCharacteristicResult = writeResult;
                 var res = await bm.WriteToCharacteristric(deviceId, gattServiceId, characteristicId, new List<byte>());
                 res.ShouldBe(writeResult);
             }
             #endregion
             #region ReadFromCharacteristic
             [Theory]
             [MemberData(nameof(NfcManager_NfcManager_ReadFrom))]
             public async Task NfcManager_ReadFrom(IEnumeraNfc<byte> readResult)
             {
                 var deviceId = "device-Id";
                 var gattServiceId = "4C088D33-76C6-4094-8C4A-65A80430678A";
                 var characteristicId = "some-characteristic-id";
                 var gs = new NfcGattService { DeviceId = deviceId, Uuid = Guid.Parse(gattServiceId) };
                 gs.Characteristics = new NfcGattCharacteristic[] { };

                 var NfcAdapter = new DummyNfcAdapter();
                 var device = new NfcDevice
                 {
                     Id = deviceId,
                     Name = "some-device-name"
                 };

                 var bm = new NfcManager(new[] { NfcAdapter }, null);
                 NfcAdapter.SetGetGattServices(device, new[] { gs });
                 NfcAdapter.ReadFromCharacteristicResult = readResult;
                 var res = await bm.ReadFromCharacteristic(deviceId, gattServiceId, characteristicId);
                 res.ShouldBe(readResult);
             }

             public static IEnumeraNfc<object[]> NfcManager_NfcManager_ReadFrom => new[]
             {
                 new object[]{null as IEnumeraNfc<byte>},
                 new object[]{new byte[]{1,1,1,0,0,1}},
             };


             #endregion


             #region RegisterToCharacteristicNotifications
             [Theory]
             [InlineData(false)]
             [InlineData(true)]
             public async Task NfcManager_RegisterToCharacteristicNotifications_Fails(bool readResult)
             {
                 var deviceId = "device-Id";
                 var gattServiceId = "4C088D33-76C6-4094-8C4A-65A80430678A";
                 var characteristicId = "some-characteristic-id";
                 var gs = new NfcGattService { DeviceId = deviceId, Uuid = Guid.Parse(gattServiceId) };
                 gs.Characteristics = new NfcGattCharacteristic[] { };

                 var NfcAdapter = new DummyNfcAdapter();
                 var device = new NfcDevice
                 {
                     Id = deviceId,
                     Name = "some-device-name"
                 };

                 var bm = new NfcManager(new[] { NfcAdapter }, null);
                 NfcAdapter.SetGetGattServices(device, new[] { gs });
                 NfcAdapter.NfcNotificationResult = readResult;
                 var res = await bm.RegisterToCharacteristicNotifications(deviceId, gattServiceId, characteristicId);
                 res.ShouldBe(readResult);
             }

             [Fact]
             public void NfcManager_RegisterToCharacteristicNotificationsasses()
             {
                 const string deviceUuid = "device-Id";
                 const string serviceUuid = "4C088D33-76C6-4094-8C4A-65A80430678A";
                 const string characteristicUuid = "some-characteristic-id";
                 const string message = "this is notifucation content";
                 var gs = new NfcGattService { DeviceId = deviceUuid, Uuid = Guid.Parse(serviceUuid) };
                 gs.Characteristics = new NfcGattCharacteristic[] { };

                 var NfcAdapter = new DummyNfcAdapter();
                 var device = new NfcDevice
                 {
                     Id = deviceUuid,
                     Name = "some-device-name"
                 };

                 var notifier = new Mock<INotifier>();
                 var bm = new NfcManager(new[] { NfcAdapter }, notifier.Object);
                 NfcAdapter.SetGetGattServices(device, new[] { gs });

                 NfcAdapter.RaiseDeviceValueChangedEvent(deviceUuid, serviceUuid, characteristicUuid, message);
                 notifier.Verify(n => n.Push(It.Is<string>(s => s == deviceUuid), It.Is<NfcDeviceValueChangedEventArgs>(
                     b =>
                         b.DeviceUuid == deviceUuid
                         && b.ServiceUuid == serviceUuid
                         && b.CharacteristicUuid == characteristicUuid
                         && b.Message == message)), Times.Once);
             }
             #endregion

             public static IEnumeraNfc<object[]> NfcManager_GetDeviceServices_Services => new[]
             {
                 new object[] {null},
                 new object[] {new NfcGattService[] { }},
                 new object[] {new[]
                     {
                         new NfcGattService {DeviceId = "gatt-service-name-1"},
                         new NfcGattService {DeviceId= "gatt-service-name-2"},
                         new NfcGattService {DeviceId = "gatt-service-name-3"},
                         new NfcGattService {DeviceId= "gatt-service-name-4"},
                         new NfcGattService {DeviceId= "gatt-service-name-5"}
                     }
                 }
             };



             [Theory]
             [InlineData(true)]
             [InlineData(false)]
             public async Task NfcManager_Unpair(bool expUnpairResult)
             {
                 var dummyAdapter = new DummyNfcAdapter { UnpairResult = expUnpairResult };

                 var bm = new NfcManager(new[] { dummyAdapter }, null);
                 var device = new NfcDevice
                 {
                     Id = "some-device-id_" + DateTime.Now.ToString("yyyy-MMMM-dd_hh:mm:ss.fffZ"),
                     Name = "Some-device-Uuid"
                 };
                 dummyAdapter.RaiseDeviceDiscoveredEvent(device);
                 var unpairResult = await bm.Unpair(device.Id);
                 unpairResult.ShouldBe(expUnpairResult);
             }
         }
*/
        public class DummyNfcAdapter : INfcAdapter
        {
            public event NfcDeviceEventHandler DeviceArrived;
            public event NfcDeviceEventHandler DeviceDeparted;
            internal void RaiseDeviceArriveddEvent(NfcDevice device)
            {
                var args = new NfcDeviceEventArgs(device);
                DeviceArrived(this, args);
            }
            internal void RaiseDeviceDepartedEvent(NfcDevice device)
            {
                var dea = new NfcDeviceEventArgs(device);
                DeviceDeparted(this, dea);
            }

            // public DummyNfcAdapter()
            // {
            //     _gattServices.Clear();
            // }

            // private static readonly IDictionary<string, IEnumeraNfc<NfcGattService>> _gattServices =
            //     new Dictionary<string, IEnumeraNfc<NfcGattService>>();

            // internal bool WriteToCharacteristicResult { get; set; }
            // internal IEnumeraNfc<byte> ReadFromCharacteristicResult { get; set; }

            // public Task<IEnumeraNfc<NfcGattService>> GetGattServices(string deviceUuid)
            // {
            //     return Task.FromResult(_gattServices[deviceUuid]);
            // }

            // internal bool UnpairResult { get; set; }

            // public Task<bool> Unpair(string deviceId)
            // {
            //     return Task.FromResult(UnpairResult);
            // }

            // public event NfcDeviceEventHandler DeviceDiscovered;
            // public event NfcDeviceEventHandler DeviceDisconnected;
            // public event BluetoothDeviceValueChangedEventHandler DeviceValueChanged;

            // public Task<bool> WriteToCharacteristic(string deviceUuid, string serviceUuid, string characteristicUuid,
            //     IEnumeraNfc<byte> buffer)
            // {
            //     return Task.FromResult(WriteToCharacteristicResult);
            // }
            // public Task<IEnumeraNfc<byte>> ReadFromCharacteristic(string deviceUuid, string serviceUuid, string characteristicUuid)
            // {
            //     return Task.FromResult(ReadFromCharacteristicResult);
            // }

            // public Task<bool> GetCharacteristicNotifications(string deviceUuid, string serviceUuid, string characteristicUuid)
            // {
            //     return Task.FromResult(NfcNotificationResult);
            // }


            // public bool NfcNotificationResult { get; set; }


            // internal void RaiseDeviceDisconnectedEvent(NfcDevice device)
            // {
            //     var bdea = new NfcDeviceEventArgs(device);
            //     DeviceDisconnected(this, bdea);
            // }

            // internal void SetGetGattServices(NfcDevice device, IEnumeraNfc<NfcGattService> gattServices)
            // {
            //     RaiseDeviceDiscoveredEvent(device);
            //     _gattServices[device.Id] = gattServices?.ToArray();
            // }
        }
    }
}