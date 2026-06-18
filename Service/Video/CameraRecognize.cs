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
    private ulong CountRecognize = 1;
    private readonly ILogger<CameraManager> _logger;

    public CameraRecognize(Camera camera, ILogger<CameraManager> logger)
    {
        _logger = logger;   

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
        recognizer = new LprRecognizer(options);    
       
    }
    private LprRecognizer recognizer;

    public List<Rect> RecognizePlate(Mat frame)
    {
        List<Rect> plateBorder = new List<Rect>();
        try
        {           
           

            using var bmpFrame = BmpFrameConverter.FromMat(frame);
            using var pinnedFrame = PinnedFrame.Pin(bmpFrame);
            using var plateBuffer = recognizer.Recognize(pinnedFrame.Image, FrameId++, "");   
                
            foreach (var plate in plateBuffer.PopAll())
            {                

                plateBorder.Add(new Rect(plate.Data.Position.X,plate.Data.Position.Y, plate.Data.Position.Width, plate.Data.Position.Height));
                
                string msg = $"Кадр: {FrameId}" + Environment.NewLine +
                                        $"Всего Кадров распознано: {CountRecognize++}" + Environment.NewLine +                
                                        $"State: {plate.State}"+ Environment.NewLine +
                                        $"Номер: {plate.Data.PlateText}" + Environment.NewLine +
                                        $"Country: {plate.Country}"+ Environment.NewLine +
                                        $"Probability: {plate.Data.Probability:P1}"+ Environment.NewLine +
                                        $"Object id: {plate.Data.Identifier}"+ Environment.NewLine +
                                        $"Frame id: {plate.FrameId}"+ Environment.NewLine +
                                        $"Rect: x={plate.Data.Position.X}, y={plate.Data.Position.Y}, w={plate.Data.Position.Width}, h={plate.Data.Position.Height}" + Environment.NewLine;
                
                _logger.LogInformation(msg);
                

                PlateNativeMemory.ReleaseOwnedBuffers(plate);
            }
            return plateBorder;
        }       
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception); 

            return plateBorder;           
        }   

    }


}