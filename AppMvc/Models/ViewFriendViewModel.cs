using Models.Interfaces;

namespace AppMvc.Models;

public class ViewFriendViewModel
{
    public IFriend Friend { get; set; }

    public Guid Id { get; set; }

    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }

    // Used by delete modal posts
    public Guid PetId { get; set; }
    public Guid QuoteId { get; set; }
}
