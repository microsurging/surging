using Surging.Core.CPlatform.Engines.Implementation;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Surging.Tools.Cli2
{
   public class SurgingServiceEngine: VirtualPathProviderServiceEngine
    {
        public SurgingServiceEngine()
        {
        
            ModuleServiceLocationFormats = new[] {
                EnvironmentHelper.GetEnvironmentVariable("${ModulePath1}|Modules"),
            };
            ComponentServiceLocationFormats  = new[] {
                 EnvironmentHelper.GetEnvironmentVariable("${ComponentPath1}|Components"),
            };
            ModulePaths= EnvironmentHelper.GetEnvironmentVariable("${ModulePaths}|").Split( "|");
            ComponentPaths = EnvironmentHelper.GetEnvironmentVariable("${ComponentPaths}|").Split("|");
            //ModuleServiceLocationFormats = new[] {
            //   ""
            //};
        }
    }
}
