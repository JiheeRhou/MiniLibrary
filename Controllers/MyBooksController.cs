using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniLibrary.Data;
using MiniLibrary.Models;
using System.Security.Claims;

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

        [Authorize]
        public async Task<IActionResult> CheckoutList()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var checkoutBookIds = await _context.Checkouts
                .Where(c => c.UserId == userId)
                .Select(c => c.BookId)
                .ToListAsync();

            var myBooks = await _context.Books
                .Include(b => b.Checkouts.Where(c => c.UserId == userId))
                .Include(b => b.Publisher)
                .Include(b => b.Author)
                .Where(b => checkoutBookIds.Contains(b.Id))
                .ToListAsync();

            return View(myBooks);
        }

        [Authorize]
        public async Task<IActionResult> Return(int? id)
        {
            var checkout = await _context.Checkouts.FirstOrDefaultAsync(c => c.Id == id);

            if (checkout == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                checkout.Return = DateTime.Today.ToString("yyyy-MM-dd");
                await _context.SaveChangesAsync();

                var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == checkout.BookId);

                if (book == null)
                {
                    return NotFound();
                }

                if (book.ReserveUserId == null)
                {
                    book.IsAvailable = true;
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("CheckoutList", "MyBooks");
            }

            return View(checkout);
        }

        [Authorize]
        public async Task<IActionResult> ReservedList()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var myBooks = await _context.Books
                .Include(b => b.Checkouts.OrderByDescending(c => c.Id))
                .Include(b => b.Publisher)
                .Include(b => b.Author)
                .Where(book => book.ReserveUserId == userId)
                .ToListAsync();

            return View(myBooks);
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

                book.ReserveUserId = null;
                book.IsAvailable = false;
                await _context.SaveChangesAsync();

                return RedirectToAction("CheckoutList", "MyBooks");
            }

            return View(book);
        }

        public async Task<IActionResult> CancelReserve(int? id)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var book = await _context.Books.FirstOrDefaultAsync(book => book.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                book.ReserveUserId = null;
                await _context.SaveChangesAsync();

                var checkout = _context.Checkouts
                    .FirstOrDefaultAsync(c => c.BookId == book.Id && c.Return == null);
                if (checkout == null)
                {
                    book.IsAvailable = true;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("ReservedList", "MyBooks");
            }

            return View(book);
        }

        public async Task<IActionResult> Details(int? id, string? previous)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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
            var checkoutUser = false;
            var reservedUser = false;
            if (checkout != null && checkout.Return == null)
            {
                status = "on Load";

                if (userId == checkout.UserId)
                {
                    checkoutUser = true;
                }
            }
            else if (book.ReserveUserId != null)
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
            ViewData["Previous"] = previous;
            ViewData["Active"] = active;
            return View(book);
        }

    }
}