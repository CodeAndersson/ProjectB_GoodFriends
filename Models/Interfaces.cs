namespace Models.Interfaces;

public interface IAlbum
{
    public Guid AlbumId { get; set; }

    public string Name { get; set; }
    public int ReleaseYear { get; set; }
    public long CopiesSold { get; set; }

    public IMusicGroup MusicGroup { get; set;} 
}

public interface IArtist
{
    public Guid ArtistId { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }

    public DateTime? BirthDay { get; set; }

    public List<IMusicGroup> MusicGroups { get; set; }
}

public enum MusicGenre {Rock, Blues, Jazz, Metal}
public interface IMusicGroup
{
    public Guid MusicGroupId { get; set; }
    public string Name { get; set; }
    public int EstablishedYear { get; set; }

    public MusicGenre Genre { get; set; }

    public List<IAlbum> Albums { get; set; }
    public List<IArtist> Artists { get; set; }
}

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
