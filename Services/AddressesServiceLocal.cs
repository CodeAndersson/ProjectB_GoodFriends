using Models.DTO;
using Models.Interfaces;
using Services.InMemory;

namespace Services;

public class AddressesServiceLocal : IAddressesService
{
    private readonly InMemoryFriendsStore _store;

    public AddressesServiceLocal(InMemoryFriendsStore store)
    {
        _store = store;
    }

    public Task<IAddress> ReadAddressAsync(Guid id, bool flat)
        => Task.FromResult(_store.ReadAddress(id));

    public Task<AddressCuDto> ReadAddressCuDtoAsync(Guid id)
        => Task.FromResult(_store.ReadAddressCuDto(id));

    public Task<IAddress> UpdateAddressAsync(Guid id, AddressCuDto item)
        => Task.FromResult(_store.UpdateAddress(id, item));
}
