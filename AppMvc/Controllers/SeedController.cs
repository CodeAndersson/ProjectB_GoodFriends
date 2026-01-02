using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AppMvc.Models;
using Services;
using Microsoft.AspNetCore.Authorization;

namespace AppMvc.Controllers;
[Authorize]
public class SeedController : Controller
{
    readonly ILogger<SeedController> _logger;
    readonly IAdminService _admin_service = null;

    //Inject services just like in WebApi
    public SeedController(IAdminService admin_service, ILogger<SeedController> logger)
    {
        _admin_service = admin_service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Seed()
    {
        //Use the Service
        var info = await _admin_service.GuestInfoAsync();

        //Create the viewModel
        var vm = new SeedViewModel() { NrOfFriends = info.Item.Db.NrSeededFriends + info.Item.Db.NrUnseededFriends };

        //Render the View
        return View("Seed", vm);
    }


    [HttpPost]
    public async Task<IActionResult> Seed(SeedViewModel vm)
    {
        if (ModelState.IsValid)
        {
            if (vm.RemoveSeeds)
            {
                await _admin_service.RemoveSeedAsync(true);
                await _admin_service.RemoveSeedAsync(false);
            }

            await _admin_service.SeedAsync(vm.NrOfItemsToSeed);
            return RedirectToAction(nameof(Seed));
        }

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

