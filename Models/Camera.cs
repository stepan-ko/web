public class Camera
{
     public int Id { get; set; }
        public string Name { get; set; } = "";         
        public bool Enable { get; set; }   
        public bool Simulate { get; set; }      
        public string StreamUrl { get; set; } = "";                   
        public Option Option { get; set; } = new Option();

}

