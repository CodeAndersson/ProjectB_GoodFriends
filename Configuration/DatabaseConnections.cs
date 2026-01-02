using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using Configuration.Options;

namespace Configuration;

public enum DatabaseServer { SQLServer, MySql, PostgreSql, SQLite }
public class DatabaseConnections
{
    readonly IConfiguration _configuration;
    readonly DbConnectionSetsOptions _options;
    private readonly DbSetDetailOptions _activeDataSet;

    static string NormalizeTag(string tag) => (tag ?? string.Empty).Trim().ToLowerInvariant();

    static DbSetDetailOptions CloneWithTag(DbSetDetailOptions source, string newTag)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        return new DbSetDetailOptions
        {
            DbTag = newTag,
            DbServer = source.DbServer,
            DbConnections = source.DbConnections?.Select(c => new DbConnectionDetailOptions
            {
                DbUserLogin = c.DbUserLogin,
                DbConnection = c.DbConnection
            }).ToList() ?? new()
        };
    }

    public DbSetDetailOptions GetActiveDbSet => _activeDataSet;
    public DbConnectionDetailOptions GetDataConnectionDetails(string user) => GetLoginDetails(user, _activeDataSet);
    DbConnectionDetailOptions GetLoginDetails(string user, DbSetDetailOptions dataSet)
    {
        if (string.IsNullOrEmpty(user) || string.IsNullOrWhiteSpace(user))
            throw new ArgumentNullException(nameof(user));

        var conn = dataSet.DbConnections.First(m => m.DbUserLogin.Trim().ToLower() == user.Trim().ToLower());
        return new DbConnectionDetailOptions
        {
            DbUserLogin = conn.DbUserLogin,
            DbConnection = conn.DbConnection,
            DbConnectionString = _configuration.GetConnectionString(conn.DbConnection)
        };
    }

    public DatabaseConnections(IConfiguration configuration, IOptions<DbConnectionSetsOptions> dbSetOption)
    {
        _configuration = configuration;
        _options = dbSetOption.Value;

        var requestedTag = configuration["DatabaseConnections:UseDataSetWithTag"];
        if (string.IsNullOrWhiteSpace(requestedTag))
            throw new ArgumentException("DatabaseConnections:UseDataSetWithTag is not set");

        // Backwards-compatible tag migration: if config uses sql-friends.* but secrets still define sql-music.*,
        // create an in-memory alias entry so the rest of the app sees the new tag.
        var requestedTagNormalized = NormalizeTag(requestedTag);
        if (_options?.DataSets != null)
        {
            var hasRequested = _options.DataSets.Any(ds => NormalizeTag(ds.DbTag) == requestedTagNormalized);
            if (!hasRequested && requestedTagNormalized.Contains("sql-friends"))
            {
                var legacyTag = requestedTagNormalized.Replace("sql-friends", "sql-music");
                var legacy = _options.DataSets.FirstOrDefault(ds => NormalizeTag(ds.DbTag) == legacyTag);
                if (legacy != null)
                {
                    _options.DataSets.Add(CloneWithTag(legacy, requestedTagNormalized));
                }
            }
        }

        _activeDataSet = _options?.DataSets?.FirstOrDefault(ds => NormalizeTag(ds.DbTag) == requestedTagNormalized);
        if (_activeDataSet == null)
            throw new ArgumentException($"Dataset with DbTag '{requestedTag}' not found");
    }
}