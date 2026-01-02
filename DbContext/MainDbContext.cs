using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using Models.DTO;
using DbModels;
using DbContext.Extensions;
using Models.Authorization;


namespace DbContext;

//DbContext namespace is a fundamental EFC layer of the database context and is
//used for all Database connection as well as for EFC CodeFirst migration and database updates 
public class MainDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
#if DEBUG
    // remove password from connection string in debug mode
    // this is useful for debugging and logging purposes, but should not be used in production code
    public string dbConnection => System.Text.RegularExpressions.Regex.Replace(
        this.Database.GetConnectionString() ?? "", @"(pwd|password)=[^;]*;?", "",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
#endif

    #region C# model of database tables
    public DbSet<FriendDbM> Friends { get; set; }
    public DbSet<AddressDbM> Addresses { get; set; }
    public DbSet<PetDbM> Pets { get; set; }
    public DbSet<QuoteDbM> Quotes { get; set; }

    // Legacy template DbSets kept for compilation only. They are ignored in OnModelCreating.
    public DbSet<MusicGroupDbM> MusicGroups { get; set; }
    public DbSet<AlbumDbM> Albums { get; set; }
    public DbSet<ArtistDbM> Artists { get; set; }

    //User for login
    //now created by Identity
    //public DbSet<UserDbM> Users { get; set; }
    #endregion

    #region constructors
    public MainDbContext() { }
    public MainDbContext(DbContextOptions options) : base(options)
    { }
    #endregion

    #region model the Views
    public DbSet<GstUsrInfoDbDto> InfoDbView { get; set; }
    #endregion

    //Here we can modify the migration building
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region model the Views
        modelBuilder.Entity<GstUsrInfoDbDto>().ToView("vwInfoDb", "gstusr").HasNoKey();
        #endregion

        #region GoodFriends domain
        modelBuilder.Entity<AddressDbM>()
            .HasIndex(a => new { a.StreetAddress, a.ZipCode, a.City, a.Country })
            .IsUnique();

        modelBuilder.Entity<FriendDbM>()
            .HasIndex(f => new { f.FirstName, f.LastName });
        modelBuilder.Entity<FriendDbM>()
            .HasIndex(f => new { f.LastName, f.FirstName });

        modelBuilder.Entity<FriendDbM>()
            .HasOne(f => f.AddressDbM)
            .WithMany(a => a.FriendsDbM)
            .HasForeignKey(f => f.AddressId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PetDbM>()
            .HasOne(p => p.FriendDbM)
            .WithMany(f => f.PetsDbM)
            .HasForeignKey(p => p.FriendId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FriendDbM>()
            .HasMany(f => f.QuotesDbM)
            .WithMany(q => q.FriendsDbM)
            .UsingEntity<FriendDbMQuoteDbM>(
                j => j
                    .HasOne(x => x.QuotesDbM)
                    .WithMany()
                    .HasForeignKey(x => x.QuotesDbMQuoteId)
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne(x => x.FriendsDbM)
                    .WithMany()
                    .HasForeignKey(x => x.FriendsDbMFriendId)
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.ToTable("FriendDbMQuoteDbM", "supusr");
                    j.HasKey(x => new { x.FriendsDbMFriendId, x.QuotesDbMQuoteId });
                });
        #endregion

        #region Legacy template entities
        modelBuilder.Ignore<MusicGroupDbM>();
        modelBuilder.Ignore<AlbumDbM>();
        modelBuilder.Ignore<ArtistDbM>();
        #endregion

        #region override modelbuilder
        #endregion
        
        base.OnModelCreating(modelBuilder);
    }

    #region DbContext for some popular databases
    public class SqlServerDbContext : MainDbContext
    {
        public SqlServerDbContext() { }
        public SqlServerDbContext(DbContextOptions options) 
            : base(options) { }


        //Used only for CodeFirst Database Migration and database update commands
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder = optionsBuilder.ConfigureForDesignTime(
                    (options, connectionString) => options.UseSqlServer(connectionString, options => options.EnableRetryOnFailure()));
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>().HaveColumnType("money");
            configurationBuilder.Properties<string>().HaveColumnType("varchar(200)");

            base.ConfigureConventions(configurationBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Add your own modelling based on done migrations
            base.OnModelCreating(modelBuilder);
        }
    }

    public class MySqlDbContext : MainDbContext
    {
        public MySqlDbContext() { }
        public MySqlDbContext(DbContextOptions options) : base(options) { }


        //Used only for CodeFirst Database Migration
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder = optionsBuilder.ConfigureForDesignTime(
                    (options, connectionString) =>
                        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                            b => b.SchemaBehavior(Pomelo.EntityFrameworkCore.MySql.Infrastructure.MySqlSchemaBehavior.Translate, (schema, table) => $"{schema}_{table}")));
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<string>().HaveColumnType("varchar(200)");

            base.ConfigureConventions(configurationBuilder);

        }
    }

    public class PostgresDbContext : MainDbContext
    {
        public PostgresDbContext() { }
        public PostgresDbContext(DbContextOptions options) : base(options){ }


        //Used only for CodeFirst Database Migration
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder = optionsBuilder.ConfigureForDesignTime(
                    (options, connectionString) => options.UseNpgsql(connectionString));
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<string>().HaveColumnType("varchar(200)");
            base.ConfigureConventions(configurationBuilder);
        }
    }
    #endregion
}
