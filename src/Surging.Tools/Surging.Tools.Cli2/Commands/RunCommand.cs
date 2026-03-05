using Autofac;
using McMaster.Extensions.CommandLineUtils;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.ServiceHosting.Internal.Implementation;
using Surging.Tools.Cli2;
using Surging.Core.ProxyGenerator;
using Microsoft.Extensions.Logging;
using Surging.Core.ServiceHosting;
using Surging.Core.Caching.Configurations;
using Surging.Core.CPlatform.Configurations;
using Surging.Core.Configuration.Apollo.Extensions;
using Surging.Core.Configuration.Apollo.Configurations;
using System;
using System.Text.RegularExpressions;
using Surging.Core.CPlatform.Module;

namespace Surging.Tools.Cli.Commands
{
    [Command("Run", Description = "Command-line microservice running tool")]
    public class RunCommand
    {

        [Option("-d|--doc", "open doc", CommandOptionType.NoValue)]
        public bool Doc { get; }

        [Option("--webService", "open WebService", CommandOptionType.NoValue)]
        public bool WebService { get; }

        [Option("--gateway", "open ApiGateway", CommandOptionType.NoValue)]
        public bool ApiGateway { get; }

        [Option("--http", "open http", CommandOptionType.NoValue)]
        public bool Http { get; }

        [Option("-ws", "open WebSocket", CommandOptionType.NoValue)]
        public bool WebSocket { get; }
          
        [Option("--grpc", "open Grpc", CommandOptionType.NoValue)]
        public bool Grpc { get; }


        [Option("-p|--path", "Multiple business module paths", CommandOptionType.MultipleValue)]
        public string[] Path { get; }

        [Option("--ip", "ip address default 127.0.0.1", CommandOptionType.SingleValue)]
        public string Ip { get; set; }

        [Option("-hp|--httpport", "httpport default 28", CommandOptionType.SingleValue)]
        public int? HttpPort { get; set; }

        [Option("-wp|--wsport", "websocketport default 96", CommandOptionType.SingleValue)]
        public int? WSPort { get; set; }

        [Option("--mqttport", "mqttport default 97", CommandOptionType.SingleValue)]
        public int? MqttPort { get; set; }

        [Option("-grpc|--grpcport", "grpcport default 95", CommandOptionType.SingleValue)]
        public int? GrpcPort { get; set; }

        [Option("--port", "port default 82", CommandOptionType.SingleValue)]
        public int? Port { get; set; }

        [Option("-m|--multi", "multi-level scan", CommandOptionType.NoValue)]
        public bool Multi { get; set; }

        [Option("-s|--Scan", " enable scan the current directory", CommandOptionType.NoValue)]
        public bool Scan { get; set; }


        private async Task OnExecute(CommandLineApplication app, IConsole console)
        {
            try
            { 
                var host = new ServiceHostBuilder()
                      .RegisterServices(builder =>
                      {
                          builder.AddMicroService(option =>
                          {
                              option.AddServiceRuntime()
                              // .AddRelateService() // no unload Proxy Generator
                              .AddRelateService2()//load and unload Proxy Generator
                              .AddConfigurationWatch()
                              //option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181")); 
                              .AddServiceEngine(typeof(SurgingServiceEngine));
                              builder.Register(p => new CPlatformContainer(ServiceLocator.Current));
                          });
                      })
                      .ConfigureLogging(logger =>
                      {
                          logger.AddConfiguration(
                              Core.CPlatform.AppConfig.GetSection("Logging"));
                      })

                      .UseServer(options => { })
                      .UseProxy()
                      .UseConsoleLifetime()
                      .MapServices(mapper =>
                        {
                            ConfigureModules(mapper);
                        })
                      .Configure(build =>
                      build.AddCacheFile("${cachepath}|cacheSettings.json", basePath: AppContext.BaseDirectory, optional: false, reloadOnChange: true))
                        .Configure(build =>
                      build.AddCPlatformFile("${surgingpath}|surgingSettings.json", optional: false, reloadOnChange: true))
                           .Configure(build => build.UseApollo(apollo => apollo.AddNamespaceSurgingApollo("surgingSettings")))
                           .Configure(build => Configure())
                      .UseStartup<Startup>()
                      .Build();

                using (host.Run())
                {
                    Console.WriteLine($"服务端启动成功，{DateTime.Now}。");
                }
            }
            catch (Exception ex)
            {
                console.ForegroundColor = ConsoleColor.Red;
                console.WriteLine(ex.Message);
                console.WriteLine(ex.StackTrace);
                console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void Configure()
        {
            AppConfig.ServerOptions.Ip = Ip;
            if(Port!=null)
            AppConfig.ServerOptions.Port = Port.Value;
            AppConfig.ServerOptions.Ports.MQTTPort = MqttPort ?? 0;
            foreach (var item in AppConfig.ServerOptions.Packages)
            {
                if (item.TypeName == "EnginePartModule")
                {
                    if (!Http)
                    {
                        item.Using = item.Using.Replace("KestrelHttpModule;", "");
                    }
                    if (!WebSocket)
                    {
                        item.Using = item.Using.Replace("WSProtocolModule;", "");
                    }
                    if (!Grpc)
                    {
                        item.Using = item.Using.Replace("GrpcModule;", "");
                    }
                }
            }

        }

        private void ConfigureModules(IContainer mapper)
        {
            mapper.Resolve<IModuleProvider>().Modules.ForEach(p =>
            {
                if (!Doc && p.ModuleName == "SwaggerModule")
                {
                    p.Enable = false;
                }
                if(!WebService && p.ModuleName == "WebServiceModule")
                {
                    p.Enable = false;
                }

                if (!ApiGateway && p.ModuleName == "StageModule")
                {
                    p.Enable = false;
                }

            });
        }
    }
}