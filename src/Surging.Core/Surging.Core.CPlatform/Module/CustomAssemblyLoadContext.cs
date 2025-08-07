using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Module
{
    public class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        private string _assemblyDirectory;

        public CustomAssemblyLoadContext(string assemblyDirectory)
            : base(isCollectible: true)
        {
            _assemblyDirectory = assemblyDirectory;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            var assemblyPath = Path.Combine(_assemblyDirectory, $"{assemblyName.Name}.dll");
            if (File.Exists(assemblyPath))
            {
                return LoadFromAssemblyPath(assemblyPath);
            }
            return null;
        }
    }
}
