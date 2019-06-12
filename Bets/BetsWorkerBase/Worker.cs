using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BetsWorkerBase.EventQueue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BetsWorkerBase
{
    public abstract class Worker<TObject> : IDisposable
    {
        protected readonly CancellationTokenSource CancellationTokenSource;
        protected readonly IObjectEventConsumer<TObject> EventConsumer;
        private readonly WorkerConfiguration _configuration;
        private readonly ILogger _logger;

        protected Worker(IObjectEventConsumer<TObject> eventConsumer, ILogger logger)
        {
            CancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, args) =>
            {
                CancellationTokenSource.Cancel();
                args.Cancel = true;
            };

            var configuration = GetConfiguration();
            _configuration = GetConfigurationSection<WorkerConfiguration>(configuration);
            EventConsumer = eventConsumer;
            _logger = logger;
        }

        public virtual async Task RunAsync()
        {
            EventConsumer.Event += HandleEvent;
            EventConsumer.BeginConsuming(_configuration.ExchangeName, _configuration.QueueName);
            _logger.LogInformation("Listening for events...".AddTimestamp());
            await Task.Delay(-1, CancellationTokenSource.Token);
            EventConsumer.Event -= HandleEvent;
        }

        protected abstract void HandleEvent(TObject @object, ulong deliveryTag);

        protected static IConfigurationRoot GetConfiguration() =>
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();

        protected static TConfiguration GetConfigurationSection<TConfiguration>(IConfiguration configuration) where TConfiguration : new()
        {
            var section = new TConfiguration();
            configuration.GetSection(typeof(TConfiguration).Name).Bind(section);
            return section;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            CancellationTokenSource?.Dispose();
            EventConsumer?.Dispose();
        }
    }
}
