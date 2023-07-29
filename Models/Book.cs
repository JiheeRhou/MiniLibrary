﻿using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace MiniLibrary.Models
{
    public class Book
    {
        public enum BookGenre
        {
            Novel,
            Fantasy,
            Science,
            Mystery,
            Romans,
            Horror,
            Historical
        }
        public int Id { get; set; }

        [Required(), MaxLength(500)]
        [Display(Name = "Book Title")]
        public string Title { get; set; }

        [Required()]
        public BookGenre Genre { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Book Description")]
        public string? Description { get; set; }

        [Required()]
        [Display(Name = "Publisher")]
        public int PublisherId { get; set; }

        [Required()]
        [Display(Name = "Published")]
        public DateTime PublishedDate { get; set; }

        [Required(), MaxLength(13)]
        public string ISBN { get; set; }

        [Required(), Range(0, 9999)]
        public int Pages { get; set; }

        [Required()]
        public bool IsAvailable { get; set; }

        [Display(Name = "Reserved")]
        public int? ReserveUserId { get; set; }

        public string? Photo { get; set; }

        [Display(Name = "Authors")]
        public List<BookAuthor>? BookAuthors { get; set; }

        public List<Checkout>? Checkouts { get; set; }

        public Publisher? Publisher { get; set; }

        public User? User { get; set; }
    }
}