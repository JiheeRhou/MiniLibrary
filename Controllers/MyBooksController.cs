using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniLibrary.Data;
using MiniLibrary.Models;
using System.Security.Claims;
using static MiniLibrary.Models.Book;

namespace MiniLibrary.Controllers
{
    [Authorize()]
    public class MyBooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MyBooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Checkouts
        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var checkoutBookIds = await _context.Checkouts
                .Where(checkout => checkout.IsReturn == false && checkout.UserId == userId)
                .Select(checkout => checkout.BookId)
                .ToListAsync();

            var myBooks = await _context.Books
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Where(book => checkoutBookIds.Contains(book.Id))
                .ToListAsync();

            return View(myBooks);
        }

        // GET: Checkouts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
            {
                return NotFound();
            }

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
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(14)
            };

            if (ModelState.IsValid)
            {
                _context.Add(checkout);
                await _context.SaveChangesAsync();

                book.IsAvailable = false;
                await _context.SaveChangesAsync();

                return RedirectToAction("MyBooks", "Checkouts");
            }

            return View(book);
        }

        public async Task<IActionResult> MyBooks()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var checkout = _context.Checkouts
                .Include(checkout => checkout.User)
                .Include(checkout => checkout.Book)
                .ThenInclude(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .FirstOrDefaultAsync(checkout => checkout.IsReturn == false && checkout.UserId == userId);

            return View(MyBooks);
        }
    }
}