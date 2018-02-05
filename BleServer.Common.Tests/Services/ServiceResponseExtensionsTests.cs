using System.Collections.Generic;
using BleServer.Common.Services;
using Shouldly;
using Xunit;

namespace BleServer.Common.Tests.Services
{
    public class ServiceResponseExtensionsTests
    {
        [Theory]
        [MemberData(nameof(ServiceResponseExtensions_HasErrors_WithErrorData))]
        public void ServiceResponseExtensions_HasErrors_ReturnsTrue(ServiceResponse<object> serviceResponse)
        {
            serviceResponse.HasErrors().ShouldBeTrue();
        }

        public static IEnumerable<object[]> ServiceResponseExtensions_HasErrors_WithErrorData =>
            new[]
            {
                new[] {new ServiceResponse<object> {ErrorMessage = "come-error-message"}},
                new[] {new ServiceResponse<object>()},
                new[] {new ServiceResponse<object> {Result = ServiceResponseResult.NotFound}},
            };
    }
}
