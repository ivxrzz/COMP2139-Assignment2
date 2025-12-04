using Assignment1.Data;
using Assignment1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Assignment1.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [Authorize(Roles = "Attendee,Organizer,Admin")]
        [HttpPost]
        public async Task<IActionResult> Rate(int purchasedEventId, int rating)
        {
            var purchased = await _context.PurchasedEvents
                .Include(p => p.Event)
                .FirstOrDefaultAsync(p => p.PurchasedEventId == purchasedEventId);

            if (purchased == null) return NotFound();

            if (rating < 1) rating = 1;
            if (rating > 5) rating = 5;

            purchased.Rating = rating;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"You rated {purchased.Event.Title} {rating} stars.";
            return RedirectToAction("Index", "Home");
        }
        
        [Authorize(Roles = "Attendee,Organizer,Admin")]
        public async Task<IActionResult> Index()
        {
            var userId = User.Identity?.Name ?? "Anonymous";

            var purchased = await _context.PurchasedEvents
                .Include(p => p.Event)
                .ThenInclude(e => e.Category)
                .Where(p => p.UserId == userId)
                .ToListAsync();

            return View(purchased);
        }
        
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Search(string? term)
        {
            var query = _context.Events
                .Include(e => e.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.Trim().ToLower();
                query = query.Where(e => e.Title.ToLower().Contains(term));
            }

            var events = await query
                .OrderBy(e => e.StartTimeDate)
                .ToListAsync();

            return PartialView("~/Views/Home/_EventPartial.cshtml", events);
        }
        
        [Authorize(Roles = "Organizer,Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(
                await _context.Categories.ToListAsync(),
                "CategoryId", "Name");

            return View();
        }

        [Authorize(Roles = "Organizer,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event ev)
        {
            if (ModelState.IsValid)
            {
                ev.StartTimeDate = DateTime.SpecifyKind(ev.StartTimeDate, DateTimeKind.Utc);
                ev.OrganizerId = User.Identity?.Name;

                _context.Events.Add(ev);
                await _context.SaveChangesAsync();
                return RedirectToAction("Privacy", "Home");
            }

            ViewBag.Categories = new SelectList(
                await _context.Categories.ToListAsync(),
                "CategoryId", "Name", ev.CategoryId);

            return View(ev);
        }
        
        [Authorize(Roles = "Organizer,Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            ViewBag.Categories = new SelectList(
                await _context.Categories.ToListAsync(),
                "CategoryId", "Name", ev.CategoryId);

            return View(ev);
        }

        [Authorize(Roles = "Organizer,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Event ev)
        {
            if (ModelState.IsValid)
            {
                var existingEvent = await _context.Events.FindAsync(ev.EventId);
                if (existingEvent == null) return NotFound();

                existingEvent.Title = ev.Title;
                existingEvent.StartTimeDate = DateTime.SpecifyKind(ev.StartTimeDate, DateTimeKind.Utc);
                existingEvent.TicketPrice = ev.TicketPrice;
                existingEvent.TicketsAvailable = ev.TicketsAvailable;
                existingEvent.CategoryId = ev.CategoryId;
                existingEvent.Description = ev.Description;

                await _context.SaveChangesAsync();
                return RedirectToAction("Privacy", "Home");
            }

            ViewBag.Categories = new SelectList(
                await _context.Categories.ToListAsync(),
                "CategoryId", "Name", ev.CategoryId);

            return View(ev);
        }
        
        [Authorize(Roles = "Organizer,Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound();
            return View(ev);
        }

        [Authorize(Roles = "Organizer,Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev != null)
            {
                _context.Events.Remove(ev);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Privacy", "Home");
        }
        
        [Authorize(Roles = "Attendee,Organizer,Admin")]
        [HttpGet]
        public async Task<IActionResult> Purchase(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            return View(ev);
        }
        
        [Authorize(Roles = "Attendee,Organizer,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(int eventId, int quantity, string guestName, string guestEmail)
        {
            var ev = await _context.Events
                .Include(e => e.PurchasedEvents)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (ev == null || quantity <= 0 || quantity > ev.TicketsAvailable)
            {
                TempData["Error"] = ev == null ? "Event not found." :
                    quantity <= 0 ? "Quantity must be at least 1." :
                    $"Cannot purchase {quantity} tickets. Only {ev.TicketsAvailable} available.";

                return RedirectToAction("Privacy", "Home");
            }

            ev.TicketsAvailable -= quantity;
            _context.Events.Update(ev);

            var purchased = new PurchasedEvent
            {
                EventId = ev.EventId,
                UserId = User.Identity?.Name ?? "Anonymous",
                GuestName = guestName,
                GuestEmail = guestEmail,
                Quantity = quantity,
                PurchaseDate = DateTime.UtcNow
            };

            _context.PurchasedEvents.Add(purchased);
            await _context.SaveChangesAsync();

            var msg = $"Purchased {quantity} tickets for {ev.Title}.";
            TempData["Success"] = msg;
            TempData["PurchaseSuccess"] = msg;

            return RedirectToAction("Index", "Home");
        }
        
        [Authorize(Roles = "Attendee,Organizer,Admin")]
        [HttpGet]
        public async Task<IActionResult> Ticket(int id)
        {
            var ticket = await _context.PurchasedEvents
                .Include(p => p.Event)
                .ThenInclude(e => e.Category)
                .FirstOrDefaultAsync(p => p.PurchasedEventId == id);

            if (ticket == null)
                return NotFound();

            return View(ticket);
        }
        
        [Authorize(Roles = "Attendee,Organizer,Admin")]
        [HttpGet]
        public IActionResult TicketPdf(int id)
        {
            return RedirectToAction("Ticket", new { id, print = true });
        }
    }
}