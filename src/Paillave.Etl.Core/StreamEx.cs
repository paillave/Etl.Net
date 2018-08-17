using Paillave.Etl.Core;
using Paillave.Etl.Core.NodeOutputs;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Helpers;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.ValuesProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SystemIO = System.IO;

namespace Paillave.Etl
{
    public static class StreamEx
    {
        #region CrossApply
        public static IStream<TOut> CrossApply<TIn, TValueIn, TValueOut, TOut>(this IStream<TIn> stream, string name, IValuesProvider<TValueIn, TValueOut> valuesProvider, Func<TIn, TValueIn> inputValueSelector, Func<TValueOut, TOut> outputValueSelector)
        {
            return new CrossApplyStreamNode<TIn, TValueIn, TValueOut, TOut>(stream, name, null, new CrossApplyArgs<TIn, TValueIn, TValueOut, TOut>
            {
                InputValueSelector = inputValueSelector,
                OutputValueSelector = outputValueSelector,
                ValuesProvider = valuesProvider
            }).Output;
        }
        public static IStream<TOut> CrossApply<TIn, TOut>(this IStream<TIn> stream, string name, IValuesProvider<TIn, TOut> valuesProvider)
        {
            return new CrossApplyStreamNode<TIn, TIn, TOut, TOut>(stream, name, null, new CrossApplyArgs<TIn, TIn, TOut, TOut>
            {
                InputValueSelector = i => i,
                OutputValueSelector = i => i,
                ValuesProvider = valuesProvider
            }).Output;
        }
        public static IStream<TOut> CrossApply<TIn, TRes, TValueIn, TValueOut, TOut>(this IStream<TIn> stream, string name, IStream<TRes> resourceStream, IValuesProvider<TValueIn, TRes, TValueOut> valuesProvider, Func<TIn, TValueIn> inputValueSelector, Func<TValueOut, TOut> outputValueSelector)
        {
            return new CrossApplyResourceStreamNode<TIn, TRes, TValueIn, TValueOut, TOut>(stream, name, null, new CrossApplyResourceArgs<TIn, TRes, TValueIn, TValueOut, TOut>
            {
                InputValueSelector = inputValueSelector,
                OutputValueSelector = outputValueSelector,
                ValuesProvider = valuesProvider,
                ResourceStream = resourceStream
            }).Output;
        }
        public static IStream<TOut> CrossApply<TIn, TRes, TOut>(this IStream<TIn> stream, string name, IStream<TRes> resourceStream, IValuesProvider<TIn, TRes, TOut> valuesProvider)
        {
            return new CrossApplyResourceStreamNode<TIn, TRes, TIn, TOut, TOut>(stream, name, null, new CrossApplyResourceArgs<TIn, TRes, TIn, TOut, TOut>
            {
                InputValueSelector = i => i,
                OutputValueSelector = i => i,
                ValuesProvider = valuesProvider,
                ResourceStream = resourceStream
            }).Output;
        }
        #endregion

