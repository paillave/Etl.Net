using System;
using System.Text;

namespace Paillave.Etl.Core;

public static class ExceptionEx
{
    public static string GetFullMessage(this Exception exception)
    {
        StringBuilder stringBuilder = new();
        AppendExceptionTexts(0, stringBuilder, exception);
        return stringBuilder.ToString();
    }
    private static void AppendExceptionTexts(int level, StringBuilder stringBuilder, Exception exception)
    {
        stringBuilder.AppendLine($"{new String('\t', level)}{exception.Message}");
        if (exception.InnerException != null)
            AppendExceptionTexts(level + 1, stringBuilder, exception.InnerException);
    }
}