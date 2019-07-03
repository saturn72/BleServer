namespace ConnectivityServer.Common.Services.Nfc
{
    public interface INfcAdapter
    {
        event NfcDeviceEventHandler DeviceArrived;
        event NfcDeviceEventHandler DeviceDeparted;
    }
    public delegate void NfcDeviceEventHandler(INfcAdapter sender, NfcDeviceEventArgs args);

}
