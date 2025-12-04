using Assignment1.Data;
using Microsoft.EntityFrameworkCore;

namespace ProjectTester.TestHelpers
{
    public static class TestDbContextFactory
    {
        public static ApplicationDbContext Create(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}