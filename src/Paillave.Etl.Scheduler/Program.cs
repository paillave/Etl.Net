// https://codeburst.io/schedule-cron-jobs-using-hostedservice-in-asp-net-core-e17c47ba06
using Paillave.Etl.Scheduler;
Console.WriteLine("Starting...");
using (var emitter = TickEmitter.Create(new { CronExpression = @"@every_second", BatchId = 15 }, i => i.CronExpression))
using (var subscription = emitter.Subscribe(i => Console.WriteLine(i.BatchId)))
{
    emitter.Start();
    Thread.Sleep(5000);
    emitter.UpdateEmitter(new { CronExpression = "* * * * *", BatchId = 16 });
    Thread.Sleep(3 * 60 * 1000);
}
