using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Models.Interfaces;

namespace DbModels;

[Table("Pets", Schema = "supusr")]
public class PetDbM
{
    [Key]
    public Guid PetId { get; set; }

    public Guid FriendId { get; set; }

    [Required]
    public string Name { get; set; }

    // Readability columns in DB (see migrations)
    public string strKind
    {
        get => Kind.ToString();
        set { }
    }

    public string strMood
    {
        get => Mood.ToString();
        set { }
    }

    public AnimalKind Kind { get; set; }

    public AnimalMood Mood { get; set; }

    public bool Seeded { get; set; }

    [JsonIgnore]
    public virtual FriendDbM FriendDbM { get; set; }
}
