using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ConnectivityServer.Common.Models.Characteristic;
using ConnectivityServer.Common.Services.Ble;
using Microsoft.AspNetCore.Mvc;

namespace ConnectivityServer.WebApi.Controllers
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
        [HttpPost("rx")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)] // bad or missing data: . msiind Id's
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.NotAcceptable)] //device disconnectws
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Accepted)] // everything's OK
        public async Task<IActionResult> WriteToCharacteristic([FromBody] BleRequest request)
        {
            if (!(VerifyBleRequest(request) && request.Buffer != null && request.Buffer.Any()))
                return BadRequest(
                    new
                    {
                        data = request,
                        message = "Bad or missing data"
                    });

            var res = await _blutoothService.WriteToCharacteristic(request.DeviceUuid,
                request.ServiceUuid, request.CharacteristicUuid, request.Buffer);

            return res.ToActionResult();
        }


        /// <summary>
        ///     Write to secific characteristics
        /// </summary>
        [HttpPost("tx")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.BadRequest)] // bad or missing data: . msiind Id's
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.NotAcceptable)] //device disconnectws
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Accepted)] // everything's OK
        public async Task<IActionResult> ReadFromCharacteristic([FromBody] BleRequest request)
        {
            if (!VerifyBleRequest(request))
                return BadRequest(
                    new
                    {
                        data = request,
                        message = "Bad or missing data"
                    });
            var res = await _blutoothService.ReadFromCharacteristic(request.DeviceUuid,
                request.ServiceUuid, request.CharacteristicUuid);

            return res.ToActionResult();
        }



        /// <summary>
        ///     Subscribe to specific characteristics
        /// </summary>
        [HttpPost("notify")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Accepted)] // everything's OK
        public async Task<IActionResult> GetCharacteristicNotifications([FromBody] BleRequest request)
        {
            if (!VerifyBleRequest(request))
                return BadRequest(
                    new
                    {
                        data = request,
                        message = "Bad or missing data"
                    });

            var res = await _blutoothService.GetCharacteristicNotifications(request.DeviceUuid,
                request.ServiceUuid, request.CharacteristicUuid);

            return res.ToActionResult();
        }

        private bool VerifyBleRequest(BleRequest request)
        {
            return request != null &&
                   !string.IsNullOrEmpty(request.DeviceUuid) &&
                   !string.IsNullOrEmpty(request.ServiceUuid) &&
                   !string.IsNullOrEmpty(request.CharacteristicUuid);
        }
    }
}