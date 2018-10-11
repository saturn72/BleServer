using System.Threading.Tasks;

namespace BleServer.Common.Services.Notifications
{
    public interface INotifier
    {
        Task Push(string key, object notification);
    }
}
