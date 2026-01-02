using Models.DTO;
using Models.Interfaces;

namespace Services;

public class FriendsServiceActive : IFriendsService
{
    private readonly IMusicServiceActive _active;
    private readonly FriendsServiceWapi _wapi;
    private readonly FriendsServiceLocal _local;

    public FriendsServiceActive(IMusicServiceActive active, FriendsServiceWapi wapi, FriendsServiceLocal local)
    {
        _active = active;
        _wapi = wapi;
        _local = local;
    }

    private IFriendsService Current => _active.ActiveDataSource == MusicDataSource.WebApi
        ? _wapi
        : _local;

    public Task<ResponsePageDto<IFriend>> ReadFriendsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize)
        => Current.ReadFriendsAsync(seeded, flat, filter, pageNumber, pageSize);

    public Task<IFriend> ReadFriendAsync(Guid id, bool flat)
        => Current.ReadFriendAsync(id, flat);

    public Task<FriendCuDto> ReadFriendCuDtoAsync(Guid id)
        => Current.ReadFriendCuDtoAsync(id);

    public Task<IFriend> UpdateFriendAsync(Guid id, FriendCuDto item)
        => Current.UpdateFriendAsync(id, item);

    public Task<IFriend> DeleteFriendAsync(Guid id)
        => Current.DeleteFriendAsync(id);
}
