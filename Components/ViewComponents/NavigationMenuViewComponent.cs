using Microsoft.AspNetCore.Mvc;
using MiniLibrary.Models;

namespace MiniLibrary.Components.ViewComponents
{
    [ViewComponent(Name = "NavigationMenu")]
    public class NavigationMenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var menuItems = new List<MenuItem>
        {
            new MenuItem { Controller = "Home", Action = "Index", Label = "Home" },
            new MenuItem { Controller = "Members", Action = "Index", Label = "Admin", DropdownItems = new List<MenuItem> {
                new MenuItem { Controller = "Books", Action = "Index", Label = "Books" },
                new MenuItem { Controller = "Authors", Action = "Index", Label = "Authors" },
                new MenuItem { Controller = "Publishers", Action = "Index", Label = "Publishers" },
                new MenuItem { Controller = "Members", Action = "Index", Label = "Memberships" },
                new MenuItem { Controller = "Books", Action = "OverdueBooks", Label = "Overdue Books" },
            }, Authorized = true, AllowedRoles = new List<string> { "Administrator" } },
            new MenuItem { Controller = "Checkouts", Action = "Index", Label = "Books" , Authorized = true, AllowedRoles = new List<string> { "Member" } },
            new MenuItem { Controller = "MyBooks", Action = "Index", Label = "My Library", DropdownItems = new List<MenuItem> {
                new MenuItem { Controller = "MyBooks", Action = "CheckoutList", Label = "My Checkout Books" },
                new MenuItem { Controller = "MyBooks", Action = "ReservedList", Label = "My Reserved Books" },
                new MenuItem { Controller = "Members", Action = "Membership", Label = "Membership" },
            }, Authorized = true, AllowedRoles = new List<string> { "Member" } },
            new MenuItem { Controller = "Home", Action = "About", Label = "About" },
            new MenuItem { Controller = "Home", Action = "Contact", Label = "Contact" },
            new MenuItem { Controller = "Home", Action = "Privacy", Label = "Privacy" },
        };

            return View(menuItems);
        }
    }
}
