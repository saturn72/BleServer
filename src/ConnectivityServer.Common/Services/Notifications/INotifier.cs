using System.Threading.Tasks;

namespace ConnectivityServer.Common.Services.Notifications
{
    public interface INotifier
    {
        Task Push(string key, object notification);
    }
}
