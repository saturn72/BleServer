namespace BleServer.Common.Models.Characteristic
{
    public sealed class SubscribeToCharacteristicRequest
    {
        public string DeviceUuid { get; set; }
        public string ServiceUuid { get; set; }
        public string CharacteristicUuid { get; set; }
    }
}