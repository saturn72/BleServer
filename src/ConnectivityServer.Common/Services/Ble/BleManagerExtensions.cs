using System;
using System.Linq;
using System.Threading.Tasks;
using ConnectivityServer.Common.Models;

namespace ConnectivityServer.Common.Services.Ble
{
    public static class BleManagerExtensions
    {
        public static async Task<BleGattService> GetGattServiceById(this IBleManager bleManager, string deviceId, string gattServiceId)
        {
            try
            {
                var allGattServices = await bleManager.GetDeviceGattServices(deviceId);
                return allGattServices?.FirstOrDefault(g => g.Uuid == Guid.Parse(gattServiceId));
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}