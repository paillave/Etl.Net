using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Paillave.Etl.ValuesProviders
{
    public class NameMappingFileValuesProvider<TOut> : IValuesProvider<Stream, TOut>
    {
        public void PushValues(Stream args, Action<TOut> pushValue)
        {

            throw new NotImplementedException();
        }
    }
}
