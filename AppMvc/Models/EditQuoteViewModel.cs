using System.ComponentModel.DataAnnotations;

using Models.DTO;

namespace AppMvc.Models;

public class EditQuoteViewModel
{
    public Guid Id { get; set; }
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

        public static QuoteInputModel FromDto(QuoteCuDto dto) => new()
        {
            Quote = dto.Quote,
            Author = dto.Author
        };

        public QuoteCuDto ToDto(Guid quoteId) => new()
        {
            QuoteId = quoteId,
            Quote = Quote,
            Author = Author
        };
    }
}
