using System.IO;
using System.Threading.Tasks;

namespace Sanatana.Notifications.NDR
{
    public interface INdrHandler
    {
        Task Handle(Stream requestStream);
        Task Handle(string requestMessage);
    }
}