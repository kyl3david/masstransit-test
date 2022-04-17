namespace InMemoryExample.Consumers
{
    using Contracts;
    using MassTransit;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;

    public class HelloMessageConsumer :
        IConsumer<HelloMessage>
    {
        private ILogger<HelloMessageConsumer> _logger;

        public HelloMessageConsumer(ILogger<HelloMessageConsumer> logger)
        {
            _logger = logger;
        }
        public Task Consume(ConsumeContext<HelloMessage> context)
        {
            _logger.LogInformation("Hello {Name}", context.Message.Name);
            return Task.CompletedTask;
        }
    }
}