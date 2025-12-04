using Assignment1.Controllers;
using Assignment1.Data;
using Assignment1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using Xunit;
using Assert = Xunit.Assert;

namespace ProjectTester.Controllers
{
    public class EventsControllerTests
    {
        private ApplicationDbContext GetDb(string name)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(name)
                .Options;

            return new ApplicationDbContext(options);
        }

        private EventsController CreateController(ApplicationDbContext db)
        {
            var controller = new EventsController(db);

            // Fake user (for User.Identity.Name)
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "testuser") },
                "TestAuth"));

            var httpContext = new DefaultHttpContext
            {
                User = user
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Fake TempData so TempData["Error"] / ["Success"] works
            var tempDataProvider = new Mock<ITempDataProvider>();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);

            return controller;
        }

        [Fact]
        public async Task Purchase_Fails_WhenQuantityTooHigh()
        {
            var db = GetDb("purchaseFail");

            db.Events.Add(new Event
            {
                EventId = 1,
                Title = "Show",
                TicketsAvailable = 5,
                TicketPrice = 10
            });
            db.SaveChanges();

            var controller = CreateController(db);

            var result = await controller.Purchase(1, 20, "Test", "t@test.com");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Privacy", redirect.ActionName);
            Assert.Equal(5, db.Events.First().TicketsAvailable); // unchanged
        }

        [Fact]
        public async Task Purchase_Succeeds_WhenValid()
        {
            var db = GetDb("purchaseSuccess");

            db.Events.Add(new Event
            {
                EventId = 1,
                Title = "Show",
                TicketsAvailable = 5,
                TicketPrice = 10
            });
            db.SaveChanges();

            var controller = CreateController(db);

            var result = await controller.Purchase(1, 2, "Test", "t@test.com");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);  // redirects to Home/Index

            var ev = db.Events.First();
            Assert.Equal(3, ev.TicketsAvailable);
            Assert.Single(db.PurchasedEvents);
        }

        [Fact]
        public async Task Rate_UpdatesRatingCorrectly()
        {
            var db = GetDb("rateTest");

            var ev = new Event
            {
                EventId = 1,
                Title = "Concert",
                TicketPrice = 10
            };

            db.Events.Add(ev);

            db.PurchasedEvents.Add(new PurchasedEvent
            {
                PurchasedEventId = 1,
                EventId = 1,
                Event = ev,
                UserId = "x",
                GuestName = "A",
                GuestEmail = "e",
                Quantity = 1
            });

            db.SaveChanges();

            var controller = CreateController(db);

            var result = await controller.Rate(1, 5);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName); // Home/Index

            var updated = db.PurchasedEvents.First();
            Assert.Equal(5, updated.Rating);
        }
    }
}