using System.Collections.Generic;
using System.Drawing;


namespace Track {
    
public class LicensePlateData
{
    public Bitmap BestImg;

    public TrakcStatus Status;

    public Rectangle Position;

    public float Weight;

    public uint CodeTMP;

    public string Country;

    public string Number;

    public uint ID;

    public uint FrameID;

    public string FrameInfo;

    public uint AreaID;

    public List<LicensePlateData> Track;
}

public enum TrakcStatus
{
    EMPTY = -1,
    TRACKING,
    LOST,
    DETECTED
}

public class Options(TypeRecognizer type)
{
    public int MinWidth;

    public int MaxWidth;

    public float MinWeight;

    public bool Tracking;

    public int NumberFrameForLose;

    public Rectangle Area;

    public TypeRecognizer Type = type;
}

public enum TypeRecognizer
{
    SSDDNN,
    CHDDNN,
    CLDDNN,
    CLANDNN,
    CLCEDNN
}
}