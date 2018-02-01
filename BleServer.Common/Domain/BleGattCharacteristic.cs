using System;

namespace BleServer.Common.Domain
{
    public class BleGattCharacteristic
    {
        public BleGattCharacteristic(Guid uuid, string description)
        {
            Uuid = uuid;
            Description = description;
        }

        public Guid Uuid { get; }
        public string Description { get; }
    }
}