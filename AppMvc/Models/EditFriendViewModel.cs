using System.ComponentModel.DataAnnotations;

using Models.DTO;

namespace AppMvc.Models;

public class EditFriendViewModel
{
    public Guid Id { get; set; }

    public FriendInputModel Input { get; set; } = new();

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
