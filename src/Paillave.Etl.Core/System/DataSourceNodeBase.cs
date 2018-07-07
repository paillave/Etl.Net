using Paillave.Etl.Core.System;
using System.Reactive.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Reactive.Disposables;

namespace Paillave.Etl.Core.System
{
    public abstract class DataSourceNodeBase<TConfig, TRow> : StreamNodeBase, IStreamNodeOutput<TRow>
    {
        public IStream<TRow> Output { get; private set; }

        protected virtual IObservable<TRow> GetObservable(TConfig config)
        {
            var subject = new Subject<TRow>();
            Task.Run(() => PopulateObserver(config, subject));
            return subject.Publish().RefCount();
            //return Observable.Create<TRow>(o =>
            //{
            //    this.PopulateObserver(config, o);
            //    return (config as IDisposable) ?? Disposable.Empty;
            //});
        }

        protected abstract void PopulateObserver(TConfig config, IObserver<TRow> observer);

        public virtual void SetupStream(IObservable<TConfig> configS)
        {
            this.Output = base.CreateStream(nameof(Output), configS.SelectMany(this.GetObservable));
        }
    }
}
