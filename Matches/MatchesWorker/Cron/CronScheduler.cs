using System;
using System.Threading;
using System.Threading.Tasks;
using NCrontab;

namespace MatchesWorker.Cron
{
    internal class CronScheduler
    {
        private readonly CrontabSchedule _schedule;
        private DateTime _nextRun;

        public CronScheduler(CronConfiguration configuration)
        {
            _schedule = CrontabSchedule.Parse(configuration.Expression);
        }

        public async Task RunAsync(Func<Task> asyncJob, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
                await WaitUntilDueAsync(cancellationToken);
                await asyncJob();
            }
        }

        private async Task WaitUntilDueAsync(CancellationToken cancellationToken)
        {
            var timeToWait = _nextRun - DateTime.Now;
            if (timeToWait > TimeSpan.Zero)
            {
                await Task.Delay(timeToWait, cancellationToken);
            }
        }
    }
}