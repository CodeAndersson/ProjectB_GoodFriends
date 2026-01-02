using Models.Interfaces;
using Models.DTO;

namespace Services;

public class PetsServiceActive : IPetsService
{
    private readonly IDataSourceActive _active;
    private readonly PetsServiceWapi _wapi;
    private readonly PetsServiceLocal _local;

    public PetsServiceActive(IDataSourceActive active, PetsServiceWapi wapi, PetsServiceLocal local)
    {
        _active = active;
        _wapi = wapi;
        _local = local;
    }

    private IPetsService Current => _active.ActiveDataSource == DataSource.WebApi
        ? _wapi
        : _local;

    public Task<IPet> DeletePetAsync(Guid id)
        => Current.DeletePetAsync(id);

    public Task<PetCuDto> ReadPetCuDtoAsync(Guid id)
        => Current.ReadPetCuDtoAsync(id);

    public Task<IPet> CreatePetAsync(PetCuDto item)
        => Current.CreatePetAsync(item);

    public Task<IPet> UpdatePetAsync(Guid id, PetCuDto item)
        => Current.UpdatePetAsync(id, item);
}
