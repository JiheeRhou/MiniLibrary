using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniLibrary.Data;
using MiniLibrary.Models;
using static MiniLibrary.Models.Book;

namespace MiniLibrary.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Books
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Books/Details/5
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

            var checkout = await _context.Checkouts
                .Where(c => c.BookId == book.Id && c.IsReturn == false)
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefaultAsync();

            var status = "Avaiable";
            var reserved = "";
            if (checkout != null && !checkout.IsReturn)
            {
                status = "on Load";
            }
            else if (book.ReserveUserId != null)
            {
                status = "Reserved";
            }

            if (book.ReserveUserId != null)
            {
                User user = await _context.Users.Where(u => u.Id == book.ReserveUserId).FirstOrDefaultAsync();
                if(user != null)
                {
                    reserved = user.Email;
                }
            }

            ViewData["Status"] = status;
            ViewData["Reserved"] = reserved;
            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["PublisherId"] = new SelectList(_context.Publishers, "Id", "Name");
            ViewData["Genre"] = new SelectList(Enum.GetValues(typeof(BookGenre)));
            ViewData["Authors"] = new SelectList(_context.Authors, "Id", "Name");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Genre,Description,PublisherId,PublishedDate,ISBN,Pages,IsAvailable,Photo")] Book book, List<int> BookAuthors, IFormFile? Photo)
        {
            if (ModelState.IsValid)
            {
                book.Photo = await UploadPhoto(Photo);

                _context.Add(book);
                await _context.SaveChangesAsync();

                if (BookAuthors != null && BookAuthors.Count > 0)
                {
                    foreach (int author in BookAuthors)
                    {
                        var bookAuthor = new BookAuthor
                        {
                            BookId = book.Id,
                            AuthorId = author
                        };
                        _context.Add(bookAuthor);
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["PublisherId"] = new SelectList(_context.Publishers, "Id", "Name", book.PublisherId);
            ViewData["Genre"] = new SelectList(Enum.GetValues(typeof(BookGenre)));
            ViewData["Authors"] = new SelectList(_context.Authors, "Id", "Name");
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["PublisherId"] = new SelectList(_context.Publishers, "Id", "Name", book.PublisherId);
            ViewData["Genre"] = new SelectList(Enum.GetValues(typeof(BookGenre)));
            ViewData["Authors"] = new SelectList(_context.Authors, "Id", "Name");
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Genre,Description,PublisherId,PublishedDate,ISBN,Pages,IsAvailable,ReserveUserId,Photo")] Book book, List<int> BookAuthors, IFormFile? Photo)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    book.Photo = await UploadPhoto(Photo);

                    _context.Update(book);
                    await _context.SaveChangesAsync();

                    var existingBookAuthors = _context.BookAuthors.Where(ba => ba.BookId == book.Id);
                    _context.BookAuthors.RemoveRange(existingBookAuthors);

                    if (BookAuthors != null && BookAuthors.Count > 0)
                    {
                        foreach (int authorId in BookAuthors)
                        {
                            var bookAuthor = new BookAuthor
                            {
                                BookId = book.Id,
                                AuthorId = authorId
                            };
                            _context.Add(bookAuthor);
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PublisherId"] = new SelectList(_context.Publishers, "Id", "Name", book.PublisherId);
            ViewData["Genre"] = new SelectList(Enum.GetValues(typeof(BookGenre)));
            ViewData["Authors"] = new SelectList(_context.Authors.ToList(), "Id", "Name");
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Books == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Books'  is null.");
            }
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
          return (_context.Books?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private async Task<string> UploadPhoto(IFormFile Photo)
        {
            if (Photo != null)
            {
                // Get temp location
                var filePath = Path.GetTempFileName();

                // Create a unique name so we don't overwrite any existing photos
                // eg: photo.jpg => abcdefg123456890-photo.jpg (Using the Globally Unique Identifier (GUID))
                var fileName = Guid.NewGuid() + "-" + Photo.FileName;

                // Set destination path dynamically so it works on any system (double slashes escape the characters)
                var uploadPath = Directory.GetCurrentDirectory() + "\\wwwroot\\img\\books\\" + fileName;

                // Execute the file copy
                using var stream = new FileStream(uploadPath, FileMode.Create);
                await Photo.CopyToAsync(stream);

                // Set the Photo property name of the new Product object
                return fileName;
            }

            return null;
        }
    }
}
