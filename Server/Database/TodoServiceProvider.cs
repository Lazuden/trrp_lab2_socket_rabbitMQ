using Server.Database.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace Server.Database
{
    public class TodoServiceProvider
    {
        private const string InsertUserCommand = @"INSERT INTO users (email, password) VALUES (@email, @password)";
        private const string SelectUserPasswordCommand = @"SELECT id, password FROM users WHERE email = @email";
        private const string InsertTodoCommand = @"INSERT INTO todos (title, description, user_id) VALUES ";
        private readonly string _connectionString;

        public TodoServiceProvider(string path)
        {
            _connectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = path
            }.ConnectionString;
        }

        public long? AddUser(User user)
        {
            var result = GetIdAndPassword(user);
            if (result.HasValue)
            {
                var id = result.Value.Item1;
                var password = result.Value.Item2;
                if (password == user.Password)
                {
                    return id;
                }
                else
                {
                    return null;
                }
            }

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(InsertUserCommand, conn))
                {
                    cmd.Parameters.AddWithValue("@email", user.Email);
                    cmd.Parameters.AddWithValue("@password", user.Password);
                    cmd.ExecuteNonQuery();
                    return conn.LastInsertRowId;
                }
            }
        }

        public void AddTodos(List<Todo> todos, long? userId)
        {
            if (todos.Count == 0)
            {
                return;
            }
            if (todos is null)
            {
                return;
            }
            if (userId is null)
            {
                return;
            }

            using (var conn = new SQLiteConnection(_connectionString))
            {
                var sb = new string[todos.Count];
                for (int i = 0; i < todos.Count; i++)
                {
                    sb[i] = $"(@title{i}, @description{i}, @user_id{i})";
                }
                var parameters = string.Join(", ", sb);

                conn.Open();
                using (var cmd = new SQLiteCommand(InsertTodoCommand + parameters, conn))
                {
                    for (int i = 0; i < todos.Count; i++)
                    {
                        cmd.Parameters.AddWithValue($"@title{i}", todos[i].Title);
                        cmd.Parameters.AddWithValue($"@description{i}", todos[i].Description);
                        cmd.Parameters.AddWithValue($"@user_id{i}", userId);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public (long, string)? GetIdAndPassword(User user)
        {
            long id = -1;
            string password = "";
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(SelectUserPasswordCommand, conn))
                {
                    cmd.Parameters.AddWithValue("@email", user.Email);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        id = (long)reader["id"];
                        password = (string)reader["password"];
                    }
                }
                conn.Close();
            }
            if (id == -1) return null;
            return (id, password);
        }
    }
}
