using Models.Interfaces;

namespace Models;

public class Pet : IPet
{
    public Guid PetId { get; set; }
    public AnimalKind Kind { get; set; }
    public AnimalMood Mood { get; set; }
    public string Name { get; set; }

    public IFriend Friend { get; set; }
}
