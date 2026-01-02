using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using AppMvc.Models;
using Models.Authorization;
using Services;

namespace AppMvc.Controllers;

[Authorize]
public class FriendController : Controller
{
    private readonly IFriendsService _friendsService;
    private readonly IAddressesService _addressesService;
    private readonly IPetsService _petsService;
    private readonly IQuotesService _quotesService;
    private readonly IAuthorizationService _authorizationService;

    public FriendController(
        IFriendsService friendsService,
        IAddressesService addressesService,
        IPetsService petsService,
        IQuotesService quotesService,
        IAuthorizationService authorizationService)
    {
        _friendsService = friendsService;
        _addressesService = addressesService;
        _petsService = petsService;
        _quotesService = quotesService;
        _authorizationService = authorizationService;
    }

    public async Task<IActionResult> ListOfFriends(int pagenr, string search, bool useSeeds = true)
    {
        var vm = new ListOfFriendsViewModel
        {
            ThisPageNr = pagenr,
            SearchFilter = search,
            UseSeeds = useSeeds
        };

        var flat = string.IsNullOrWhiteSpace(vm.SearchFilter);
        var resp = await _friendsService.ReadFriendsAsync(seeded: vm.UseSeeds, flat: flat, filter: vm.SearchFilter, pageNumber: vm.ThisPageNr, pageSize: vm.PageSize);
        vm.Friends = resp.PageItems;
        vm.NrOfFriends = resp.DbItemsCount;
        vm.UpdatePagination(resp.DbItemsCount);

        return View(vm);
    }

    [HttpPost]
    public IActionResult SearchFriends(ListOfFriendsViewModel vm)
    {
        // Redirect to GET so paging links preserve state.
        return RedirectToAction(nameof(ListOfFriends), new
        {
            pagenr = 0,
            search = vm.SearchFilter,
            useSeeds = vm.UseSeeds
        });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteFriend(Guid friendId, ListOfFriendsViewModel vm)
    {
        var friend = await _friendsService.ReadFriendAsync(friendId, flat: true);
        var auth = await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Delete);
        if (!auth.Succeeded) return Forbid();

        await _friendsService.DeleteFriendAsync(friendId);

        // Re-read list to render same view
        var flat = string.IsNullOrWhiteSpace(vm.SearchFilter);
        var resp = await _friendsService.ReadFriendsAsync(seeded: vm.UseSeeds, flat: flat, filter: vm.SearchFilter, pageNumber: vm.ThisPageNr, pageSize: vm.PageSize);
        vm.Friends = resp.PageItems;
        vm.NrOfFriends = resp.DbItemsCount;
        vm.UpdatePagination(resp.DbItemsCount);

        return View("ListOfFriends", vm);
    }

