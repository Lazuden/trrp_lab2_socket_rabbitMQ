using Communication.Common;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Client.Core.Clients
{
    public class SocketClient
    {
        private readonly IPEndPoint _remoteEP;
        private readonly IPAddress _ip;
        public SocketClient(IPAddress ip, int port)
        {
            _ip = ip ?? throw new ArgumentNullException(nameof(ip));
            _remoteEP = new IPEndPoint(ip, port);
        }

        public void Send(string data)
        {
            try
            {
                if (data.Length != 0)
                {
                    Socket socket = new Socket(_ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(_remoteEP);

                    var des = GetDes(socket);

                    socket.Send(Cryptographer.SymmetricEncrypt(data, des));
                    //socket.Send(Encoding.UTF8.GetBytes(data));

                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    socket.Dispose();
                }
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
        }

        private DESCryptoServiceProvider GetDes(Socket socket)
        {
            byte[] buffer = new byte[1024];

            // Принимаем RSA public
            int sizeRSA = socket.Receive(buffer);
            var publicKeyJsonByte = new byte[sizeRSA];
            Array.Copy(buffer, 0, publicKeyJsonByte, 0, sizeRSA);

            var publicKeyJson = Encoding.UTF8.GetString(publicKeyJsonByte);
            var publicKey = JsonConvert.DeserializeObject<RsaPublicKeyParameters>(publicKeyJson);
            var publicKeyParameters = publicKey.GetRSAParameters();



            DESCryptoServiceProvider des = Cryptographer.GetDES();
            var iv = Cryptographer.RSAEncrypt(des.IV, publicKeyParameters);
            var key = Cryptographer.RSAEncrypt(des.Key, publicKeyParameters);


            var encrypdedDes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new DesParameters(iv, key)));
            socket.Send(encrypdedDes);


            return des;
        }
    }
}
