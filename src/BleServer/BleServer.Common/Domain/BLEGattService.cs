using System;
using System.Collections.Generic;

namespace BleServer.Common.Domain
{
    public class BleGattService
    {
        public Guid Uuid { get; set; }
        public string DeviceId { get; set; }
        public IEnumerable<BleGattCharacteristic> Characteristics { get; set; }
        public ushort AssignedNumber { get; set; }
    }
}
