using Surging.Core.CPlatform;
using Surging.Core.TemplateEngine.DataModels;
using Surging.Core.TemplateEngine.Utilities;
using System.Text;
using Surging.Core.CPlatform.Utilities;

namespace Surging.Core.TemplateEngine.Runtime.Implementation
{
    public class TemplateEngineBuilder : ITemplateEngineBuilder
    {
        private readonly ITemplateRender _templateRender;
        private readonly VirtualPathProviderTemplateEngine _templateEngine;
        private const string PROJDICNAME="Proj";
        public TemplateEngineBuilder(ITemplateEngine templateEngine, ITemplateRender templateRender) {
            _templateRender = templateRender;
            _templateEngine = (VirtualPathProviderTemplateEngine)templateEngine;
        }
        public async Task Build(TemplateBuildModel model)
        {
            Console.WriteLine("正在生成模块组件文件");
            try
            {
                var files = GetFiles(model.Name);

                foreach (var file in files)
                {
                    var extension = Path.GetExtension(file);
                    if (extension.ToLower() == ".tpl")
                    {
                        await GenerateFile(file, new TemplateModel
                        {
                            Name = model.Name,
                            Prefix = model.Prefix,
                            Project = model.Project,
                        });
                    }
                    else if (extension.ToLower() == ".ctpl")
                    {
                        foreach (var item in model.Tables)
                        {
                            await GenerateFile(file, new TemplateModel
                            {
                                Name = model.Name,
                                Prefix = model.Prefix,
                                Project = model.Project,
                                Table = item
                            });
                        }
                    }
                    else if (extension.ToLower() == ".ptpl")
                    {
                        await GenerateFile(file, new AppTemplateModel
                        {
                            Name = model.Name,
                            Prefix = model.Prefix,
                            Project = model.Project,
                            Tables = model.Tables
                        });
                    }
                    else
                    {
                        await GenerateFile(file, new AppTemplateModel
                        {
                            Name = model.Name,
                            Prefix = model.Prefix,
                            Project = model.Project,
                            Tables = model.Tables
                        }, false);
                    }
                }
            }
            catch (Exception ex) { 
                 Console.WriteLine(ex.ToString());  
            }
            Console.WriteLine("生成完成！");
        }

        public async Task GenerateFile(string file, TemplateModel model,bool isTemplate=true)
        {
            var cultureName = GetCultureName(model.Name, file);   
            var projdir = GetProjDir(model.Name, Path.GetDirectoryName(cultureName) ?? "");
            if (!isTemplate)
                File.Copy(file, Path.Combine(projdir, Path.GetFileName(file)),true);
            else
            {
                var templateText = await _templateRender.RenderAsync(model.Name, model: model, cultureName: GetCultureName(model.Name, file));
                using (var stream = File.Open(Path.Combine(projdir, FilePatternParser.ParseTemplate(Path.GetFileName(file).TrimEnd("ptpl").TrimEnd("ctpl").TrimEnd("tpl").ToString(), prefix: model.Prefix, table: model.Table?.Name, project: model.Project.Name)), FileMode.OpenOrCreate))
                {
                    await stream.WriteAsync(new byte[0]);
                    await stream.WriteAsync(UTF8Encoding.UTF8.GetBytes(templateText));
                }
            }
        }

        private string[] GetFiles(string templateName)
        { 
           var virtualPath= GetVirtualPath(templateName);
           return    Directory.EnumerateFiles(virtualPath, "*.*", SearchOption.AllDirectories).ToArray();
        }

       private string GetVirtualPath(string templateName)
        {
            string rootPath = string.IsNullOrEmpty(AppConfig.ServerOptions.RootPath) ?
              AppContext.BaseDirectory : AppConfig.ServerOptions.RootPath;
            var virtualPath = Path.Combine(rootPath, _templateEngine.TemplateLocationFormat, templateName);
            return virtualPath;
        }

        private string GetCultureName(string templateName,string filePath)
        {
           return  filePath.Replace($@"{GetVirtualPath(templateName)}\", "");
        }

        private string GetProjDir(string templateName,string cultureDir)
        {
            string rootPath = string.IsNullOrEmpty(AppConfig.ServerOptions.RootPath) ?
         AppContext.BaseDirectory : AppConfig.ServerOptions.RootPath;
            var virtualPath = Path.Combine(rootPath, PROJDICNAME, templateName, cultureDir);
            if (!Path.Exists(virtualPath)) { Directory.CreateDirectory(virtualPath); } 
            return virtualPath;
        }
    }
}
