using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTO;
using Services;

namespace AppRazor.Pages;

public class CreateQuoteModel : PageModel
{
    private readonly IQuotesService _quotesService;

    [BindProperty(SupportsGet = true)]
    public Guid FriendId { get; set; }

    [BindProperty]
    public QuoteInputModel Input { get; set; } = new();

    public CreateQuoteModel(IQuotesService quotesService)
    {
        _quotesService = quotesService;
    }

    public IActionResult OnGet()
    {
        if (FriendId == Guid.Empty) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (FriendId == Guid.Empty) return NotFound();
        if (!ModelState.IsValid) return Page();

        var dto = Input.ToDto(friendId: FriendId);

        try
        {
            await _quotesService.CreateQuoteAsync(dto);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            ModelState.AddModelError(string.Empty, ex.Message?.Trim().Trim('"') ?? "Bad request");
            return Page();
        }

        return Redirect($"~/Members/ViewFriend?id={FriendId}");
    }

    public class QuoteInputModel
    {
        [Required]
        [StringLength(200)]
        public string Quote { get; set; }

        [Required]
        [StringLength(80)]
        public string Author { get; set; }

        public QuoteCuDto ToDto(Guid friendId) => new()
        {
            Quote = Quote,
            Author = Author,
            FriendsId = new List<Guid> { friendId }
        };
    }
}
