using System;
using System.Collections.Generic;

using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Paillave.Etl.ExecutionToolkit.ConsoleApp;
using Paillave.Etl.Core;
using Terminal.Gui;

namespace Paillave.Etl.ExecutionToolkit;

public class AdvancedConsoleExecutionDisplay : TraceReporterBase
{
    static AdvancedConsoleExecutionDisplay()
    {
        Application.Init();
    }
    private readonly System.Diagnostics.Stopwatch _stopwatch = new();
    private bool _dirty = false;
    private readonly App _consoleApp;
    private readonly Application.RunState _runToken;
    private Dictionary<string, TaskUnit> _taskUnits;
    private readonly System.Timers.Timer _timer;
    public AdvancedConsoleExecutionDisplay()
    {
        _consoleApp = new App();
        _runToken = Application.Begin(_consoleApp);
        Task.Run(() => Application.RunLoop(_runToken));
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += RefreshData;
        _timer.AutoReset = true;
        _timer.Enabled = true;
    }
    private void RefreshData(Object source, ElapsedEventArgs e)
    {
        if (!_dirty) return;
        // Application.Refresh();
        _consoleApp.DataRefreshed();
    }

    protected override void HandleCounterSummary(TraceEvent traceEvent, CounterSummaryStreamTraceContent counterSummary)
    {
        var taskUnit = this._taskUnits[traceEvent.NodeName];
        taskUnit.Issued = counterSummary.Counter;
        taskUnit.Status = TaskUnitStatus.Done;
        _dirty = true;
    }
    protected override void HandleRowProcess(TraceEvent traceEvent, RowProcessStreamTraceContent rowProcess)
    {
        var taskUnit = this._taskUnits[traceEvent.NodeName];
        taskUnit.AvgDuration = rowProcess.AverageDuration;
        taskUnit.Issued = rowProcess.Position;
        taskUnit.Status = TaskUnitStatus.Processing;
        _dirty = true;
    }
    protected override void HandleUnhandledException(TraceEvent traceEvent, UnhandledExceptionStreamTraceContent rowProcess)
    {
        var taskUnit = this._taskUnits[traceEvent.NodeName];
        taskUnit.Status = TaskUnitStatus.Error;
        _dirty = true;
    }
    public override void Initialize(JobDefinitionStructure jobDefinitionStructure)
    {
        this._taskUnits = jobDefinitionStructure.Nodes.Select(node => new TaskUnit
        {
            Type = node.TypeName,
            Node = node.NodeName
        }).ToDictionary(i => i.Node);
        _stopwatch.Start();
        _consoleApp.SetData(this._taskUnits.Values.ToList());
        _timer.Start();
    }
    public override void Dispose()
    {
        Application.End(_runToken);
        Application.UngrabMouse();
        Application.Shutdown();
        Application.RequestStop();
        _stopwatch.Stop();
        TimeSpan ts = _stopwatch.Elapsed;
        Console.WriteLine($"Execution done in {ts.Hours:00}:{ts.Minutes:00} {ts.Seconds:00}.{ts.Milliseconds / 10:00}");
        _timer.Stop();
        _timer.Dispose();
    }
}