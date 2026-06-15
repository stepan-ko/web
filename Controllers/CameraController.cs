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
            Enable = model.Enable,

            Option = new CameraOption
            {
                MinWidth = model.Option.MinWidth,
                MaxWidth = model.Option.MaxWidth,
                MinWeight = model.Option.MinWeight,
                Tracking = model.Option.Tracking,
                NumberFrameForLose = model.Option.NumberFrameForLose,
                UseArea = model.Option.UseArea,
                AreaX = model.Option.AreaX,
                AreaY = model.Option.AreaY,
                AreaWidth = model.Option.AreaWidth,
                AreaHeight = model.Option.AreaHeight
            }       
        };

        await _cameraService.AddAsync(camera);

        return RedirectToAction(nameof(Index));
    }

    // GET: /Camera/Edit
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var  camera = await _cameraService.GetByIdAsync(id);

        if (camera == null)
            return NotFound();

        var model = new CameraViewModel
        {
            Id = camera.Id,
            Name = camera.Name,
            StreamUrl = camera.StreamUrl,
            Simulate = camera.Simulate,
            Enable = camera.Enable,

            Option = new CameraOptionViewModel
            {
                MinWidth = camera.Option.MinWidth,
                MaxWidth = camera.Option.MaxWidth,
                MinWeight = camera.Option.MinWeight,
                Tracking = camera.Option.Tracking,
                NumberFrameForLose = camera.Option.NumberFrameForLose,
                UseArea = camera.Option.UseArea,
                AreaX = camera.Option.AreaX,
                AreaY = camera.Option.AreaY,
                AreaWidth = camera.Option.AreaWidth,
                AreaHeight = camera.Option.AreaHeight
            }
        };

        return View(model);
    }

    // POST: /Camera/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CameraViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
       
        var  camera = await _cameraService.GetByIdAsync(model.Id);

       if (camera == null)
        throw new Exception("Camera not found");

        camera.Name = model.Name;
        camera.StreamUrl = model.StreamUrl;
        camera.Simulate = model.Simulate;
        camera.Enable = model.Enable;

        camera.Option.MinWidth = model.Option.MinWidth;
        camera.Option.MaxWidth = model.Option.MaxWidth;
        camera.Option.MinWeight = model.Option.MinWeight;
        camera.Option.Tracking = model.Option.Tracking;
        camera.Option.NumberFrameForLose = model.Option.NumberFrameForLose;
        camera.Option.UseArea = model.Option.UseArea;
        camera.Option.AreaX = model.Option.AreaX;
        camera.Option.AreaY = model.Option.AreaY;
        camera.Option.AreaWidth = model.Option.AreaWidth;
        camera.Option.AreaHeight = model.Option.AreaHeight;

        await _cameraService.UpdateAsync(camera);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var  camera = await _cameraService.GetByIdAsync(id);
        if (camera == null)
            NotFound();

        await _cameraService.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }
    

}