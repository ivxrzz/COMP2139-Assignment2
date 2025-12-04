using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assignment1.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime StartTimeDate { get; set; }

        public decimal TicketPrice { get; set; }

        public int TicketsAvailable { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<PurchasedEvent>? PurchasedEvents { get; set; }

        public string Description { get; set; } = string.Empty;
        public string? OrganizerId { get; set; }
    }
}