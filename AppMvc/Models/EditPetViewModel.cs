using System.ComponentModel.DataAnnotations;

using Models.DTO;
using Models.Interfaces;

namespace AppMvc.Models;

public class EditPetViewModel
{
    public Guid Id { get; set; }
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
