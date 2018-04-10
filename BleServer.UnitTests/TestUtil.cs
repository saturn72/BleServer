namespace BleServer.WebApi.Tests
{
    public class TestUtil
    {
        public static object GetPropertyValue(object obj, string propertyName)
        {
            var pi = obj.GetType().GetProperty(propertyName);
            return pi.GetValue(obj);
        }
    }
}