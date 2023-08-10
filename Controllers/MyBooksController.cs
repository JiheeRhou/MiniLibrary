﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniLibrary.Data;
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
                .Where(checkout => checkout.UserId == userId)
                .Select(checkout => checkout.BookId)
                .ToListAsync();

            var myBooks = await _context.Books
                .Include(b => b.Checkouts.OrderByDescending(c => c.Id))
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Where(book => checkoutBookIds.Contains(book.Id))
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
                .Include(b => b.Checkouts)
                .Include(b => b.Publisher)
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .Where(book => book.ReserveUserId == userId)
                .ToListAsync();

            return View(myBooks);
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
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
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
            if (checkout != null && checkout.Return == null)
            {
                status = "on Load";
            }
            else if (book.ReserveUserId != null)
            {
                status = "Reserved";
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
            ViewData["Previous"] = previous;
            ViewData["Active"] = active;
            return View(book);
        }

    }
}