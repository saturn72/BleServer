using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectivityServer.Common.Models;

namespace ConnectivityServer.Common.Services.Nfc
{
    public interface INfcManager
    {
        IEnumerable<NfcDevice> GetDiscoveredDevices();
        Task<bool> SubscribeForMessage(string messageType, Action action);
    }
}
