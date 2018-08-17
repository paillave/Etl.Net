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
using Paillave.Etl.ValuesProviders;

namespace Paillave.Etl.StreamNodes
{
    //public static partial class StreamEx
    //{
    //    public static IStream<string> CrossApplyFolderFiles<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
    //    {
    //        return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs { RootFolder = getFolderPath(i), SearchPattern = pattern }, i => i.Name);
    //    }
    //    public static IStream<string> CrossApplyFolderFiles(this IStream<string> stream, string name, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
    //    {
    //        return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs { RootFolder = i, SearchPattern = pattern }, i => i.Name);
    //    }
    //    public static IStream<string> CrossApplyFolderFiles<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<TIn, string> getSearchPattern, SearchOption option = SearchOption.TopDirectoryOnly)
    //    {
    //        return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs { RootFolder = getFolderPath(i), SearchPattern = getSearchPattern(i) }, i => i.Name);
    //    }
    //}
}