namespace BleServer.Common.Services
{
    public class ServiceResponse<T>
    {
        #region ctor

        public ServiceResponse()
        {
            Result = ServiceResponseResult.NotSet;
        }

        #endregion

        #region Properties

        public ServiceResponseResult Result { get; set; }
        public string ErrorMessage { get; set; }
        public T Data { get; set; }

        #endregion
    }
}
