using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Paillave.Etl.ExecutionToolkit.ConsoleApp;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Core.TraceContents;
using Paillave.Etl.Extensions;

namespace Paillave.Etl.ExecutionToolkit
{
    public class SimpleConsoleExecutionDisplay : TraceReporterBase
    {
        private Dictionary<string, TaskUnit> _taskUnits;
        private Stopwatch _stopwatch = new Stopwatch();

        public SimpleConsoleExecutionDisplay(Dictionary<string, TaskUnit> taskUnits, Stopwatch stopwatch)
        {
            _taskUnits = taskUnits;
            _stopwatch = stopwatch;
        }

        protected override void HandleCounterSummary(TraceEvent traceEvent, CounterSummaryStreamTraceContent counterSummary)
        {
            var taskUnit = this._taskUnits[traceEvent.NodeName];
            taskUnit.Issued = counterSummary.Counter;
            taskUnit.Status = TaskUnitStatus.Done;
            Console.WriteLine(taskUnit);
        }
        protected override void HandleUnhandledException(TraceEvent traceEvent, UnhandledExceptionStreamTraceContent rowProcess)
        {
            var taskUnit = this._taskUnits[traceEvent.NodeName];
            taskUnit.Status = TaskUnitStatus.Error;
        }
        protected override void HandleRowProcess(TraceEvent traceEvent, RowProcessStreamTraceContent rowProcess)
        {
            var taskUnit = this._taskUnits[traceEvent.NodeName];
            taskUnit.AvgDuration = rowProcess.AverageDuration;
            taskUnit.Issued = rowProcess.Position;
            if (taskUnit.Status != TaskUnitStatus.Processing)
            {
                taskUnit.Status = TaskUnitStatus.Processing;
                Console.WriteLine(taskUnit);
            }
        }
        public override void Initialize(JobDefinitionStructure jobDefinitionStructure)
        {
            Console.WriteLine("Running processes...");
            _stopwatch.Start();
            this._taskUnits = jobDefinitionStructure.Nodes.Select(node => new TaskUnit
            {
                Type = node.TypeName,
                Node = node.NodeName
            }).ToDictionary(i => i.Node);
        }
        // public void HandleTraces(IStream<TraceEvent> traceStream, ISingleStream<ProcessContext> contextStream) => traceStream
        //     .Where("remove verbose", i => i.Content.Level != TraceLevel.Verbose)
        //     // .ThroughAction("trace to debug", i => Debug.WriteLine(i))
        //     .ThroughAction("trace to console", i =>
        //     {
        //         var currentColor = Console.ForegroundColor;
        //         Console.ForegroundColor = GetTraceLevelColor(i.Content.Level) ?? currentColor;
        //         Console.WriteLine(i);
        //     });

        // private static ConsoleColor? GetTraceLevelColor(TraceLevel traceLevel)
        // {
        //     switch (traceLevel)
        //     {
        //         case TraceLevel.Error: return ConsoleColor.Red;
        //         case TraceLevel.Warning: return ConsoleColor.Yellow;
        //         case TraceLevel.Info: return ConsoleColor.Blue;
        //         case TraceLevel.Verbose: return ConsoleColor.Gray;
        //         default: return null;
        //     }
        // }

        // public void SetTasks(List<TaskRunner> tasks)
        // {
        // }

        public override void Dispose()
        {
            _stopwatch.Stop();
            TimeSpan ts = _stopwatch.Elapsed;
            Console.WriteLine($"Execution done in {ts.Hours:00}:{ts.Minutes:00} {ts.Seconds:00}.{ts.Milliseconds / 10:00}");
        }
    }
}