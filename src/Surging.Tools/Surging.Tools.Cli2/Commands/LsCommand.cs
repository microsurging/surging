using Autofac;
using McMaster.Extensions.CommandLineUtils;
using MessagePack.Formatters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Surging.Core.Caching.Configurations;
using Surging.Core.Configuration.Apollo.Configurations;
using Surging.Core.Configuration.Apollo.Extensions;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Configurations;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Runtime.Server.Implementation;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.Protocol.WS.Runtime;
using Surging.Core.ProxyGenerator;
using Surging.Core.ServiceHosting;
using Surging.Core.ServiceHosting.Internal.Implementation;
using Surging.Tools.Cli2;
using Surging.Tools.Cli2.Commands;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using WebSocketCore;
using static System.Net.WebRequestMethods;

namespace Surging.Tools.Cli2.Commands
{
    [Command("Ls", Description = "Command-line list all services, modules, and routes   tool")]
    public class LsCommand
    {
        [Option("-r|--route", "service route", CommandOptionType.MultipleValue)]
        public string[] Route { get; }

        [Option("--regtype", "registry center", CommandOptionType.SingleValue)]
        public RegistryType? Registry { get; }

        [Option("--node", "route node", CommandOptionType.NoValue)]
        public bool RouteNode { get; set; }

        [Option("-m|--module", "list modules and components", CommandOptionType.NoValue)]
        public bool Module { get; }

        [Option("-p|--path", "multiple business module paths", CommandOptionType.MultipleValue)]
        public string[] Path { get; }

        [Option("--rootpath", "scan root path", CommandOptionType.SingleValue)]
        public string RootPath { get; }
        private async Task OnExecute(CommandLineApplication app, IConsole console)
        {
            try
            {
                ConfigureEnvironment();
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
                      .UseClient()
                      .UseProxy()
                      .MapServices(async mapper =>
                      {
                          ConfigureModules(mapper);
                         await WriteRoute(mapper);
                          await WriteModule(mapper);
                          await WriteEntry(mapper);

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
                    // Console.WriteLine($"服务端启动成功，{DateTime.Now}。");
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

        private async Task WriteRoute(IContainer mapper)
        {

            if (Route != null)
            {
                Console.WriteLine("Route:");
                var routes = mapper.Resolve<IServiceRouteManager>().GetRoutesAsync().Result;
                routes = routes.Where(p => Route.Contains(p.ServiceDescriptor.RoutePath)).ToList();
                var notexists = Route.Where(p => !routes.Select(p=>p.ServiceDescriptor.RoutePath).Contains(p));
                if (notexists.Count()>0) Console.WriteLine($"{string.Join(",", notexists)} no route data");
                foreach (var route in routes)
                {

                    if (!RouteNode)
                    {
                        Console.WriteLine(mapper.Resolve<ISerializer<string>>().Serialize(route.ServiceDescriptor));
                        WriteNode(route);
                    }
                    else
                    {
                        WriteNode(route);
                    }
                }
            }
        }

        private void WriteNode(ServiceRoute route)
        {
            var count = 0;
            foreach (var addr in route.Address)
            {
                Console.Write($"node {++count}:{addr.ToString()}");
                Console.Write(",");
                var isHealth = Check(addr.CreateEndPoint(), 3000).Result;
                Console.Write("ishealth: ");
                if (isHealth)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("√");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("×");
                }
                Console.ResetColor();
            }
        }

        private async Task<bool> Check(EndPoint address, int timeout)
        {
            bool isHealth = false;
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { SendTimeout = timeout })
            {
                try
                {
                    await socket.ConnectAsync(address);
                    isHealth = true;
                }
                catch
                {

                }
                return isHealth;
            }
        }

        private async Task WriteModule(IContainer mapper)
        {

            if (Module)
            {
                Console.WriteLine("Module:");
                var modules = mapper.Resolve<IModuleProvider>().Modules;
                foreach (var module in modules)
                {
                    Console.WriteLine($"module name:{module.ModuleName}, type:{module.TypeName},desc:{module.Description}");
                }
                Console.WriteLine($"{modules.Count()} count module");
            }
        }

        private async Task WriteEntry(IContainer mapper)
        {
            Console.WriteLine("Entry:");
            var entries = mapper.Resolve<IServiceEntryManager>().GetAllEntries();
            Console.WriteLine(string.Join(";", entries.Select(p => p.RoutePath)));
            Console.WriteLine($"{entries.Count()} count service");
        }

        private void ConfigureEnvironment()
        {
            if (Path != null)
                Environment.SetEnvironmentVariable("ModulePaths", $"{string.Join('|', Path)}");
        }


        private void Configure()
        {
            if (RootPath != null)
                AppConfig.ServerOptions.RootPath = RootPath;
            AppConfig.ServerOptions.Packages.ForEach(item =>
            {
                if (item.TypeName == "EnginePartModule")
                {
                    item.Using = item.Using.Replace("ConsulModule;", "");

                    if (Registry == null)
                    {
                        item.Using += "SharedFileModule;";
                    }
                    if (Registry == RegistryType.consul)
                    {
                        item.Using += "ConsulModule;";
                    }
                    if (Registry == RegistryType.zookeeper)
                    {
                        item.Using += "ZookeeperModule;";
                    }

                }
            });
        }

        private void ConfigureModules(IContainer mapper)
        {
            mapper.Resolve<IModuleProvider>().Modules.ForEach(p =>
            {
                if (p.ModuleName == "SwaggerModule")
                {
                    p.Enable = false;
                }
                if (p.ModuleName == "WebServiceModule")
                {
                    p.Enable = false;
                }

                if (p.ModuleName == "StageModule")
                {
                    p.Enable = false;
                }

            });
        }
    }
}
