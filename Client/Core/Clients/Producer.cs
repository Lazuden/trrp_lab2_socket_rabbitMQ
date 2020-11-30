using Communication.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Client.Core.Clients
{
    public class Producer
    {
        private readonly ConnectionFactory _factory;
        private const string FromServer = "from_server";
        private const string ToServer = "to_server";
        private DESCryptoServiceProvider _des;

        public Producer(string ip)
        {
            _factory = new ConnectionFactory() { HostName = ip, UserName = "Jojo", Password = "Jojo" };
            _des = Cryptographer.GetDES();
        }

        public void Send(string message)
        {
            using var connection = _factory.CreateConnection();
            using IModel channel = connection.CreateModel();
            DeclareQueues(channel, FromServer, ToServer);
            /* Две задачи:
            1) подписаться на FromServer, и, когда придет rsa, зашифровать им des, и отправить результат в очередь ToServer 
            2) зашифровать данные и положить их в очередь ToServer */

            // 1 задача
            //var consumer = new EventingBasicConsumer(channel);
            //consumer.Received += (model, ea) =>
            //{
            //    var rsa = ea.Body.ToArray();
            //    Cw("полученный паблик РСА", rsa);
            //    var publicRSA = Cryptographer.GetRSAFromBytes(rsa);
            //    var encryptedDES = Cryptographer.EncryptDesByRSA(publicRSA, _des);
            //    Send(channel, encryptedDES);
            //};

            //channel.BasicConsume(
            //    queue: FromServer,
            //    autoAck: true,
            //    consumer: consumer);

            // 2 задача
            var body = Encoding.UTF8.GetBytes(message);
            //var body = Cryptographer.SymmetricEncrypt(message, _des);
            Send(channel, body);
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
                routingKey: ToServer,
                basicProperties: null,
                body: body);
        }
    }
}
