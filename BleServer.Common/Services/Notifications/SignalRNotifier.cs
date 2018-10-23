using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ServiceStack.Text;

namespace BleServer.Common.Services.Notifications
{
    public class SignalRNotifier : INotifier
    {

        private readonly IHubContext<MessageHub> _messaggeHub;
        private readonly ILogger _logger;

        public SignalRNotifier(IHubContext<MessageHub> messageHub, ILogger<MessageHub> logger)
        {
            _messaggeHub = messageHub;
            _logger = logger;
        }

        public Task Push(string key, object notification)
        {
            var body = JsonSerializer.SerializeToString(notification);
            var notifyTask = _messaggeHub.Clients.All.SendAsync(key, body);
            _logger.LogInformation($"Send notification to clients: key=\'{key}\' body=\'{body}\'");

            return notifyTask;
        }
    }

    public class MessageHub : Hub
    {
    }
}