using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Publisher.Observers
{
    public class PublishObserver : IPublishObserver
    {
        private ILogger<PublishObserver> _logger;

        public PublishObserver(ILogger<PublishObserver> logger)
        {
            _logger = logger;
        }
        public Task PrePublish<T>(PublishContext<T> context)
            where T : class
        {
            // called right before the message is published (sent to exchange or topic)
            return Task.CompletedTask;
        }

        public Task PostPublish<T>(PublishContext<T> context)
            where T : class
        {
            // called after the message is published (and acked by the broker if RabbitMQ)
            return Task.CompletedTask;
        }

        public Task PublishFault<T>(PublishContext<T> context, Exception exception)
            where T : class
        {
            // called if there was an exception publishing the message
            _logger.LogError(exception, "Failed to publish message: {Message}",
                context.Message.ToString());
            return Task.CompletedTask;
        }
    }
}
