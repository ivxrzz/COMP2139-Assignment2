using Microsoft.AspNetCore.Mvc;

namespace Assignment1.Controllers;

public class LoggingController : Controller
{
    private readonly ILogger<LoggingController> _logger;
    public LoggingController(ILogger<LoggingController> logger)
    {
        _logger = logger;
    }
    public IActionResult Index()
    {
        _logger.LogInformation("Visited Logging Index page at {Time}", DateTime.Now);
        return View(); // renders Views/Logging/Index.cshtml
    }
    public IActionResult LogExample()
    {
        _logger.LogWarning("testing this");
        _logger.LogError("error test");
        return Ok("Logs written");
    }
}