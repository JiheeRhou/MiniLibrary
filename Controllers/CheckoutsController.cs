using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniLibrary.Data;
using MiniLibrary.Models;
using System.Linq;
using System.Security.Claims;
using static MiniLibrary.Models.Book;

namespace MiniLibrary.Controllers
{
    [Authorize()]
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

            var books = await _context.Books
                .Include(b => b.Checkouts)
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .ToListAsync();

            foreach (var book in books)
            {
                var checkout = await _context.Checkouts.Where(c => c.BookId == book.Id && c.IsReturn == false).OrderByDescending(c => c.StartDate).FirstOrDefaultAsync();

                if (checkout != null)
                {
                    var checkoutUserId = checkout.UserId;
                    if (userId == checkoutUserId)
                    {
                        book.User = new User();
                        book.User.Id = checkoutUserId;
                    }
                } else if (userId == book.ReserveUserId)
                {
                    book.User = new User();
                    book.User.Id = book.ReserveUserId;
                }
            }

            return View(books);
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
                StartDate = DateTime.Today.Date,
                EndDate = DateTime.Today.Date.AddDays(14)
            };

            if (ModelState.IsValid)
            {
                _context.Add(checkout);
                await _context.SaveChangesAsync();

                book.IsAvailable = false;
                await _context.SaveChangesAsync();

                return RedirectToAction("Checkout", "MyBooks");
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
                checkout.IsReturn = true;
                _context.Add(checkout);
                await _context.SaveChangesAsync();

                var book = await _context.Books.FirstOrDefaultAsync(book => book.Id == checkout.BookId);

                if (book != null && book.ReserveUserId == null)
                {
                    book.IsAvailable = false;
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Checkout", "MyBooks");
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

                return RedirectToAction("Reserved", "MyBooks");
            }

            return View(book);
        }
    }
}