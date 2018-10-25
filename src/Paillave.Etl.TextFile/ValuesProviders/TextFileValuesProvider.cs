using Paillave.Etl.Core;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Paillave.Etl.TextFile.ValuesProviders
{
    public class TextFileValuesProvider
    {
        public void PushValues(Stream input, Action<string> push)
        {
            using (var sr = new StreamReader(input))
                while (!sr.EndOfStream)
                    push(sr.ReadLine());
        }
    }
}
