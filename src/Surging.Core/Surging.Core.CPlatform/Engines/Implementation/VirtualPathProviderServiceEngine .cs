using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Engines.Implementation
{
   public abstract class VirtualPathProviderServiceEngine: IServiceEngine
    {
        public string[] ModuleServiceLocationFormats { get; set; } 

        public string[] ComponentServiceLocationFormats { get; set; }

        public string[] ModulePaths { get; set; }

        public string[] ComponentPaths {  get; set; }
    }
}
