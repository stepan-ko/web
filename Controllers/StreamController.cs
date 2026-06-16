
using Microsoft.AspNetCore.Mvc;



public class StreamController : Controller
{
    private readonly FrameBuffer _frameBuffer;

    public StreamController(FrameBuffer frameBuffer)
    {
        _frameBuffer = frameBuffer;
    }
   
    [HttpGet]
    public IActionResult Details(int id)
    {
        return View(id);
    }

    [HttpGet]
    public async Task Stream(int cameraId)
    {
        Response.ContentType =
            "multipart/x-mixed-replace; boundary=frame";

        while (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            var frame = _frameBuffer.GetFrame(cameraId);

            if (frame != null)
            {
                await Response.WriteAsync("--frame\r\n");
                await Response.WriteAsync("Content-Type: image/jpeg\r\n\r\n");

                await Response.Body.WriteAsync(frame);

                await Response.WriteAsync("\r\n");
                await Response.Body.FlushAsync();
            }

            await Task.Delay(50);
        }
    }

    
    
}