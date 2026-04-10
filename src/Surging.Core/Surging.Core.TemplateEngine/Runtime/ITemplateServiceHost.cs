using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.TemplateEngine.Runtime
{
    public interface ITemplateServiceHost:IDisposable
    {
        IDisposable Run();

        IContainer Initialize();
    }
}
