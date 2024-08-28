using NCrontab;
using Timer = System.Timers.Timer;

namespace cronworker.CronJobService;

public abstract class CronJob(string config) : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly CrontabSchedule _schedule = CrontabSchedule.Parse(config);

    public virtual async Task DoWork(CancellationToken cancellationToken)
    {
        await Task.Delay(5000, cancellationToken);  // do the work
    }
    
    private async Task ScheduleJob(CancellationToken cancellationToken)
    {
        DateTime next = _schedule.GetNextOccurrence(DateTime.Now);
        TimeSpan delay = next - DateTimeOffset.Now;
        if (delay.TotalMilliseconds <= 0)
        {
            await ScheduleJob(cancellationToken);
        }

        _timer = new Timer(delay.TotalMilliseconds);
        _timer.Elapsed += async (sender, args) =>
        {
            _timer.Dispose();
            _timer = null;

            if (!cancellationToken.IsCancellationRequested)
            {
                await DoWork(cancellationToken);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                await ScheduleJob(cancellationToken);
            }

        };
        _timer.Start();

        await Task.CompletedTask;
    }
    
    #region IHostedService

    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        await ScheduleJob(cancellationToken);
    }
    
    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Stop();
        await Task.CompletedTask;
    }
    
    #endregion
    
    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
    }
    
    #endregion
}

public interface ISchedule<T>
{
    public string CronExpression { get; set; }
}

public class Schedule<T> : ISchedule<T>
{
    public string CronExpression { get; set; } = string.Empty;
}

public static class CronJobServiceExtensions
{
    public static IServiceCollection AddCronJob<T>(
        this IServiceCollection services,
        Action<ISchedule<T>> options) where T : CronJob
    {
        ArgumentNullException.ThrowIfNull(options);

        Schedule<T> config = new();
        options.Invoke(config);
        if (string.IsNullOrWhiteSpace(config.CronExpression))
        {
            throw new ArgumentNullException(nameof(options), "Please provide a cron expression.");
        }

        services.AddSingleton<ISchedule<T>>(config);
        services.AddHostedService<T>();
        return services;
    }
}