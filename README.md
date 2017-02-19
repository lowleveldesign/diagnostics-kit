Diagnostics Kit
===============

Diagnostics Kit is a set of tools created to help you monitor .NET applications. I tried to make the installation easy, with only minimal changes required in the configuration files. Diagnostics Kit is composed of few layers, but its architecture is really simple: **Musketeer** instances (Windows services) and **Harvesters** (assemblies from Nuget packages) installed in your apps deliver logs to the central point, called the **Diagnostics Castle** (an Owin-Nancy web application). Two most important parts of the Castle are the monitoring grid and the log viewer. I paste sample screenshots below:

![Monitoring grid](https://raw.githubusercontent.com/lowleveldesign/diagnostics-kit/master/docs/diaggrid.png)

![Log viewer](https://raw.githubusercontent.com/lowleveldesign/diagnostics-kit/master/docs/diaglog.png)

To make configuration simpler, **applications are identified by their paths** so logs from applications installed under the same paths on various servers will be treated as logs from one application. I know it is quite restrictive, but believe me: it makes things much easier to maintain.

An interesting part of the Diagnostics Kit is also the Fiddler plugin, named **Bishop**. Bishop integrates with the Castle and provides different ways of tampering the requests. With its help you may:

- skip the load-balancers and send requests directly to the servers where you deployed applications
- emulate border routers HTTPS encryption on localhost when testing/developing applications
- test regex rules for your load-balancers or reverse proxies
- forward all the traffic to your test server (for instance on Docker)

The detailed documentation can be found in wiki: **<https://github.com/lowleveldesign/diagnostics-kit/wiki>**

## The intended audience

There are many monitoring services available on the market, thus you might be wondering why I wrote another one. My point was to create a monitoring solution for small and midsize .NET projects (mainly web, but performance monitoring of Windows services is supported), which is fast to setup and easily cusomizable by project developers. I tried to keep the source code consise and easy to read. The requirements list is also short: a web server (preferably IIS), storage for logs (works with: Elastic Search, MySql or SQL Server). You should have them already deployed in your infrastructure.

## How to get started?

Download the components you need from [the release page](https://github.com/lowleveldesign/diagnostics-kit/releases) and read [the installation guide on wiki](https://github.com/lowleveldesign/diagnostics-kit/wiki/1.2.installation). Then, depending on your application type, read one of the logs collection guides:

- [ASP.NET applications](https://github.com/lowleveldesign/diagnostics-kit/wiki/2.3.log-collection-aspnet)
- [ASP.NET MVC applications](https://github.com/lowleveldesign/diagnostics-kit/wiki/2.4.log-collection-aspnet-mvc)
- [ASP.NET WebAPI applications](https://github.com/lowleveldesign/diagnostics-kit/wiki/2.5.log-collection-aspnet-webapi)
- [Owin applications](https://github.com/lowleveldesign/diagnostics-kit/wiki/2.6.log-collection-owin)
- [Sending logs from logging libraries (System.Diagnostics, NLog, log4net)](https://github.com/lowleveldesign/diagnostics-kit/wiki/2.7.log-collection-libs)

## How to contribute?

If you like the project and would like to help, feel free to create a pull request. The features I currently consider most required include:

- ASP.NET Core Harvester
- Serilog sink for Diagnostics Kit
- .NET Core CLR support
- support for PostgreSql as a Log Store

## Links

You may also have a look at the following articles, which provide overview of the features available in the kit:

- [Diagnostics Kit - a monitoring solution for .NET apps](https://www.codeproject.com/Articles/1103161/Diagnostics-Kit-a-monitoring-solution-for-NET-apps)
- [Monitoring .NET applications with ELK (using Musketeer with Logstash)](http://kmdpoland.pl/monitoring-net-applications-with-elk/)
