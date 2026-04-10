using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.TemplateEngine.Runtime.Implementation
{
    public class DefaultTemplateEngine: VirtualPathProviderTemplateEngine
    {
        public DefaultTemplateEngine() {
            TemplateLocationFormat =
                EnvironmentHelper.GetEnvironmentVariable("${TemplatePath}|Templates");

            ProjectLocationFormat=
                     EnvironmentHelper.GetEnvironmentVariable("${TemplatePath}|Project");
        }
    }
}
