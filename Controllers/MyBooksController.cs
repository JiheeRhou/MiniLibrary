﻿using Microsoft.AspNetCore.Authorization;
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

        public async Task<IActionResult> Checkout()
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

        public async Task<IActionResult> Return(int? id)
        {
            var checkout = await _context.Checkouts.FirstOrDefaultAsync(c => c.Id == id);

            if (checkout == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                checkout.IsReturn = true;
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

                return RedirectToAction("Checkout", "MyBooks");
            }

            return View(Checkout);
        }

        public async Task<IActionResult> Reserved()
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
                await _context.SaveChangesAsync();

                return RedirectToAction("Reserved", "MyBooks");
            }

            return View(Reserved);
        }

        public async Task<IActionResult> Details(int? id, string? previous)
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

            ViewData["Previous"] = previous;
            return View(book);
        }

    }
}