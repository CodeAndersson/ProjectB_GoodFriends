using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTO;
using Models.Interfaces;
using Services;

namespace AppRazor.Pages;

public class CreatePetModel : PageModel
{
    private readonly IPetsService _petsService;

    [BindProperty(SupportsGet = true)]
    public Guid FriendId { get; set; }

    [BindProperty]
    public PetInputModel Input { get; set; } = new();

    public CreatePetModel(IPetsService petsService)
    {
        _petsService = petsService;
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

        var dto = Input.ToDto(FriendId);

        try
        {
            await _petsService.CreatePetAsync(dto);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            ModelState.AddModelError(string.Empty, ex.Message?.Trim().Trim('"') ?? "Bad request");
            return Page();
        }

        return Redirect($"~/Members/ViewFriend?id={FriendId}");
    }

    public class PetInputModel
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        public AnimalKind Kind { get; set; }

        [Required]
        public AnimalMood Mood { get; set; }

        public PetCuDto ToDto(Guid friendId) => new()
        {
            FriendId = friendId,
            Name = Name,
            Kind = Kind,
            Mood = Mood
        };
    }
}