        #region CrossApplyFolderFiles
        public static IStream<string> CrossApplyFolderFiles<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs { RootFolder = getFolderPath(i), SearchPattern = pattern }, i => i.Name);
        }
        public static IStream<string> CrossApplyFolderFiles(this IStream<string> stream, string name, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs { RootFolder = i, SearchPattern = pattern }, i => i.Name);
        }
        public static IStream<string> CrossApplyFolderFiles<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<TIn, string> getSearchPattern, SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs { RootFolder = getFolderPath(i), SearchPattern = getSearchPattern(i) }, i => i.Name);
        }
        public static IStream<TOut> CrossApplyFolderFiles<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<LocalFilesValue, TOut> selector, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs { RootFolder = getFolderPath(i), SearchPattern = pattern }, selector);
        }
        public static IStream<TOut> CrossApplyFolderFiles<TOut>(this IStream<string> stream, string name, Func<LocalFilesValue, TOut> selector, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs { RootFolder = i, SearchPattern = pattern }, selector);
        }
        public static IStream<TOut> CrossApplyFolderFiles<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<TIn, string> getSearchPattern, Func<LocalFilesValue, TOut> selector, SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs { RootFolder = getFolderPath(i), SearchPattern = getSearchPattern(i) }, selector);
        }
        #endregion

        #region CrossApplyTextFile
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<string> stream, string name, ColumnNameFlatFileDescriptor<TOut> args, bool noParallelisation = false)
            where TOut : new()
        {
            return stream.CrossApply(name, new NameMappingFileValuesProvider<string, TOut, TOut>(new NameMappingFileValuesProviderArgs<string, TOut, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(i),
                Mapping = args,
                NoParallelisation = noParallelisation,
                ResultSelector = (i, o) => o
            }), i => i, i => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<Stream> stream, string name, ColumnNameFlatFileDescriptor<TOut> args, bool noParallelisation = false)
            where TOut : new()
        {
            return stream.CrossApply(name, new NameMappingFileValuesProvider<Stream, TOut, TOut>(new NameMappingFileValuesProviderArgs<Stream, TOut, TOut>()
            {
                DataStreamSelector = i => i,
                Mapping = args,
                NoParallelisation = noParallelisation,
                ResultSelector = (i, o) => o
            }), i => i, i => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TIn, TOut>(this IStream<TIn> stream, string name, ColumnNameFlatFileDescriptor<TOut> args, Func<TIn, string> filePathSelector, bool noParallelisation = false)
            where TOut : new()
        {
            return stream.CrossApply(name, new NameMappingFileValuesProvider<TIn, TOut, TOut>(new NameMappingFileValuesProviderArgs<TIn, TOut, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                Mapping = args,
                NoParallelisation = noParallelisation,
                ResultSelector = (i, o) => o
            }), i => i, i => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TIn, TParsed, TOut>(this IStream<TIn> stream, string name, ColumnNameFlatFileDescriptor<TParsed> args, Func<TIn, string> filePathSelector, Func<TIn, TParsed, TOut> resultSelector, bool noParallelisation = false) where TParsed : new()
        {
            return stream.CrossApply(name, new NameMappingFileValuesProvider<TIn, TParsed, TOut>(new NameMappingFileValuesProviderArgs<TIn, TParsed, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                Mapping = args,
                NoParallelisation = noParallelisation,
                ResultSelector = resultSelector
            }), i => i, i => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TParsed, TOut>(this IStream<string> stream, string name, ColumnNameFlatFileDescriptor<TParsed> args, Func<string, TParsed, TOut> resultSelector, bool noParallelisation = false) where TParsed : new()
        {
            return stream.CrossApply(name, new NameMappingFileValuesProvider<string, TParsed, TOut>(new NameMappingFileValuesProviderArgs<string, TParsed, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(i),
                Mapping = args,
                NoParallelisation = noParallelisation,
                ResultSelector = resultSelector
            }), i => i, i => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TParsed, TOut>(this IStream<Stream> stream, string name, ColumnNameFlatFileDescriptor<TParsed> args, Func<TParsed, TOut> resultSelector, bool noParallelisation = false) where TParsed : new()
        {
            return stream.CrossApply(name, new NameMappingFileValuesProvider<Stream, TParsed, TOut>(new NameMappingFileValuesProviderArgs<Stream, TParsed, TOut>()
            {
                DataStreamSelector = i => i,
                Mapping = args,
                NoParallelisation = noParallelisation,
                ResultSelector = (s, o) => resultSelector(o)
            }), i => i, i => i);
        }

        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<string> stream, string name, ColumnIndexFlatFileDescriptor<TOut> args, bool noParallelisation = false)
            where TOut : new()
        {
            return stream.CrossApply(name, new IndexMappingFileValuesProvider<string, TOut, TOut>(new IndexMappingFileValuesProviderArgs<string, TOut, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(i),
                Mapping = args,
                NoParallelisation = noParallelisation,
                ResultSelector = (i, o) => o
            }), i => i, i => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<Stream> stream, string name, ColumnIndexFlatFileDescriptor<TOut> args, bool noParallelisation = false)
            where TOut : new()
        {
            return stream.CrossApply(name, new IndexMappingFileValuesProvider<Stream, TOut, TOut>(new IndexMappingFileValuesProviderArgs<Stream, TOut, TOut>()
            {
                DataStreamSelector = i => i,
                Mapping = args,
                NoParallelisation = noParallelisation,
                ResultSelector = (i, o) => o
            }), i => i, i => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TIn, TOut>(this IStream<TIn> stream, string name, ColumnIndexFlatFileDescriptor<TOut> args, Func<TIn, string> filePathSelector, bool noParallelisation = false)
            where TOut : new()
        {
            return stream.CrossApply(name, new IndexMappingFileValuesProvider<TIn, TOut, TOut>(new IndexMappingFileValuesProviderArgs<TIn, TOut, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                Mapping = args,
                NoParallelisation = noParallelisation,
                ResultSelector = (i, o) => o
            }), i => i, i => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TIn, TParsed, TOut>(this IStream<TIn> stream, string name, ColumnIndexFlatFileDescriptor<TParsed> args, Func<TIn, string> filePathSelector, Func<TIn, TParsed, TOut> resultSelector, bool noParallelisation = false) where TParsed : new()
        {
            return stream.CrossApply(name, new IndexMappingFileValuesProvider<TIn, TParsed, TOut>(new IndexMappingFileValuesProviderArgs<TIn, TParsed, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                Mapping = args,
                NoParallelisation = noParallelisation,
                ResultSelector = resultSelector
            }), i => i, i => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TParsed, TOut>(this IStream<string> stream, string name, ColumnIndexFlatFileDescriptor<TParsed> args, Func<string, TParsed, TOut> resultSelector, bool noParallelisation = false) where TParsed : new()
        {
            return stream.CrossApply(name, new IndexMappingFileValuesProvider<string, TParsed, TOut>(new IndexMappingFileValuesProviderArgs<string, TParsed, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(i),
                Mapping = args,
                NoParallelisation = noParallelisation,
                ResultSelector = resultSelector
            }), i => i, i => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TParsed, TOut>(this IStream<Stream> stream, string name, ColumnIndexFlatFileDescriptor<TParsed> args, Func<TParsed, TOut> resultSelector, bool noParallelisation = false) where TParsed : new()
        {
            return stream.CrossApply(name, new IndexMappingFileValuesProvider<Stream, TParsed, TOut>(new IndexMappingFileValuesProviderArgs<Stream, TParsed, TOut>()
            {
                DataStreamSelector = i => i,
                Mapping = args,
                NoParallelisation = noParallelisation,
                ResultSelector = (s, o) => resultSelector(o)
            }), i => i, i => i);
        }

        public static IStream<string> CrossApplyTextFile(this IStream<string> stream, string name, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new TextFileValuesProvider<string, string>(new TextFileValuesProviderArgs<string, string>()
            {
                DataStreamSelector = i => File.OpenRead(i),
                NoParallelisation = noParallelisation,
                ResultSelector = (i, o) => o
            }), i => i, i => i);
        }
        public static IStream<string> CrossApplyTextFile(this IStream<Stream> stream, string name, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new TextFileValuesProvider<Stream, string>(new TextFileValuesProviderArgs<Stream, string>()
            {
                DataStreamSelector = i => i,
                NoParallelisation = noParallelisation,
                ResultSelector = (i, o) => o
            }), i => i, i => i);
        }
        public static IStream<string> CrossApplyTextFile<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> filePathSelector, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new TextFileValuesProvider<TIn, string>(new TextFileValuesProviderArgs<TIn, string>()
            {
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                NoParallelisation = noParallelisation,
                ResultSelector = (i, o) => o
            }), i => i, i => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, string> filePathSelector, Func<TIn, string, TOut> resultSelector, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new TextFileValuesProvider<TIn, TOut>(new TextFileValuesProviderArgs<TIn, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                NoParallelisation = noParallelisation,
                ResultSelector = resultSelector
            }), i => i, i => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<string> stream, string name, Func<string, string, TOut> resultSelector, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new TextFileValuesProvider<string, TOut>(new TextFileValuesProviderArgs<string, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(i),
                NoParallelisation = noParallelisation,
                ResultSelector = resultSelector
            }), i => i, i => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<Stream> stream, string name, Func<string, TOut> resultSelector, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new TextFileValuesProvider<Stream, TOut>(new TextFileValuesProviderArgs<Stream, TOut>()
            {
                DataStreamSelector = i => i,
                NoParallelisation = noParallelisation,
                ResultSelector = (s, o) => resultSelector(o)
            }), i => i, i => i);
        }
        #endregion

        #region CrossApplyAction
        public static IStream<TOut> CrossApplyAction<TIn, TOut>(this IStream<TIn> stream, string name, Action<TIn, Action<TOut>> valuesProducer, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new ActionValuesProvider<TIn, TOut>(new ActionValuesProviderArgs<TIn, TOut>()
            {
                NoParallelisation = noParallelisation,
                ProduceValues = valuesProducer
            }), i => i, i => i);
        }
        public static IStream<TOut> CrossApplyAction<TIn, TRes, TOut>(this IStream<TIn> stream, string name, IStream<TRes> resourceStream, Action<TIn, TRes, Action<TOut>> valuesProducer, bool noParallelisation = false)
        {
            return stream.CrossApply(name, resourceStream, new ActionResourceValuesProvider<TIn, TRes, TOut>(new ActionResourceValuesProviderArgs<TIn, TRes, TOut>()
            {
                NoParallelisation = noParallelisation,
                ProduceValues = valuesProducer
            }), i => i, i => i);
        }
        #endregion

        #region EnsureKeyed
        public static IKeyedStream<TIn> EnsureKeyed<TIn>(this IStream<TIn> stream, string name, params Expression<Func<TIn, IComparable>>[] sortFields)
        {
            return new EnsureKeyedStreamNode<TIn>(stream, name, null, sortFields.Select(i => new SortCriteria<TIn>(i)).ToList()).Output;
        }
        public static IKeyedStream<TIn> EnsureKeyed<TIn>(this IStream<TIn> stream, string name, params SortCriteria<TIn>[] sortCriterias)
        {
            return new EnsureKeyedStreamNode<TIn>(stream, name, null, sortCriterias).Output;
        }
        public static IKeyedStream<TIn> EnsureKeyed<TIn>(this IStream<TIn> stream, string name, Func<TIn, IEnumerable<SortCriteria<TIn>>> sortCriteriasBuilder)
        {
            return new EnsureKeyedStreamNode<TIn>(stream, name, null, sortCriteriasBuilder(default(TIn)).ToList()).Output;
        }
        public static IKeyedStream<TIn> EnsureKeyed<TIn>(this IStream<TIn> stream, string name, Func<TIn, SortCriteria<TIn>> sortCriteriasBuilder)
        {
            return new EnsureKeyedStreamNode<TIn>(stream, name, null, new[] { sortCriteriasBuilder(default(TIn)) }).Output;
        }
        #endregion

        #region EnsureSorted
        public static ISortedStream<TIn> EnsureSorted<TIn>(this IStream<TIn> stream, string name, params Expression<Func<TIn, IComparable>>[] sortFields)
        {
            return new EnsureSortedStreamNode<TIn>(stream, name, null, sortFields.Select(i => new SortCriteria<TIn>(i)).ToList()).Output;
        }
        public static ISortedStream<TIn> EnsureSorted<TIn>(this IStream<TIn> stream, string name, params SortCriteria<TIn>[] sortCriterias)
        {
            return new EnsureSortedStreamNode<TIn>(stream, name, null, sortCriterias).Output;
        }
        public static ISortedStream<TIn> EnsureSorted<TIn>(this IStream<TIn> stream, string name, Func<TIn, IEnumerable<SortCriteria<TIn>>> sortCriteriasBuilder)
        {
            return new EnsureSortedStreamNode<TIn>(stream, name, null, sortCriteriasBuilder(default(TIn)).ToList()).Output;
        }
        public static ISortedStream<TIn> EnsureSorted<TIn>(this IStream<TIn> stream, string name, Func<TIn, SortCriteria<TIn>> sortCriteriasBuilder)
        {
            return new EnsureSortedStreamNode<TIn>(stream, name, null, new[] { sortCriteriasBuilder(default(TIn)) }).Output;
        }
        #endregion

        #region LeftJoin
        public static IStream<TOut> LeftJoin<TInLeft, TInRight, TOut>(this ISortedStream<TInLeft> leftStream, string name, IKeyedStream<TInRight> rightStream, Func<TInLeft, TInRight, TOut> resultSelector)
        {
            return new JoinStreamNode<TInLeft, TInRight, TOut>(leftStream, name, null, new JoinArgs<TInLeft, TInRight, TOut>
            {
                RightInputStream = rightStream,
                ResultSelector = resultSelector,
                RedirectErrorsInsteadOfFail = false
            }).Output;
        }
        public static INodeOutputError<TOut, TInLeft, TInRight> LeftJoinKeepErrors<TInLeft, TInRight, TOut>(this ISortedStream<TInLeft> leftStream, string name, IKeyedStream<TInRight> rightStream, Func<TInLeft, TInRight, TOut> resultSelector)
        {
            var ret = new JoinStreamNode<TInLeft, TInRight, TOut>(leftStream, name, null, new JoinArgs<TInLeft, TInRight, TOut>
            {
                RightInputStream = rightStream,
                ResultSelector = resultSelector,
                RedirectErrorsInsteadOfFail = true
            });
            return new NodeOutputError<JoinStreamNode<TInLeft, TInRight, TOut>, TOut, TInLeft, TInRight>(ret);
        }
        #endregion

        #region Merge
        public static IStream<I> Merge<I>(this IStream<I> stream, string name, IStream<I> inputStream2)
        {
            return new MergeStreamNode<I>(stream, name, null, new MergeArgs<I> { SecondStream = inputStream2 }).Output;
        }
        #endregion

        #region Select
        public static IStream<TOut> Select<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, TOut> mapper)
        {
            return new SelectStreamNode<TIn, TOut>(stream, name, null, new SelectArgs<TIn, TOut>
            {
                Mapper = mapper,
                RedirectErrorsInsteadOfFail = false
            }).Output;
        }
        public static IStream<TOut> Select<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, int, TOut> mapper)
        {
            return new SelectStreamNode<TIn, TOut>(stream, name, null, new SelectArgs<TIn, TOut>
            {
                IndexMapper = mapper,
                RedirectErrorsInsteadOfFail = false
            }).Output;
        }
        public static INodeOutputError<TOut, TIn> SelectKeepErrors<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, TOut> mapper)
        {
            var ret = new SelectStreamNode<TIn, TOut>(stream, name, null, new SelectArgs<TIn, TOut>
            {
                Mapper = mapper,
                RedirectErrorsInsteadOfFail = true
            });
            return new NodeOutputError<SelectStreamNode<TIn, TOut>, TOut, TIn>(ret);
        }
        public static INodeOutputError<TOut, TIn> SelectKeepErrors<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, int, TOut> mapper)
        {
            var ret = new SelectStreamNode<TIn, TOut>(stream, name, null, new SelectArgs<TIn, TOut>
            {
                IndexMapper = mapper,
                RedirectErrorsInsteadOfFail = true
            });
            return new NodeOutputError<SelectStreamNode<TIn, TOut>, TOut, TIn>(ret);
        }
        #endregion

        #region Skip
        public static IStream<TIn> Skip<TIn>(this IStream<TIn> stream, string name, int count)
        {
            return new SkipStreamNode<TIn>(stream, name, null, count).Output;
        }
        public static ISortedStream<TIn> Skip<TIn>(this ISortedStream<TIn> stream, string name, int count)
        {
            return new SkipSortedStreamNode<TIn>(stream, name, null, count).Output;
        }
        public static IKeyedStream<TIn> Skip<TIn>(this IKeyedStream<TIn> stream, string name, int count)
        {
            return new SkipKeyedStreamNode<TIn>(stream, name, null, count).Output;
        }
        #endregion

        #region Sort
        public static ISortedStream<TIn> Sort<TIn>(this IStream<TIn> stream, string name, params Expression<Func<TIn, IComparable>>[] sortFields)
        {
            return new SortStreamNode<TIn>(stream, name, null, sortFields.Select(i => new Core.SortCriteria<TIn>(i)).ToList()).Output;
        }
        public static ISortedStream<TIn> Sort<TIn>(this IStream<TIn> stream, string name, params Core.SortCriteria<TIn>[] sortCriterias)
        {
            return new SortStreamNode<TIn>(stream, name, null, sortCriterias).Output;
        }
        public static ISortedStream<TIn> Sort<TIn>(this IStream<TIn> stream, string name, Func<TIn, IEnumerable<Core.SortCriteria<TIn>>> sortCriteriasBuilder)
        {
            return new SortStreamNode<TIn>(stream, name, null, sortCriteriasBuilder(default(TIn)).ToList()).Output;
        }
        public static ISortedStream<TIn> Sort<TIn>(this IStream<TIn> stream, string name, Func<TIn, Core.SortCriteria<TIn>> sortCriteriasBuilder)
        {
            return new SortStreamNode<TIn>(stream, name, null, new[] { sortCriteriasBuilder(default(TIn)) }).Output;
        }
        #endregion

        #region ToAction
        public static IStream<TIn> ToAction<TIn>(this IStream<TIn> stream, string name, Action<TIn> action)
        {
            return new ToActionStreamNode<TIn>(stream, name, null, action).Output;
        }
        public static IStream<TOut> ToAction<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, TOut> action)
        {
            return new ToActionStreamNode<TIn, TOut>(stream, name, null, action).Output;
        }
        public static IStream<TIn> ToAction<TIn, TRes>(this IStream<TIn> stream, string name, IStream<TRes> resourceStream, Action<TIn, TRes> action)
        {
            return new ToActionResourceStreamNode<TIn, TRes>(stream, name, null, new ToActionArgs<TIn, TRes>
            {
                Action = action,
                ResourceStream = resourceStream
            }).Output;
        }
        public static IStream<TOut> ToAction<TIn, TRes, TOut>(this IStream<TIn> stream, string name, IStream<TRes> resourceStream, Func<TIn, TRes, TOut> action)
        {
            return new ToActionResourceStreamNode<TIn, TRes, TOut>(stream, name, null, new ToActionArgs<TIn, TRes, TOut>
            {
                Action = action,
                ResourceStream = resourceStream
            }).Output;
        }
        #endregion

        #region ToTextFile
        public static IStream<TIn> ToTextFile<TIn>(this IStream<TIn> stream, string name, IStream<SystemIO.StreamWriter> resourceStream, ColumnIndexFlatFileDescriptor<TIn> mapping) where TIn : new()
        {
            return new ToIndexMappingFileStreamNode<TIn>(stream, name, null, new ToIndexMappingFileArgs<TIn>
            {
                Mapping = mapping,
                ResourceStream = resourceStream
            }).Output;
        }
        public static IStream<TIn> ToTextFile<TIn>(this IStream<TIn> stream, string name, IStream<SystemIO.StreamWriter> resourceStream, ColumnNameFlatFileDescriptor<TIn> mapping) where TIn : new()
        {
            return new ToNameMappingFileStreamNode<TIn>(stream, name, null, new ToNameMappingFileArgs<TIn>
            {
                Mapping = mapping,
                ResourceStream = resourceStream
            }).Output;
        }
        #endregion

        #region Top
        public static ISortedStream<TIn> Top<TIn>(this ISortedStream<TIn> stream, string name, int count)
        {
            return new TopSortedStreamNode<TIn>(stream, name, null, count).Output;
        }
        public static IKeyedStream<TIn> Top<TIn>(this IKeyedStream<TIn> stream, string name, int count)
        {
            return new TopKeyedStreamNode<TIn>(stream, name, null, count).Output;
        }
        public static IStream<TIn> Top<TIn>(this IStream<TIn> stream, string name, int count)
        {
            return new TopStreamNode<TIn>(stream, name, null, count).Output;
        }
        #endregion

        #region UseResource
        public static IStream<TOut> UseResource<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, TOut> mapper) where TOut : IDisposable
        {
            return new UseResourceStreamNode<TIn, TOut>(stream, name, null, new CreateResourceArgs<TIn, TOut>
            {
                Mapper = mapper,
                RedirectErrorsInsteadOfFail = false
            }).Output;
        }
        public static INodeOutputError<TOut, TIn> UseResourceKeepErrors<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, TOut> mapper) where TOut : IDisposable
        {
            var ret = new UseResourceStreamNode<TIn, TOut>(stream, name, null, new CreateResourceArgs<TIn, TOut>
            {
                Mapper = mapper,
                RedirectErrorsInsteadOfFail = true
            });
            return new NodeOutputError<UseResourceStreamNode<TIn, TOut>, TOut, TIn>(ret);
        }
        #endregion

        #region Where
        public static IKeyedStream<TIn> Where<TIn>(this IKeyedStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            return new WhereKeyedStreamNode<TIn>(stream, name, null, new WhereArgs<TIn>
            {
                Predicate = predicate,
                RedirectErrorsInsteadOfFail = false
            }).Output;
        }
        public static IKeyedNodeOutputError<TIn, TIn> WhereKeepErrors<TIn>(this IKeyedStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            var ret = new WhereKeyedStreamNode<TIn>(stream, name, null, new WhereArgs<TIn>
            {
                Predicate = predicate,
                RedirectErrorsInsteadOfFail = true
            });
            return new KeyedNodeOutputError<WhereKeyedStreamNode<TIn>, TIn, TIn>(ret);
        }
        public static ISortedStream<TIn> Where<TIn>(this ISortedStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            return new WhereSortedStreamNode<TIn>(stream, name, null, new WhereArgs<TIn>
            {
                Predicate = predicate,
                RedirectErrorsInsteadOfFail = false
            }).Output;
        }
        public static ISortedNodeOutputError<TIn, TIn> WhereKeepErrors<TIn>(this ISortedStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            var ret = new WhereSortedStreamNode<TIn>(stream, name, null, new WhereArgs<TIn>
            {
                Predicate = predicate,
                RedirectErrorsInsteadOfFail = true
            });
            return new SortedNodeOutputError<WhereSortedStreamNode<TIn>, TIn, TIn>(ret);
        }
        public static IStream<TIn> Where<TIn>(this IStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            return new WhereStreamNode<TIn>(stream, name, null, new WhereArgs<TIn>
            {
                Predicate = predicate,
                RedirectErrorsInsteadOfFail = false
            }).Output;
        }
        public static INodeOutputError<TIn, TIn> WhereKeepErrors<TIn>(this IStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            var ret = new WhereStreamNode<TIn>(stream, name, null, new WhereArgs<TIn>
            {
                Predicate = predicate,
                RedirectErrorsInsteadOfFail = true
            });
            return new NodeOutputError<WhereStreamNode<TIn>, TIn, TIn>(ret);
        }
        #endregion

        #region CombineLatest
        public static IStream<TOut> CombineLatest<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, IStream<TIn2> inputStream2, Func<TIn1, TIn2, TOut> resultSelector)
        {
            return new CombineLatestStreamNode<TIn1, TIn2, TOut>(stream, name, null, new CombineLatestArgs<TIn1, TIn2, TOut>
            {
                InputStream2 = inputStream2,
                ResultSelector = resultSelector,
                RedirectErrorsInsteadOfFail = false
            }).Output;
        }
        public static INodeOutputError<TOut, TIn1, TIn2> CombineLatestKeepErrors<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, IStream<TIn2> inputStream2, Func<TIn1, TIn2, TOut> resultSelector)
        {
            var ret = new CombineLatestStreamNode<TIn1, TIn2, TOut>(stream, name, null, new CombineLatestArgs<TIn1, TIn2, TOut>
            {
                InputStream2 = inputStream2,
                ResultSelector = resultSelector,
                RedirectErrorsInsteadOfFail = true
            });
            return new NodeOutputError<CombineLatestStreamNode<TIn1, TIn2, TOut>, TOut, TIn1, TIn2>(ret);
        }
        #endregion
    }
}
