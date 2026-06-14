namespace Model.Settings
{
     public class VideoStream
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";         
        public bool Enable { get; set; }   
        public bool Simulate { get; set; }      
        public string StreamUrl { get; set; } = "";                   
        public Option Option { get; set; } = new Option();
    }

    public class Option
    {
        public int MinWidth { get; set; }
        public int MaxWidth { get; set; }
        public float MinWeight { get; set; }
        public bool Tracking { get; set; }
        public int NumberFrameForLose { get; set; }
        public bool UseArea { get; set; }
        public int AreaX { get; set; }
        public int AreaY { get; set; }
        public int AreaWidth { get; set; }
        public int AreaHeight { get; set; }
    }
}
   
