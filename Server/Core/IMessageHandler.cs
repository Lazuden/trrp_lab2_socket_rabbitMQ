namespace Server.Core
{
    public interface IMessageHandler
    {
        void Handle(string message);
    }
}
