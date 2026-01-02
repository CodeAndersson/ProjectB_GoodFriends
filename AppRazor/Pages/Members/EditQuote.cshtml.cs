using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTO;
using Services;

namespace AppRazor.Pages;

public class EditQuoteModel : PageModel
{
    private readonly IQuotesService _quotesService;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FriendId { get; set; }

    [BindProperty]
    public QuoteInputModel Input { get; set; } = new();

    public EditQuoteModel(IQuotesService quotesService)
    {
        _quotesService = quotesService;
    }

    public async Task<IActionResult> OnGet()
    {
        if (Id == Guid.Empty) return NotFound();

        var dto = await _quotesService.ReadQuoteCuDtoAsync(Id);
        if (dto is null) return NotFound();

        Input = QuoteInputModel.FromDto(dto);
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (Id == Guid.Empty) return NotFound();
        if (!ModelState.IsValid) return Page();

        var existing = await _quotesService.ReadQuoteCuDtoAsync(Id);
        if (existing is null) return NotFound();

        var dto = Input.ToDto(Id);
        dto.FriendsId ??= existing.FriendsId; // preserve relations

        try
        {
            await _quotesService.UpdateQuoteAsync(Id, dto);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            ModelState.AddModelError(string.Empty, ex.Message?.Trim().Trim('"') ?? "Bad request");
            return Page();
        }

        // Prefer returning to the friend the user came from (if provided)
        var backId = FriendId != Guid.Empty
            ? FriendId
            : (existing.FriendsId?.FirstOrDefault() ?? Guid.Empty);

        if (backId == Guid.Empty) return Redirect("~/Members/ListOfFriends");
        return Redirect($"~/Members/ViewFriend?id={backId}");
    }

    public class QuoteInputModel
    {
        [Required]
        [StringLength(200)]
        public string Quote { get; set; }

        [Required]
        [StringLength(80)]
        public string Author { get; set; }

        public static QuoteInputModel FromDto(QuoteCuDto dto) => new()
        {
            Quote = dto.Quote,
            Author = dto.Author
        };

        public QuoteCuDto ToDto(Guid quoteId) => new()
        {
            QuoteId = quoteId,
            Quote = Quote,
            Author = Author
        };
    }
}
