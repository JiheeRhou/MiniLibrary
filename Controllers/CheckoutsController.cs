using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniLibrary.Data;
using MiniLibrary.Models;
using System.Security.Claims;

namespace MiniLibrary.Controllers
{
    public class CheckoutsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckoutsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Checkouts
        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var checkoutUser = false;
            var reservedUser = false;
            
            var books = await _context.Books
                .Include(b => b.Checkouts)
                .Include(b => b.Publisher)
                .Include(b => b.Author)
                .ToListAsync();

            foreach (var book in books)
            {
                var checkout = await _context
                    .Checkouts.Where(c => c.BookId == book.Id && c.Return == null)
                    .OrderByDescending(c => c.StartDate)
                    .FirstOrDefaultAsync();

                if (checkout != null)
                {
                    if (userId == checkout.UserId)
                    {
                        checkoutUser = true;
                    }
                } else if (userId == book.ReserveUserId)
                {
                    reservedUser = true;
                }
            }

            var member = await _context.Members
                .Where(m => m.UserId == userId && m.Active == true)
                .FirstOrDefaultAsync();

            var active = false;
            if (member != null)
            {
                if (member.ExpiredDate < DateTime.Today)
                {
                    member.Active = false;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    active = true;
                }
            }

            ViewData["CheckoutUser"] = checkoutUser;
            ViewData["ReservedUser"] = reservedUser;
            ViewData["Active"] = active;
            return View(books);
        }

        // GET: Checkouts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var checkoutUser = false;
            var reservedUser = false;

            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Publisher)
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            var checkout = await _context.Checkouts
                .Where(c => c.BookId == book.Id && c.Return == null)
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefaultAsync();

            var status = "Avaiable";
            if(checkout != null && checkout.Return == null)
            {
                status = "on Load";

                if (userId == checkout.UserId)
                {
                    checkoutUser = true;
                }
            }
            else if(book.ReserveUserId != null)
            {
                status = "Reserved";

                if (userId == book.ReserveUserId)
                {
                    reservedUser = true;
                }
            }

            var member = await _context.Members
                .Where(m => m.UserId == userId && m.Active == true)
                .FirstOrDefaultAsync();

            var active = false;
            if (member != null)
            {
                if (member.ExpiredDate < DateTime.Today)
                {
                    member.Active = false;
                    await _context.SaveChangesAsync();
                }
                else
                {
                    active = true;
                }
            }

            ViewData["Status"] = status;
            ViewData["CheckoutUser"] = checkoutUser;
            ViewData["ReservedUser"] = reservedUser;
            ViewData["Active"] = active;
            return View(book);
        }

        public async Task<IActionResult> Checkout(int? id)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var book = await _context.Books.FirstOrDefaultAsync(book => book.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            var checkout = new Checkout
            {
                UserId = userId,
                BookId = book.Id,
                StartDate = DateTime.Today.Date,
                EndDate = DateTime.Today.Date.AddDays(14)
            };

            if (ModelState.IsValid)
            {
                _context.Add(checkout);
                await _context.SaveChangesAsync();

                book.IsAvailable = false;
                await _context.SaveChangesAsync();

                return RedirectToAction("CheckoutList", "MyBooks");
            }

            return View(book);
        }

        public async Task<IActionResult> Return(int? id)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var checkout = await _context.Checkouts.FirstOrDefaultAsync(checkout => checkout.Id == id);

            if (checkout == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                checkout.Return = DateTime.Today.ToString("yyyy-MM-dd");
                _context.Add(checkout);
                await _context.SaveChangesAsync();

                var book = await _context.Books.FirstOrDefaultAsync(book => book.Id == checkout.BookId);

                if (book != null && book.ReserveUserId == null)
                {
                    book.IsAvailable = false;
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("CheckoutList", "MyBooks");
            }

            return View(checkout);
        }

        public async Task<IActionResult> Reserve(int? id)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var book = await _context.Books.FirstOrDefaultAsync(book => book.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                book.ReserveUserId = userId;
                await _context.SaveChangesAsync();

                return RedirectToAction("ReservedList", "MyBooks");
            }

            return View(book);
        }
    }
}