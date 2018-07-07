using Paillave.Etl.Core.System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System;
using System.Reactive.Subjects;

namespace Paillave.Etl.Core.StreamNodes
{
    public class DataStreamSourceNode : DataSourceNodeBase<Stream, string>
    {
        protected override void PopulateObserver(Stream config, IObserver<string> subject)
        {
            Console.WriteLine("tmp");
            using (var sr = new StreamReader(config))
                while (!sr.EndOfStream)
                    subject.OnNext(sr.ReadLine());
            subject.OnCompleted();
        }
    }
}