using {{ prefix }}.Core.Common.Repsitories;
using {{ prefix }}.Core.Common.Repsitories.Implementation;
using Surging.Core.CPlatform.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace {{ prefix }}.Core.Common
{
    public class CommonModule : SystemModule
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
            builder.RegisterGeneric(typeof(EFRepository<>)).As(typeof(IEFRepository<>)).SingleInstance();
        }
    }
}
