using Autofac;
using Autofac.Extensions.DependencyInjection;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Surging.Core.CPlatform.Utilities;
using Surging.Tools.Cli.Commands;
using Surging.Tools.Cli2.Commands;
using Surging.Tools.Cli2.Commands.Net;

namespace Surging.Tools.Cli2
{
    /// <summary>
    /// cmd: engine-cli run 
    /// cmd help:  engine-cli run -h
    /// </summary>
    [HelpOption(Inherited = true)]
    [Command(Description = "command line terminal engine network request and configuration tool")]
    class Program
    {
        private readonly IServiceProvider _serviceProvider;
        // private readonly CommandLineApplication _curlCommand;
        private readonly CommandLineApplication _runCommand;
        private readonly CommandLineApplication _lsCommand;
        private readonly CommandLineApplication _newCommand;
        private readonly CommandLineApplication _netModuleCommand;
        // private readonly CommandLineApplication _installCommand;
        public Program()
        {
            // _curlCommand = new CommandLineApplication<CurlCommand>();
            _runCommand = new CommandLineApplication<RunCommand>();
            //_installCommand = new CommandLineApplication<InstallCommand>();
            _lsCommand = new CommandLineApplication<LsCommand>();
            _newCommand = new CommandLineApplication<NewCommand>();
            _netModuleCommand = new CommandLineApplication<NetModuleCommand>();
            _serviceProvider = ConfigureServices();

        }

        static int Main(string[] args)
        {

            return new Program().Execute(args);
        }

        [Option("-m|--method", "open doc", CommandOptionType.NoValue)]
        public bool Method { get; }

        [Option("-d|--data", "open doc", CommandOptionType.NoValue)]
        public bool data { get; }

        private int Execute(string[] args)
        {
            var app = new CommandLineApplication<Program>();
            app.AddSubcommand(_runCommand);
            // app.AddSubcommand(_installCommand);
            _newCommand.AddSubcommand(_netModuleCommand); 
            app.AddSubcommand(_newCommand);
            app.AddSubcommand(_lsCommand);
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(_serviceProvider);

            var console = (IConsole)_serviceProvider.GetService(typeof(IConsole));
            app.VersionOptionFromAssemblyAttributes("--version", typeof(Program).Assembly);

            try
            {
                return app.Execute(args);
            }
            catch (UnrecognizedCommandParsingException ex)
            {
                console.WriteLine(ex.Message);
                return -1;
            }
        }

        private IServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConsole>(PhysicalConsole.Singleton);
            var builder = new ContainerBuilder();
            builder.Populate(serviceCollection);

            // builder.Register(provider=> _curlCommand).As<CommandLineApplication>().SingleInstance();
            ServiceLocator.Current = builder.Build();
            return serviceCollection.BuildServiceProvider();
        }

    }
}
