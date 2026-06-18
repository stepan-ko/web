using web.av.AngelVisionLpr;
using web.av.Imaging;
using OpenCvSharp;


public class CameraRecognize
{
    
    public static bool Configure(int countThreads)
    {
        try
        {
            NativeLibraryLoader.Configure();

            if (countThreads > 0)
            {
                // Глобальная настройка SDK: сколько рабочих потоков может использовать распознавание.
                NativeMethods.SetNumberOfThreads(countThreads);
            }
            
            return true;
        }
        catch (DllNotFoundException exception)
        {
            Console.Error.WriteLine("Не удалось загрузить native-библиотеку AngelVision LPR.");
            Console.Error.WriteLine(exception.Message);
            return false;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return false;
        } 
    }

    private ulong FrameId = 1;
    private readonly ILogger<CameraManager> _logger;

    public CameraRecognize(Camera camera, ILogger<CameraManager> logger)
    {
        _logger = logger;   

        _logger.LogDebug("START CameraRecognize(Camera camera, ILogger<CameraManager> logger)");
        var options = new AitAvOptions
        {
            Type = TypeRecognizer.CldDnn,
            Tracking = camera.Option.Tracking,
            FramesBeforeLosing = camera.Option.NumberFrameForLose,
            MinPlateWidth = camera.Option.MinPlateWidth,
            MaxPlateWidth = camera.Option.MaxPlateWidth,
            MinProbability = camera.Option.MinProbability,
            Area = camera.Option.UseArea ? 
            new AitRect(camera.Option.AreaX, camera.Option.AreaY, camera.Option.AreaWidth, camera.Option.AreaHeight) : 
            new AitRect(0, 0, camera.Width, camera.Height)
        };
        _logger.LogDebug("MIDDLE CameraRecognize(Camera camera, ILogger<CameraManager> logger)");
        recognizer = new LprRecognizer(options);    
        _logger.LogDebug("END CameraRecognize(Camera camera, ILogger<CameraManager> logger)");
    }
    private LprRecognizer recognizer;

    public void RecognizePlate(Mat frame)
    {
        try
        {           
            using var bmpFrame = BmpFrameConverter.FromMat(frame);
            using var pinnedFrame = PinnedFrame.Pin(bmpFrame);
            using var plateBuffer = recognizer.Recognize(pinnedFrame.Image, FrameId, "");   
            
            var found = false;

            foreach (var plate in plateBuffer.PopAll())
            {
                found = true;               
                _logger.LogInformation($"Кадр: {FrameId} - Номер:");
                _logger.LogInformation($"State: {plate.State}");
                _logger.LogInformation($"Номер: {plate.Data.PlateText}");
                _logger.LogInformation($"Country: {plate.Country}");
                _logger.LogInformation($"Probability: {plate.Data.Probability:P1}");
                _logger.LogInformation($"Object id: {plate.Data.Identifier}");
                _logger.LogInformation($"Frame id: {plate.FrameId}");
                _logger.LogInformation($"Rect: x={plate.Data.Position.X}, y={plate.Data.Position.Y}, w={plate.Data.Position.Width}, h={plate.Data.Position.Height}");
                _logger.LogInformation("");

                PlateNativeMemory.ReleaseOwnedBuffers(plate);
            }

            if (!found)
            {
                _logger.LogInformation($"Кадр: {FrameId} - Номера не найдены");
            }
        }
       
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);            
        }    
    }


}