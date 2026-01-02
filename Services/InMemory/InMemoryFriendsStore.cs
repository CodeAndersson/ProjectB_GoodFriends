#nullable enable

using Models;
using Models.DTO;
using Models.Interfaces;

namespace Services.InMemory;

public class InMemoryFriendsStore
{
    private readonly object _lock = new();

    private readonly Dictionary<Guid, (Friend Friend, bool Seeded)> _friends = new();
    private readonly Dictionary<Guid, (Address Address, bool Seeded)> _addresses = new();
    private readonly Dictionary<Guid, (Pet Pet, bool Seeded)> _pets = new();
    private readonly Dictionary<Guid, (Quote Quote, bool Seeded)> _quotes = new();

    public void Clear(bool seeded)
    {
        lock (_lock)
        {
            foreach (var id in _friends.Where(kvp => kvp.Value.Seeded == seeded).Select(kvp => kvp.Key).ToList())
                _friends.Remove(id);

            foreach (var id in _addresses.Where(kvp => kvp.Value.Seeded == seeded).Select(kvp => kvp.Key).ToList())
                _addresses.Remove(id);

            foreach (var id in _pets.Where(kvp => kvp.Value.Seeded == seeded).Select(kvp => kvp.Key).ToList())
                _pets.Remove(id);

            foreach (var id in _quotes.Where(kvp => kvp.Value.Seeded == seeded).Select(kvp => kvp.Key).ToList())
                _quotes.Remove(id);

            RebuildReverseLinks();
        }
    }

    public void Seed(IEnumerable<(Friend Friend, Address? Address, List<Pet> Pets, List<Quote> Quotes)> items)
    {
        lock (_lock)
        {
            foreach (var (friend, address, pets, quotes) in items)
            {
                if (friend.FriendId == Guid.Empty)
                    friend.FriendId = Guid.NewGuid();

                if (address is not null)
                {
                    if (address.AddressId == Guid.Empty)
                        address.AddressId = Guid.NewGuid();

                    _addresses[address.AddressId] = (address, true);
                    friend.Address = address;
                }

                foreach (var pet in pets ?? new List<Pet>())
                {
                    if (pet.PetId == Guid.Empty)
                        pet.PetId = Guid.NewGuid();

                    pet.Friend = friend;
                    _pets[pet.PetId] = (pet, true);
                    friend.Pets.Add(pet);
                }

                foreach (var quote in quotes ?? new List<Quote>())
                {
                    if (quote.QuoteId == Guid.Empty)
                        quote.QuoteId = Guid.NewGuid();

                    quote.Friends.Add(friend);
                    _quotes[quote.QuoteId] = (quote, true);
                    friend.Quotes.Add(quote);
                }

                _friends[friend.FriendId] = (friend, true);
            }

            RebuildReverseLinks();
        }
    }

    public (int SeededFriends, int UnseededFriends,
        int SeededAddresses, int UnseededAddresses,
        int SeededPets, int UnseededPets,
        int SeededQuotes, int UnseededQuotes) GetCounts()
    {
        lock (_lock)
        {
            return (
                _friends.Count(kvp => kvp.Value.Seeded),
                _friends.Count(kvp => !kvp.Value.Seeded),
                _addresses.Count(kvp => kvp.Value.Seeded),
                _addresses.Count(kvp => !kvp.Value.Seeded),
                _pets.Count(kvp => kvp.Value.Seeded),
                _pets.Count(kvp => !kvp.Value.Seeded),
                _quotes.Count(kvp => kvp.Value.Seeded),
                _quotes.Count(kvp => !kvp.Value.Seeded)
            );
        }
    }

