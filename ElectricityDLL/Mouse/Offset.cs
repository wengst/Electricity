namespace ElectricityDLL {
    public class Offset {
        public float X { get; set; }
        public float Y { get; set; }
        public Offset() { }
        public Offset(float x, float y) { X = x; Y = y; }
        public void Zero() { X = 0; Y = 0; }
    }
}
