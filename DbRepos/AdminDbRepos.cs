using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using Seido.Utilities.SeedGenerator;
using Models.DTO;
using DbModels;
using DbContext;
using Encryption;


namespace DbRepos;

public class AdminDbRepos
{
    private const string _seedSource = "./app-seeds.json";
    private readonly ILogger<AdminDbRepos> _logger;
    private Encryptions _encryptions;
    private readonly MainDbContext _dbContext;

    public AdminDbRepos(ILogger<AdminDbRepos> logger, Encryptions encryptions, MainDbContext context)
    {
        _logger = logger;
        _encryptions = encryptions;
        _dbContext = context;
    }

    public async Task<ResponseItemDto<GstUsrInfoAllDto>> InfoAsync() => await DbInfo();


    private async Task<ResponseItemDto<GstUsrInfoAllDto>> DbInfo()
    {
        var info = new GstUsrInfoAllDto();
        info.Db = await _dbContext.InfoDbView.FirstAsync();
        return new ResponseItemDto<GstUsrInfoAllDto>
        {
#if DEBUG
            ConnectionString = _dbContext.dbConnection,
#endif

            Item = info
        };
    }

    public async Task<ResponseItemDto<GstUsrInfoAllDto>> SeedAsync(int nrOfItems)
    {
        var fn = Path.GetFullPath(_seedSource);
        var seeder = new SeedGenerator(fn);

        if (nrOfItems <= 0)
            return await DbInfo();

        // Addresses: create a pool so multiple friends can share the same address
        var nrAddresses = Math.Max(1, nrOfItems / 2);
        var addresses = new List<AddressDbM>(capacity: nrAddresses);
        var addressKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (addresses.Count < nrAddresses)
        {
            var country = seeder.Country;
            var city = seeder.City(country);
            var street = seeder.StreetAddress(country);
            var zip = seeder.ZipCode;

            var key = $"{street}|{zip}|{city}|{country}";
            if (!addressKeys.Add(key))
                continue;

            addresses.Add(new AddressDbM
            {
                AddressId = Guid.NewGuid(),
                StreetAddress = street,
                ZipCode = zip,
                City = city,
                Country = country,
                Seeded = true
            });
        }

        // Quotes: create a pool so quotes can be shared among friends
        var nrQuotes = Math.Max(1, nrOfItems / 2);
        var quotes = seeder.Quotes(nrQuotes)
            .Select(q => new QuoteDbM
            {
                QuoteId = Guid.NewGuid(),
                QuoteText = q.Quote,
                Author = q.Author,
                Seeded = true
            })
            .ToList();

        var friends = new List<FriendDbM>(capacity: nrOfItems);
        for (int i = 0; i < nrOfItems; i++)
        {
            var firstName = seeder.FirstName;
            var lastName = seeder.LastName;

            var friend = new FriendDbM
            {
                FriendId = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Email = seeder.Email(firstName, lastName),
                Birthday = seeder.DateAndTime(fromYear: 1950, toYear: DateTime.Today.Year - 10),
                Seeded = true
            };

            // Roughly half of friends get an address
            if (addresses.Count > 0 && seeder.Bool)
            {
                var adr = addresses[seeder.Next(0, addresses.Count)];
                friend.AddressDbM = adr;
                friend.AddressId = adr.AddressId;
            }

            // 0-2 pets per friend
            var nrPets = seeder.Next(0, 3);
            for (int p = 0; p < nrPets; p++)
            {
                friend.PetsDbM.Add(new PetDbM
                {
                    PetId = Guid.NewGuid(),
                    FriendId = friend.FriendId,
                    FriendDbM = friend,
                    Name = seeder.PetName,
                    Kind = seeder.FromEnum<Models.Interfaces.AnimalKind>(),
                    Mood = seeder.FromEnum<Models.Interfaces.AnimalMood>(),
                    Seeded = true
                });
            }

            // 0-3 quotes per friend
            var nrFriendQuotes = seeder.Next(0, 4);
            if (nrFriendQuotes > 0 && quotes.Count > 0)
            {
                friend.QuotesDbM = seeder.UniqueIndexPickedFromList(nrFriendQuotes, quotes);
            }

            friends.Add(friend);
        }

        _dbContext.Addresses.AddRange(addresses);
        _dbContext.Quotes.AddRange(quotes);
        _dbContext.Friends.AddRange(friends);

        await _dbContext.SaveChangesAsync();
        return await DbInfo();
    }
      
    public async Task<ResponseItemDto<GstUsrInfoAllDto>> RemoveSeedAsync(bool seeded)
    {
        // Remove friends first (cascades to pets and join table)
        var friendsToRemove = await _dbContext.Friends
            .Where(f => f.Seeded == seeded)
            .Include(f => f.PetsDbM)
            .Include(f => f.QuotesDbM)
            .ToListAsync();

        if (friendsToRemove.Count > 0)
        {
            _dbContext.Friends.RemoveRange(friendsToRemove);
            await _dbContext.SaveChangesAsync();
        }

        // Remove orphaned quotes/addresses that match the seeded flag
        var quotesToRemove = await _dbContext.Quotes
            .Where(q => q.Seeded == seeded)
            .Where(q => !q.FriendsDbM.Any())
            .ToListAsync();

        if (quotesToRemove.Count > 0)
            _dbContext.Quotes.RemoveRange(quotesToRemove);

        var addressesToRemove = await _dbContext.Addresses
            .Where(a => a.Seeded == seeded)
            .Where(a => !a.FriendsDbM.Any())
            .ToListAsync();

        if (addressesToRemove.Count > 0)
            _dbContext.Addresses.RemoveRange(addressesToRemove);

        if (quotesToRemove.Count > 0 || addressesToRemove.Count > 0)
            await _dbContext.SaveChangesAsync();

        return await DbInfo();
    }
}
