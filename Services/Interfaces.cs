using Models.Interfaces;
using Models.DTO;

namespace Services;

public interface IAdminService
{
    public Task<ResponseItemDto<GstUsrInfoAllDto>> GuestInfoAsync();
    public Task<ResponseItemDto<GstUsrInfoAllDto>> SeedAsync(int nrOfItems);
    public Task<ResponseItemDto<GstUsrInfoAllDto>> RemoveSeedAsync(bool seeded);
    public Task<ResponseItemDto<UsrInfoDto>> SeedUsersAsync(int nrOfUsers, int nrOfSuperUsers, int nrOfSysAdmin);
}
public interface IMusicGroupsService
{
    public Task<ResponsePageDto<IMusicGroup>> ReadMusicGroupsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
    public Task<ResponseItemDto<IMusicGroup>> ReadMusicGroupAsync(Guid id, bool flat);
    public Task<ResponseItemDto<IMusicGroup>> DeleteMusicGroupAsync(Guid id);
    public Task<ResponseItemDto<IMusicGroup>> UpdateMusicGroupAsync(MusicGroupCUdto item);
    public Task<ResponseItemDto<IMusicGroup>> CreateMusicGroupAsync(MusicGroupCUdto item);
}

public interface IAlbumsService
{
    public Task<ResponsePageDto<IAlbum>> ReadAlbumsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
    public Task<ResponseItemDto<IAlbum>> ReadAlbumAsync(Guid id, bool flat);
    public Task<ResponseItemDto<IAlbum>> DeleteAlbumAsync(Guid id);
    public Task<ResponseItemDto<IAlbum>> UpdateAlbumAsync(AlbumCUdto item);
    public Task<ResponseItemDto<IAlbum>> CreateAlbumAsync(AlbumCUdto item);
}

public interface IArtistsService
{
    public Task<ResponsePageDto<IArtist>> ReadArtistsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
    public Task<ResponseItemDto<IArtist>> ReadArtistAsync(Guid id, bool flat);
    public Task<ResponseItemDto<IArtist>> DeleteArtistAsync(Guid id);
    public Task<ResponseItemDto<IArtist>> UpdateArtistAsync(ArtistCUdto item);
    public Task<ResponseItemDto<IArtist>> CreateArtistAsync(ArtistCUdto item);
}

public interface ILoginService
{
    public Task<ResponseItemDto<LoginUserSessionDto>> LoginUserAsync(LoginCredentialsDto usrCreds);
}

// ----------------------------
// GoodFriends domain (WebAPI)
// ----------------------------

public interface IFriendsService
{
    public Task<ResponsePageDto<Models.Interfaces.IFriend>> ReadFriendsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize);
    public Task<Models.Interfaces.IFriend> ReadFriendAsync(Guid id, bool flat);
    public Task<Models.DTO.FriendCuDto> ReadFriendCuDtoAsync(Guid id);
    public Task<Models.Interfaces.IFriend> UpdateFriendAsync(Guid id, Models.DTO.FriendCuDto item);
    public Task<Models.Interfaces.IFriend> DeleteFriendAsync(Guid id);
}

public interface IAddressesService
{
    public Task<Models.Interfaces.IAddress> ReadAddressAsync(Guid id, bool flat);
    public Task<Models.DTO.AddressCuDto> ReadAddressCuDtoAsync(Guid id);
    public Task<Models.Interfaces.IAddress> UpdateAddressAsync(Guid id, Models.DTO.AddressCuDto item);
}

public interface IPetsService
{
    public Task<Models.Interfaces.IPet> DeletePetAsync(Guid id);

    public Task<Models.DTO.PetCuDto> ReadPetCuDtoAsync(Guid id);
    public Task<Models.Interfaces.IPet> CreatePetAsync(Models.DTO.PetCuDto item);
    public Task<Models.Interfaces.IPet> UpdatePetAsync(Guid id, Models.DTO.PetCuDto item);
}

public interface IQuotesService
{
    public Task<Models.Interfaces.IQuote> DeleteQuoteAsync(Guid id);

    public Task<Models.DTO.QuoteCuDto> ReadQuoteCuDtoAsync(Guid id);
    public Task<Models.Interfaces.IQuote> CreateQuoteAsync(Models.DTO.QuoteCuDto item);
    public Task<Models.Interfaces.IQuote> UpdateQuoteAsync(Guid id, Models.DTO.QuoteCuDto item);
}


public enum MusicDataSource { SQLDatabase, WebApi }
public interface IMusicServiceActive
{
    public MusicDataSource ActiveDataSource {get; set;}
}

