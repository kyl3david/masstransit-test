namespace Consumer.Consumers
{
    using MassTransit;

    public class RabbitMessageConsumerDefinition :
        ConsumerDefinition<RabbitMessageConsumer>
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<RabbitMessageConsumer> consumerConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 1000));
        }
    }
}