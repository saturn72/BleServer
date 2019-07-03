using ConnectivityServer.Common.Models;

namespace ConnectivityServer.Common.Services.Nfc
{
    public partial class NfcManager
    {
        public class ProxiedNfcDevice
        {
            public ProxiedNfcDevice(INfcAdapter adapter, NfcDevice device)
            {
                Device = device;
                Adapter = adapter;
            }

            internal NfcDevice Device { get; }
            internal INfcAdapter Adapter { get; }
        }
    }
}
