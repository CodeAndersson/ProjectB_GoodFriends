using Models.Interfaces;
using Models.DTO;
using Services.InMemory;

namespace Services;

public class PetsServiceLocal : IPetsService
{
    private readonly InMemoryFriendsStore _store;

    public PetsServiceLocal(InMemoryFriendsStore store)
    {
        _store = store;
    }

    public Task<IPet> DeletePetAsync(Guid id)
        => Task.FromResult(_store.DeletePet(id));

    public Task<PetCuDto> ReadPetCuDtoAsync(Guid id)
        => Task.FromResult(_store.ReadPetCuDto(id));

    public Task<IPet> CreatePetAsync(PetCuDto item)
        => Task.FromResult(_store.CreatePet(item));

    public Task<IPet> UpdatePetAsync(Guid id, PetCuDto item)
        => Task.FromResult(_store.UpdatePet(id, item));
}
