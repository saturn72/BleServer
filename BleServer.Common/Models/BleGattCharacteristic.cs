using System;

namespace BleServer.Common.Models
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