using Scriban;
using Surging.Core.CPlatform;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Surging.Core.TemplateEngine.Runtime.Implementation
{
    public class ScribanTemplateRender : ITemplateRender
    {
        private readonly VirtualPathProviderTemplateEngine _templateEngine;
        public ScribanTemplateRender(ITemplateEngine templateEngine) { 
            _templateEngine = templateEngine as VirtualPathProviderTemplateEngine; 
        }

        public async Task<string> RenderAsync([NotNull] string templateName, object? model = null, string? cultureName = null, Dictionary<string, object>? globalContext = null)
        {
            var content = "";
            var path = AppContext.BaseDirectory;
            if (!string.IsNullOrEmpty(AppConfig.ServerOptions.RootPath))
                path = AppConfig.ServerOptions.RootPath;
            using (var fileStream = new FileStream(Path.Combine(path.ToString(), _templateEngine.TemplateLocationFormat, templateName, cultureName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var reader = new StreamReader(fileStream, Encoding.UTF8);
                content =await reader.ReadToEndAsync();
            }

            var template = Template.Parse(content);
            return await template.RenderAsync(model);
        }
    }
}
