using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ywxt.Novel.Exceptions;
using Ywxt.Novel.Models;

namespace Ywxt.Novel.Configuration
{
    public class TemplateConfiguration
    {
        private static TemplateConfiguration _templateConfiguration;
        public static TemplateConfiguration GetTemplateConfiguration()
        {
            if (_templateConfiguration==null)
            {
                _templateConfiguration = new TemplateConfiguration();
            }

            return _templateConfiguration;
        }
        
        private HttpClient _httpClient = new HttpClient();
        public const string TemplatePath = "config/template";

        public IEnumerable<Template> Templates { get; set; }

        private TemplateConfiguration()
        {
            if (!Directory.Exists(TemplatePath))
            {
                Directory.CreateDirectory(TemplatePath);
            }
        }

        private IEnumerable<Template> _GetTemplates()
        {
            var files = Directory.GetFiles(TemplatePath, "*.template");
            foreach (var file in files)
            {
                Template template;
                try
                {
                    template = JsonConvert.DeserializeObject<Template>(File.ReadAllText(file));
                }
                catch (Exception e)
                {
                    template = null;
                }

                yield return template;
            }
        }

        public IEnumerable<Template> GetTemplates()
        {
            Templates = _GetTemplates();
            return Templates;
        }

        public async Task InstallTemplate(Template template, string path, bool isOverride = false)
        {
            var templatePath = Path.Combine(path, $"{template.Id}.template");
            if (File.Exists(templatePath))
            {
                if (isOverride)
                {
                    Console.WriteLine($"以下模板将被覆盖:{templatePath}");
                }
                else
                {
                    throw new InstallException($"存在同名模板：{template.Id}");
                }
            }

            var fileText = JsonConvert.SerializeObject(template);
            await File.WriteAllTextAsync(templatePath, fileText);
        }

        public async Task<Template> ParseRemoteTemplate(Uri uri)
        {
            var text = await _httpClient.GetStringAsync(uri);
            var template = JsonConvert.DeserializeObject<Template>(text);
            return template;
        }

        public async Task<Template> ParseLocalTemplate(string path)
        {
            var text = await File.ReadAllTextAsync(path);
            return JsonConvert.DeserializeObject<Template>(text);
        }
    }
}