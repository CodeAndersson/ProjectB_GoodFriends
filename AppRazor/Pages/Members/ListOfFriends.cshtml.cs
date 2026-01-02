using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Models.Interfaces;
using Services;

namespace AppRazor.Pages;

public class ListOfFriendsModel : PageModel
{
    private readonly IFriendsService _friendsService;
    private readonly IAuthorizationService _authorizationService;

    public List<IFriend> Friends { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool UseSeeds { get; set; } = true;

    // Pagination (same structure as ListOfGroups)
    public int NrOfPages { get; set; }
    public int PageSize { get; } = 10;
    public int ThisPageNr { get; set; } = 0;
    public int PrevPageNr { get; set; } = 0;
    public int NextPageNr { get; set; } = 0;
    public int NrVisiblePages { get; set; } = 0;

    public int NrOfFriends { get; set; }

    // ModelBinding for the form
    [BindProperty]
    public string SearchFilter { get; set; } = null;

    public ListOfFriendsModel(IFriendsService friendsService, IAuthorizationService authorizationService)
    {
        _friendsService = friendsService;
        _authorizationService = authorizationService;
    }

    public async Task<IActionResult> OnGet()
    {
        if (int.TryParse(Request.Query["pagenr"], out int pagenr))
        {
            ThisPageNr = pagenr;
        }

        // Accept both old/new query keys (Filter from other pages, search from template)
        SearchFilter = Request.Query["search"].FirstOrDefault()
            ?? Request.Query["Filter"].FirstOrDefault()
            ?? Request.Query["filter"].FirstOrDefault();

        var flat = string.IsNullOrWhiteSpace(SearchFilter);
        var resp = await _friendsService.ReadFriendsAsync(seeded: UseSeeds, flat: flat, filter: SearchFilter, pageNumber: ThisPageNr, pageSize: PageSize);
        Friends = resp.PageItems;
        NrOfFriends = resp.DbItemsCount;

        UpdatePagination(resp.DbItemsCount);
        return Page();
    }

    private void ReadPageNrFromQuery()
    {
        if (int.TryParse(Request.Query["pagenr"], out int pagenr))
        {
            ThisPageNr = pagenr;
        }
    }

    private void UpdatePagination(int nrOfItems)
    {
        NrOfPages = (int)Math.Ceiling((double)nrOfItems / PageSize);
        PrevPageNr = Math.Max(0, ThisPageNr - 1);
        NextPageNr = Math.Min(Math.Max(NrOfPages - 1, 0), ThisPageNr + 1);
        NrVisiblePages = Math.Min(10, NrOfPages);
    }

    public IActionResult OnPostSearch()
    {
        // Redirect to GET so the URL reflects the active filter and paging links keep the state.
        var search = Uri.EscapeDataString(SearchFilter ?? string.Empty);
        var useSeeds = UseSeeds.ToString().ToLowerInvariant();
        return Redirect($"~/Members/ListOfFriends?pagenr=0&search={search}&useSeeds={useSeeds}");
    }

    public async Task<IActionResult> OnPostDeleteFriend(Guid friendId)
    {
        ReadPageNrFromQuery();

        var friend = await _friendsService.ReadFriendAsync(friendId, flat: true);
        var result = await _authorizationService.AuthorizeAsync(User, friend, Models.Authorization.CrudOperations.Delete);
        if (!result.Succeeded)
        {
            return Forbid();
        }

        await _friendsService.DeleteFriendAsync(friendId);

        var resp = await _friendsService.ReadFriendsAsync(seeded: UseSeeds, flat: true, filter: SearchFilter, pageNumber: ThisPageNr, pageSize: PageSize);
        Friends = resp.PageItems;
        NrOfFriends = resp.DbItemsCount;

        UpdatePagination(resp.DbItemsCount);
        return Page();
    }
}
