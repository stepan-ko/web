using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using web.Models;

namespace web.Controllers;

public class SettingController : Controller
{
    private readonly ILogger<SettingController> _logger;

    public SettingController(ILogger<SettingController> logger)
    {
        _logger = logger;
        _logger.LogInformation("SettingController создан");
    }

    public IActionResult Main()
    {
        return View();       
    }

    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
