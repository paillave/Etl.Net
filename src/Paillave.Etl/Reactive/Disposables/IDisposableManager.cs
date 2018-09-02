using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Disposables
{
    public interface IDisposableManager : IDisposable
    {
        void Set(IDisposable disposable);
        void TryDispose(IDisposable disposable);
        bool IsDisposed { get; }
    }
}
