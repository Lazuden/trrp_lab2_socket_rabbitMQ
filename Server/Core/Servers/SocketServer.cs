using Communication.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Core.Servers
{
    public class SocketServer : IServer
    {
        private readonly Socket _tcpSocket;
        private readonly List<Task> _tasks;

        public SocketServer(IPAddress ip, int port)
        {
            _tcpSocket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _tcpSocket.Bind(new IPEndPoint(ip, port));
            _tasks = new List<Task>();
        }

        public void Run(IMessageHandler handler, CancellationToken cancellationToken)
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _tcpSocket.Listen(5);

            var cancelWaitTask = Task.Run(() =>
            {
                using var resetEvent = new ManualResetEvent(false);
                cancellationToken.Register(() => resetEvent.Set());
                resetEvent.WaitOne();
            });

            while (!cancellationToken.IsCancellationRequested)
            {
                var socketAcceptTask = _tcpSocket.AcceptAsync();
                Task.WaitAny(socketAcceptTask, cancelWaitTask);

                if (cancelWaitTask.IsCompleted)
                {
                    break;
                }

                var task = Task.Run(() => ReceiveMessage(handler, socketAcceptTask.Result));

                _tasks.Add(task);
            }

            Task.WaitAll(_tasks.ToArray());
        }

        private void ReceiveMessage(IMessageHandler handler, Socket socket)
        {
            try
            {
                var des = GetDes(socket);
                var buffer = new byte[1024];

                var dataJ = new List<byte[]>();

                do
                {
                    int size = socket.Receive(buffer);
                    var bytes = new byte[size];
                    Array.Copy(buffer, 0, bytes, 0, size);
                    dataJ.Add(bytes);
                }
                while (socket.Available > 0);

                var dataSize = dataJ.Sum(x => x.Length);
                var array = new byte[dataSize];
                int index = 0;
                for (int i = 0; i < dataJ.Count; i++)
                {
                    for (int j = 0; j < dataJ[i].Length; j++)
                    {
                        array[index] = dataJ[i][j];
                        index++;
                    }
                }
                var data = Cryptographer.SymmetricDecrypt(array, des);
                handler.Handle(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket.Dispose();
            }
        }

        private DESCryptoServiceProvider GetDes(Socket socket)
        {
            var rsa = Cryptographer.GetRSA();

            var publicKey = rsa.ExportParameters(false);
            var privateKey = rsa.ExportParameters(true);

            var rsaBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new RsaPublicKeyParameters(publicKey)));
            socket.Send(rsaBytes);

            var buffer = new byte[1024];
            int sizeDES = socket.Receive(buffer);
            var encryptedDES = new byte[sizeDES];
            Array.Copy(buffer, 0, encryptedDES, 0, sizeDES);
            var encryptedDESstr = Encoding.UTF8.GetString(encryptedDES);

            var encrypdedDesParams = JsonConvert.DeserializeObject<DesParameters>(encryptedDESstr);

            var desIV = Cryptographer.RSADecrypt(encrypdedDesParams.IV, privateKey);
            var desKey = Cryptographer.RSADecrypt(encrypdedDesParams.Key, privateKey);

            DESCryptoServiceProvider des = Cryptographer.GetDES(desIV, desKey);


            return des;
        }
    }
}
