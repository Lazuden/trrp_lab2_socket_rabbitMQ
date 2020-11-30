using Communication.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Security.Cryptography;
using System.Threading;

namespace Server.Core.Servers
{
    public class Consumer : IServer
    {
        private readonly ConnectionFactory _factory;
        private const string FromServer = "from_server";
        private const string ToServer = "to_server";
        private RSACryptoServiceProvider _rsa;
        private DESCryptoServiceProvider _des;

        public Consumer(string hostName)
        {
            _factory = new ConnectionFactory() { HostName = hostName };
            _rsa = Cryptographer.GetRSA();
        }

        public void Run(IMessageHandler handler, CancellationToken cancellationToken)
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            DeclareQueues(channel, FromServer, ToServer);
            /* Две задачи:
            1) отправить publicRSA в FromServer
            2) подписаться на ToServer, и, когда придет des, расшифровать его с помощью privateRSA. Принимать другие сообщения */

            // 1 задача
            var publicRsaBytes = Cryptographer.GetBytesOfPublicRSA(_rsa.ExportParameters(false));
            //Cw("отправляем паблик РСА", publicRsaBytes);
            Send(channel, publicRsaBytes);

            // 2 задача
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();

                if (_des is null)
                {
                    _des = Cryptographer.GetDesFromBytes(body, _rsa.ExportParameters(true));
                }
                else
                {
                    var message = Cryptographer.SymmetricDecrypt(body, _des);
                    handler.Handle(message);
                }
            };

            var consumerTag = channel.BasicConsume(
                queue: ToServer,
                autoAck: true,
                consumer: consumer);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            using var resetEvent = new ManualResetEvent(false);
            cancellationToken.Register(() =>
            {
                channel.BasicCancel(consumerTag);
                resetEvent.Set();
            });

            resetEvent.WaitOne();
        }
        private void DeclareQueues(IModel channel, params string[] queues)
        {
            foreach (var queue in queues)
            {
                channel.QueueDeclare(
                    queue: queue,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
            }
        }

        private void Send(IModel channel, byte[] body)
        {
            channel.BasicPublish(
                exchange: "",
                routingKey: FromServer,
                basicProperties: null,
                body: body);
        }
    }
}
