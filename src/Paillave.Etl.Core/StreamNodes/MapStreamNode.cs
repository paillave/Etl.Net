using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class MapStreamNode<I, O> : OutputErrorStreamNodeBase<O, ErrorRow<I>>
    {
        public MapStreamNode(IStream<I> inputStream, Func<I, O> mapper, string name, bool redirectErrorsInsteadOfFail, IEnumerable<string> parentsName = null) : base(inputStream, name, parentsName)
        {
            //http://www.introtorx.com/Content/v1.0.10621.0/11_AdvancedErrorHandling.html#Catch
            if (redirectErrorsInsteadOfFail)
            {
                var errorManagedResult = inputStream.Observable.Select(base.ErrorManagementWrapFunction(mapper));
                this.CreateOutputStream(errorManagedResult.Where(i => !i.OnException).Select(i => i.Output));
                this.CreateErrorStream(errorManagedResult.Where(i => i.OnException).Select(i => new ErrorRow<I>(i.Input, i.Exception)));
            }
            else
                this.CreateOutputStream(inputStream.Observable.Select(mapper));
        }
        public MapStreamNode(IStream<I> inputStream, Func<I, int, O> mapper, string name, bool redirectErrorsInsteadOfFail, IEnumerable<string> parentsName = null) : base(inputStream, name, parentsName)
        {
            if (redirectErrorsInsteadOfFail)
            {
                var errorManagedResult = inputStream.Observable.Select(base.ErrorManagementWrapFunction(mapper));
                this.CreateOutputStream(errorManagedResult.Where(i => !i.OnException).Select(i => i.Output));
                this.CreateErrorStream(errorManagedResult.Where(i => i.OnException).Select(i => new ErrorRow<I>(i.Input, i.Exception)));
            }
            else
                this.CreateOutputStream(inputStream.Observable.Select(mapper));
        }
    }
    public static partial class StreamEx
    {
        public static IStream<O> Map<I, O>(this IStream<I> stream, string name, Func<I, O> mapper)
        {
            return new MapStreamNode<I, O>(stream, mapper, name, false).Output;
        }
        public static IStream<O> Map<I, O>(this IStream<I> stream, string name, Func<I, int, O> mapper)
        {
            return new MapStreamNode<I, O>(stream, mapper, name, false).Output;
        }
        public static NodeOutput<O, I> MapKeepErrors<I, O>(this IStream<I> stream, string name, Func<I, O> mapper)
        {
            var ret= new MapStreamNode<I, O>(stream, mapper, name, true);
            return new NodeOutput<O, I>(ret.Output, ret.Error);
        }
        public static NodeOutput<O, I> MapKeepErrors<I, O>(this IStream<I> stream, string name, Func<I, int, O> mapper)
        {
            var ret = new MapStreamNode<I, O>(stream, mapper, name, true);
            return new NodeOutput<O, I>(ret.Output, ret.Error);
        }
    }
}
