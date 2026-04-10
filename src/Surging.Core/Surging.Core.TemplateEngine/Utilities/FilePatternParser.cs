using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.TemplateEngine.Utilities
{
    public class FilePatternParser
    {
        public static string ParseTemplate(string template, string? prefix =null,string? project=null, string? table=null)
        {
            string result = template;
            Dictionary<string, string?> values = new Dictionary<string, string?>();
            values.Add(nameof(prefix), prefix);
            values.Add(nameof(project), project);
            values.Add(nameof(table), table);
            foreach (var pair in values)
            {
                string placeholder = $"{{{pair.Key}}}";
                result = result.Replace(placeholder, pair.Value);
            }
            return result;
        }
    }
}
