using System.Threading;

namespace Server.Core
{
    public interface IServer
    {
        void Run(IMessageHandler handler, CancellationToken cancellationToken);
    }
}
