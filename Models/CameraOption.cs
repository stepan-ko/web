public class CameraOption
    {
        public int Id { get; set; }
        public int CameraId { get; set; }
        public Camera Camera { get; set; } = null!;

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