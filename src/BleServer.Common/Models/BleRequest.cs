using System.Collections.Generic;

namespace BleServer.Common.Models.Characteristic
{
    public sealed class BleRequest
    {
        public string DeviceUuid { get; set; }
        public string ServiceUuid { get; set; }
        public string CharacteristicUuid { get; set; }
        public byte[] Buffer { get; set; }
    }
}