using av.AngelVisionLpr;
using av.Imaging;
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
    private readonly ILogger<CameraRecognize> _logger;

    public CameraRecognize(ILogger<CameraRecognize> logger)
    {
        _logger = logger;        
    }

    internal void RecognizePlate(Camera camera, Mat frame, AitAvOptions options)
    {
        try
        {           
            
            // var options = new AitAvOptions
            // {
            //     Type = arguments.Type,
            //     Tracking = arguments.Tracking,
            //     FramesBeforeLosing = arguments.FramesBeforeLosing,
            //     MinPlateWidth = arguments.MinPlateWidth,
            //     MaxPlateWidth = arguments.MaxPlateWidth,
            //     MinProbability = arguments.MinProbability,
            //     Area = arguments.Area ?? new AitRect(0, 0, frame.Width, frame.Height)
            // };

            using var recognizer = new LprRecognizer(options);

            // Данные кадра должны оставаться по одному и тому же адресу в памяти на время native-вызова.
            // Поэтому managed byte[] фиксируется через GCHandle и освобождается сразу после Recognize.
            using var pinnedFrame = PinnedFrame.Pin(frame);
            
            using var plateBuffer = recognizer.Recognize(pinnedFrame.Image, FrameId);   
            
            PlateBuffer
            PinnedFrame

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