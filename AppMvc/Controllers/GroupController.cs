using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppMvc.Controllers;

// The project has been refactored to the Friends domain.
// Keep this controller as a compatibility shim so any leftover /Group/* links don't crash at runtime.
[Authorize]
public class GroupController : Controller
{
    private IActionResult RedirectToFriends() => RedirectToAction(actionName: "ListOfFriends", controllerName: "Friend");

    [HttpGet]
    public IActionResult ListOfGroups(int pagenr = 0, string search = null) => RedirectToFriends();

    [HttpPost]
    public IActionResult SearchGroup() => RedirectToFriends();

    [HttpPost]
    public IActionResult DeleteGroup() => RedirectToFriends();

    [HttpGet]
    public IActionResult ViewGroup() => RedirectToFriends();

    [HttpGet]
    public IActionResult EditGroup() => RedirectToFriends();

    [HttpPost]
    public IActionResult Save() => RedirectToFriends();

    [HttpPost]
    public IActionResult Undo() => RedirectToFriends();

    [HttpPost]
    public IActionResult AddArtist() => RedirectToFriends();

    [HttpPost]
    public IActionResult DeleteArtist() => RedirectToFriends();

    [HttpPost]
    public IActionResult EditArtist() => RedirectToFriends();

    [HttpPost]
    public IActionResult AddAlbum() => RedirectToFriends();

    [HttpPost]
    public IActionResult DeleteAlbum() => RedirectToFriends();

    [HttpPost]
    public IActionResult EditAlbum() => RedirectToFriends();
}

