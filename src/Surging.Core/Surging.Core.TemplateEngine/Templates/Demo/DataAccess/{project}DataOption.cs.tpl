using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace {{ prefix }}.DataAccess.{{ project.name }}
{
    public class {{ project.name }}DataOption
    {
        public string Name { get; set; }

        public DatabaseType DatabaseType { get; set; }

        public string Connstring { get; set; }
    }
}
