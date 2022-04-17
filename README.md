# masstransit-test
Playing around with MassTransit

# InMemoryExample: In memory example
Followed getting started example from this link: [in-memory](https://masstransit-project.com/quick-starts/in-memory.html).
Single project with publishing and consumption of message.

1. Create mtworker project to start, this will have both the worker to publish files and te consumer to read the messages
2. Create the mtconsumer to create a message and consumer.
    1. Rename message to `HelloMessage` and value of `Name`.
    2. Refactor consumer to fo the `HelloMessage` message.
    3. Add logger to consumer to log message when a `HelloMessage` is consumed.
3. Create back ground worker to publish messages for the consumer to read the messages.
    1. Add logger to log when message is published.

# RabbitMQ: RabbitMQ solution example
Solution example with different projects for publishing and consuming.

### Get RabbitMQ up and running
For this quick start we recommend running the preconfigured [Docker image maintained by the MassTransit team](https://github.com/MassTransit/docker-rabbitmq). It includes the delayed exchange plug-in, as well as the Management interface enabled.
```bash
docker run -p 15672:15672 -p 5672:5672 masstransit/rabbitmq
```
Admin UI: 
- url - http://localhost:15672/
- username - guest
- password - guest

### New solution
1. New folder for solution `mkdir src\RabbitMQ`.
2. Change directory to new folder `cd src\RabbitMQ`.
3. Create sln `dotnet new sln -n RabbitMQ`.

### Consumer project
1. New mtworker project `dotnet new mtworker -n Consumer`.
2. Add project to solution `dotnet sln add Consumer`.
3. New mtconsumer with message and consumer.
    ```bash
    cd Consumer
    dotnet new mtconsumer -n RabbitMessage
    ```
4. Fix namespaces of mtconsumer files.
    1. RabbitMessageConsumer.cs
        ```c#
        namespace Consumer.Consumers
        ```
    2. RabbitMessageConsumerDefinition.cs
        ```c#
        namespace Consumer.Consumers
        ```
    3. Message.cs **NB the namespace of the message needs to be the same on all projects, is gets send with the message and used to identify which messages will be consumed**.
        ```c#
        namespace Company.Contracts
        ```
5. Add `ILogger` to `RabbitMessageConsumer.cs` to visualize when a message comes through.
    ```c#
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
    ```
6. Add the MassTransit.RabbitMQ package to the project `dotnet add package MassTransit.RabbitMQ`.
7. Change `UsingInMemory` to `UsingRabbitMq`
```c#
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddMassTransit(x =>
            {
                //...
                x.UsingRabbitMq((context,cfg) =>
                {
                    cfg.Host("localhost", "/", h => {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });
        });
```

### Publishing project
1. New mtworker project `dotnet new mtworker -n Publisher`.
2. Add project to solution `dotnet sln add Publisher`.
3. Change directory.
    ```bash
    cd Publisher
    ```
3. Create `Message.cs` record. **Same namespace as Consumer project**
    ```c#
    namespace Company.Contracts
    {
        public record RabbitMessage
        {
            public string Value { get; init; }
        }
    }
    ```
4. Add back ground worker to publish message.
    ```c#
    using MassTransit;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Company.Contracts;
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
    ```
5. Add back ground service to configure services.
```c#
.ConfigureServices((hostContext, services) =>
{
//...
    services.AddHostedService<PublishWorker>();
});
```
6. Add the MassTransit.RabbitMQ package to the project `dotnet add package MassTransit.RabbitMQ`.
7. Change `UsingInMemory` to `UsingRabbitMq`
```c#
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddMassTransit(x =>
            {
                //...
                x.UsingRabbitMq((context,cfg) =>
                {
                    cfg.Host("localhost", "/", h => {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });
        });
```