using System;
using System.Linq;
using System.Threading.Tasks;
using Assignment1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Assignment1.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            await context.Database.MigrateAsync();
            
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { CategoryId = 1, Name = "KPOP Concert" },
                    new Category { CategoryId = 2, Name = "R&B Concerts" },
                    new Category { CategoryId = 3, Name = "Others" }
                );
            }
            
            if (!context.Events.Any())
            {
                context.Events.AddRange(
                    new Event
                    {
                        EventId = 1,
                        Title = "Enhypen: WALK THE LINE",
                        CategoryId = 1,
                        StartTimeDate = new DateTime(2025, 11, 4, 20, 0, 0, DateTimeKind.Utc),
                        TicketPrice = 30,
                        TicketsAvailable = 7
                    },
                    new Event
                    {
                        EventId = 2,
                        Title = "Sabrina Carpenter: SHORT N'SWEET",
                        CategoryId = 2,
                        StartTimeDate = new DateTime(2025, 12, 4, 19, 0, 0, DateTimeKind.Utc),
                        TicketPrice = 30,
                        TicketsAvailable = 4
                    },
                    new Event
                    {
                        EventId = 3,
                        Title = "The Weeknd: After Hours Til Dawn",
                        CategoryId = 2,
                        StartTimeDate = new DateTime(2026, 1, 4, 21, 0, 0, DateTimeKind.Utc),
                        TicketPrice = 30,
                        TicketsAvailable = 3
                    }
                );
            }

            await context.SaveChangesAsync();
        }
    }
}