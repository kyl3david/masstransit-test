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
