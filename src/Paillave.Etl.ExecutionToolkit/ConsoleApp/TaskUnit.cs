using System.Collections.Generic;

namespace Paillave.Etl.ExecutionToolkit.ConsoleApp;

public enum TaskUnitStatus
{
    Awaiting,
    Processing,
    Done,
    Error
}
public class TaskUnit
{
    private static readonly Dictionary<TaskUnitStatus, char> taskUnitStatusGlyph = new()
    {
        [TaskUnitStatus.Awaiting] = ' ',
        [TaskUnitStatus.Processing] = '-',
        [TaskUnitStatus.Done] = 'x',
        [TaskUnitStatus.Error] = '!'
    };
    public TaskUnitStatus Status { get; set; } = TaskUnitStatus.Awaiting;
    public string Node { get; set; }
    public string Type { get; set; }
    public int Issued { get; set; }
    public int? AvgDuration { get; set; }
    public override string ToString() => $"[{taskUnitStatusGlyph[Status]}] {Issued,9} row(s) - {AvgDuration?.ToString() ?? "-",4}ms - {Type}: {Node}";
}