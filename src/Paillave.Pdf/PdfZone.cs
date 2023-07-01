using System;

namespace Paillave.Pdf
{
    public enum Units
    {
        Millimeters = 1,
        Centimeters = 2,
        Inches = 3,
        CentimetersAsA4 = 4
    }
    public class PdfZone
    {
        public double Left { get; set; }
        public double Width { get; set; }
        public double Top { get; set; }
        public double Height { get; set; }
        public Units Units { get; set; } = Units.CentimetersAsA4;
        public int? PageNumber { get; set; }
        private UglyToad.PdfPig.Core.PdfRectangle? _rectangle = null;
        private const double StandardA4PointsWidth = 72 * 21 / 2.54;
        private const double StandardA4PointsHeight = 72 * 29.7 / 2.54;
        private UglyToad.PdfPig.Core.PdfRectangle GetRectangle(double pageWidth, double pageHeight)
        {
            if (_rectangle != null) return _rectangle.Value;
            _rectangle = new UglyToad.PdfPig.Core.PdfRectangle(
                ToPdfUnits(this.Left, pageWidth / StandardA4PointsWidth),
                ToPdfUnits(this.Top, pageHeight / StandardA4PointsHeight),
                ToPdfUnits(this.Left + this.Width, pageWidth / StandardA4PointsWidth),
                ToPdfUnits(this.Top - this.Height, pageHeight / StandardA4PointsHeight));
            return _rectangle.Value;
        }
        private double ToPdfUnits(double value, double pageRatioToA4) => this.Units switch
        {
            Units.Millimeters => (72 / 25.4) * value,
            Units.Centimeters => (72 / 2.54) * value,
            Units.Inches => 72 * value,
            Units.CentimetersAsA4 => (72 / 2.54) * value * pageRatioToA4,
            _ => throw new Exception("unknown unit")
        };
        internal bool IsInZone(UglyToad.PdfPig.Core.PdfRectangle rectangle, double pageWidth, double pageHeight) =>
            rectangle.Bottom < this.GetRectangle(pageWidth, pageHeight).Top
            && rectangle.Top > this.GetRectangle(pageWidth, pageHeight).Bottom
            && rectangle.Left < this.GetRectangle(pageWidth, pageHeight).Right
            && rectangle.Right > this.GetRectangle(pageWidth, pageHeight).Left;
    }
}