using System.Threading.Tasks;

namespace BleServer.Common.Services.Notifications
{
    public interface INotifier
    {
        Task Push(string key, object notification);
    }

    public class SignalRNotifier : INotifier
    {
        public Task Push(string key, object notification)
        {
            throw new System.NotImplementedException();
        }
    }
}
