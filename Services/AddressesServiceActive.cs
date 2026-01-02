using Models.DTO;
using Models.Interfaces;

namespace Services;

public class AddressesServiceActive : IAddressesService
{
    private readonly IDataSourceActive _active;
    private readonly AddressesServiceWapi _wapi;
    private readonly AddressesServiceLocal _local;

    public AddressesServiceActive(IDataSourceActive active, AddressesServiceWapi wapi, AddressesServiceLocal local)
    {
        _active = active;
        _wapi = wapi;
        _local = local;
    }

    private IAddressesService Current => _active.ActiveDataSource == DataSource.WebApi
        ? _wapi
        : _local;

    public Task<IAddress> ReadAddressAsync(Guid id, bool flat)
        => Current.ReadAddressAsync(id, flat);

    public Task<AddressCuDto> ReadAddressCuDtoAsync(Guid id)
        => Current.ReadAddressCuDtoAsync(id);

    public Task<IAddress> UpdateAddressAsync(Guid id, AddressCuDto item)
        => Current.UpdateAddressAsync(id, item);
}
