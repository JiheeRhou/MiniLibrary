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

    public class PaymentHistory
    {
        public int Id { get; set; }

        [Required()]
        public int MemberId { get; set; } = 0;

        [Required()]
        public decimal Fee { get; set; }

        [Required()]
        public PaymentMethods PaymentMethod { get; set; }

        [Required()]
        public DateTime PurchaseDate { get; set; }

        public Member? Member { get; set; }

    }
}
