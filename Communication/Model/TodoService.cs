namespace Communication.Model
{
    public class TodoService
    {
        public TodoService(string title, string description, string email, string password)
        {
            Title = title ?? throw new System.ArgumentNullException(nameof(title));
            Description = description ?? throw new System.ArgumentNullException(nameof(description));
            Email = email ?? throw new System.ArgumentNullException(nameof(email));
            Password = password ?? throw new System.ArgumentNullException(nameof(password));
        }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }
}
