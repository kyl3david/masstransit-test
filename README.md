# masstransit-test
Playing around with MassTransit

## InMemoryExample: In memory example
Followed getting started example from this link: [in-memory](https://masstransit-project.com/quick-starts/in-memory.html).
Single project with publishing and consumption of message.

1. Create mtworker project to start, this will have both the worker to publish files and te consumer to read the messages
2. Create the mtconsumer to create a message and consumer.
    1. Rename message to `HelloMessage` and value of `Name`.
    2. Refactor consumer to fo the `HelloMessage` message.
    3. Add logger to consumer to log message when a `HelloMessage` is consumed.
3. Create back ground worker to publish messages for the consumer to read the messages.
    1. Add logger to log when message is published.

## InMemory: In memory solution example