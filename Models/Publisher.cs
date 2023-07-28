using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace MiniLibrary.Models
{
    public class Publisher
    {
        public int Id { get; set; }

        [Required()]
        [Display(Name = "Publisher")]
        public string Name { get; set; }

        public List<Book>? Books { get; set; }
    }
}
