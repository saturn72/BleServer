
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Shouldly;

namespace ConnectivityServer.E2E
{
    [TestFixture]
    public class TxRxTests
    {
        private const string Rx = "6e400002-b5a3-f393-e0a9-e50e24dcca9e";
        private const string Tx = "6e400003-b5a3-f393-e0a9-e50e24dcca9e";
        private const string CharacteristicUri = "/api/Characteristic/";
        private const string ServiceUuid = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
        private const string DeviceName = "Automation BLE test";
        private string _deviceUuid;
        private HttpClient _client;

        [OneTimeSetUp]
        public async Task GivenARequestToTheController()
        {
            _client = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:56963/")
            };
            var deviceRes = await _client.GetAsync("api/device");
            var deviceContent = await deviceRes.Content.ReadAsStringAsync();
            var allDevices = JArray.Parse(deviceContent);
            var d = allDevices.FirstOrDefault(x => x["name"].Value<string>().ToLower() == DeviceName.ToLower());
            _deviceUuid = d["id"].Value<string>();

        }
        [Test]
        [Repeat(3000)]
        public async Task RxTxMainTest()
        {
            var notifyBody = new
            {
                deviceUuid = "BluetoothLE#BluetoothLE5c:f3:70:8b:2c:d7-cd:e2:c6:0f:3c:74",
                serviceUuid = "6e400001-b5a3-f393-e0a9-e50e24dcca9e",
                characteristicUuid = Tx
            };

            var res = await _client.PostAsJsonAsync(CharacteristicUri + "notify", notifyBody);
            res.EnsureSuccessStatusCode();
            var stringBuffer = "Roi Shabtai";
            var byteBuffer = Encoding.UTF8.GetBytes(stringBuffer);
            var writeBody = new
            {
                deviceUuid = "BluetoothLE#BluetoothLE5c:f3:70:8b:2c:d7-cd:e2:c6:0f:3c:74",
                serviceUuid = "6e400001-b5a3-f393-e0a9-e50e24dcca9e",
                characteristicUuid = Rx,
                buffer = byteBuffer
            };

            res = await _client.PostAsJsonAsync(CharacteristicUri + "rx", writeBody);
            res.EnsureSuccessStatusCode();
            var writeContent = await res.Content.ReadAsAsync<byte[]>();
            writeContent.ShouldBe(byteBuffer);
            Encoding.UTF8.GetString(byteBuffer).ShouldBe(stringBuffer);

            var readBody = new
            {
                deviceUuid = "BluetoothLE#BluetoothLE5c:f3:70:8b:2c:d7-cd:e2:c6:0f:3c:74",
                serviceUuid = "6e400001-b5a3-f393-e0a9-e50e24dcca9e",
                characteristicUuid = Tx
            };
            res = await _client.PostAsJsonAsync(CharacteristicUri + "tx", readBody);
            res.EnsureSuccessStatusCode();
            var readContent = await res.Content.ReadAsAsync<object>();
            var readBytes = Convert.FromBase64String(readContent.ToString());
            Encoding.UTF8.GetString(readBytes).ShouldBe(stringBuffer);
        }
    }
}