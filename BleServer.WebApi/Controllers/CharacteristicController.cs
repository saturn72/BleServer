using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BleServer.Common.Models.Characteristic;
using BleServer.Common.Services.Ble;
using Microsoft.AspNetCore.Mvc;

namespace BleServer.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class CharacteristicController : Controller
    {
        private readonly IBleService _blutoothService;

        public CharacteristicController(IBleService blutoothService)
        {
            _blutoothService = blutoothService;
        }

        /// <summary>
        ///     Write to secific characteristics
        /// </summary>
        [HttpPost("write")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)] // bad or missing data: . msiind Id's
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.NotAcceptable)] //device disconnectws
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Accepted)] // everything's OK
        public async Task<IActionResult> WriteToCharacteristic([FromBody] BleRequest writeRequest)
        {
            if (!VerifyWriteToCharacteristicsModelModel(writeRequest))
                return BadRequest(
                    new
                    {
                        data = writeRequest,
                        message = "Bad or missing data"
                    });
            var buffer = writeRequest.Buffer.Select(s => Convert.ToByte(s, 16)).ToArray();
            var res = await _blutoothService.WriteToCharacteristic(writeRequest.DeviceUuid,
                writeRequest.ServiceUuid, writeRequest.CharacteristicUuid, buffer);

            return res.ToActionResult();
        }

        private bool VerifyWriteToCharacteristicsModelModel(BleRequest request)
        {
            return request != null &&
                   !string.IsNullOrEmpty(request.DeviceUuid) &&
                   !string.IsNullOrEmpty(request.ServiceUuid) &&
                   !string.IsNullOrEmpty(request.CharacteristicUuid) &&
                   request.Buffer != null && request.Buffer.Any();
        }

        /// <summary>
        ///     Subscribe to specific characteristics
        /// </summary>
        [HttpPost("read")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Accepted)] // everything's OK
        public async Task<IActionResult> ReadFromCharacteristic([FromBody] BleRequest subscribeRequest)
        {
            if (!VerifySubscribeToBleRequest(subscribeRequest))
                return BadRequest(
                    new
                    {
                        data = subscribeRequest,
                        message = "Bad or missing data"
                    });

            var res = await _blutoothService.ReadFromCharacteristic(subscribeRequest.DeviceUuid,
                subscribeRequest.ServiceUuid, subscribeRequest.CharacteristicUuid);

            return res.ToActionResult();
        }

        private bool VerifySubscribeToBleRequest(BleRequest request)
        {
            return request != null &&
                   !string.IsNullOrEmpty(request.DeviceUuid) &&
                   !string.IsNullOrEmpty(request.ServiceUuid) &&
                   !string.IsNullOrEmpty(request.CharacteristicUuid);
        }
    }
}