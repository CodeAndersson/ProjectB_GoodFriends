namespace AppMvc.Models
{
    public class FriendsByCityInCountryViewModel
    {
        public class CityInfo
        {
            public string City { get; set; }
            public int NrFriends { get; set; }
            public int NrPets { get; set; }
        }

        public string Country { get; set; }
        public List<CityInfo> Cities { get; set; } = new();
    }
}
