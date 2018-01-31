using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BleServer.Common.Domain;
using BleServer.Common.Services.BLE;

namespace BleServer.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class DeviceController : Controller
    {
        private readonly IBluetoothService _blutoothLEservice;

        public DeviceController(IBluetoothService blutoothLEservice)
        {
            _blutoothLEservice = blutoothLEservice;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDevicesAsync()
        {
            var devices = await _blutoothLEservice.GetDevices() ?? new BluetoothDevice[] { };
            return Ok(devices);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeviceByIdAsync(string id)
        {
            var device = await _blutoothLEservice.GetDeviceById(id);
            return device != null ?
                Ok(device) :
                NotFound(new
                {
                    message = "Failed to find bluetooth device",
                    @id = id
                }) as IActionResult;
        }

        //[HttpGet("services/{id}")]
        //public async Task<IActionResult> GetDeviceBluetoothService(string id)
        //{
        //    throw new NotImplementedException("Getdevice Services");
        //}
    }
}
