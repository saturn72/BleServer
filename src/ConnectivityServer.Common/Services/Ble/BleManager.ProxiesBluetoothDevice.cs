using ConnectivityServer.Common.Models;

namespace ConnectivityServer.Common.Services.Ble
{
    public partial class BleManager
    {
        #region nested classes

        public class ProxiesBluetoothDevice
        {
            public ProxiesBluetoothDevice(IBleAdapter adapter, BleDevice device)
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
