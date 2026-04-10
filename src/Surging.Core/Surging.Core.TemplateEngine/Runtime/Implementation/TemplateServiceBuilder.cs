using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform;
using Surging.Core.ServiceHosting.Internal;
using Surging.Core.ServiceHosting.Internal.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Surging.Core.TemplateEngine.Runtime.Implementation
{
    public class TemplateServiceBuilder : ITemplateServiceBuilder
    {
        private readonly List<Action<IServiceCollection>> _configureServicesDelegates;
        private readonly List<Action<ContainerBuilder>> _registerServicesDelegates;
        private readonly List<Action<IConfigurationBuilder>> _configureDelegates;
        private readonly List<Action<IContainer>> _mapServicesDelegates;
        private readonly List<Func<IContainer,Task>> _mapAsyncServicesDelegates;
        private Action<ILoggingBuilder> _loggingDelegate;

        public TemplateServiceBuilder()
        {
            _configureServicesDelegates = new List<Action<IServiceCollection>>();
            _registerServicesDelegates = new List<Action<ContainerBuilder>>();
            _configureDelegates = new List<Action<IConfigurationBuilder>>();
            _mapServicesDelegates = new List<Action<IContainer>>();
            _mapAsyncServicesDelegates = new List<Func<IContainer,Task>>();
        }

        public ITemplateServiceHost Build()
        {

            var services = BuildCommonServices();
            var config = Configure();
            if (_loggingDelegate != null)
                services.AddLogging(_loggingDelegate);
            else
                services.AddLogging();
            services.AddSingleton(typeof(IConfigurationBuilder), config);
            var hostingServices = RegisterServices();
            var applicationServices = services.Clone();
            var hostingServiceProvider = services.BuildServiceProvider();
            hostingServices.Populate(services);
            var hostLifetime = hostingServiceProvider.GetService<IHostLifetime>();
            var host = new TemplateServiceHost(hostingServices, hostingServiceProvider, hostLifetime, _mapServicesDelegates, _mapAsyncServicesDelegates);
            var container = host.Initialize();
            return host;
        }

        public ITemplateServiceBuilder MapServices(Action<IContainer> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            _mapServicesDelegates.Add(mapper);
            return this;
        }

        public ITemplateServiceBuilder MapServices(Func<IContainer,Task> mapper)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            _mapAsyncServicesDelegates.Add(mapper);
            return this;
        }

        public ITemplateServiceBuilder RegisterServices(Action<ContainerBuilder> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            _registerServicesDelegates.Add(builder);
            return this;
        }

        public ITemplateServiceBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            if (configureServices == null)
            {
                throw new ArgumentNullException(nameof(configureServices));
            }
            _configureServicesDelegates.Add(configureServices);
            return this;
        }

        public ITemplateServiceBuilder Configure(Action<IConfigurationBuilder> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            _configureDelegates.Add(builder);
            return this;
        }

        private IServiceCollection BuildCommonServices()
        {
            var services = new ServiceCollection();
            foreach (var configureServices in _configureServicesDelegates)
            {
                configureServices(services);
            }
            return services;
        }

        private IConfigurationBuilder Configure()
        {
            var config = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory);
            foreach (var configure in _configureDelegates)
            {
                configure(config);
            }
            return config;
        }

        private ContainerBuilder RegisterServices()
        {
            var hostingServices = new ContainerBuilder();
            foreach (var registerServices in _registerServicesDelegates)
            {
                registerServices(hostingServices);
            }
            return hostingServices;
        }

        public ITemplateServiceBuilder ConfigureLogging(Action<ILoggingBuilder> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            _loggingDelegate = configure;
            return this;
        }
    }
} 
