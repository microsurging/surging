using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.TemplateEngine.Runtime
{
    public interface ITemplateServiceBuilder
    {
        ITemplateServiceHost Build();

        ITemplateServiceBuilder RegisterServices(Action<ContainerBuilder> builder);

        ITemplateServiceBuilder ConfigureLogging(Action<ILoggingBuilder> configure);

        ITemplateServiceBuilder ConfigureServices(Action<IServiceCollection> configureServices);

        ITemplateServiceBuilder Configure(Action<IConfigurationBuilder> builder);

        ITemplateServiceBuilder MapServices(Action<IContainer> mapper);

        ITemplateServiceBuilder MapServices(Func<IContainer,Task> mapper);
         
    }
}
