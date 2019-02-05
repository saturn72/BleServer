using BleServer.Common.Services;
using Shouldly;
using Xunit;

namespace BleServer.Common.Tests.Services
{
    public class ServiceResponseTests
    {
        [Fact]
        public void ServiceResponse_SetResultToNotSetInCtor()
        {
            var sr = new ServiceResponse<string>();
            sr.Result.ShouldBe(ServiceResponseResult.NotSet);
        }
    }
}
