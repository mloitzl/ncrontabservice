using cronworker.CronJobService;

namespace cronworker.Jobs;

public class SampleCronJob1(ISchedule<SampleCronJob1> config, ILogger<SampleCronJob1> logger) : CronJob(config.CronExpression)
{
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("{Now}: {Name} starting...", DateTime.Now, nameof(SampleCronJob1));
        return base.StartAsync(cancellationToken);
    }

    public override Task DoWork(CancellationToken cancellationToken)
    {
        logger.LogInformation("{Now}: {Name} working...", DateTime.Now, nameof(SampleCronJob1));
        return base.DoWork(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("{Now}: {Name} stopping...", DateTime.Now, nameof(SampleCronJob1));
        return base.StopAsync(cancellationToken);
    }
}