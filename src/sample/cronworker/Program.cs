using cronworker.Jobs;
using cronworker.CronJobService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddCronJob<SampleCronJob1>(
            o => o.CronExpression = "*/5 * * * *");
    })
    .Build();

host.Run();