    public List<IFriend> ReadFriends(bool seeded, bool flat, string filter)
    {
        lock (_lock)
        {
            IEnumerable<(Friend Friend, bool Seeded)> query = _friends.Values;

            query = query.Where(x => x.Seeded == seeded);

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var tokens = filter
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(t => t.Trim())
                    .Where(t => t.Length > 0)
                    .ToArray();

                if (tokens.Length > 0)
                {
                    query = query.Where(x => MatchesAllTokens(x.Friend, tokens));
                }
            }

            var items = query
                .Select(x => CloneFriend(x.Friend, flat))
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .ToList<IFriend>();

            return items;
        }
    }

    private static bool MatchesAllTokens(Friend friend, string[] tokens)
    {
        foreach (var token in tokens)
        {
            if (!MatchesToken(friend, token))
                return false;
        }

        return true;
    }

    private static bool MatchesToken(Friend friend, string token)
    {
        static bool Contains(string? value, string token)
            => !string.IsNullOrWhiteSpace(value) && value.Contains(token, StringComparison.OrdinalIgnoreCase);

        if (Contains(friend.FirstName, token)) return true;
        if (Contains(friend.LastName, token)) return true;
        if (Contains(friend.Email, token)) return true;

        if (friend.Address is Address a)
        {
            if (Contains(a.StreetAddress, token)) return true;
            if (a.ZipCode.ToString().Contains(token, StringComparison.OrdinalIgnoreCase)) return true;
            if (Contains(a.City, token)) return true;
            if (Contains(a.Country, token)) return true;
        }

        foreach (var pet in friend.Pets.OfType<Pet>())
        {
            if (Contains(pet.Name, token)) return true;
            if (pet.Kind.ToString().Contains(token, StringComparison.OrdinalIgnoreCase)) return true;
            if (pet.Mood.ToString().Contains(token, StringComparison.OrdinalIgnoreCase)) return true;
        }

        foreach (var quote in friend.Quotes.OfType<Quote>())
        {
            if (Contains(quote.Author, token)) return true;
            if (Contains(quote.QuoteText, token)) return true;
        }

        return false;
    }

    public IFriend? ReadFriend(Guid id, bool flat)
    {
        lock (_lock)
        {
            return _friends.TryGetValue(id, out var entry)
                ? CloneFriend(entry.Friend, flat)
                : null;
        }
    }

    public FriendCuDto? ReadFriendCuDto(Guid id)
    {
        lock (_lock)
        {
            if (!_friends.TryGetValue(id, out var entry)) return null;

            var f = entry.Friend;
            return new FriendCuDto
            {
                FriendId = f.FriendId,
                FirstName = f.FirstName,
                LastName = f.LastName,
                Email = f.Email,
                Birthday = f.Birthday,
                AddressId = f.Address?.AddressId,
                PetsId = f.Pets?.Select(p => p.PetId).ToList() ?? new List<Guid>(),
                QuotesId = f.Quotes?.Select(q => q.QuoteId).ToList() ?? new List<Guid>()
            };
        }
    }

    public IFriend? UpdateFriend(Guid id, FriendCuDto dto)
    {
        lock (_lock)
        {
            if (!_friends.TryGetValue(id, out var entry)) return null;

            var f = entry.Friend;
            f.FirstName = dto.FirstName;
            f.LastName = dto.LastName;
            f.Email = dto.Email;
            f.Birthday = dto.Birthday;

            if (dto.AddressId.HasValue && dto.AddressId.Value != Guid.Empty)
            {
                if (_addresses.TryGetValue(dto.AddressId.Value, out var adrEntry))
                {
                    f.Address = adrEntry.Address;
                }
            }

            _friends[id] = (f, entry.Seeded);
            RebuildReverseLinks();
            return CloneFriend(f, flat: false);
        }
    }

    public IFriend? DeleteFriend(Guid id)
    {
        lock (_lock)
        {
            if (!_friends.TryGetValue(id, out var entry)) return null;

            var deleted = entry.Friend;
            _friends.Remove(id);

            foreach (var petId in deleted.Pets.Select(p => p.PetId).ToList())
                _pets.Remove(petId);

            foreach (var quoteId in deleted.Quotes.Select(q => q.QuoteId).ToList())
            {
                if (_quotes.TryGetValue(quoteId, out var qEntry))
                {
                    qEntry.Quote.Friends.RemoveAll(fr => fr.FriendId == id);
                    if (qEntry.Quote.Friends.Count == 0)
                        _quotes.Remove(quoteId);
                }
            }

            // Remove address if no one references it anymore
            if (deleted.Address?.AddressId is Guid addressId)
            {
                var stillUsed = _friends.Values.Any(fr => (fr.Friend.Address?.AddressId ?? Guid.Empty) == addressId);
                if (!stillUsed)
                    _addresses.Remove(addressId);
            }

            RebuildReverseLinks();
            return CloneFriend(deleted, flat: false);
        }
    }

    public IAddress? ReadAddress(Guid id)
    {
        lock (_lock)
        {
            return _addresses.TryGetValue(id, out var entry)
                ? CloneAddress(entry.Address)
                : null;
        }
    }

    public AddressCuDto? ReadAddressCuDto(Guid id)
    {
        lock (_lock)
        {
            if (!_addresses.TryGetValue(id, out var entry)) return null;

            var a = entry.Address;
            return new AddressCuDto
            {
                AddressId = a.AddressId,
                StreetAddress = a.StreetAddress,
                ZipCode = a.ZipCode,
                City = a.City,
                Country = a.Country,
                FriendsId = a.Friends?.Select(f => f.FriendId).ToList() ?? new List<Guid>()
            };
        }
    }

    public IAddress? UpdateAddress(Guid id, AddressCuDto dto)
    {
        lock (_lock)
        {
            if (!_addresses.TryGetValue(id, out var entry)) return null;

            var a = entry.Address;
            a.StreetAddress = dto.StreetAddress;
            a.ZipCode = dto.ZipCode;
            a.City = dto.City;
            a.Country = dto.Country;

            _addresses[id] = (a, entry.Seeded);
            RebuildReverseLinks();
            return CloneAddress(a);
        }
    }

    public IPet? DeletePet(Guid id)
    {
        lock (_lock)
        {
            if (!_pets.TryGetValue(id, out var entry)) return null;

            _pets.Remove(id);
            foreach (var fr in _friends.Values.Select(v => v.Friend))
            {
                fr.Pets.RemoveAll(p => p.PetId == id);
            }

            RebuildReverseLinks();
            return entry.Pet;
        }
    }

    public PetCuDto? ReadPetCuDto(Guid id)
    {
        lock (_lock)
        {
            if (!_pets.TryGetValue(id, out var entry)) return null;

            var p = entry.Pet;
            return new PetCuDto
            {
                PetId = p.PetId,
                FriendId = (p.Friend as Friend)?.FriendId ?? Guid.Empty,
                Kind = p.Kind,
                Mood = p.Mood,
                Name = p.Name
            };
        }
    }

    public IPet? CreatePet(PetCuDto dto)
    {
        lock (_lock)
        {
            if (dto.FriendId == Guid.Empty) return null;
            if (!_friends.TryGetValue(dto.FriendId, out var friendEntry)) return null;

            var friend = friendEntry.Friend;
            var petId = (dto.PetId.HasValue && dto.PetId.Value != Guid.Empty) ? dto.PetId.Value : Guid.NewGuid();

            var pet = new Pet
            {
                PetId = petId,
                Name = dto.Name,
                Kind = dto.Kind,
                Mood = dto.Mood,
                Friend = friend
            };

            _pets[petId] = (pet, friendEntry.Seeded);
            friend.Pets.RemoveAll(p => p.PetId == petId);
            friend.Pets.Add(pet);

            _friends[friend.FriendId] = (friend, friendEntry.Seeded);
            RebuildReverseLinks();
            return pet;
        }
    }

    public IPet? UpdatePet(Guid id, PetCuDto dto)
    {
        lock (_lock)
        {
            if (!_pets.TryGetValue(id, out var entry)) return null;

            var pet = entry.Pet;

            // Move pet between friends if requested
            var currentFriendId = (pet.Friend as Friend)?.FriendId ?? Guid.Empty;
            if (dto.FriendId != Guid.Empty && dto.FriendId != currentFriendId)
            {
                if (_friends.TryGetValue(dto.FriendId, out var newFriendEntry))
                {
                    foreach (var fr in _friends.Values.Select(v => v.Friend))
                        fr.Pets.RemoveAll(p => p.PetId == id);

                    pet.Friend = newFriendEntry.Friend;
                    newFriendEntry.Friend.Pets.Add(pet);
                }
            }

            pet.Name = dto.Name;
            pet.Kind = dto.Kind;
            pet.Mood = dto.Mood;

            _pets[id] = (pet, entry.Seeded);
            RebuildReverseLinks();
            return pet;
        }
    }

    public IQuote? DeleteQuote(Guid id)
    {
        lock (_lock)
        {
            if (!_quotes.TryGetValue(id, out var entry)) return null;

            _quotes.Remove(id);
            foreach (var fr in _friends.Values.Select(v => v.Friend))
            {
                fr.Quotes.RemoveAll(q => q.QuoteId == id);
            }

            RebuildReverseLinks();
            return entry.Quote;
        }
    }

    public QuoteCuDto? ReadQuoteCuDto(Guid id)
    {
        lock (_lock)
        {
            if (!_quotes.TryGetValue(id, out var entry)) return null;

            var q = entry.Quote;
            return new QuoteCuDto
            {
                QuoteId = q.QuoteId,
                Quote = q.QuoteText,
                Author = q.Author,
                FriendsId = q.Friends?.Select(f => f.FriendId).ToList() ?? new List<Guid>()
            };
        }
    }

    public IQuote? CreateQuote(QuoteCuDto dto)
    {
        lock (_lock)
        {
            var friendIds = (dto.FriendsId ?? new List<Guid>())
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (friendIds.Count == 0) return null;

            var quoteId = (dto.QuoteId.HasValue && dto.QuoteId.Value != Guid.Empty) ? dto.QuoteId.Value : Guid.NewGuid();

            var quote = new Quote
            {
                QuoteId = quoteId,
                QuoteText = dto.Quote,
                Author = dto.Author,
                Friends = new List<IFriend>()
            };

            // seeded flag follows the first friend found
            var seeded = false;
            foreach (var friendId in friendIds)
            {
                if (_friends.TryGetValue(friendId, out var frEntry))
                {
                    seeded = frEntry.Seeded;
                    break;
                }
            }

            _quotes[quoteId] = (quote, seeded);

            foreach (var friendId in friendIds)
            {
                if (_friends.TryGetValue(friendId, out var frEntry))
                {
                    frEntry.Friend.Quotes.RemoveAll(q => q.QuoteId == quoteId);
                    frEntry.Friend.Quotes.Add(quote);
                }
            }

            RebuildReverseLinks();
            return quote;
        }
    }

    public IQuote? UpdateQuote(Guid id, QuoteCuDto dto)
    {
        lock (_lock)
        {
            if (!_quotes.TryGetValue(id, out var entry)) return null;

            var quote = entry.Quote;
            quote.QuoteText = dto.Quote;
            quote.Author = dto.Author;

            if (dto.FriendsId is not null)
            {
                var friendIds = dto.FriendsId.Where(x => x != Guid.Empty).Distinct().ToHashSet();

                foreach (var fr in _friends.Values.Select(v => v.Friend))
                {
                    if (friendIds.Contains(fr.FriendId))
                    {
                        if (!fr.Quotes.Any(q => q.QuoteId == id))
                            fr.Quotes.Add(quote);
                    }
                    else
                    {
                        fr.Quotes.RemoveAll(q => q.QuoteId == id);
                    }
                }
            }

            _quotes[id] = (quote, entry.Seeded);
            RebuildReverseLinks();
            return quote;
        }
    }

    private void RebuildReverseLinks()
    {
        foreach (var adr in _addresses.Values.Select(v => v.Address))
            adr.Friends = new List<IFriend>();

        foreach (var q in _quotes.Values.Select(v => v.Quote))
            q.Friends = new List<IFriend>();

        foreach (var f in _friends.Values.Select(v => v.Friend))
        {
            if (f.Address is Address adr)
            {
                adr.Friends.Add(f);
            }

            foreach (var pet in f.Pets.OfType<Pet>())
                pet.Friend = f;

            foreach (var quote in f.Quotes.OfType<Quote>())
            {
                if (!quote.Friends.Any(fr => fr.FriendId == f.FriendId))
                    quote.Friends.Add(f);
            }
        }
    }

    private static Friend CloneFriend(Friend f, bool flat)
    {
        var clone = new Friend
        {
            FriendId = f.FriendId,
            FirstName = f.FirstName,
            LastName = f.LastName,
            Email = f.Email,
            Birthday = f.Birthday,
            Address = f.Address is Address adr ? CloneAddress(adr) : null
        };

        if (!flat)
        {
            foreach (var p in f.Pets.OfType<Pet>())
            {
                clone.Pets.Add(new Pet
                {
                    PetId = p.PetId,
                    Name = p.Name,
                    Kind = p.Kind,
                    Mood = p.Mood,
                    Friend = null
                });
            }

            foreach (var q in f.Quotes.OfType<Quote>())
            {
                clone.Quotes.Add(new Quote
                {
                    QuoteId = q.QuoteId,
                    QuoteText = q.QuoteText,
                    Author = q.Author,
                    Friends = new List<IFriend>()
                });
            }
        }

        return clone;
    }

    private static Address CloneAddress(Address a)
        => new()
        {
            AddressId = a.AddressId,
            StreetAddress = a.StreetAddress,
            ZipCode = a.ZipCode,
            City = a.City,
            Country = a.Country,
            Friends = new List<IFriend>()
        };
}
