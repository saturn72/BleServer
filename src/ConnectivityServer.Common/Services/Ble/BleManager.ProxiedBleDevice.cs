using ConnectivityServer.Common.Models;

namespace ConnectivityServer.Common.Services.Ble
{
    public partial class BleManager
    {
        #region nested classes

        public class ProxiedBleDevice
        {
            public ProxiedBleDevice(IBleAdapter adapter, BleDevice device)
            {
                Device = device;
                Adapter = adapter;
            }

            internal BleDevice Device { get; }
            internal IBleAdapter Adapter { get; }
        }
        #endregion
    }
}
