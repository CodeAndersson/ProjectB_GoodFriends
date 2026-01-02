using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTO;
using Services;

namespace AppRazor.Pages;

public class EditFriendModel : PageModel
{
    private readonly IFriendsService _friendsService;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public FriendInputModel Input { get; set; } = new();

    public EditFriendModel(IFriendsService friendsService)
    {
        _friendsService = friendsService;
    }

    public async Task<IActionResult> OnGet()
    {
        if (Id == Guid.Empty) return NotFound();

        var dto = await _friendsService.ReadFriendCuDtoAsync(Id);
        Input = FriendInputModel.FromDto(dto);
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (Id == Guid.Empty) return NotFound();
        if (!ModelState.IsValid) return Page();

        // Preserve relationships that are not edited on this form.
        // The WebApi update DTO includes PetsId/QuotesId/AddressId; sending null/empty can clear relations.
        var existing = await _friendsService.ReadFriendCuDtoAsync(Id);
        var dto = Input.ToDto(Id);
        dto.AddressId ??= existing?.AddressId;
        dto.PetsId ??= existing?.PetsId;
        dto.QuotesId ??= existing?.QuotesId;
        try
        {
            await _friendsService.UpdateFriendAsync(Id, dto);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            var msg = ex.Message?.Trim().Trim('"') ?? "Bad request";
            if (msg.Contains("FirstName", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Input.FirstName", msg);
            }
            else if (msg.Contains("LastName", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Input.LastName", msg);
            }
            else if (msg.Contains("Email", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Input.Email", msg);
            }
            else if (msg.Contains("Birthday", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Input.Birthday", msg);
            }
            else
            {
                ModelState.AddModelError(string.Empty, msg);
            }

            return Page();
        }

        return Redirect($"~/Members/ViewFriend?id={Id}");
    }

    public class FriendInputModel
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public DateTime? Birthday { get; set; }

        public Guid? AddressId { get; set; }

        public static FriendInputModel FromDto(FriendCuDto dto) => new()
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Birthday = dto.Birthday,
            AddressId = dto.AddressId
        };

        public FriendCuDto ToDto(Guid friendId) => new()
        {
            FriendId = friendId,
            FirstName = FirstName,
            LastName = LastName,
            Email = Email,
            Birthday = Birthday,
            AddressId = AddressId
        };
    }
}
