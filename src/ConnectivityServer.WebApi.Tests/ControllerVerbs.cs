using ConnectivityServer.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace Connectivity.ServerWebApi.Tests
{
    public class HttpVerbsTests
    {
        [Theory]
        [InlineData(typeof(CharacteristicController),  "api/ble/characteristic")]
        [InlineData(typeof(DeviceController),  "api/ble/device")]
        [InlineData(typeof(GattServiceController), "api/ble/gattservice")]
        public void ControllerRouteTests(Type ctrlType, string expRoute)
        {
            var rAtt = ctrlType.GetCustomAttributes(typeof(RouteAttribute), true)[0] as RouteAttribute;
            rAtt.Template.ToLower().ShouldBe(expRoute);
        }

        public const string Post = "POST";
        public const string Get = "GET";
        public const string Delete = "DELETE";

        [Theory]
        [InlineData(typeof(CharacteristicController), nameof(CharacteristicController.WriteToCharacteristic), "rx", Post)]
        [InlineData(typeof(CharacteristicController), nameof(CharacteristicController.ReadFromCharacteristic), "tx", Post)]
        [InlineData(typeof(CharacteristicController), nameof(CharacteristicController.GetCharacteristicNotifications), "notify", Post)]
        [InlineData(typeof(DeviceController), nameof(DeviceController.GetAllDiscoveredDevices), null, Get)]
        [InlineData(typeof(DeviceController), nameof(DeviceController.GetDeviceById), "{id}", Get)]
        [InlineData(typeof(DeviceController), nameof(DeviceController.DisconnectDeviceById), "{id}", Delete)]
        [InlineData(typeof(GattServiceController), nameof(GattServiceController.GetGattServicesByDeviceId), "{id}", Get)]
        public void ControllerMthodRouteTests(Type ctrlType, string methodName, string expHttpVerb, string exptemplate)
        {
            var mi = ctrlType.GetMethod(methodName);
            var hma = mi.GetCustomAttributes(typeof(HttpMethodAttribute), true)[0] as HttpMethodAttribute;
            hma.HttpMethods.Count().ShouldBe(1);
            hma.HttpMethods.First().ShouldBe(expHttpVerb);
            hma.Template.ShouldBe(exptemplate);
        }
    }
}