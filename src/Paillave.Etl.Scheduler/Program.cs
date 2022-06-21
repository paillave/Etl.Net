// https://codeburst.io/schedule-cron-jobs-using-hostedservice-in-asp-net-core-e17c47ba06
using Paillave.Etl.Scheduler;
Console.WriteLine("Starting...");
using (var emitter = TickEmitter.Create(new { CronExpression = @"@every_second", BatchId = 15 }, i => i.CronExpression))
using (var subscription = emitter.Subscribe(i => Console.WriteLine($"{DateTime.Now: ss fff}")))
{
    emitter.Start();
    // Thread.Sleep(5000);
    // emitter.UpdateEmitter(new { CronExpression = "* * * * *", BatchId = 16 });
    Console.WriteLine("Press any key to stop...");
    Console.ReadKey();
}

// using (var emitter = TickEmitter.Create(new { CronExpression = @"@every_second", BatchId = 15 }, i => i.CronExpression))
// using (var subscription = emitter.Subscribe(i => Console.WriteLine(i.BatchId)))
// {
//     emitter.Start();
//     Thread.Sleep(5000);
//     emitter.UpdateEmitter(new { CronExpression = "* * * * *", BatchId = 16 });
//     Thread.Sleep(3 * 60 * 1000);
// }
// using (var manager = TickEmitterManager.Create((Element i) => i.Id, i => Console.WriteLine($"{DateTime.Now: mm ss} - {i.Id}"), i => i.CronExpression))
// {
//     manager.SynchronizeEmitters(new[]{
//         new Element{ CronExpression="@every_second", Id=1},
//         new Element{ CronExpression="@every_second", Id=2},
//         new Element{ CronExpression="@every_second", Id=3},
//         new Element{ CronExpression="@every_second", Id=4},
//         new Element{ CronExpression="@every_second", Id=5},
//      });
//     Thread.Sleep(3 * 60 * 1000);
// }
public class Element
{
    public int Id { get; set; }
    public string CronExpression { get; set; }
}
