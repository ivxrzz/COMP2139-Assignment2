using Assignment1.Areas.AdminOrganizer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Assignment1.Data;
using Assignment1.Models;

namespace Assignment1.Areas.AdminOrganizer.Controllers
{
    [Area("AdminOrganizer")]
    [Authorize(Roles = "Admin,Organizer")]
    public class AnalyticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult MyAnalytics()
        {
            return View();
        }

        // Tickets sold by category
        public async Task<IActionResult> TicketSalesByCategory()
        {
            var data = await _context.Categories
                .Select(c => new TicketSalesByCategoryModel
                {
                    Category = c.Name,
                    TicketsSold = _context.PurchasedEvents
                        .Include(pe => pe.Event)
                        .Count(pe => pe.Event.CategoryId == c.CategoryId)
                })
                .ToListAsync();

            return Json(data);
        }

        // Revenue per month
        public async Task<IActionResult> RevenuePerMonth()
        {
            var data = await _context.PurchasedEvents
                .GroupBy(pe => pe.PurchaseDate.Month)
                .Select(g => new RevenuePerMonthModel
                {
                    Month = new DateTime(1, g.Key, 1).ToString("MMMM"),
                    Revenue = g.Sum(pe => pe.Quantity * pe.Event.TicketPrice)
                })
                .ToListAsync();

            return Json(data);
        }

        // Top 5 best-selling events
        public async Task<IActionResult> TopEvents()
        {
            var data = await _context.Events
                .Select(e => new TopEventModel
                {
                    Name = e.Title,
                    TicketsSold = e.PurchasedEvents.Sum(pe => pe.Quantity),
                    Revenue = e.PurchasedEvents.Sum(pe => pe.Quantity * e.TicketPrice)
                })
                .OrderByDescending(e => e.TicketsSold)
                .Take(5)
                .ToListAsync();

            return Json(data);
        }
    }
}
