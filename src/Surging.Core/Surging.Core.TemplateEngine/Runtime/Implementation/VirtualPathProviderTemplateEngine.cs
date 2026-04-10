using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.TemplateEngine.Runtime.Implementation
{
    public abstract class VirtualPathProviderTemplateEngine: ITemplateEngine
    {
         public string TemplateLocationFormat { get; set; }

        public string ProjectLocationFormat {  get; set; }
    }
}
