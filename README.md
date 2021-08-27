# GhostEventBus

It's lightweight library for implementing sending and listening events.
Library exposes all you need interfaces and models to build simple solution for event processing.

## How to start event proccesing based on Redis server for ASP.Net

See public class RedisMqExtension.

To start event listenings you need add to your ASP-DI by extension-method AddHostedWorkerService.
Also solution have implemented event sender. 
To add this you need add implementation of IEventSender to your DI by extension-method AddEventSenderAsSingletone.

Also pay your attention to added handlers that implemented IEventHandler interfaces to your DI. 
It have decraled wich event workers will start after sucsesfull connecting to Redis-Server.
