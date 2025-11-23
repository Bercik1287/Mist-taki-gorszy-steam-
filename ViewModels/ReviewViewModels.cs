using System.ComponentModel.DataAnnotations;

namespace mist.ViewModels
{
    public class CreateReviewViewModel
    {
        public int GameId { get; set; }

        [Required(ErrorMessage = "Ocena jest wymagana")]
        [Range(1, 5, ErrorMessage = "Ocena musi być między 1 a 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Tytuł recenzji jest wymagany")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tytuł musi mieć 3-100 znaków")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Treść recenzji jest wymagana")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Recenzja musi mieć 10-1000 znaków")]
        public string Content { get; set; }
    }

    public class EditReviewViewModel
    {
        public int Id { get; set; }
        public int GameId { get; set; }

        [Required(ErrorMessage = "Ocena jest wymagana")]
        [Range(1, 5, ErrorMessage = "Ocena musi być między 1 a 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Tytuł recenzji jest wymagany")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tytuł musi mieć 3-100 znaków")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Treść recenzji jest wymagana")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Recenzja musi mieć 10-1000 znaków")]
        public string Content { get; set; }
    }
}