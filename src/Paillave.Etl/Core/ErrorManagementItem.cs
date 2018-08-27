using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core
{
    public class ErrorManagementItem<TIn1, TIn2, TOut> : ErrorManagementItem<TIn1, TOut>, IErrorManagementItem<TIn1, TIn2>
    {
        public TIn2 Input2 { get; }
        public ErrorManagementItem(TIn1 input, TIn2 input2, Exception exception) : base(input, exception)
        {
            this.Input2 = Input2;
        }
        public ErrorManagementItem(TIn1 input, TIn2 input2, TOut output) : base(input, output)
        {
            this.Input2 = Input2;
        }
    }
    public class ErrorManagementItem<TIn, TOut> : IErrorManagementItem<TIn>
    {
        public ErrorManagementItem(TIn input, Exception exception)
        {
            this.Input = input;
            this.Output = default(TOut);
            this.Exception = exception;
            OnException = true;
        }
        public ErrorManagementItem(TIn input, TOut output)
        {
            this.Input = input;
            this.Output = output;
            this.Exception = null;
            OnException = false;
        }
        public bool OnException { get; }
        public TIn Input { get; }
        public TOut Output { get; }
        public Exception Exception { get; }
    }
}
