using System.ComponentModel.DataAnnotations;

namespace MiniLibrary.Models
{
    public enum PaymentMethods
    {
        VISA,
        Mastercard,
        InteracDebit,
        PayPal,
        Stripe
    }

    public class Member
    {
        public int Id { get; set; }

        [Required()]
        public string UserId { get; set; }

        [Required()]
        public decimal Fee { get; set; }

        [Required()]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime RegisterDate { get; set; }

        [Required()]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ExpiredDate { get; set; }

        public PaymentMethods PaymentMethod { get; set; }

        [Required()]
        public User User { get; set; }
    }
}
