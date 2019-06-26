using System.Collections.Generic;

namespace BleServer.Common.Models.Characteristic
{
    public sealed class BleRequest
    {
        private IEnumerable<byte> _buffer;

        public string DeviceUuid { get; set; }
        public string ServiceUuid { get; set; }
        public string CharacteristicUuid { get; set; }

        public IEnumerable<byte> Buffer
        {
            get => _buffer ?? (_buffer = new List<byte>());
            set => _buffer = value;
        }
    }
}