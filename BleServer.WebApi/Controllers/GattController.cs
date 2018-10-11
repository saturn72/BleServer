using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BleServer.Common.Models;
using BleServer.Common.Services;
using BleServer.Common.Services.Ble;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace BleServer.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class GattController : Controller
    {
        private readonly IBleService _blutoothService;

        public GattController(IBleService blutoothService)
        {
            _blutoothService = blutoothService;
        }

        /// <summary>
        ///     Gets all GATT services
        /// </summary>
        /// <param name="id">device's Id</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IEnumerable<BleGattService>), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetGattServicesByDeviceId(string id)
        {
            var gattServices = await _blutoothService.GetGattServicesByDeviceId(id);
            if (gattServices.Result == ServiceResponseResult.NotFound)
                return NotFound(new
                {
                    message = "Failed to find thre required resource",
                    id
                });
            return Ok(gattServices.Data ?? new BleGattService[] { });
        }

        /// <summary>
        ///     Write to secific characteristics
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(object), (int) HttpStatusCode.BadRequest)] // bad or missing data: . msiind Id's
        [ProducesResponseType(typeof(object), (int) HttpStatusCode.NotAcceptable)] //device disconnectws
        [ProducesResponseType(typeof(CommandResponse), (int) HttpStatusCode.Accepted)] // everything's OK
        public async Task<IActionResult> WriteToCharacteristic([FromBody] CommandRequest commandRequest)
        {
            if (!VerifyWriteToCharacteristicsModelModel(commandRequest))
                return BadRequest(
                    new
                    {
                        data = commandRequest,
                        message = "Bad or missing data"
                    });
            var buffer = commandRequest.Buffer.Select(s=>Convert.ToByte(s, 16)).ToArray();
            var buffer2 = commandRequest.Buffer.Select(s=>byte.Parse(s, NumberStyles.HexNumber)).ToArray();
            var res = await _blutoothService.WriteToCharacteristic(commandRequest.DeviceUuid,
                commandRequest.ServiceUuid, commandRequest.CharacteristicUuid, buffer);

            return res.ToActionResult();
        }

        private bool VerifyWriteToCharacteristicsModelModel(CommandRequest model)
        {
            return model != null &&
                   !string.IsNullOrEmpty(model.DeviceUuid) &&
                   !string.IsNullOrEmpty(model.ServiceUuid) &&
                   !string.IsNullOrEmpty(model.CharacteristicUuid) &&
                   model.Buffer != null && model.Buffer.Any();
            //!string.IsNullOrEmpty(model.Buffer);
        }
    }
}