using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using {{ prefix }}.DataAccess.{{ project.name }}.Implementation;
using Microsoft.Extensions.Configuration;
using Surging.Core.CPlatform.Module;

namespace {{ prefix }}.DataAccess.{{ project.name }}
{
      public class {{ project.name }}DataModule : SystemModule
    {
        public override async void Initialize(AppModuleContext context)
        {
            await context.ServiceProvoider.GetInstances<DataContext>("sqlite").InitializeAsync();
            base.Initialize(context);
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
            var option = new List<{{ project.name }}DataOption>();
            var section = Surging.Core.CPlatform.AppConfig.GetSection("DataAccess");
            if (section.Exists())
                option = section.Get<List<{{ project.name }}DataOption>>();
            AppConfig.{{ project.name }}DataOptions = option.Where(p => p.Name == "{{ project.name }}Data").FirstOrDefault();
            builder.Register(p => new SqliteContext(AppConfig.{{ project.name }}DataOptions?.Connstring)).Named<DataContext>("sqlite").InstancePerDependency();
            //builder.AddClientIntercepted(typeof(LogProviderInterceptor));
        }
    }
}
