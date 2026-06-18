using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using web.Models;

public class CameraController : Controller
{
    private readonly ICameraService _cameraService;
    private readonly CameraManager _cameraManager;

    public CameraController(ICameraService cameraService, CameraManager cameraManager)
    {
        _cameraService = cameraService;
         _cameraManager = cameraManager;
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
            Width = model.Width,
            Height = model.Height,
            Fps = model.Fps,
            Option = new CameraOption
            {
                MinPlateWidth = model.Option.MinPlateWidth,
                MaxPlateWidth = model.Option.MaxPlateWidth,
                MinProbability = model.Option.MinProbability,
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
            Width = camera.Width,
            Height = camera.Height,
            Fps = camera.Fps,
            Option = new CameraOptionViewModel
            {
                MinPlateWidth = camera.Option.MinPlateWidth,
                MaxPlateWidth = camera.Option.MaxPlateWidth,
                MinProbability = camera.Option.MinProbability,
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
        camera.Width = model.Width;
        camera.Height = model.Height;
        camera.Fps = model.Fps;
        camera.Option.MinPlateWidth = model.Option.MinPlateWidth;
        camera.Option.MaxPlateWidth = model.Option.MaxPlateWidth;
        camera.Option.MinProbability = model.Option.MinProbability;
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

    [HttpPost]
    public async Task<IActionResult> Restart(int id)
    {
        var camera = await _cameraService.GetByIdAsync(id);

        if (camera == null)
            return NotFound();

        await _cameraManager.RestartCamera(camera);

        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    public async Task<IActionResult> Stop(int id)
    {
        var camera = await _cameraService.GetByIdAsync(id);

        if (camera == null)
            return NotFound();

        await _cameraManager.StopCamera(id);

        return RedirectToAction(nameof(Index));
    }

        [HttpPost]
    public async Task<IActionResult> Start(int id)
    {
        var camera = await _cameraService.GetByIdAsync(id);

        if (camera == null)
            return NotFound();

        _cameraManager.StartCamera(camera);

        return RedirectToAction(nameof(Index));
    }

}