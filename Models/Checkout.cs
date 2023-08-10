using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Xml.Linq;

namespace MiniLibrary.Models
{
    public class Checkout
    {
        public int Id { get; set; }

        [Required()]
        [Display(Name = "User Name")]
        public string UserId { get; set; }

        [Required()]
        [Display(Name = "Book Title")]
        public int BookId { get; set; }

        [Required()]
        [Display(Name = "Checkout Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Required()]
        [Display(Name = "Duedate")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Return Date")]
        [StringLength(10)]
        public string? Return { get; set; }

        public User? User { get; set; }

        public Book? Book { get; set; }
    }
}
