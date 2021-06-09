using Communication.Model;
using System.Collections.Generic;
using System.Collections;
using System.Data.SQLite;

namespace Client
{
    public class TodoServiceProvider
    {
        private readonly string _connectionString;

        public TodoServiceProvider(string path)
        {
            _connectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = path,
            }.ConnectionString;
        }

        public IEnumerable<List<TodoService>> GetAll(int sliceSize)
        {
            using var conn = new SQLiteConnection(_connectionString);
            var sCommand = new SQLiteCommand()
            {
                Connection = conn,
                CommandText = @"SELECT title, description, email, password FROM todo_service;"
            };

            conn.Open();
            var reader = sCommand.ExecuteReader();

            var result = new List<TodoService>();

            while (reader.Read())
            {
                string title = (string)reader["title"];
                string description = (string)reader["description"];
                string email = (string)reader["email"];
                string password = (string)reader["password"];
                result.Add(new TodoService(title, description, email, password));

                if (result.Count == sliceSize)
                {
                    yield return result;
                    result.Clear();
                }
            }

            if (result.Count > 0)
            {
                yield return result;
            }
        }
    }
}
