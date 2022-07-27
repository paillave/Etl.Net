using System;

namespace Paillave.Pdf
{
    public enum Units
    {
        Millimeters = 1,
        Centimeters = 2,
        Inches = 3
    }
    public class PdfZone
    {
        public double Left { get; set; }
        public double Width { get; set; }
        public double Top { get; set; }
        public double Height { get; set; }
        public Units Units { get; set; } = Units.Centimeters;
        public int? PageNumber { get; set; }
        private UglyToad.PdfPig.Core.PdfRectangle? _rectangle = null;
        private UglyToad.PdfPig.Core.PdfRectangle Rectangle
        {
            get
            {
                if (_rectangle != null) return _rectangle.Value;
                _rectangle = new UglyToad.PdfPig.Core.PdfRectangle(ToPdfUnits(this.Left), ToPdfUnits(this.Top), ToPdfUnits(this.Left + this.Width), ToPdfUnits(this.Top - this.Height));
                return _rectangle.Value;
            }
        }
        private double ToPdfUnits(double value) => this.Units switch
        {
            Units.Millimeters => (72 / 25.4) * value,
            Units.Centimeters => (72 / 2.54) * value,
            Units.Inches => 72 * value,
            _ => throw new Exception("unknown unit")
        };
        internal bool IsInZone(UglyToad.PdfPig.Core.PdfRectangle rectangle) =>
            rectangle.Bottom < this.Rectangle.Top
            && rectangle.Top > this.Rectangle.Bottom
            && rectangle.Left < this.Rectangle.Right
            && rectangle.Right > this.Rectangle.Left;
    }
}