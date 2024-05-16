namespace API.Load.Tests.Models.Requests
{
    public class CreateUserRequest
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public Address? Address { get; set; }
    }
}
