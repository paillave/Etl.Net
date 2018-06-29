using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public abstract class OutputErrorStreamNodeBase<O, E> : OutputStreamNodeBase<O>
    {
        public OutputErrorStreamNodeBase(ExecutionContextBase context, string name, IEnumerable<string> parentNodeNamePath = null) : base(context, name, parentNodeNamePath)
        {
        }
        public OutputErrorStreamNodeBase(IContextual contextual, string name, IEnumerable<string> parentNodeNamePath = null) : base(contextual.Context, name, parentNodeNamePath)
        {
        }
        protected virtual void CreateErrorStream(IObservable<E> observable)
        {
            this.Error = this.CreateStream<E>(nameof(Error), observable);
        }
        public IStream<E> Error { get; private set; }
        protected Func<I, ErrorManagementItem<I, O2>> ErrorManagementWrapFunction<I, O2>(Func<I, O2> call)
        {
            return (I input) =>
            {
                try
                {
                    return new ErrorManagementItem<I, O2>(input, call(input));
                }
                catch (Exception ex)
                {
                    return new ErrorManagementItem<I, O2>(input, ex);
                }
            };
        }
        protected Func<I, I2, ErrorManagementItem<I, O>> ErrorManagementWrapFunction<I, I2>(Func<I, I2, O> call)
        {
            return (I input, I2 input2) =>
            {
                try
                {
                    return new ErrorManagementItem<I, I2, O>(input, input2, call(input, input2));
                }
                catch (Exception ex)
                {
                    return new ErrorManagementItem<I, I2, O>(input, input2, ex);
                }
            };
        }
    }
    public class ErrorRow<I>
    {
        public ErrorRow(I input, Exception exception)
        {
            this.Input = input;
            this.Exception = exception;
        }

        public I Input { get; }
        public Exception Exception { get; }
    }
    public class ErrorManagementItem<I, I2, O> : ErrorManagementItem<I, O>
    {
        public I2 Input2 { get; }
        public ErrorManagementItem(I input, I2 input2, Exception exception) : base(input, exception)
        {
            this.Input2 = Input2;
        }
        public ErrorManagementItem(I input, I2 input2, O output) : base(input, output)
        {
            this.Input2 = Input2;
        }
    }
    public class ErrorManagementItem<I, O>
    {
        //public ErrorManagementItem(I input, int index, Exception exception) : this(input, exception)
        //{
        //    this.Index = index;
        //}
        public ErrorManagementItem(I input, Exception exception)
        {
            this.Input = input;
            this.Output = default(O);
            this.Exception = exception;
            OnException = true;
        }
        public ErrorManagementItem(I input, O output)
        {
            this.Input = input;
            this.Output = output;
            this.Exception = null;
            OnException = false;
        }
        public bool OnException { get; }
        public I Input { get; }
        public O Output { get; }
        public Exception Exception { get; }
    }
}
