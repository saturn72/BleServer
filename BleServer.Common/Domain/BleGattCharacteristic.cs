using System;

namespace BleServer.Common.Domain
{
    public class BleGattCharacteristic
    {
        public Guid Uuid { get; set; }
        public string Description { get; set; }
        public ushort AssignedNumber { get; set; }
    }
}