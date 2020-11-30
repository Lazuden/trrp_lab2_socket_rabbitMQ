using Communication.Model;
using Newtonsoft.Json;
using Server.Core;
using Server.Core.Servers;
using Server.Database;
using Server.Database.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class Program : IMessageHandler
    {
        private const string DBPath = @"E:\10 триместр\Технологии разработки распределенных приложений\ЛР 2\Lab2\Server\todo_service.sqlite";

        private readonly CancellationTokenSource _cancellationTokenSource;
        private object _locker;
        public Program()
        {
            _locker = new object();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        static void Main()
        {
            var program = new Program();
            Console.CancelKeyPress += (sender, e) =>
            {
                try
                {
                    program.Cancel();
                }
                finally
                {
                    e.Cancel = true;
                }
            };
            program.Run();
        }

        public void Run()
        {
            //var ip = new IPAddress(new byte[] { 192, 168, 1, 37 });
            var ip = Dns.GetHostEntry("localhost").AddressList[0];
            var port = 11000;

            var servers = new List<IServer>()
            {
                new SocketServer(ip, port),
                new Consumer("localhost")
            };

            var tasks = servers
                .Select(server => Task.Run(() => server.Run(this, _cancellationTokenSource.Token)))
                .ToArray();

            Task.WaitAll(tasks);
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Handle(string message)
        {
            Console.WriteLine("данные получены:");
            Console.WriteLine(message);

            var todoServices = JsonConvert.DeserializeObject<List<TodoService>>(message);
            var groups = todoServices.GroupBy(item => (item.Email, item.Password));

            var service = new TodoServiceProvider(DBPath);

            foreach (var group in groups)
            {
                var user = new User(group.Key.Email, group.Key.Password);
                var todos = group.Select(x => new Todo(x.Title, x.Description)).ToList();

                long? userId;

                lock (_locker)
                {
                    userId = service.AddUser(user);
                }
                service.AddTodos(todos, userId);
            }

            Console.WriteLine("Данные записаны.");
        }
    }
}
