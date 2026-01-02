using System.ComponentModel.DataAnnotations;

using Models.DTO;

namespace AppMvc.Models;

public class EditAddressViewModel
{
    public Guid Id { get; set; }
    public Guid FriendId { get; set; }

    public AddressInputModel Input { get; set; } = new();

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
