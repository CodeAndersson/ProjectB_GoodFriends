using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTO;
using Services;

namespace AppRazor.Pages;

public class FriendsByCountryModel : PageModel
{
    private readonly IAdminService _adminService;

    public List<GstUsrInfoFriendsDto> Countries { get; set; }

    public FriendsByCountryModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> OnGet()
    {
        var info = (await _adminService.GuestInfoAsync()).Item;

        Countries = (info.Friends ?? new List<GstUsrInfoFriendsDto>())
            .Where(x => !string.IsNullOrWhiteSpace(x.Country))
            .GroupBy(x => x.Country)
            .Select(g => new GstUsrInfoFriendsDto { Country = g.Key, NrFriends = g.Sum(x => x.NrFriends) })
            .OrderBy(x => x.Country)
            .ToList();

        return Page();
    }
}
