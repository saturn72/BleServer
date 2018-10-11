namespace BleServer.Common.Models
{
    public sealed class CommandResponse
    {
        public CommandResponse(CommandRequest commandRequest)
        {
            CommandRequest = commandRequest;

        }
        public CommandRequest CommandRequest { get; }
        
        public object Result { get; set; }
    }
}
