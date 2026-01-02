#nullable enable

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;
using Models.DTO;
using Models.Interfaces;
using Seido.Utilities.SeedGenerator;
using Services.InMemory;

namespace Services;

public class AdminServiceLocal : IAdminService
{
    private readonly InMemoryFriendsStore _store;
    private readonly ILogger<AdminServiceLocal> _logger;
    private readonly IHostEnvironment _hostEnvironment;

    public AdminServiceLocal(InMemoryFriendsStore store, IHostEnvironment hostEnvironment, ILogger<AdminServiceLocal> logger)
    {
        _store = store;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    public Task<ResponseItemDto<GstUsrInfoAllDto>> GuestInfoAsync()
    {
        var (sf, uf, sa, ua, sp, up, sq, uq) = _store.GetCounts();

        var allFriends = _store.ReadFriends(seeded: true, flat: false, filter: string.Empty)
            .Concat(_store.ReadFriends(seeded: false, flat: false, filter: string.Empty))
            .OfType<Friend>()
            .ToList();

        var friendsByCountryCity = allFriends
            .Where(f => f.Address is Address)
            .GroupBy(f => new
            {
                Country = ((Address)f.Address).Country ?? string.Empty,
                City = ((Address)f.Address).City ?? string.Empty
            })
            .Where(g => !string.IsNullOrWhiteSpace(g.Key.Country))
            .Select(g => new GstUsrInfoFriendsDto
            {
                Country = g.Key.Country,
                City = g.Key.City,
                NrFriends = g.Count()
            })
            .OrderBy(x => x.Country)
            .ThenBy(x => x.City)
            .ToList();

        var petsByCountryCity = allFriends
            .Where(f => f.Address is Address)
            .SelectMany(f => f.Pets.OfType<Pet>().Select(p => new { Friend = f, Pet = p }))
            .GroupBy(x => new
            {
                Country = (((Address)x.Friend.Address).Country ?? string.Empty),
                City = (((Address)x.Friend.Address).City ?? string.Empty)
            })
            .Where(g => !string.IsNullOrWhiteSpace(g.Key.Country))
            .Select(g => new GstUsrInfoPetsDto
            {
                Country = g.Key.Country,
                City = g.Key.City,
                NrPets = g.Count()
            })
            .OrderBy(x => x.Country)
            .ThenBy(x => x.City)
            .ToList();

        var quotesByAuthor = allFriends
            .SelectMany(f => f.Quotes.OfType<Quote>())
            .GroupBy(q => q.Author ?? string.Empty)
            .Where(g => !string.IsNullOrWhiteSpace(g.Key))
            .Select(g => new GstUsrInfoQuotesDto
            {
                Author = g.Key,
                NrQuotes = g.Count()
            })
            .OrderBy(x => x.Author)
            .ToList();

        var friendsWithAddress = allFriends.Count(f => f.Address is Address);

        var info = new GstUsrInfoAllDto
        {
            Db = new GstUsrInfoDbDto
            {
                NrSeededFriends = sf,
                NrUnseededFriends = uf,
                NrSeededAddresses = sa,
                NrUnseededAddresses = ua,
                NrSeededPets = sp,
                NrUnseededPets = up,
                NrSeededQuotes = sq,
                NrUnseededQuotes = uq,
                NrFriendsWithAddress = friendsWithAddress
            },
            Friends = friendsByCountryCity,
            Pets = petsByCountryCity,
            Quotes = quotesByAuthor
        };

        return Task.FromResult(new ResponseItemDto<GstUsrInfoAllDto> { Item = info });
    }

    public async Task<ResponseItemDto<GstUsrInfoAllDto>> SeedAsync(int nrOfItems)
    {
        var seedFile = Path.Combine(_hostEnvironment.ContentRootPath, "app-seeds.json");
        SeedGenerator? seedGenerator = null;

        try
        {
            if (File.Exists(seedFile))
                seedGenerator = new SeedGenerator(Path.GetFullPath(seedFile));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load seed file {SeedFile}; falling back to random generator", seedFile);
        }

        seedGenerator ??= new SeedGenerator();

        var items = new List<(Friend Friend, Address? Address, List<Pet> Pets, List<Quote> Quotes)>();
        for (int i = 0; i < nrOfItems; i++)
        {
            var friend = new Friend
            {
                FriendId = Guid.NewGuid(),
                FirstName = seedGenerator.FirstName,
                LastName = seedGenerator.LastName,
                Email = seedGenerator.Email(),
                Birthday = seedGenerator.DateAndTime(1960, 2010)
            };

            Address? address = null;
            if (seedGenerator.Next(0, 10) < 8)
            {
                var country = seedGenerator.Country;
                address = new Address
                {
                    AddressId = Guid.NewGuid(),
                    Country = country,
                    City = seedGenerator.City(country),
                    StreetAddress = seedGenerator.StreetAddress(country),
                    ZipCode = seedGenerator.ZipCode
                };
            }

            var pets = new List<Pet>();
            var petCount = seedGenerator.Next(0, 3);
            for (int p = 0; p < petCount; p++)
            {
                pets.Add(new Pet
                {
                    PetId = Guid.NewGuid(),
                    Name = seedGenerator.PetName,
                    Kind = seedGenerator.FromEnum<AnimalKind>(),
                    Mood = seedGenerator.FromEnum<AnimalMood>(),
                    Friend = null
                });
            }

            var quotes = new List<Quote>();
            if (seedGenerator.Next(0, 10) < 5)
            {
                var q = seedGenerator.Quote;
                quotes.Add(new Quote
                {
                    QuoteId = Guid.NewGuid(),
                    QuoteText = q?.Quote ?? seedGenerator.LatinSentence,
                    Author = q?.Author ?? "Unknown"
                });
            }

            items.Add((friend, address, pets, quotes));
        }

        _store.Seed(items);
        return await GuestInfoAsync();
    }

    public async Task<ResponseItemDto<GstUsrInfoAllDto>> RemoveSeedAsync(bool seeded)
    {
        _store.Clear(seeded);
        return await GuestInfoAsync();
    }

    public Task<ResponseItemDto<UsrInfoDto>> SeedUsersAsync(int nrOfUsers, int nrOfSuperUsers, int nrOfSysAdmin)
        => throw new NotImplementedException();
}
