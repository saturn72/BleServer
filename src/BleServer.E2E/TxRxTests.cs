
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Shouldly;

namespace BleServer.E2E
{
    [TestFixture]
    public class TxRxTests
    {
    private const string Rx = "6e400002-b5a3-f393-e0a9-e50e24dcca9e";
    private const string Tx = "6e400003-b5a3-f393-e0a9-e50e24dcca9e";

    private const string ServiceUuid = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
    private const string DeviceName = "Automation BLE test";
    private string _deviceUuid;
        private HttpClient _client;


         [OneTimeSetUp]
        public async Task GivenARequestToTheController()
        {
            _client = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:5000/")
            };
            var deviceRes =  await _client.GetAsync("api/device");
            var deviceContent = await deviceRes.Content.ReadAsStringAsync();
            var allDevices = JArray.Parse(deviceContent);
            var d = allDevices.FirstOrDefault(x => x["name"].Value<string>().ToLower() == DeviceName.ToLower());
            _deviceUuid = d["id"].Value<string>();

        }
        [Test]
        public void RxTxMainTest()
        {

            true.ShouldBeFalse("tjis is expected false");
        }
    }
}