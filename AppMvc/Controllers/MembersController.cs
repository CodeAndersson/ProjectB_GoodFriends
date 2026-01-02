using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppMvc.Models;
using Models.DTO;
using Services;

namespace AppMvc.Controllers;

[Authorize]
public class MembersController : Controller
{
    private readonly IAdminService _adminService;

    public MembersController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<IActionResult> FriendsByCountry()
    {
        var info = (await _adminService.GuestInfoAsync()).Item;

        var countries = (info?.Friends ?? new List<GstUsrInfoFriendsDto>())
            .Where(x => !string.IsNullOrWhiteSpace(x.Country))
            .GroupBy(x => x.Country)
            .Select(g => new GstUsrInfoFriendsDto { Country = g.Key, NrFriends = g.Sum(x => x.NrFriends) })
            .OrderBy(x => x.Country)
            .ToList();

        var vm = new FriendsByCountryViewModel { Countries = countries };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> FriendsByCityInCountry(string country)
    {
        if (string.IsNullOrWhiteSpace(country))
            return RedirectToAction(nameof(FriendsByCountry));

        var info = (await _adminService.GuestInfoAsync()).Item;

        var friends = (info?.Friends ?? new List<GstUsrInfoFriendsDto>())
            .Where(x => string.Equals(x.Country, country, StringComparison.OrdinalIgnoreCase));

        var pets = (info?.Pets ?? new List<GstUsrInfoPetsDto>())
            .Where(x => string.Equals(x.Country, country, StringComparison.OrdinalIgnoreCase));

        var cities = friends
            .GroupBy(x => x.City ?? string.Empty)
            .Select(g => new FriendsByCityInCountryViewModel.CityInfo
            {
                City = g.Key,
                NrFriends = g.Sum(x => x.NrFriends),
                NrPets = pets.Where(p => string.Equals(p.City ?? string.Empty, g.Key, StringComparison.OrdinalIgnoreCase)).Sum(p => p.NrPets)
            })
            .OrderBy(x => x.City)
            .ToList();

        var vm = new FriendsByCityInCountryViewModel
        {
            Country = country,
            Cities = cities
        };

        return View(vm);
    }
}
