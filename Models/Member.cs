using System.ComponentModel.DataAnnotations;

namespace MiniLibrary.Models
{
    public class Member
    {
        public int Id { get; set; }

        [Required()]
        public string UserId { get; set; }

        [Required()]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime RegisterDate { get; set; }

        [Required()]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ExpiredDate { get; set; }

        public bool Active { get; set; } = true;

        public User? User { get; set; }

        public List<PaymentHistory>? PaymentHistory { get; set; }
    }
}
