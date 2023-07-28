using Microsoft.AspNetCore.Identity;

namespace MiniLibrary.Models
{
    public class User : IdentityUser
    {
        public List<Book>? Books { get; set; }

        public List<Checkout>? Checkouts { get; set; }
    }
}
