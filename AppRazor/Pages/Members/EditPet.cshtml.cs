using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTO;
using Models.Interfaces;
using Services;

namespace AppRazor.Pages;

public class EditPetModel : PageModel
{
    private readonly IPetsService _petsService;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FriendId { get; set; }

    [BindProperty]
    public PetInputModel Input { get; set; } = new();

    public EditPetModel(IPetsService petsService)
    {
        _petsService = petsService;
    }

    public async Task<IActionResult> OnGet()
    {
        if (Id == Guid.Empty) return NotFound();

        var dto = await _petsService.ReadPetCuDtoAsync(Id);
        if (dto is null) return NotFound();

        FriendId = FriendId != Guid.Empty ? FriendId : dto.FriendId;
        Input = PetInputModel.FromDto(dto);
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (Id == Guid.Empty) return NotFound();
        if (!ModelState.IsValid) return Page();

        var existing = await _petsService.ReadPetCuDtoAsync(Id);
        if (existing is null) return NotFound();

        var dto = Input.ToDto(Id);
        dto.FriendId = existing.FriendId; // preserve relationship

        try
        {
            await _petsService.UpdatePetAsync(Id, dto);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            ModelState.AddModelError(string.Empty, ex.Message?.Trim().Trim('"') ?? "Bad request");
            return Page();
        }

        var backId = FriendId != Guid.Empty ? FriendId : existing.FriendId;
        return Redirect($"~/Members/ViewFriend?id={backId}");
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

        public static PetInputModel FromDto(PetCuDto dto) => new()
        {
            Name = dto.Name,
            Kind = dto.Kind,
            Mood = dto.Mood
        };

        public PetCuDto ToDto(Guid petId) => new()
        {
            PetId = petId,
            Name = Name,
            Kind = Kind,
            Mood = Mood
        };
    }
}
