using Assignment1.Data;
using Assignment1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;

namespace Assignment1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.UserName ?? "Unknown";

            var purchases = await _context.PurchasedEvents
                .Include(p => p.Event)
                .ThenInclude(e => e.Category)
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.Event.StartTimeDate)
                .ToListAsync();

            var myEvents = await _context.Events
                .Include(e => e.Category)
                .Include(e => e.PurchasedEvents)
                .Where(e => e.OrganizerId == userId)
                .ToListAsync();

            ViewBag.CurrentUser = user;
            ViewBag.MyEvents = myEvents;

            return View(purchases);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Privacy(string? searchTitle, int? categoryId, string? sortBy)
        {
            var query = _context.Events.Include(e => e.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchTitle))
                query = query.Where(e => e.Title.ToLower().Contains(searchTitle.ToLower()));

            if (categoryId.HasValue && categoryId > 0)
                query = query.Where(e => e.CategoryId == categoryId.Value);

            query = sortBy switch
            {
                "title" => query.OrderBy(e => e.Title),
                "price" => query.OrderBy(e => e.TicketPrice),
                "date" => query.OrderBy(e => e.StartTimeDate),
                _ => query
            };

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(await query.ToListAsync());
        }
        
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> EventsPartial(string? searchTitle, int? categoryId, string? sortBy)
        {
            var eventsQuery = _context.Events
                .Include(e => e.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTitle))
            {
                var lower = searchTitle.ToLower();
                eventsQuery = eventsQuery.Where(e => e.Title.ToLower().Contains(lower));
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                eventsQuery = eventsQuery.Where(e => e.CategoryId == categoryId.Value);
            }

            eventsQuery = sortBy switch
            {
                "title" => eventsQuery.OrderBy(e => e.Title),
                "price" => eventsQuery.OrderBy(e => e.TicketPrice),
                "date" => eventsQuery.OrderBy(e => e.StartTimeDate),
                _ => eventsQuery
            };

            var events = await eventsQuery.ToListAsync();
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return PartialView("_EventPartial", events);
        }
        
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(
            string fullName,
            string phoneNumber,
            IFormFile profilePicture)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Could not load user.";
                return RedirectToAction("Index");
            }
            
            user.FullName = fullName;
            
            var currentPhone = await _userManager.GetPhoneNumberAsync(user);
            if (phoneNumber != currentPhone)
            {
                var phoneResult = await _userManager.SetPhoneNumberAsync(user, phoneNumber ?? "");
                if (!phoneResult.Succeeded)
                {
                    TempData["Error"] = "Could not update phone number.";
                    return RedirectToAction("Index");
                }
            }
            
            if (profilePicture != null && profilePicture.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "profile-pictures");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var extension = Path.GetExtension(profilePicture.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                user.ProfilePictureUrl = $"/profile-pictures/{fileName}";
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                TempData["Error"] = "Could not update profile.";
            }
            else
            {
                TempData["ProfileUpdated"] = "Profile updated successfully.";
            }

            return RedirectToAction("Index");
            
            
        }
        [Route("Home/InternalError")]
        public IActionResult InternalError()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionFeature != null)
            {
                Serilog.Log.Error(exceptionFeature.Error, "Unhandled exception occurred at {Path}", exceptionFeature.Path);
            }

            return View("InternalError");
        }

        [Route("Home/NotFoundPage")]
        public IActionResult NotFoundPage(int? code)
        {
            ViewData["StatusCode"] = code ?? 404;
            return View("NotFound");
        }
    }
}