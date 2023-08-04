using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace MiniLibrary.Models
{
    public class Author
    {
        public int Id { get; set; }

        [Required(), MaxLength(150)]
        [Display(Name = "Author")]
        public string Name { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? BOD { get; set; }

        public List<BookAuthor>? BookAuthors { get; set; }
    }
}
