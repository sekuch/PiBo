using System;

namespace PiBo.Shared
{
    public class ImageInfo
    {
        public string Id { get; set; }
        public string Filename { get; set; }
        public DateTime CaptureDate { get; set; }
        public string Camera { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double ExposureTime { get; set; }
        public double FNumber { get; set; }
        public double FocalLength { get; set; }
        public double FocalLength35 { get; set; }

        public string LensModel { get; set; }
        public Position GpsInfo { get; set; }
    }
}