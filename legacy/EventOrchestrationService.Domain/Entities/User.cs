using EventOrchestrationService.Domain.Enums;

namespace EventOrchestrationService.Domain.Entities;

public class User
{
    private User()
    {
    }

    public User(string login, string passwordHash, Role role)
    {
        Login = login;
        PasswordHash = passwordHash;
        Role = role;
    }

    public int Id { get; private set; }
    public string Login { get; private set; }
    public string PasswordHash { get; private set; }
    public Role Role { get; private set; }
    public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();
}