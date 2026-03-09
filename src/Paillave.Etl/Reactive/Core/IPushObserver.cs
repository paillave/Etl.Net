using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Core;

public interface IPushObserver<in T> : IDisposable
{
    void PushValue(T value);
    void Complete();
    void PushException(Exception exception);
}
