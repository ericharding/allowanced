using System.Collections.Immutable;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.FileProviders;
using Scriban;

namespace allowanced
{
    public interface IFileProvider
    {
        string getFileContents(string fileName);
        Scriban.Template getTemplate(string fileName);
    }

    public record LocalFileProvider(string root) : IFileProvider
    {
        public string getFileContents(string fileName)
        {
            return File.ReadAllText(Path.Combine(root, fileName));
        }

        public Template getTemplate(string fileName)
        {
            var content = getFileContents(fileName);
            return Template.Parse(content);
        }
    }

    public record EmbeddedResourceProvider(System.Reflection.Assembly assembly, string root) : IFileProvider
    {
        EmbeddedFileProvider embeddedFileProvider = new EmbeddedFileProvider(assembly, root);
        ImmutableDictionary<string, Template> templateCache = ImmutableDictionary<string, Template>.Empty;

        public string getFileContents(string fileName)
        {
            var fileInfo = embeddedFileProvider.GetFileInfo(fileName);
            using var stream = fileInfo.CreateReadStream();
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public Template getTemplate(string fileName)
        {
            if (templateCache.TryGetValue(fileName, out var template))
            {
                return template;
            }
            else
            {
                var content = getFileContents(fileName);
                template = Template.Parse(content);
                templateCache = templateCache.Add(fileName, template);
                return template;
            }
        }
    }

    public static class FileProviderExtensions {
        public static IServiceCollection ConfigureFileProvider(this IServiceCollection services) {
            #if DEBUG
                services.AddSingleton<IFileProvider>(new LocalFileProvider("www"));
            #else
                services.AddSingleton<IFileProvider>(new embeddedFileProvider(
                    System.Reflection.Assembly.GetExecutingAssembly(),
                    "allowanced/www")
            #endif
            return services;
        }
    }
}
