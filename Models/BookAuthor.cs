using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace MiniLibrary.Models
{
    public class BookAuthor
    {
        public int Id { get; set; }

        [Required()]
        [Display(Name = "Book Title")]
        public int BookId { get; set; }

        [Required()]
        [Display(Name = "Author")]
        public int AuthorId { get; set; }

        public Book? Book { get; set; }

        public Author? Author { get; set; }
    }
}
