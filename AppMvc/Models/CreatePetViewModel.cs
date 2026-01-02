using System.ComponentModel.DataAnnotations;

using Models.DTO;
using Models.Interfaces;

namespace AppMvc.Models;

public class CreatePetViewModel
{
    public Guid FriendId { get; set; }

    public PetInputModel Input { get; set; } = new();

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
