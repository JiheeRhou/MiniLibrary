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

        public async Task<IActionResult> checkout()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var checkoutBookIds = await _context.Checkouts
                .Where(checkout => checkout.IsReturn == false && checkout.UserId == userId)
                .Select(checkout => checkout.BookId)
                .ToListAsync();

            var myBooks = await _context.Books
                .Include(b => b.Checkouts)
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Where(book => checkoutBookIds.Contains(book.Id))
                .ToListAsync();

            return View(myBooks);
        }

        public async Task<IActionResult> reserved()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var myBooks = await _context.Books
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Where(book => book.ReserveUserId == userId)
                .ToListAsync();

            return View(myBooks);
        }

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

    }
}