﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Core
{
    public class LambdaComparer<T>(Func<T, T, int> comparer) : IComparer<T>
    {
        public Func<T, T, int> Comparer { get; } = comparer;

        public int Compare(T x, T y) => this.Comparer(x, y);
        //public bool Equals(T x, T y)
        //{
        //    return Comparer(x, y);
        //}

        //public int GetHashCode(T obj)
        //{
        //    return obj.GetHashCode();
        //}
    }
}
