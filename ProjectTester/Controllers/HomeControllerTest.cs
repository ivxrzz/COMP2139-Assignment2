using Assignment1.Controllers;
using Assignment1.Data;
using Assignment1.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace ProjectTester.Controllers
{
    public class HomeControllerTests
    {
        private ApplicationDbContext GetDb(string name)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(name)
                .Options;

            return new ApplicationDbContext(options);
        }

        private Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task Privacy_ReturnsViewWithModel()
        {
            var db = GetDb("privacyTest");

            db.Events.Add(new Event
            {
                EventId = 1,
                Title = "Concert",
                CategoryId = 1,
                Category = new Category { CategoryId = 1, Name = "Music" }
            });
            db.SaveChanges();

            var userManager = MockUserManager();
            var env = Mock.Of<IWebHostEnvironment>();

            var controller = new HomeController(db, userManager.Object, env);

            var result = await controller.Privacy(null, null, null);

            var view = Assert.IsType<ViewResult>(result);
            Assert.NotNull(view.Model);
        }
    }
}