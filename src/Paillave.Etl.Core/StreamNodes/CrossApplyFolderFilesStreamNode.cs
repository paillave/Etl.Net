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
    public class CrossApplyFolderFilesArgs<T>
    {
        public string Pattern { get; set; }
        public SearchOption Option { get; set; }
        public Func<T, string> GetFolderPath { get; set; }
    }

    public class CrossApplyFolderFilesStreamNode<TIn> : StreamNodeBase<IStream<TIn>, TIn, CrossApplyFolderFilesArgs<TIn>>, IStreamNodeOutput<string>
    {
        public CrossApplyFolderFilesStreamNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, CrossApplyFolderFilesArgs<TIn> args) : base(input, name, parentNodeNamePath, args)
        {
            this.Output = base.CreateStream(nameof(Output), input.Observable.FlatMap(elt => new DeferedPushObservable<string>(i =>
            {
                foreach (var item in Directory.GetFiles(this.Arguments.GetFolderPath(elt), args.Pattern, args.Option))
                    i(item);
            }, true)));
        }

        public IStream<string> Output { get; }
    }

    public static partial class StreamEx
    {
        public static IStream<string> CrossApplyFolderFiles<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return new CrossApplyFolderFilesStreamNode<TIn>(stream, name, null, new CrossApplyFolderFilesArgs<TIn>
            {
                GetFolderPath = getFolderPath,
                Option = option,
                Pattern = pattern
            }).Output;
        }
        public static IStream<string> CrossApplyFolderFiles(this IStream<string> stream, string name, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return new CrossApplyFolderFilesStreamNode<string>(stream, name, null, new CrossApplyFolderFilesArgs<string>
            {
                GetFolderPath = i => i,
                Option = option,
                Pattern = pattern
            }).Output;
        }
    }
}