
namespace Configuration.VideoConfig
{
    public class VideoConfig
    {
        public string Dirlicens {  get; set; }
        public bool RU { get; set; }
        public List<Input> Inputs { get; set; }
       
    }

    public class Input
    {
        public int Id { get; set; }
        public bool Showvideo { get; set; }
        public bool Simulate { get; set; }        
        public string Dirlink { get; set; }
        public string Dirsave { get; set; }
        public string Dirlogs { get; set; }
        public bool Usecamera { get; set; }
        public int Camera { get; set; }
        public Option Option { get; set; }
    }

    public class Option
    {
        public int MinWidth { get; set; }
        public int MaxWidth { get; set; }
        public float MinWeight { get; set; }
        public bool Tracking { get; set; }
        public int NumberFrameForLose { get; set; }
        public bool AreaFull { get; set; }
        public bool AreaShow { get; set; }
        public int AreaX { get; set; }
        public int AreaY { get; set; }
        public int AreaWidth { get; set; }
        public int AreaHeight { get; set; }
    }


}