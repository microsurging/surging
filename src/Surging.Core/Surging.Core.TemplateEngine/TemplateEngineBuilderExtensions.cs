using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.ServiceHosting.Internal;
using Surging.Core.ServiceHosting.Internal.Implementation;
using Surging.Core.ServiceHosting.Startup;
using Surging.Core.ServiceHosting.Startup.Implementation;
using Surging.Core.TemplateEngine.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Surging.Core.TemplateEngine
{
    public static class TemplateEngineBuilderExtensions
    {
        public static ITemplateServiceBuilder UseStartup(this ITemplateServiceBuilder hostBuilder, Type startupType)
        {
            return hostBuilder
                .ConfigureServices(services =>
                {
                    if (typeof(IStartup).GetTypeInfo().IsAssignableFrom(startupType.GetTypeInfo()))
                    {
                        services.AddSingleton(typeof(IStartup), startupType);
                    }
                    else
                    {
                        services.AddSingleton(typeof(IStartup), sp =>
                        {
                            var config = sp.GetService<IConfigurationBuilder>();
                            return new ConventionBasedStartup(StartupLoader.LoadMethods(sp, config, startupType, ""));
                        });

                    }
                });
        }

        public static ITemplateServiceBuilder UseStartup<TStartup>(this ITemplateServiceBuilder hostBuilder) where TStartup : class
        {
            return hostBuilder.UseStartup(typeof(TStartup));
        }

    }
}
