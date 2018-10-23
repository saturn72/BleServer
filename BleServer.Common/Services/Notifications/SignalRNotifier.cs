using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ServiceStack.Text;

namespace BleServer.Common.Services.Notifications
{
    public class SignalRNotifier : INotifier
    {

        private readonly IHubContext<MessageHub> _messaggeHub;

        public SignalRNotifier(IHubContext<MessageHub> messageHub)
        {
            _messaggeHub = messageHub;
        }

        public Task Push(string key, object notification)
        {
            return _messaggeHub.Clients.All.SendAsync(key, JsonSerializer.SerializeToString(notification));
        }
    }

    public class MessageHub : Hub
    {
    }
}