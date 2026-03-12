using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Module;
using Surging.Core.CPlatform.Mqtt;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks.Implementation;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Routing.Implementation
{
    public class SharedFileModule : EnginePartModule
    {
        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            
            base.RegisterBuilder(builder);
            var rootpath = AppConfig.ServerOptions.RootPath;
            if (string.IsNullOrEmpty(rootpath))
                rootpath = AppContext.BaseDirectory;
            UseSharedFileRouteManager(builder,System.IO.Path.Combine(rootpath, "reg", $"reg_route.jreg"))
                              .UseSharedFileCommandManager(builder, System.IO.Path.Combine(rootpath, "reg", $"reg_command.jreg"))
                              .UseSharedFileCacheManager(builder, System.IO.Path.Combine(rootpath, "reg", $"reg_cahce.jreg"))
                              .UseSharedFileMqttRouteManager(builder, System.IO.Path.Combine(rootpath, "reg", $"reg_mqtt.jreg"));
        }

        public SharedFileModule UseSharedFileRouteManager(ContainerBuilderWrapper builder, string filePath)
        {
              UseRouteManager(builder, provider =>
            new SharedFileServiceRouteManager(
                filePath,
                provider.GetRequiredService<ISerializer<string>>(),
                provider.GetRequiredService<IServiceRouteFactory>(),
                provider.GetRequiredService<ILogger<SharedFileServiceRouteManager>>()));
            return this;
        }

        public SharedFileModule UseSharedFileCommandManager(ContainerBuilderWrapper builder, string filePath)
        {
              UseCommandManager(builder,provider =>
            new SharedFileServiceCommandManager(
                filePath,
                provider.GetRequiredService<ISerializer<string>>(),
                provider.GetRequiredService<IServiceEntryManager>(),
                provider.GetRequiredService<ILogger<SharedFileServiceCommandManager>>()));
            return this;
        }

        public SharedFileModule UseSharedFileCacheManager(ContainerBuilderWrapper builder, string filePath)
        {
              UseCacheManager(builder,provider =>
            new SharedFileServiceCacheManager(
                filePath,
                provider.GetRequiredService<ISerializer<string>>(),
                provider.GetRequiredService<IServiceCacheFactory>(),
                provider.GetRequiredService<ILogger<SharedFileServiceCacheManager>>()));
            return this;
        }

        public SharedFileModule UseSharedFileMqttRouteManager(ContainerBuilderWrapper builder, string filePath)
        {
              UseMqttRouteManager(builder,provider =>
            new SharedFileMqttServiceRouteManager(
                filePath,
                provider.GetRequiredService<ISerializer<string>>(),
                provider.GetRequiredService<IMqttServiceFactory>(),
                provider.GetRequiredService<ILogger<SharedFileMqttServiceRouteManager>>()));
            return this;
        }

        public ContainerBuilderWrapper UseCommandManager(ContainerBuilderWrapper builder, Func<IServiceProvider, IServiceCommandManager> factory)
        {
            builder.RegisterAdapter(factory).InstancePerLifetimeScope();
            return builder;
        }

        public ContainerBuilderWrapper UseCacheManager(ContainerBuilderWrapper builder, Func<IServiceProvider, IServiceCacheManager> factory)
        {
            builder.RegisterAdapter(factory).InstancePerLifetimeScope();
            return builder;
        }

        public ContainerBuilderWrapper UseRouteManager(ContainerBuilderWrapper builder, Func<IServiceProvider, IServiceRouteManager> factory)
        {
            builder.RegisterAdapter(factory).InstancePerLifetimeScope();
            return builder;
        }

        public ContainerBuilderWrapper UseMqttRouteManager(ContainerBuilderWrapper builder, Func<IServiceProvider, IMqttServiceRouteManager> factory)
        {
            builder.RegisterAdapter(factory).InstancePerLifetimeScope();
            return builder;
        }

    }
}

