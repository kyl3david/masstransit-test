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
        private int _counter;

        public PublishWorker(ILogger<PublishWorker> logger, IBus bus)
        {
            _logger = logger;
            _bus = bus;
            _counter = 1;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Publishing message #{counter}", _counter);
                _ = _bus.Publish(new RabbitMessage
                {
                    Count = _counter,
                    Value = string.Format("message {0}", System.DateTime.Now)
                }, stoppingToken);

                if (_counter % 30 == 0)
                    //wait 30 seconds every 30 messages
                    await Task.Delay(30000, stoppingToken);
                else
                    await Task.Delay(20, stoppingToken);

                _counter++;
            }
        }
    }
}
