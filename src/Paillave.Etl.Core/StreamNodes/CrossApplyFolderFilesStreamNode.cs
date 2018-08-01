using Paillave.Etl.Core.System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.System.Streams;
using Paillave.Etl.Core.MapperFactories;
using Paillave.Etl.Core.Helpers.MapperFactories;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Paillave.RxPush.Core;

namespace Paillave.Etl.Core.StreamNodes
{
    public class CrossApplyFolderFilesArgs
    {
        public string Pattern { get; set; }
        public SearchOption Option { get; set; }
    }

    public class CrossApplyFolderFilesStreamNode : StreamNodeBase<IStream<string>, string, CrossApplyFolderFilesArgs>, IStreamNodeOutput<string>
    {
        public CrossApplyFolderFilesStreamNode(IStream<string> input, string name, IEnumerable<string> parentNodeNamePath, CrossApplyFolderFilesArgs args) : base(input, name, parentNodeNamePath, args)
        {
            this.Output = base.CreateStream(nameof(Output), input.Observable.FlatMap(folder => new DeferedPushObservable<string>(i =>
            {
                foreach (var item in Directory.GetFiles(folder, args.Pattern, args.Option))
                    i(item);
            }, true)));
        }

        public IStream<string> Output { get; }
    }

    public static partial class StreamEx
    {
        public static IStream<string> CrossApplyFolderFiles(this IStream<string> stream, string name, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return new CrossApplyFolderFilesStreamNode(stream, name, null, new CrossApplyFolderFilesArgs { Option = option, Pattern = pattern }).Output;
        }
    }
}