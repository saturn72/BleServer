using ConnectivityServer.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ConnectivityServer.WebApi.Controllers
{
    /// <summary>
    ///     ServiceResponse object's extensions
    /// </summary>
    public static class ServiceResponseExtensions
    {
        private static readonly IDictionary<ServiceResponseResult, Func<object, string, IActionResult>>
            _serviceResponseToActionResultDictionary =
                new Dictionary<ServiceResponseResult, Func<object, string, IActionResult>>
                {
                     {
                        ServiceResponseResult.Fail,
                        (obj, errMsg) => BuildObjectResult(obj, "Need to find better response code for failed operations",
                            StatusCodes.Status501NotImplemented)
                    },
                    {
                        ServiceResponseResult.NotSet,
                        (obj, errMsg) => BuildObjectResult(obj, "Service Response value was not set.",
                            StatusCodes.Status500InternalServerError)
                    },
                    {
                        ServiceResponseResult.Success, (obj, errMsg) => new OkObjectResult(obj)
                    },
                    {
                        ServiceResponseResult.Created,
                        (obj, errMsg) => new CreatedResult("", obj)
                    },
                    {
                        ServiceResponseResult.Updated,
                        (obj, errMsg) => new AcceptedResult("", obj)
                    },
                    {
                        ServiceResponseResult.BadOrMissingData,
                        (obj, errMsg) => new BadRequestObjectResult(new
                        {
                            Message = "Failed due to bad or missing data.",
                            Data = obj
                        })
                    },
                    {
                        ServiceResponseResult.NotFound,
                        (obj, errMsg) => new NotFoundObjectResult(new
                        {
                            Message = "The Object was not found",
                            Data = obj
                        })
                    },
                    {
                        ServiceResponseResult.NotAcceptable,
                        (obj, msg) => BuildObjectResult(obj, msg, StatusCodes.Status406NotAcceptable)
                    }
                };

        private static IActionResult BuildObjectResult(object data, string message, int httpStatusCode)
        {
            return new ObjectResult(new
            {
                message = message,
                data
            })
            {
                StatusCode = (int)httpStatusCode
            };
        }

        /// <summary>
        ///     Converts ServiceResponse to Generic IActionResult instance
        /// </summary>
        /// <param name="serviceResponse"></param>
        /// <returns></returns>
        public static IActionResult ToActionResult<T>(this ServiceResponse<T> serviceResponse)
        {
            return _serviceResponseToActionResultDictionary[serviceResponse.Result](serviceResponse.Data, serviceResponse.Message);
        }
    }
}