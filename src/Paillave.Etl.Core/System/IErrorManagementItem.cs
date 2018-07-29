using System;

namespace Paillave.Etl.Core.System
{
    public interface IErrorManagementItem<TIn1, TIn2>: IErrorManagementItem<TIn1>
    {
        TIn2 Input2 { get; }
    }
    public interface IErrorManagementItem<TIn>
    {
        Exception Exception { get; }
        TIn Input { get; }
    }
}