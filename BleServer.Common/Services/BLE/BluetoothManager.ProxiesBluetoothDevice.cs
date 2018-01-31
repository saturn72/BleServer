using BleServer.Common.Domain;

namespace BleServer.Common.Services.BLE
{
    public partial class BluetoothManager
    {
        #region nested classes

        public class ProxiesBluetoothDevice
        {
            public ProxiesBluetoothDevice(IBluetoothAdapter adapter, BluetoothDevice device)
            {
                Device = device;
                Adapter = adapter;
            }

            internal BluetoothDevice Device { get; }
            internal IBluetoothAdapter Adapter { get; }
        }
        #endregion
    }
}
