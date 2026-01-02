using Models.DTO;

namespace AppMvc.Models
{
    public class FriendsByCountryViewModel
    {
        public List<GstUsrInfoFriendsDto> Countries { get; set; } = new();
    }
}
