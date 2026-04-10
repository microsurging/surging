using Autofac;
using Surging.Core.CPlatform.Module;
using Surging.Core.TemplateEngine.Runtime;
using Surging.Core.TemplateEngine.Runtime.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.TemplateEngine
{
    internal class TemplateEngineModule :EnginePartModule
    {
        public override void Initialize(AppModuleContext serviceProvider)
        {
            base.Initialize(serviceProvider);
        }

        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
            builder.RegisterType<ScribanTemplateRender>().As<ITemplateRender>().SingleInstance();
            builder.RegisterType<DefaultTemplateEngine>().As<ITemplateEngine>().SingleInstance();
            builder.RegisterType<TemplateEngineBuilder>().As<ITemplateEngineBuilder>().SingleInstance();
        }
    }
}