    public async Task<IActionResult> ViewFriend(Guid id)
    {
        if (id == Guid.Empty) return NotFound();

        var friend = await _friendsService.ReadFriendAsync(id, flat: false);
        if (friend is null) return NotFound();

        var canEdit = (await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Edit)).Succeeded;
        var canDelete = (await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Delete)).Succeeded;

        var vm = new ViewFriendViewModel
        {
            Id = id,
            Friend = friend,
            CanEdit = canEdit,
            CanDelete = canDelete
        };

        return View(vm);
    }

    public async Task<IActionResult> EditFriend(Guid id)
    {
        if (id == Guid.Empty) return NotFound();

        var friend = await _friendsService.ReadFriendAsync(id, flat: true);
        if (friend is null) return NotFound();

        var auth = await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Edit);
        if (!auth.Succeeded) return Forbid();

        var dto = await _friendsService.ReadFriendCuDtoAsync(id);
        var vm = new EditFriendViewModel { Id = id, Input = EditFriendViewModel.FriendInputModel.FromDto(dto) };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> EditFriend(EditFriendViewModel vm)
    {
        if (vm.Id == Guid.Empty) return NotFound();

        var friend = await _friendsService.ReadFriendAsync(vm.Id, flat: true);
        if (friend is null) return NotFound();

        var auth = await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Edit);
        if (!auth.Succeeded) return Forbid();

        if (!ModelState.IsValid) return View(vm);

        // Preserve relations not edited on this form
        var existing = await _friendsService.ReadFriendCuDtoAsync(vm.Id);
        var dto = vm.Input.ToDto(vm.Id);
        dto.AddressId ??= existing?.AddressId;
        dto.PetsId ??= existing?.PetsId;
        dto.QuotesId ??= existing?.QuotesId;

        try
        {
            await _friendsService.UpdateFriendAsync(vm.Id, dto);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            var msg = ex.Message?.Trim().Trim('"') ?? "Bad request";
            if (msg.Contains("FirstName", StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError("Input.FirstName", msg);
            else if (msg.Contains("LastName", StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError("Input.LastName", msg);
            else if (msg.Contains("Email", StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError("Input.Email", msg);
            else if (msg.Contains("Birthday", StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError("Input.Birthday", msg);
            else
                ModelState.AddModelError(string.Empty, msg);

            return View(vm);
        }

        return RedirectToAction(nameof(ViewFriend), new { id = vm.Id });
    }

    public async Task<IActionResult> EditAddress(Guid id, Guid friendId)
    {
        if (id == Guid.Empty) return NotFound();

        var friend = friendId != Guid.Empty ? await _friendsService.ReadFriendAsync(friendId, flat: true) : null;
        if (friend is not null)
        {
            var auth = await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Edit);
            if (!auth.Succeeded) return Forbid();
        }

        var dto = await _addressesService.ReadAddressCuDtoAsync(id);
        var vm = new EditAddressViewModel { Id = id, FriendId = friendId, Input = EditAddressViewModel.AddressInputModel.FromDto(dto) };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> EditAddress(EditAddressViewModel vm)
    {
        if (vm.Id == Guid.Empty) return NotFound();

        var friend = vm.FriendId != Guid.Empty ? await _friendsService.ReadFriendAsync(vm.FriendId, flat: true) : null;
        if (friend is not null)
        {
            var auth = await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Edit);
            if (!auth.Succeeded) return Forbid();
        }

        if (!ModelState.IsValid) return View(vm);

        var existing = await _addressesService.ReadAddressCuDtoAsync(vm.Id);
        var dto = vm.Input.ToDto(vm.Id);
        dto.FriendsId ??= existing?.FriendsId;

        try
        {
            await _addressesService.UpdateAddressAsync(vm.Id, dto);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            var msg = ex.Message?.Trim().Trim('"') ?? "Bad request";
            if (msg.Contains("StreetAddress", StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError("Input.StreetAddress", msg);
            else if (msg.Contains("ZipCode", StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError("Input.ZipCode", msg);
            else if (msg.Contains("City", StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError("Input.City", msg);
            else if (msg.Contains("Country", StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError("Input.Country", msg);
            else
                ModelState.AddModelError(string.Empty, msg);

            return View(vm);
        }

        if (vm.FriendId != Guid.Empty)
            return RedirectToAction(nameof(ViewFriend), new { id = vm.FriendId });

        return RedirectToAction(nameof(ListOfFriends));
    }

    public async Task<IActionResult> CreatePet(Guid friendId)
    {
        if (friendId == Guid.Empty) return NotFound();

        var friend = await _friendsService.ReadFriendAsync(friendId, flat: true);
        if (friend is null) return NotFound();

        var auth = await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Edit);
        if (!auth.Succeeded) return Forbid();

        return View(new CreatePetViewModel { FriendId = friendId });
    }

    [HttpPost]
    public async Task<IActionResult> CreatePet(CreatePetViewModel vm)
    {
        if (vm.FriendId == Guid.Empty) return NotFound();

        var friend = await _friendsService.ReadFriendAsync(vm.FriendId, flat: true);
        if (friend is null) return NotFound();

        var auth = await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Edit);
        if (!auth.Succeeded) return Forbid();

        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _petsService.CreatePetAsync(vm.Input.ToDto(vm.FriendId));
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            ModelState.AddModelError(string.Empty, ex.Message?.Trim().Trim('"') ?? "Bad request");
            return View(vm);
        }

        return RedirectToAction(nameof(ViewFriend), new { id = vm.FriendId });
    }

    public async Task<IActionResult> EditPet(Guid id, Guid friendId)
    {
        if (id == Guid.Empty) return NotFound();

        var dto = await _petsService.ReadPetCuDtoAsync(id);
        if (dto is null) return NotFound();

        var backId = friendId != Guid.Empty ? friendId : dto.FriendId;
        var friend = await _friendsService.ReadFriendAsync(backId, flat: true);
        if (friend is null) return NotFound();

        var auth = await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Edit);
        if (!auth.Succeeded) return Forbid();

        var vm = new EditPetViewModel { Id = id, FriendId = backId, Input = EditPetViewModel.PetInputModel.FromDto(dto) };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> EditPet(EditPetViewModel vm)
    {
        if (vm.Id == Guid.Empty) return NotFound();

        var existing = await _petsService.ReadPetCuDtoAsync(vm.Id);
        if (existing is null) return NotFound();

        var backId = vm.FriendId != Guid.Empty ? vm.FriendId : existing.FriendId;
        var friend = await _friendsService.ReadFriendAsync(backId, flat: true);
        if (friend is null) return NotFound();

        var auth = await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Edit);
        if (!auth.Succeeded) return Forbid();

        if (!ModelState.IsValid) return View(vm);

        var dto = vm.Input.ToDto(vm.Id);
        dto.FriendId = existing.FriendId;

        try
        {
            await _petsService.UpdatePetAsync(vm.Id, dto);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            ModelState.AddModelError(string.Empty, ex.Message?.Trim().Trim('"') ?? "Bad request");
            return View(vm);
        }

        return RedirectToAction(nameof(ViewFriend), new { id = backId });
    }

    [HttpPost]
    public async Task<IActionResult> DeletePet(Guid id, Guid petId)
    {
        if (id == Guid.Empty || petId == Guid.Empty) return BadRequest();

        var friend = await _friendsService.ReadFriendAsync(id, flat: true);
        if (friend is null) return NotFound();

        var auth = await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Delete);
        if (!auth.Succeeded) return Forbid();

        await _petsService.DeletePetAsync(petId);
        return RedirectToAction(nameof(ViewFriend), new { id });
    }

    public async Task<IActionResult> CreateQuote(Guid friendId)
    {
        if (friendId == Guid.Empty) return NotFound();

        var friend = await _friendsService.ReadFriendAsync(friendId, flat: true);
        if (friend is null) return NotFound();

        var auth = await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Edit);
        if (!auth.Succeeded) return Forbid();

        return View(new CreateQuoteViewModel { FriendId = friendId });
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuote(CreateQuoteViewModel vm)
    {
        if (vm.FriendId == Guid.Empty) return NotFound();

        var friend = await _friendsService.ReadFriendAsync(vm.FriendId, flat: true);
        if (friend is null) return NotFound();

        var auth = await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Edit);
        if (!auth.Succeeded) return Forbid();

        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _quotesService.CreateQuoteAsync(vm.Input.ToDto(vm.FriendId));
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            ModelState.AddModelError(string.Empty, ex.Message?.Trim().Trim('"') ?? "Bad request");
            return View(vm);
        }

        return RedirectToAction(nameof(ViewFriend), new { id = vm.FriendId });
    }

    public async Task<IActionResult> EditQuote(Guid id, Guid friendId)
    {
        if (id == Guid.Empty) return NotFound();

        var dto = await _quotesService.ReadQuoteCuDtoAsync(id);
        if (dto is null) return NotFound();

        var backId = friendId != Guid.Empty ? friendId : (dto.FriendsId?.FirstOrDefault() ?? Guid.Empty);
        var friend = await _friendsService.ReadFriendAsync(backId, flat: true);
        if (friend is null) return NotFound();

        var auth = await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Edit);
        if (!auth.Succeeded) return Forbid();

        var vm = new EditQuoteViewModel { Id = id, FriendId = backId, Input = EditQuoteViewModel.QuoteInputModel.FromDto(dto) };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> EditQuote(EditQuoteViewModel vm)
    {
        if (vm.Id == Guid.Empty) return NotFound();

        var existing = await _quotesService.ReadQuoteCuDtoAsync(vm.Id);
        if (existing is null) return NotFound();

        var backId = vm.FriendId != Guid.Empty ? vm.FriendId : (existing.FriendsId?.FirstOrDefault() ?? Guid.Empty);
        var friend = await _friendsService.ReadFriendAsync(backId, flat: true);
        if (friend is null) return NotFound();

        var auth = await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Edit);
        if (!auth.Succeeded) return Forbid();

        if (!ModelState.IsValid) return View(vm);

        var dto = vm.Input.ToDto(vm.Id);
        dto.FriendsId ??= existing.FriendsId;

        try
        {
            await _quotesService.UpdateQuoteAsync(vm.Id, dto);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            ModelState.AddModelError(string.Empty, ex.Message?.Trim().Trim('"') ?? "Bad request");
            return View(vm);
        }

        if (backId == Guid.Empty) return RedirectToAction(nameof(ListOfFriends));
        return RedirectToAction(nameof(ViewFriend), new { id = backId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteQuote(Guid id, Guid quoteId)
    {
        if (id == Guid.Empty || quoteId == Guid.Empty) return BadRequest();

        var friend = await _friendsService.ReadFriendAsync(id, flat: true);
        if (friend is null) return NotFound();

        var auth = await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Delete);
        if (!auth.Succeeded) return Forbid();

        await _quotesService.DeleteQuoteAsync(quoteId);
        return RedirectToAction(nameof(ViewFriend), new { id });
    }
}
