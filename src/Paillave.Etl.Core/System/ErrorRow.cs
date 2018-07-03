using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.System
{
    public class ErrorRow<TIn>
    {
        public ErrorRow(TIn input, Exception exception)
        {
            this.Input = input;
            this.Exception = exception;
        }

        public TIn Input { get; }
        public Exception Exception { get; }
    }

    public class ErrorRow<TIn1, TIn2> : ErrorRow<TIn1>
    {
        public ErrorRow(TIn1 input1, TIn2 input2, Exception exception) : base(input1, exception)
        {
            this.Input2 = input2;
        }

        public TIn2 Input2 { get; }
    }
}
