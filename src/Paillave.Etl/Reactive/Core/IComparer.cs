using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Core
{
    public interface IComparer<T1, T2>
    {
        int Compare(T1 x, T2 y);
    }
}
