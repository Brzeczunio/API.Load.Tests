using API.Load.Tests.Models.Requests;
using Bogus;

namespace API.Load.Tests.Data
{
    internal static class FakeUsersGenerator
    {
        public static IEnumerable<CreateUserRequest> GenerateFakeUsers(int count)
        {
            var guid = Guid.NewGuid().ToString();

            var addressFaker = new Faker<Address>()
                .RuleFor(u => u.City, f => f.Person.Address.City)
                .RuleFor(u => u.Street, f => f.Person.Address.Street);

            var userFaker = new Faker<CreateUserRequest>()
                .RuleFor(u => u.Name, f => f.Person.FirstName + guid)
                .RuleFor(u => u.Surname, f => f.Person.LastName)
                .RuleFor(u => u.Email, f => f.Person.Email)
                .RuleFor(u => u.UserName, f => f.Person.UserName + guid)
                .RuleFor(u => u.Password, f => guid)
                .RuleFor(u => u.Address, f => addressFaker);

            return userFaker.GenerateLazy(count);
        }
    }
}
