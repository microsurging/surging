using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.TemplateEngine.DataModels
{
    public class TableModel
    {
        public string Name { get; set; }

        public List<ColumnModel> Columns { get; set; }

        public string Desc {  get; set; }
    }
}
