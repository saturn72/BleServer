﻿using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ConnectivityServer.Common.Models;
using ConnectivityServer.Common.Services;
using ConnectivityServer.Common.Services.Ble;
using Microsoft.AspNetCore.Mvc;

namespace ConnectivityServer.WebApi.Controllers
{
    [Route("api/ble/[controller]")]
    public class GattServiceController : Controller
    {
        private readonly IBleService _blutoothService;

        public GattServiceController(IBleService blutoothService)
        {
            _blutoothService = blutoothService;
        }

        /// <summary>
        ///     Gets all GATT services
        /// </summary>
        /// <param name="id">device's Id</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IEnumerable<BleGattService>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetGattServicesByDeviceId(string id)
        {
            var gattServices = await _blutoothService.GetGattServicesByDeviceId(id);
            if (gattServices.Result == ServiceResponseResult.NotFound)
                return NotFound(new
                {
                    message = "Failed to find thre required resource",
                    id = id
                });
            return Ok(gattServices.Data ?? new BleGattService[] { });
        }
    }
}