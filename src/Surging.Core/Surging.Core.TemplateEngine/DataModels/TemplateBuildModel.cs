using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.TemplateEngine.DataModels
{
    public class TemplateBuildModel: AbstractTemplateModel
    {
        public List<TableModel> Tables { get; set; }
    }
}
