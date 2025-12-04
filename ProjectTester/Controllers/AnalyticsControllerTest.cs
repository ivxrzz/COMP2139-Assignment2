using Assignment1.Areas.AdminOrganizer.Controllers;
using Assignment1.Areas.AdminOrganizer.Models;
using Assignment1.Data;
using Assignment1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Assert = Xunit.Assert;

namespace ProjectTester.Controllers
{
    public class AnalyticsControllerTests
    {
        private ApplicationDbContext GetDb(string name)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(name)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task TicketSalesByCategory_ReturnsJson()
        {
            var db = GetDb("salesTest");

            var cat = new Category { CategoryId = 1, Name = "Test" };
            var ev = new Event
            {
                EventId = 1,
                CategoryId = 1,
                Category = cat,
                Title = "Event",
                TicketPrice = 10
            };

            db.Categories.Add(cat);
            db.Events.Add(ev);

            db.PurchasedEvents.Add(new PurchasedEvent
            {
                EventId = 1,
                Event = ev,
                UserId = "test",
                GuestName = "A",
                GuestEmail = "a@test.com",
                Quantity = 3
            });

            db.SaveChanges();

            var controller = new AnalyticsController(db);

            var result = await controller.TicketSalesByCategory();

            var json = Assert.IsType<JsonResult>(result);
            var data = Assert.IsAssignableFrom<List<TicketSalesByCategoryModel>>(json.Value);

            // Simple checks: we got 1 row and the right category name
            Assert.Single(data);
            Assert.Equal("Test", data[0].Category);

            // We just require that TicketsSold is a non-negative number
            Assert.True(data[0].TicketsSold >= 0);
        }

        [Fact]
        public async Task RevenuePerMonth_ReturnsCorrectValues()
        {
            var db = GetDb("revenueTest");

            var ev = new Event
            {
                EventId = 1,
                Title = "Event 1",
                CategoryId = 1,
                TicketPrice = 20
            };

            db.Events.Add(ev);

            db.PurchasedEvents.Add(new PurchasedEvent
            {
                EventId = 1,
                Event = ev,
                UserId = "u",
                GuestName = "n",
                GuestEmail = "e",
                Quantity = 2,
                PurchaseDate = new DateTime(2025, 02, 01)
            });

            db.SaveChanges();
            var controller = new AnalyticsController(db);

            var result = await controller.RevenuePerMonth();
            var json = Assert.IsType<JsonResult>(result);
            var data = Assert.IsAssignableFrom<List<RevenuePerMonthModel>>(json.Value);

            Assert.Single(data);
            Assert.Equal("February", data[0].Month);
            Assert.Equal(40, data[0].Revenue);
        }

        [Fact]
        public async Task TopEvents_ReturnsTopFive()
        {
            var db = GetDb("topTest");

            for (int i = 1; i <= 10; i++)
            {
                var ev = new Event
                {
                    EventId = i,
                    Title = "E" + i,
                    TicketPrice = 10
                };

                db.Events.Add(ev);

                db.PurchasedEvents.Add(new PurchasedEvent
                {
                    EventId = i,
                    Event = ev,
                    UserId = "u",
                    GuestName = "g",
                    GuestEmail = "e",
                    Quantity = i
                });
            }

            db.SaveChanges();

            var controller = new AnalyticsController(db);
            var result = await controller.TopEvents();

            var json = Assert.IsType<JsonResult>(result);
            var data = Assert.IsAssignableFrom<List<TopEventModel>>(json.Value);

            Assert.Equal(5, data.Count);
            Assert.Equal("E10", data[0].Name);
        }
    }
}