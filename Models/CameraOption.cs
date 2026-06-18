public class CameraOption
    {
        public int Id { get; set; }
        public int CameraId { get; set; }
        public Camera Camera { get; set; } = null!;

        public int MinPlateWidth { get; set; }
        public int MaxPlateWidth { get; set; }
        public double MinProbability { get; set; }
        public bool Tracking { get; set; }
        public int NumberFrameForLose { get; set; }
        
        public bool UseArea { get; set; }
        public int AreaX { get; set; }
        public int AreaY { get; set; }
        public int AreaWidth { get; set; }
        public int AreaHeight { get; set; }
    }