// using Microsoft.Extensions.Hosting;
// using Paillave.Etl.Core;

// namespace Paillave.Etl.Scheduler;

// public class SchedulerBackgroundService<TSource, TKey> : BackgroundService where TKey : IEquatable<TKey>
// {
//     private readonly ITickSourceConnection<TSource, TKey> _tickSourceConnection;
//     private readonly TickSourceManager<TSource, TKey> _tickSourceManager;

//     public SchedulerBackgroundService(ITickSourceConnection<TSource, TKey> tickSourceConnection)
//     {
//         _tickSourceConnection = tickSourceConnection;
//     }

//     protected override Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         throw new NotImplementedException();
//     }
//     public override Task StartAsync(CancellationToken cancellationToken)
//     {
//             var executionOptions = new ExecutionOptions<List<Security>>
//             {
//                 Resolver = new SimpleDependencyResolver().Register<DbContext>(context),
//             };

//             var processRunner = StreamProcessRunner.Create<List<Security>>(i =>
//             {
//                 i.CrossApply("zser", i => i)
//                 .EfCoreSave("qsdf", i => i.SeekOn(j => j.InternalCode));
//             });
//             var res = processRunner.ExecuteAsync(secus, executionOptions).Result;

//         return base.StartAsync(cancellationToken);
//     }
// }
