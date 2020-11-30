using Client.Core.Clients;
using Communication.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Client
{
    public class Program
    {
        const string DBPath = @"E:\10 триместр\Технологии разработки распределенных приложений\ЛР 2\Lab2\Client\todo_service_one_table.sqlite";

        public static void Main()
        {
            var todoService = new TodoServiceProvider(DBPath);

            //Socket(todoService);
            RabbitMQ(todoService);

            Console.WriteLine("Для завершения нажмите любую кнопку");
            Console.ReadKey();
        }

        private static void Socket(TodoServiceProvider todoService)
        {
            //var ip = new IPAddress(new byte[] { 192, 168, 1, 43 });
            var ip = Dns.GetHostEntry("localhost").AddressList[0];
            var port = 11000;
            var socketClient = new SocketClient(ip, port);

            Console.WriteLine("----------------------Сокеты----------------------");
            int slicesCount = 0;
            foreach (List<TodoService> data in todoService.GetAll(2))
            {
                var jsonData = JsonConvert.SerializeObject(data);

                socketClient.Send(jsonData);

                Console.WriteLine($"-----Данные отправлены ({slicesCount++}):");
                Console.WriteLine(jsonData);
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
        }

        private static void RabbitMQ(TodoServiceProvider todoService)
        {
            Console.WriteLine("---------------------RabbitMQ---------------------");
            var producer = new Producer("localhost"); //"192.168.1.43"
            int slicesCount = 0;

            foreach (List<TodoService> data in todoService.GetAll(2))
            {
                Console.WriteLine($"----------Отправляем данные ({slicesCount++})----------");
                for (int i = 0; i < data.Count; i++)
                {
                    var json = JsonConvert.SerializeObject(new List<TodoService> { data[i] });
                    Console.WriteLine($"-----Отправка {i + 1} из {data.Count}...");
                    Console.WriteLine(json);
                    producer.Send(json);
                }
            }

            Console.WriteLine();
        }

    }
}
