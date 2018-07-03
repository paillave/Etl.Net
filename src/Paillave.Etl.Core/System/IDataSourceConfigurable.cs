using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.System
{
    public interface IDataSourceConfigurable<TConfig, TRow>
    {
        void Configure(IObservable<TRow> observable, TConfig config);
    }
}
