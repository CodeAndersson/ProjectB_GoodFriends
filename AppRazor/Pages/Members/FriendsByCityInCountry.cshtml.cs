using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTO;
using Services;

namespace AppRazor.Pages;

public class FriendsByCityInCountryModel : PageModel
{
    private readonly IAdminService _adminService;

    [BindProperty(SupportsGet = true)]
    public string Country { get; set; }

    public List<(string City, int NrFriends, int NrPets)> Cities { get; set; }

    public FriendsByCityInCountryModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> OnGet()
    {
        var info = (await _adminService.GuestInfoAsync()).Item;

        var friends = (info.Friends ?? new List<GstUsrInfoFriendsDto>())
            .Where(x => string.Equals(x.Country, Country, StringComparison.OrdinalIgnoreCase));

        var pets = (info.Pets ?? new List<GstUsrInfoPetsDto>())
            .Where(x => string.Equals(x.Country, Country, StringComparison.OrdinalIgnoreCase));

        Cities = friends
            .GroupBy(x => x.City ?? string.Empty)
            .Select(g => (
                City: g.Key,
                NrFriends: g.Sum(x => x.NrFriends),
                NrPets: pets.Where(p => string.Equals(p.City ?? string.Empty, g.Key, StringComparison.OrdinalIgnoreCase)).Sum(p => p.NrPets)
            ))
            .OrderBy(x => x.City)
            .ToList();

        return Page();
    }
}
