namespace Consumer.Consumers
{
    using Company.Contracts;
    using MassTransit;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;

    public class RabbitMessageConsumer :
        IConsumer<RabbitMessage>
    {
        private ILogger<RabbitMessageConsumer> _logger;

        public RabbitMessageConsumer(ILogger<RabbitMessageConsumer> logger)
        {
            _logger = logger;
        }
        public Task Consume(ConsumeContext<RabbitMessage> context)
        {
            _logger.LogInformation("Message: {Value}", context.Message.Value);
            return Task.CompletedTask;
        }
    }
}