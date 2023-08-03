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
            new MenuItem { Controller = "Books", Action = "Index", Label = "Books", DropdownItems = new List<MenuItem> {
                new MenuItem { Controller = "Books", Action = "Index", Label = "List" },
                new MenuItem { Controller = "Books", Action = "Create", Label = "Create" },
            }, Authorized = true, AllowedRoles = new List<string> { "Administrator" } },
            new MenuItem { Controller = "Checkouts", Action = "Index", Label = "Books" , Authorized = true, AllowedRoles = new List<string> { "Member", "User" } },
            new MenuItem { Controller = "Authors", Action = "Index", Label = "Authors", DropdownItems = new List<MenuItem> {
                new MenuItem { Controller = "Authors", Action = "Index", Label = "List" },
                new MenuItem { Controller = "Authors", Action = "Create", Label = "Create" },
            }, Authorized = true, AllowedRoles = new List<string> { "Administrator" } },
            new MenuItem { Controller = "Publishers", Action = "Index", Label = "Publishers", DropdownItems = new List<MenuItem> {
                new MenuItem { Controller = "Publishers", Action = "Index", Label = "List" },
                new MenuItem { Controller = "Publishers", Action = "Create", Label = "Create" },
            }, Authorized = true, AllowedRoles = new List<string> { "Administrator" } },
            new MenuItem { Controller = "Home", Action = "About", Label = "About" },
            new MenuItem { Controller = "Home", Action = "Contact", Label = "Contact" },
            new MenuItem { Controller = "Home", Action = "Privacy", Label = "Privacy" },
        };

            return View(menuItems);
        }
    }
}
