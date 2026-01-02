using Microsoft.AspNetCore.Mvc;

using Models.Interfaces;

namespace AppMvc.Models;

public class ListOfFriendsViewModel
{
    [BindProperty]
    public bool UseSeeds { get; set; } = true;

    public List<IFriend> Friends { get; set; }
    public int NrOfFriends { get; set; }

    // Pagination
    public int NrOfPages { get; set; }
    public int PageSize { get; } = 10;

    public int ThisPageNr { get; set; } = 0;
    public int PrevPageNr { get; set; } = 0;
    public int NextPageNr { get; set; } = 0;
    public int NrVisiblePages { get; set; } = 0;

    // ModelBinding for the form
    [BindProperty]
    public string SearchFilter { get; set; } = null;

    public void UpdatePagination(int nrOfItems)
    {
        NrOfPages = (int)Math.Ceiling((double)nrOfItems / PageSize);
        PrevPageNr = Math.Max(0, ThisPageNr - 1);
        NextPageNr = Math.Min(Math.Max(NrOfPages - 1, 0), ThisPageNr + 1);
        NrVisiblePages = Math.Min(10, NrOfPages);
    }
}
