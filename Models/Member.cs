using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace MiniLibrary.Models
{
    public class Member
    {
        public int Id { get; set; }

        [Required()]
        [Display(Name = "User Name")]
        public string UserId { get; set; }

        [Required()]
        public DateTime RegisterDate { get; set; }

        [Required()]
        public DateTime ExpiredDate { get; set; }
    }
}
