using Autofac;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Surging.Core.Caching.Configurations;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Configurations;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.ServiceHosting.Internal.Implementation;
using Surging.Core.TemplateEngine;
using Surging.Core.TemplateEngine.DataModels;
using Surging.Core.TemplateEngine.Runtime;
using Surging.Core.TemplateEngine.Runtime.Implementation;

namespace Surging.Tools.Cli2.Commands.Net
{
    [Command("NetModule", Description = " Module generation, compilation, and publishing based on template engine tool")]
    public class NetModuleCommand
    {
        [Option("--rootpath", "scan root path", CommandOptionType.SingleValue)]
        public string RootPath { get; }

        private async Task OnExecute(CommandLineApplication app, IConsole console)
        {
            var host = new TemplateServiceBuilder()
                      .RegisterServices(builder =>
                      {
                          builder.AddMicroService(option =>
                          {
                              option.AddServiceRuntime()
                              // .AddRelateService() // no unload Proxy Generator
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
                      .MapServices( async mapper =>
                      {
                          await mapper.Resolve<ITemplateEngineBuilder>().Build(GetTemplateModel());
                      })
                        .Configure(build =>
                      build.AddCacheFile("${cachepath}|cacheSettings.json", basePath: AppContext.BaseDirectory, optional: false, reloadOnChange: true))
                        .Configure(build =>
                      build.AddCPlatformFile("${surgingpath}|surgingSettings.json", optional: false, reloadOnChange: true))
                           .Configure(build => Configure())
                      .UseStartup<Startup>()
                      .Build();

            using (host.Run())
            {
                // Console.WriteLine($"服务端启动成功，{DateTime.Now}。");
            }
        }

        private TemplateBuildModel GetTemplateModel()
        {
            var templateModel = new TemplateBuildModel
            {
                Prefix = "Kayak",
                Name = "Demo",
                Project = new ProjectModel
                {
                    Name = "Demo",
                },
                Tables = new List<TableModel>
                {
                    new TableModel(){
                    Name = "User",
                    Columns = new List<ColumnModel>
                    {
                        new ColumnModel() { Name = "UserName", Datatype = "string" },
                        new ColumnModel() { Name = "Age", Datatype = "int" },
                        new ColumnModel() { Name = "Email", Datatype = "string" },
                        new ColumnModel() { Name = "Sex", Datatype = "int" },
                        new ColumnModel() { Name = "Phone", Datatype = "string" },
                    }
                },
                    new TableModel(){
                    Name = "Organization",
                    Columns = new List<ColumnModel>
                    {
                        new ColumnModel() { Name = "Name", Datatype = "string" },
                        new ColumnModel() { Name = "LevelCode", Datatype = "string" },
                        new ColumnModel() { Name = "Level", Datatype = "int" },
                        new ColumnModel() { Name = "City", Datatype = "string" },
                        new ColumnModel() { Name = "Address", Datatype = "string" },
                        new ColumnModel() { Name = "Phone", Datatype = "string" },
                        new ColumnModel() { Name = "Email", Datatype = "string" },
                        new ColumnModel() { Name = "Contacter", Datatype = "string" },
                        new ColumnModel() { Name = "SysOrgType", Datatype = "int" },
                        new ColumnModel() { Name = "Status", Datatype = "int" }
                    }
                },
                }
            };
            return templateModel;
        }
        private void Configure()
        {
            AppConfig.ServerOptions.RootPath=AppContext.BaseDirectory;
            if (RootPath != null)
                AppConfig.ServerOptions.RootPath = RootPath;

            foreach (var item in AppConfig.ServerOptions.Packages)
            {
                if (item.TypeName == "EnginePartModule")
                {
                    item.Using += "TemplateEngineModule";
                }
            }
        }
    }
}
