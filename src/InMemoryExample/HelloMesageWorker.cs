using InMemoryExample.Contracts;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace InMemoryExample
{
    /// <summary>
    /// Worker used to publish messages for the consumer to pick up.
    /// </summary>
    public class HelloMesageWorker : BackgroundService
    {
        private IBus _bus;
        private ILogger<HelloMesageWorker> _logger;

        public HelloMesageWorker(IBus bus, ILogger<HelloMesageWorker> logger)
        {
            _bus = bus;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Publishing message");
                await _bus.Publish(new HelloMessage
                {
                    Name = "World"
                }, stoppingToken);

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
