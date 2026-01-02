using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTO;
using Services;

namespace AppRazor.Pages;

public class EditAddressModel : PageModel
{
    private readonly IAddressesService _addressesService;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FriendId { get; set; }

    [BindProperty]
    public AddressInputModel Input { get; set; } = new();

    public EditAddressModel(IAddressesService addressesService)
    {
        _addressesService = addressesService;
    }

    public async Task<IActionResult> OnGet()
    {
        if (Id == Guid.Empty) return NotFound();

        var dto = await _addressesService.ReadAddressCuDtoAsync(Id);
        Input = AddressInputModel.FromDto(dto);
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (Id == Guid.Empty) return NotFound();
        if (!ModelState.IsValid) return Page();

        // Preserve relationships that are not edited on this form.
        // The WebApi address update DTO includes FriendsId; sending null/empty can detach relations.
        var existing = await _addressesService.ReadAddressCuDtoAsync(Id);
        var dto = Input.ToDto(Id);
        dto.FriendsId ??= existing?.FriendsId;
        try
        {
            await _addressesService.UpdateAddressAsync(Id, dto);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            var msg = ex.Message?.Trim().Trim('"') ?? "Bad request";
            if (msg.Contains("StreetAddress", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Input.StreetAddress", msg);
            }
            else if (msg.Contains("ZipCode", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Input.ZipCode", msg);
            }
            else if (msg.Contains("City", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Input.City", msg);
            }
            else if (msg.Contains("Country", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Input.Country", msg);
            }
            else
            {
                ModelState.AddModelError(string.Empty, msg);
            }

            return Page();
        }

        if (FriendId != Guid.Empty)
            return Redirect($"~/Members/ViewFriend?id={FriendId}");

        return Redirect($"~/Members/ListOfFriends");
    }

    public class AddressInputModel
    {
        [Required]
        [RegularExpression("^[a-zA-Z0-9 ]+$", ErrorMessage = "StreetAddress can only contain letters (a-z), numbers (0-9), and spaces.")]
        [StringLength(100)]
        public string StreetAddress { get; set; }

        [Range(0, 99999)]
        public int ZipCode { get; set; }

        [Required]
        [StringLength(80)]
        public string City { get; set; }

        [Required]
        [StringLength(80)]
        public string Country { get; set; }

        public static AddressInputModel FromDto(AddressCuDto dto) => new()
        {
            StreetAddress = dto.StreetAddress,
            ZipCode = dto.ZipCode,
            City = dto.City,
            Country = dto.Country
        };

        public AddressCuDto ToDto(Guid addressId) => new()
        {
            AddressId = addressId,
            StreetAddress = StreetAddress,
            ZipCode = ZipCode,
            City = City,
            Country = Country
        };
    }
}
