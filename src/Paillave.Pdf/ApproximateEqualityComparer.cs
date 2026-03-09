using System.Collections.Generic;

namespace Paillave.Pdf;

public class ApproximateEqualityComparer(double proximity = 0) : IEqualityComparer<double>
{
    private readonly double _proximity = proximity;

    public bool Equals(double x1, double x2) => x1 - _proximity <= x2 && x1 + _proximity >= x2;
    public int GetHashCode(double obj) => 0;
}
