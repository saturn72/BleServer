using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BleServer.Common.Domain;
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
            return device != null
                ? Ok(device)
                : NotFound(new
                {
                    message = "Failed to find bluetooth device",
                    @id = id
                }) as IActionResult;
        }

        /// <summary>
        /// Disconnects from device
        /// </summary>
        /// <param name="id">deviceId</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DisconnectDeviceAsync(string id)
        {
            var wasDisconnected = await _blutoothservice.UnpairDeviceById(id);
            return wasDisconnected
                ? Accepted()
                : StatusCode((int) HttpStatusCode.NotAcceptable, new
                {
                    message = "Failed to disconnect device",
                    @id = id
                }) as IActionResult;
        }
    }
}
