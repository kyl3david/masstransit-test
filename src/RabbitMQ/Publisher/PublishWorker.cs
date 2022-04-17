using Company.Contracts;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Publisher
{
    public class PublishWorker : BackgroundService
    {
        private ILogger<PublishWorker> _logger;
        private IBus _bus;

        public PublishWorker(ILogger<PublishWorker> logger, IBus bus)
        {
            _logger = logger;
            _bus = bus;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Publishing message");
                await _bus.Publish(new RabbitMessage
                {
                    Value = string.Format("message {0}", System.DateTime.Now)
                }, stoppingToken);

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
