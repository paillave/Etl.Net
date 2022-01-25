using System;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.Graphics.Colors;

namespace Paillave.Pdf
{
    public class GridLine : IBounds
    {
        public PdfPoint P1 { get; }
        public PdfPoint P2 { get; }
        public double Left { get; }
        public double Right { get; }
        public double Top { get; }
        public double Bottom { get; }
        public bool IsVertical { get; }
        public bool IsHorizontal { get; }
        public IColor Color { get; }
        public GridLine(PdfPoint p1, PdfPoint p2, double proximity, IColor color)
        {
            this.Color = color;
            this.P1 = p1;
            var x2 = p1.X <= p2.X + proximity && p1.X >= p2.X - proximity ? p1.X : p2.X;
            var y2 = p1.Y <= p2.Y + proximity && p1.Y >= p2.Y - proximity ? p1.Y : p2.Y;
            this.P2 = new PdfPoint(x2, x2);
            if (P1.Y > y2)
            {
                this.Top = P1.Y;
                this.Bottom = y2;
            }
            else
            {
                this.Top = y2;
                this.Bottom = P1.Y;
            }
            if (P1.X > x2)
            {
                this.Right = P1.X;
                this.Left = x2;
            }
            else
            {
                this.Right = x2;
                this.Left = P1.X;
            }
            this.IsHorizontal = this.Top == this.Bottom;
            this.IsVertical = this.Left == this.Right;
        }
        public override string ToString() => $"{this.GetType()}[x1={this.P1.X:0},y1={this.P1.Y:0},x2={this.P2.X:0},y2={this.P2.Y:0},rgb=({this.Color.ToRGBValues()})]";
    }
}
