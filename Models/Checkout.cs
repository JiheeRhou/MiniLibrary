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
        public DateTime StartDate { get; set; }

        [Required()]
        [Display(Name = "Dudate")]
        public DateTime EndDate { get; set; }

        [Required(), DefaultValue(false)]
        [Display(Name = "Return")]
        public bool IsReturn { get; set; }

        public User? User { get; set; }

        public Book? Book { get; set; }
    }
}
