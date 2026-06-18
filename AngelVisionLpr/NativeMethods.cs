using System.Runtime.InteropServices;

namespace AngelVisionLprDemo.AngelVisionLpr;

internal static class NativeMethods
{
    public const string LibraryName = "libav_lpr_c";

    // Создает экземпляр распознавателя. Возвращенный handle нужно освободить через av_lpr_recognizer_free.
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "av_lpr_recognizer_alloc")]
    public static extern IntPtr RecognizerAlloc(AitAvOptions options);

    // Освобождает распознаватель и обнуляет handle на native-стороне.
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "av_lpr_recognizer_free")]
    public static extern void RecognizerFree(ref IntPtr recognizer);

    // Главный вызов распознавания одного кадра. plateBuffer читается через av_license_plate_buffer_pop.
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "av_lpr_recognize")]
    public static extern AitError Recognize(IntPtr recognizer, AitImage image, out IntPtr plateBuffer, ulong id, string info);

    // Извлекает следующий результат из буфера. Возвращает 0, когда результатов больше нет.
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "av_license_plate_buffer_pop")]
    public static extern int PlateBufferPop(ref IntPtr plateBuffer, out AitAvLicensePlate plate);

    // Освобождает буфер результатов, если он не был полностью освобожден при чтении.
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "av_license_plate_buffer_free")]
    public static extern void PlateBufferFree(ref IntPtr plateBuffer);

    // Глобальная настройка количества потоков SDK.
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "av_set_number_of_threads")]
    public static extern void SetNumberOfThreads(int threads);
}
