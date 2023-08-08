using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniLibrary.Data;
using MiniLibrary.Models;

namespace MiniLibrary.Controllers
{
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Members
        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var memberHistory = await _context.Members
                                .Where(m => m.UserId == userId)
                                .ToListAsync();

            var isMember = false;
            foreach (Member memberRegister in memberHistory)
            {
                if (memberRegister.ExpiredDate < DateTime.Today)
                {
                    isMember = true;
                    break;
                }
            }

            ViewData["IsMember"] = isMember;
            return View(memberHistory);
        }

        // GET: Members/Purchase
        public async Task<IActionResult> Purchase()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return NotFound();
            }

            var membership = await _context.Members
                .Include(m => m.User)
                .Where(m => m.UserId == userId)
                .FirstOrDefaultAsync();

            if (membership == null)
            {
                membership = new Member
                {
                    User = await _context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync(),
                    RegisterDate = DateTime.Today,
                    ExpiredDate = DateTime.Today,
                };
            }

            if (membership.ExpiredDate > DateTime.Today)
            {
                membership.RegisterDate = membership.ExpiredDate.AddDays(1);
            }
            else
            {
                membership.RegisterDate = DateTime.Today;
            }

            membership.ExpiredDate = membership.RegisterDate.AddYears(1);
            ViewData["PaymentMethods"] = new SelectList(Enum.GetValues(typeof(PaymentMethods)));

            return View(membership);
        }

        // POST: Members/Purchase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment([Bind("Id,UserId,Fee,RegisterDate,ExpiredDate")] Member member)
        {

            return View(member);
        }

        private bool MemberExists(int id)
        {
          return (_context.Members?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
