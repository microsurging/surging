using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.TemplateEngine.DataModels
{
    public abstract class AbstractTemplateModel
    {
        public string Prefix { get; set; }
        public string Name { get; set; }
        public ProjectModel Project { get; set; }
    }
}
