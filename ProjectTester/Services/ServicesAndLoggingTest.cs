using Assignment1.Controllers;
using Assignment1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace ProjectTester.Services
{
    public class ServicesAndLoggingTests
    {
        [Fact]
        public async Task ConsoleEmailSender_DoesNotThrow()
        {
            var sender = new ConsoleEmailSender();

            // Should simply complete without errors
            await sender.SendEmailAsync("test@test.com", "Hello", "<p>World</p>");

            Assert.True(true); // If no exception, test passes
        }

        [Fact]
        public void LoggingController_WritesLogs_AndReturnsOk()
        {
            // Arrange
            var logger = new Mock<ILogger<LoggingController>>();
            var controller = new LoggingController(logger.Object);

            // Act
            var result = controller.LogExample();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Logs written", okResult.Value);
        }
    }
}