using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BleServer.Common.Domain;
using BleServer.Common.Services;
using BleServer.Common.Services.Ble;

namespace BleServer.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class DeviceController : Controller
    {
        private readonly IBleService _blutoothservice;

        public DeviceController(IBleService blutoothservice)
        {
            _blutoothservice = blutoothservice;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDevicesAsync()
        {
            var devices = await _blutoothservice.GetDevices() ?? new BleDevice[] { };
            return Ok(devices);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeviceByIdAsync(string id)
        {
            var device = await _blutoothservice.GetDeviceById(id);
            return device != null ?
                Ok(device) :
                NotFound(new
                {
                    message = "Failed to find bluetooth device",
                    @id = id
                }) as IActionResult;
        }

        [HttpGet("gatt-services/{id}")]
        public async Task<IActionResult> GetGattServicesByDeviceId(string id)
        {
            var gattServices = await _blutoothservice.GetGattServicesByDeviceId(id);
            if (gattServices.Result == ServiceResponseResult.NotFound)
                return NotFound(new
                {
                    message = "Failed to find thre required resource",
                    @id = id
                });
            return Ok(gattServices.Data ?? new BleGattService[]{});
        }
    }
}
