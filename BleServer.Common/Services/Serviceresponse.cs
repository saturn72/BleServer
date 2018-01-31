namespace BleServer.Common.Services
{
    public class ServiceResponse<T>
    {
        public ServiceResponseResult Result { get; set; }
        public string ErrorMessage { get; set; }
        public T Data { get; set; }
    }

    public enum ServiceResponseResult
    {
        NotFound,
        Success,
    }
}
