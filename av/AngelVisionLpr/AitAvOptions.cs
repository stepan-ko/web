using System.Runtime.InteropServices;

namespace AngelVisionLpr;

[StructLayout(LayoutKind.Sequential)]
internal struct AitAvOptions
{
    // Минимальная и максимальная ширина номера в пикселях.
    public int MinPlateWidth;
    public int MaxPlateWidth;

    // Включает внутренний трекинг номера между кадрами.
    public bool Tracking;

    // Сколько кадров объект может отсутствовать, прежде чем SDK вернет состояние Lost.
    public int FramesBeforeLosing;

    // Нижний порог уверенности распознавания: 0.0 .. 1.0.
    public double MinProbability;

    // Область распознавания в координатах исходного кадра.
    public AitRect Area;

    // Тип модели/алгоритма распознавания.
    public TypeRecognizer Type;
}
