using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    public class WaitWhenDoneArgs<TOut, TOutStream> where TOutStream : IStream<TOut>
    {
        public IStream<object> Input1ToWait { get; set; }
        public IStream<object> Input2ToWait { get; set; }
        public IStream<object> Input3ToWait { get; set; }
        public IStream<object> Input4ToWait { get; set; }
        public IStream<object> Input5ToWait { get; set; }
        public IStream<object> Input6ToWait { get; set; }
        public IStream<object> Input7ToWait { get; set; }
        public IStream<object> Input8ToWait { get; set; }
        public IStream<object> Input9ToWait { get; set; }
        public IStream<object> Input10ToWait { get; set; }
        public TOutStream Input { get; set; }
    }
    public class WaitWhenDoneStreamNode<TOut, TOutStream> : StreamNodeBase<TOut, TOutStream, WaitWhenDoneArgs<TOut, TOutStream>> where TOutStream : IStream<TOut>
    {
        public WaitWhenDoneStreamNode(string name, WaitWhenDoneArgs<TOut, TOutStream> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override TOutStream CreateOutputStream(WaitWhenDoneArgs<TOut, TOutStream> args)
        {
            var outputStream = args.Input.Observable;
            if (args.Input1ToWait != null)
                outputStream = outputStream.CombineWithLatest(args.Input1ToWait.Observable.Map(i => new object()).Completed(), (i, _) => i, true);
            if (args.Input2ToWait != null)
                outputStream = outputStream.CombineWithLatest(args.Input2ToWait.Observable.Map(i => new object()).Completed(), (i, _) => i, true);
            if (args.Input3ToWait != null)
                outputStream = outputStream.CombineWithLatest(args.Input3ToWait.Observable.Map(i => new object()).Completed(), (i, _) => i, true);
            if (args.Input4ToWait != null)
                outputStream = outputStream.CombineWithLatest(args.Input4ToWait.Observable.Map(i => new object()).Completed(), (i, _) => i, true);
            if (args.Input5ToWait != null)
                outputStream = outputStream.CombineWithLatest(args.Input5ToWait.Observable.Map(i => new object()).Completed(), (i, _) => i, true);
            if (args.Input6ToWait != null)
                outputStream = outputStream.CombineWithLatest(args.Input6ToWait.Observable.Map(i => new object()).Completed(), (i, _) => i, true);
            if (args.Input7ToWait != null)
                outputStream = outputStream.CombineWithLatest(args.Input7ToWait.Observable.Map(i => new object()).Completed(), (i, _) => i, true);
            if (args.Input8ToWait != null)
                outputStream = outputStream.CombineWithLatest(args.Input8ToWait.Observable.Map(i => new object()).Completed(), (i, _) => i, true);
            if (args.Input9ToWait != null)
                outputStream = outputStream.CombineWithLatest(args.Input9ToWait.Observable.Map(i => new object()).Completed(), (i, _) => i, true);
            if (args.Input10ToWait != null)
                outputStream = outputStream.CombineWithLatest(args.Input10ToWait.Observable.Map(i => new object()).Completed(), (i, _) => i, true);
            return base.CreateMatchingStream(outputStream, args.Input);
        }
    }
}
