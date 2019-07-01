using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ConnectivityServer.Common.Models;
using ConnectivityServer.Common.Services;
using ConnectivityServer.Common.Services.Ble;
using Microsoft.AspNetCore.Mvc;

namespace ConnectivityServer.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ServiceController : Controller
    {
        private readonly IBleService _blutoothService;

        public ServiceController(IBleService blutoothService)
        {
            _blutoothService = blutoothService;
        }

        /// <summary>
        ///     Gets all GATT services
        /// </summary>
        /// <param name="deviceId">device's Id</param>
        [HttpGet("{deviceId}")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IEnumerable<BleGattService>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetGattServicesByDeviceId(string deviceId)
        {
            var gattServices = await _blutoothService.GetGattServicesByDeviceId(deviceId);
            if (gattServices.Result == ServiceResponseResult.NotFound)
                return NotFound(new
                {
                    message = "Failed to find thre required resource",
                    id = deviceId
                });
            return Ok(gattServices.Data ?? new BleGattService[] { });
        }
    }
}