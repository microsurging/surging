using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Surging.Core.ServiceHosting.Internal;
using Surging.Core.TemplateEngine.DataModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.TemplateEngine.Runtime
{
    public interface ITemplateEngineBuilder
    {
        Task Build(TemplateBuildModel model); 
    }
}
