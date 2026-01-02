namespace Models.Interfaces;
public interface IUser
{
    public Guid UserId { get; set; }

    public string UserName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }

    public string UserRole { get; set; }
}

// ----------------------------
// GoodFriends domain (WebAPI)
// ----------------------------

public enum AnimalKind { Dog, Cat, Bird, Fish, Other }
public enum AnimalMood { Happy, Sad, Angry, Excited, Calm, Other }

public interface IAddress
{
    public Guid AddressId { get; set; }
    public string StreetAddress { get; set; }
    public int ZipCode { get; set; }
    public string City { get; set; }
    public string Country { get; set; }

    public List<IFriend> Friends { get; set; }
}

public interface IPet
{
    public Guid PetId { get; set; }
    public AnimalKind Kind { get; set; }
    public AnimalMood Mood { get; set; }
    public string Name { get; set; }

    public IFriend Friend { get; set; }
}

public interface IQuote
{
    public Guid QuoteId { get; set; }
    public string QuoteText { get; set; }
    public string Author { get; set; }

    public List<IFriend> Friends { get; set; }
}

public interface IFriend
{
    public Guid FriendId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime? Birthday { get; set; }

    public IAddress Address { get; set; }
    public List<IPet> Pets { get; set; }
    public List<IQuote> Quotes { get; set; }
}

