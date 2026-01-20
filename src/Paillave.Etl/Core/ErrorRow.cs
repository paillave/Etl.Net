using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core;

public class ErrorRow<TIn>(TIn input, Exception exception)
{
    public ErrorRow(IErrorManagementItem<TIn> errorManagementItem)
        : this(errorManagementItem.Input, errorManagementItem.Exception) { }

    public TIn Input { get; } = input;
    public Exception Exception { get; } = exception;
}

public class ErrorRow<TIn1, TIn2> : ErrorRow<TIn1>
{
    public ErrorRow(IErrorManagementItem<TIn1, TIn2> errorManagementItem)
        : this(errorManagementItem.Input, errorManagementItem.Input2, errorManagementItem.Exception) { }

    private ErrorRow(TIn1 input1, TIn2 input2, Exception exception) : base(input1, exception)
    {
        this.Input2 = input2;
    }

    public TIn2 Input2 { get; }
}
