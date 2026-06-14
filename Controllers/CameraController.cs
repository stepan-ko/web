using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using web.Models;

public class CameraController : Controller
{
    private readonly ICameraService _cameraService;

    public CameraController(ICameraService cameraService)
    {
        _cameraService = cameraService;
    }

    public async Task<IActionResult> Index()
    {
        var cameras = await _cameraService.GetAllAsync();

        return View(cameras);
    }

    // GET: /Camera/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Camera/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CameraViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var camera = new Camera
        {
            Name = model.Name,
            StreamUrl = model.StreamUrl,
            Simulate = model.Simulate,
            Enable = model.Enable
        };

        await _cameraService.AddAsync(camera);

        return RedirectToAction(nameof(Index));
    }
}