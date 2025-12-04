using System;
using System.ComponentModel.DataAnnotations;

namespace Assignment1.Models
{
    public class PurchasedEvent
    {
        [Key]
        public int PurchasedEventId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string GuestName { get; set; } = string.Empty;

        [Required]
        public string GuestEmail { get; set; } = string.Empty;

        [Required]
        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }

        public DateTime PurchaseDate { get; set; }
        public int? Rating {get; set; }
    }
}