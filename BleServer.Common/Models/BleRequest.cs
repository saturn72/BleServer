using System.Collections.Generic;

namespace BleServer.Common.Models.Characteristic
{
    public sealed class BleRequest
    {
        private IEnumerable<string> _buffer;

        public string DeviceUuid { get; set; }
        public string ServiceUuid { get; set; }
        public string CharacteristicUuid { get; set; }

        public IEnumerable<string> Buffer
        {
            get => _buffer ?? (_buffer = new List<string>());
            set => _buffer = value;
        }
    }
}