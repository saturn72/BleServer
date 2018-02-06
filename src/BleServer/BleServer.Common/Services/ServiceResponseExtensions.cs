namespace BleServer.Common.Services
{
    public static class ServiceResponseExtensions
    {
        public static bool HasErrors<T>(this ServiceResponse<T> serviceResponse)
        {
            return !string.IsNullOrWhiteSpace(serviceResponse.ErrorMessage) 
                   || !string.IsNullOrEmpty(serviceResponse.ErrorMessage)
                   || serviceResponse.Result != ServiceResponseResult.Success;
        }
    }
}
