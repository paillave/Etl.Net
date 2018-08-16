using Paillave.Etl.Core;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.MapperFactories;
using Paillave.Etl.Helpers.MapperFactories;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Paillave.RxPush.Core;
using Paillave.Etl.Core.StreamNodes;

namespace Paillave.Etl.StreamNodes
{
    public class CrossApplyFolderFilesArgs<T>
    {
        public string SearchPattern { get; set; }
        public SearchOption Option { get; set; }
        public Func<T, string> GetFolderPath { get; set; }
        public Func<T, string> GetSearchPattern { get; set; }
    }

    public class CrossApplyFolderFilesStreamNode<TIn> : StreamNodeBase<IStream<TIn>, TIn, CrossApplyFolderFilesArgs<TIn>>, IStreamNodeOutput<string>
    {
        public CrossApplyFolderFilesStreamNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, CrossApplyFolderFilesArgs<TIn> args) : base(input, name, parentNodeNamePath, args)
        {
            this.Output = base.CreateStream(nameof(Output), input.Observable.FlatMap(elt => new DeferedPushObservable<string>(i =>
            {
                string searchPattern = args.GetSearchPattern != null ? args.GetSearchPattern(elt) : args.SearchPattern;
                foreach (var item in Directory.GetFiles(this.Arguments.GetFolderPath(elt), searchPattern, args.Option))
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
                SearchPattern = pattern
            }).Output;
        }
        public static IStream<string> CrossApplyFolderFiles(this IStream<string> stream, string name, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return new CrossApplyFolderFilesStreamNode<string>(stream, name, null, new CrossApplyFolderFilesArgs<string>
            {
                GetFolderPath = i => i,
                Option = option,
                SearchPattern = pattern
            }).Output;
        }
        public static IStream<string> CrossApplyFolderFiles<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<TIn, string> getSearchPattern, SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return new CrossApplyFolderFilesStreamNode<TIn>(stream, name, null, new CrossApplyFolderFilesArgs<TIn>
            {
                GetFolderPath = getFolderPath,
                Option = option,
                GetSearchPattern = getSearchPattern
            }).Output;
        }
    }
}