# GhostEventBus

It's lightweight library for implementing sending and listening events.
Library exposes all you need interfaces and models to build simple event-processing solution.

## How to start event proccesing based on Redis server for ASP.Net

See public class RedisMqExtension.

Solution have implemented event sender. For using you need add implementation of IEventSender to your DI by extension-method 'AddEventSenderAsSingletone'.

To start event listening you need add RedisEventsHostedService by extension-method 'AddHostedWorkerService' to your ASP-DI. 

To specify types of listened events you need implement 'IEventHandler<TEvent>' interface and add implementations to your DI. 
On starts event listening solution analizes your assemblies by reflection and try getting implementations of this interface by 'IServiceProvider'.

### Pay your attention
Receiving and sending data to queue is working by 'LPUSH' && LPOP redis-commands.

## How get

Find package as 'GhostEventBus' in nuget-gallery or visit: https://www.nuget.org/packages/Ghost-EventBus
