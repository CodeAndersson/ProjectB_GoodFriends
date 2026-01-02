using System.ComponentModel.DataAnnotations;

using Models.DTO;

namespace AppMvc.Models;

public class CreateQuoteViewModel
{
    public Guid FriendId { get; set; }

    public QuoteInputModel Input { get; set; } = new();

    public class QuoteInputModel
    {
        [Required]
        [StringLength(200)]
        public string Quote { get; set; }

        [Required]
        [StringLength(80)]
        public string Author { get; set; }

        public QuoteCuDto ToDto(Guid friendId) => new()
        {
            Quote = Quote,
            Author = Author
            ,FriendsId = new List<Guid> { friendId }
        };
    }
}
