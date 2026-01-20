using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Core;

public interface IDeferredPushObservable<out T>: IPushObservable<T>
{
    void Start();
}
