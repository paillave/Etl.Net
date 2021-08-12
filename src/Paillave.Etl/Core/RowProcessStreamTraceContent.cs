using System.Diagnostics;

namespace Paillave.Etl.Core
{
    public class RowProcessStreamTraceContent : StreamTraceContentBase
    {
        public RowProcessStreamTraceContent(int position, int? averageDuration, object row)
        {
            this.Position = position;
            this.Row = row;
            this.AverageDuration = averageDuration;
        }
        public override TraceLevel Level => TraceLevel.Verbose;

        public int Position { get; }
        public int? AverageDuration { get; }
        public object Row { get; }

        public override string Message => $"row {Position} processing (avg:{AverageDuration ?? 0} ms)";
    }
}
