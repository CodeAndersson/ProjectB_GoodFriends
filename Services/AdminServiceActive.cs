using Microsoft.Extensions.Logging;
using Models.DTO;

namespace Services;

public class AdminServiceActive : IAdminService
{
    private readonly IMusicServiceActive _active;
    private readonly AdminServiceWapi _wapi;
    private readonly AdminServiceLocal _local;
    private readonly ILogger<AdminServiceActive> _logger;

    public AdminServiceActive(
        IMusicServiceActive active,
        AdminServiceWapi wapi,
        AdminServiceLocal local,
        ILogger<AdminServiceActive> logger)
    {
        _active = active;
        _wapi = wapi;
        _local = local;
        _logger = logger;
    }

    private IAdminService Current => _active.ActiveDataSource == MusicDataSource.WebApi
        ? _wapi
        : _local;

    public Task<ResponseItemDto<GstUsrInfoAllDto>> GuestInfoAsync()
        => Current.GuestInfoAsync();

    public Task<ResponseItemDto<GstUsrInfoAllDto>> SeedAsync(int nrOfItems)
    {
        if (_active.ActiveDataSource == MusicDataSource.WebApi)
        {
            _logger.LogWarning("Seeding is not supported for Friends WebApi; seeding local in-memory requires SQLDatabase selection.");
            return _wapi.GuestInfoAsync();
        }

        return _local.SeedAsync(nrOfItems);
    }

    public Task<ResponseItemDto<GstUsrInfoAllDto>> RemoveSeedAsync(bool seeded)
    {
        if (_active.ActiveDataSource == MusicDataSource.WebApi)
        {
            _logger.LogWarning("Seed removal is not supported for Friends WebApi; clear local in-memory requires SQLDatabase selection.");
            return _wapi.GuestInfoAsync();
        }

        return _local.RemoveSeedAsync(seeded);
    }

    public Task<ResponseItemDto<UsrInfoDto>> SeedUsersAsync(int nrOfUsers, int nrOfSuperUsers, int nrOfSysAdmin)
        => throw new NotImplementedException();
}
