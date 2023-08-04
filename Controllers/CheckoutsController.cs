using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniLibrary.Data;

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
            var applicationDbContext = _context.Books
                .Include(b => b.Publisher)
                .Include(b => b.Checkouts)
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author);
            return View(await applicationDbContext.ToListAsync());
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

        private bool CheckoutExists(int id)
        {
          return (_context.Checkouts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
