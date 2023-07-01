using System.Collections.Generic;

namespace Paillave.Pdf
{
    public class ApproximateEqualityComparer : IEqualityComparer<double>
    {
        private readonly double _proximity = 0;
        public ApproximateEqualityComparer(double proximity = 0) => _proximity = proximity;
        public bool Equals(double x1, double x2) => x1 - _proximity <= x2 && x1 + _proximity >= x2;
        public int GetHashCode(double obj) => 0;
    }
}
