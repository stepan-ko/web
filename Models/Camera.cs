public class Camera
{
        public int Id { get; set; }
        public string Name { get; set; } = "";         
        public bool Enable { get; set; }   
        public bool Simulate { get; set; }      
        public string StreamUrl { get; set; } = "";
        public int Width { get; set; }
        public int Height { get; set; }
        public int Fps { get; set; }
        public CameraOption Option { get; set; } = null!;

}

