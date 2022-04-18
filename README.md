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
    3. RabbitMessage.cs **NB the namespace of the message needs to be the same on all projects, is gets send with the message and used to identify which messages will be consumed**.
        ```c#
        namespace Company.Contracts
        {
            public record RabbitMessage
            {
                public int Count { get; set; }
                public string Value { get; init; }
            }
        }
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
                _logger.LogInformation("Message #{Count}: {Value}",
                    context.Message.Count, context.Message.Value);
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
            public int Count { get; set; }
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
                        Count = _counter,
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

8. Add Publish Observer to log failures on publishing.
    1. Add `PublishObserver` to log faults
    ```c#
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
    ```
    2. Add `BusObserver` to connect a publish observer.
    ```c#
    using MassTransit;
    using System;
    using System.Threading.Tasks;

    namespace Publisher.Observers
    {
        public class BusObserver :
        IBusObserver
        {
            private IPublishObserver _publishObserver;

            public BusObserver(IPublishObserver publishObserver)
            {
                _publishObserver = publishObserver;
            }
            public void PostCreate(IBus bus)
            {
                // called after the bus has been created, but before it has been started.
            }

            public void CreateFaulted(Exception exception)
            {
                // called if the bus creation fails for some reason
            }

            public Task PreStart(IBus bus)
            {
                // called just before the bus is started
                bus.ConnectPublishObserver(_publishObserver);
                return Task.CompletedTask;
            }

            public Task PostStart(IBus bus, Task<BusReady> busReady)
            {
                // called once the bus has been started successfully. The task can be used to wait for
                // all of the receive endpoints to be ready.
                return Task.CompletedTask;
            }

            public Task StartFaulted(IBus bus, Exception exception)
            {
                // called if the bus fails to start for some reason (dead battery, no fuel, etc.)
                return Task.CompletedTask;
            }

            public Task PreStop(IBus bus)
            {
                // called just before the bus is stopped
                return Task.CompletedTask;
            }

            public Task PostStop(IBus bus)
            {
                // called after the bus has been stopped
                return Task.CompletedTask;
            }

            public Task StopFaulted(IBus bus, Exception exception)
            {
                // called if the bus fails to stop (no brakes)
                return Task.CompletedTask;
            }
        }
    }
    ```
    3. Add a `PublishObserver` to dependency injection for the bus observer and the `BusObserver` to MassTransit, in the Program.cs.
    ```c#
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddMassTransit(x =>
                {
                    x.SetKebabCaseEndpointNameFormatter();

                    // By default, sagas are in-memory, but should be changed to a durable
                    // saga repository.
                    x.SetInMemorySagaRepositoryProvider();

                    var entryAssembly = Assembly.GetEntryAssembly();

                    x.AddConsumers(entryAssembly);
                    x.AddSagaStateMachines(entryAssembly);
                    x.AddSagas(entryAssembly);
                    x.AddActivities(entryAssembly);
                    x.AddBusObserver<BusObserver>();

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host("localhost", "/", h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        cfg.ConfigureEndpoints(context);
                    });
                });
                services.AddHostedService<PublishWorker>();
                services.AddSingleton<IPublishObserver, PublishObserver>();
            });
    ```
