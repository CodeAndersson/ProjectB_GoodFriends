using Models.DTO;
using Models.Interfaces;
using Services.InMemory;

namespace Services;

public class FriendsServiceLocal : IFriendsService
{
    private readonly InMemoryFriendsStore _store;

    public FriendsServiceLocal(InMemoryFriendsStore store)
    {
        _store = store;
    }

    public Task<ResponsePageDto<IFriend>> ReadFriendsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize)
    {
        var all = _store.ReadFriends(seeded, flat, filter);
        var pageItems = all
            .Skip(Math.Max(0, pageNumber) * Math.Max(1, pageSize))
            .Take(Math.Max(1, pageSize))
            .ToList();

        return Task.FromResult(new ResponsePageDto<IFriend>
        {
            PageItems = pageItems,
            DbItemsCount = all.Count,
            PageNr = pageNumber,
            PageSize = pageSize
        });
    }

    public Task<IFriend> ReadFriendAsync(Guid id, bool flat)
        => Task.FromResult(_store.ReadFriend(id, flat));

    public Task<FriendCuDto> ReadFriendCuDtoAsync(Guid id)
        => Task.FromResult(_store.ReadFriendCuDto(id));

    public Task<IFriend> UpdateFriendAsync(Guid id, FriendCuDto item)
        => Task.FromResult(_store.UpdateFriend(id, item));

    public Task<IFriend> DeleteFriendAsync(Guid id)
        => Task.FromResult(_store.DeleteFriend(id));
}
