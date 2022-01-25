using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UglyToad.PdfPig.Core;

namespace Paillave.Pdf
{
    public class PointEqualityComparer : IEqualityComparer<PdfPoint>
    {
        private readonly double _proximity = 0;
        public PointEqualityComparer() { }
        public PointEqualityComparer(double proximity) => _proximity = proximity;

        public bool Equals(PdfPoint a, PdfPoint b)
            => a.X >= b.X - _proximity && a.X <= b.X + _proximity
            && a.Y >= b.Y - _proximity && a.Y <= b.Y + _proximity;
        public int GetHashCode(PdfPoint obj) => 0;
    }
}
