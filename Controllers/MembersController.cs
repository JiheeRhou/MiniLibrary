using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniLibrary.Data;
using MiniLibrary.Models;
using Stripe;
using Stripe.Checkout;

namespace MiniLibrary.Controllers
{
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IConfiguration _configuration;

        //Constructor
        public MembersController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Members
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var members = await _context.Members
                .Include(m => m.User)
                .ToListAsync();

            return View(members);
        }

        // GET: Members/Membership
        [Authorize]
        public async Task<IActionResult> Membership()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var memberHistories = await _context.Members
                                .Where(m => m.UserId == userId)
                                .OrderByDescending(m => m.Id)
                                .ToListAsync();

            var isMember = false;
            foreach (Member memberHistory in memberHistories)
            {
                if (memberHistory.Active == true)
                {
                    if (memberHistory.ExpiredDate < DateTime.Today)
                    {
                        memberHistory.Active = false;
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        isMember = true;
                        break;
                    }
                }
            }

            ViewData["IsMember"] = isMember;
            return View(memberHistories);
        }

        [Authorize]
        public async Task<IActionResult> Purchase()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return NotFound();
            }

            var membership = await _context.Members
                .Include(m => m.User)
                .Include(m => m.PaymentHistory)
                .Where(m => m.UserId == userId && m.Active == true)
                .FirstOrDefaultAsync();

            if (membership == null)
            {
                membership = new Member
                {
                    UserId = userId,
                    RegisterDate = DateTime.Today,
                    ExpiredDate = DateTime.Today.AddYears(1),
                };
            }
            else
            {
                membership.RegisterDate = membership.ExpiredDate;
                membership.ExpiredDate = membership.RegisterDate.AddYears(1);
            }

            ViewData["PaymentMethods"] = new SelectList(Enum.GetValues(typeof(PaymentMethods)));
            return View(membership);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Payment([Bind("Id,UserId,RegisterDate,ExpiredDate")] Member member, List<PaymentHistory> paymentHistory)
        {
            if (member == null)
            {
                return NotFound();
            }

            // Add membership data to the session
            HttpContext.Session.SetString("Id", member.Id.ToString());
            HttpContext.Session.SetString("Fee", paymentHistory[0].Fee.ToString());
            HttpContext.Session.SetString("RegisterDate", member.RegisterDate.ToString());
            HttpContext.Session.SetString("ExpiredDate", member.ExpiredDate.ToString());
            HttpContext.Session.SetString("PaymentMethod", paymentHistory[0].PaymentMethod.ToString());

            // Set Stripe API key
            StripeConfiguration.ApiKey = _configuration.GetSection("Stripe")["SecretKey"];

            // Create the Stripe options
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(paymentHistory[0].Fee * 100),
                            Currency = "cad",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Mini Library Membership"
                            },
                        },
                        Quantity = 1,
                    },
                },
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                Mode = "payment",
                SuccessUrl = "https://" + Request.Host + "/Members/SaveMembership",
                CancelUrl = "https://" + Request.Host + "/Members/Membership",
            };

            var service = new Stripe.Checkout.SessionService();
            Stripe.Checkout.Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);

            return new StatusCodeResult(303);
        }

        [Authorize]
        public async Task<IActionResult> SaveMembership()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Get the data out of the session
            var idString = HttpContext.Session.GetString("Id");
            var fee = HttpContext.Session.GetString("Fee");
            var registerDate = HttpContext.Session.GetString("RegisterDate");
            var expiredDate = HttpContext.Session.GetString("ExpiredDate");
            var paymentMethod = HttpContext.Session.GetString("PaymentMethod");
            var id = int.Parse(idString);

            if (id > 0)
            {
                var member = await _context.Members.Where(m => m.Id == id).FirstOrDefaultAsync();
                member.ExpiredDate = DateTime.Parse(expiredDate);
                _context.Update(member);
                await _context.SaveChangesAsync();
            }
            else
            {
                var member = new Member
                {
                    UserId = userId,
                    RegisterDate = DateTime.Parse(registerDate),
                    ExpiredDate = DateTime.Parse(expiredDate),
                    Active = true,
                };

                await _context.AddAsync(member);
                await _context.SaveChangesAsync();
                id = member.Id;
            }

            var paymentHistory = new PaymentHistory
            {
                Fee = decimal.Parse(fee),
                MemberId = id,
                PaymentMethod = (PaymentMethods)Enum.Parse(typeof(PaymentMethods), paymentMethod),
                PurchaseDate = DateTime.Today,
            };

            await _context.AddAsync(paymentHistory);
            await _context.SaveChangesAsync();

            return RedirectToAction("Membership", "Members");
        }
    }
}
