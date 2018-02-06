using System;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BleChannelStability.Init();
             var summary = BenchmarkRunner.Run<BleChannelStability>();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public class BleChannelStability
        {
            private const string GenericAccessServiceAssignedNumber = "0x1800";
            private const string DeviceNameCharacteristicAssignedNumber = "0x2a00";

            private const string AvosetBleDeviceId =
                "BluetoothLE%23BluetoothLEa4%3A34%3Ad9%3A92%3A93%3Aec-f8%3Af0%3A05%3Ae5%3Add%3A87";
            private const string BaseUrl = "http://localhost:56963/api/Device/";

            private static readonly HttpClient _httpClient = new HttpClient();

            public static void Init()
            {
                //pull services from device
                var getGattServicesResourceUri = new Uri(BaseUrl + "gatt-services/" + AvosetBleDeviceId);

                var getServicesResoinse = _httpClient.GetAsync(getGattServicesResourceUri).Result;
                if (!getServicesResoinse.IsSuccessStatusCode)
                {
                    WriteToConsole(false, "Failed to pull services from device : " + AvosetBleDeviceId);
                    return;
                }

            }
            [Benchmark]
            public async Task BleOvertime()
            {
                
                var readCharacterUri = new Uri(BaseUrl + string.Format("gatt-characteristic/{0}/{1}/{2}",
                                                   AvosetBleDeviceId, GenericAccessServiceAssignedNumber,
                                                   DeviceNameCharacteristicAssignedNumber));

                    var chrResponse = _httpClient.GetAsync(readCharacterUri).Result;
                //chrResponse.
                //    var isSuccess = chrResponse.IsSuccessStatusCode;
                //    WriteToConsole(isSuccess, string.Format("Status code: {0}, Managed to get characteristics from BLE device:{1}", chrResponse.StatusCode, AvosetBleDeviceId));
            }

            private static void WriteToConsole(bool isSuccess, string message)
            {
                var temp = Console.ForegroundColor;
                Console.ForegroundColor = isSuccess ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine(DateTime.Now + "\t>>>\t" + message);
                Console.ForegroundColor = temp;
            }
        }
    }
}
