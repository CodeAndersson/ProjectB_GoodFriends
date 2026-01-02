using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Authorization;
using Models.Interfaces;
using Services;

namespace AppRazor.Pages;

public class ViewFriendModel : PageModel
{
    private readonly IFriendsService _friendsService;
    private readonly IPetsService _petsService;
    private readonly IQuotesService _quotesService;
    private readonly IAuthorizationService _authorizationService;

    public IFriend Friend { get; set; }

    public bool CanEdit { get; private set; }
    public bool CanDelete { get; private set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public Guid PetId { get; set; }

    [BindProperty]
    public Guid QuoteId { get; set; }

    public ViewFriendModel(
        IFriendsService friendsService,
        IPetsService petsService,
        IQuotesService quotesService,
        IAuthorizationService authorizationService)
    {
        _friendsService = friendsService;
        _petsService = petsService;
        _quotesService = quotesService;
        _authorizationService = authorizationService;
    }

    public async Task<IActionResult> OnGet()
    {
        if (Id == Guid.Empty) return NotFound();

        Friend = await _friendsService.ReadFriendAsync(Id, flat: false);

        if (Friend is not null)
        {
            CanEdit = (await _authorizationService.AuthorizeAsync(User, Friend, CrudOperations.Edit)).Succeeded;
            CanDelete = (await _authorizationService.AuthorizeAsync(User, Friend, CrudOperations.Delete)).Succeeded;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDeletePet()
    {
        if (Id == Guid.Empty || PetId == Guid.Empty) return BadRequest();

        var friend = await _friendsService.ReadFriendAsync(Id, flat: false);
        if (friend is null) return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Delete);
        if (!authResult.Succeeded) return Forbid();

        await _petsService.DeletePetAsync(PetId);
        return Redirect($"~/Members/ViewFriend?id={Id}");
    }

    public async Task<IActionResult> OnPostDeleteQuote()
    {
        if (Id == Guid.Empty || QuoteId == Guid.Empty) return BadRequest();

        var friend = await _friendsService.ReadFriendAsync(Id, flat: false);
        if (friend is null) return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, friend, CrudOperations.Delete);
        if (!authResult.Succeeded) return Forbid();

        await _quotesService.DeleteQuoteAsync(QuoteId);
        return Redirect($"~/Members/ViewFriend?id={Id}");
    }
}
