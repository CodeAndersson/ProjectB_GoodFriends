using Models.Interfaces;

namespace Models;

public class Friend : IFriend
{
    public Guid FriendId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime? Birthday { get; set; }

    public IAddress Address { get; set; }
    public List<IPet> Pets { get; set; } = new();
    public List<IQuote> Quotes { get; set; } = new();
}
