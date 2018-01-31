using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BleServer.Common.Domain;

namespace BleServer.Common.Services.BLE
{
    public class BluetoothService : IBluetoothService
    {
        private readonly IBluetoothManager _bluetoothManager;

        #region ctor
        public BluetoothService(IBluetoothManager bluetoothManager)
        {
            _bluetoothManager = bluetoothManager;
        }

        #endregion

        public async Task<IEnumerable<BluetoothDevice>> GetDevices()
        {
            return await Task.FromResult(_bluetoothManager.GetDiscoveredDevices() ?? new BluetoothDevice[] { });
        }

        public async Task<BluetoothDevice> GetDeviceById(string deviceId)
        {
            var allDevices = await GetDevices();
            return allDevices.FirstOrDefault(x => x.Id == deviceId);
        }
    }
}
